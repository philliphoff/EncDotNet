using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using EncDotNet.ChartViewer.Charts;
using EncDotNet.ChartViewer.Models;
using EncDotNet.ChartViewer.ViewModels;
using EncDotNet.S57;
using Mapsui;
using Microsoft.Extensions.DependencyInjection;
using Mapsui.Extensions;
using Mapsui.Layers;
using System.Collections.Immutable;
using Mapsui.Manipulations;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using Mapsui.Widgets.ScaleBar;

namespace EncDotNet.ChartViewer.Views;

public partial class MainWindow : Window
{
    private static readonly VectorStyle NormalBoundaryStyle = new() { Fill = null, Outline = new Pen(Color.Red, 1) };
    private static readonly VectorStyle HighlightBoundaryStyle = new() { Fill = null, Outline = new Pen(Color.Yellow, 3) };

    private readonly Dictionary<GeometryFeature, ChartViewModel> _boundaryFeatureToChart = new();
    private readonly List<GeometryFeature> _boundaryFeatures = [];
    private GeometryFeature? _highlightedBoundaryFeature;

    /// <summary>Tracks which charts are currently loaded on the map.</summary>
    private readonly HashSet<ChartViewModel> _loadedCharts = new();

    /// <summary>Charts currently being loaded (prevents duplicate loads).</summary>
    private readonly HashSet<ChartViewModel> _loadingCharts = new();

    /// <summary>Debounce timer for viewport change evaluation.</summary>
    private CancellationTokenSource? _viewportDebounce;

    /// <summary>Whether the catalog has been loaded and viewport-driven loading is active.</summary>
    private bool _viewportLoadingActive;

    public MainWindow()
    {
        InitializeComponent();

        MyMapControl.Map?.Widgets.Add(
            new ScaleBarWidget(MyMapControl.Map)
            {
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center,
                MaxWidth = 200,
                UnitConverter = NauticalUnitConverter.Instance,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top,
            });

        MyMapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Enable pinch-to-zoom on the map control via trackpad magnify gesture
        MyMapControl.AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, OnMapMagnify);

        // Enable double-tap to zoom in
        MyMapControl.DoubleTapped += OnMapDoubleTapped;

        // Enable trackpad scroll/swipe to pan the map (tunnel phase to intercept before MapControl)
        MyMapControl.AddHandler(PointerWheelChangedEvent, OnMapPointerWheelChanged, RoutingStrategies.Tunnel);

        // Enable hover highlighting of chart boundaries
        MyMapControl.PointerMoved += OnMapPointerMoved;

        // Enable click-to-identify on default-rendered (red circle) point features
        MyMapControl.Tapped += OnMapFeatureTapped;

        ZoomInButton.Click += OnZoomInClick;
        ZoomOutButton.Click += OnZoomOutClick;
        DiagnosticButton.Click += OnDiagnosticButtonClick;

        // Ctrl+Shift+D (or Cmd+Shift+D on macOS) captures diagnostic state to clipboard.
        KeyDown += OnDiagnosticKeyDown;
    }

    private void OnChartItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { DataContext: ChartViewModel chart })
            return;

        if (chart.ProjectedBounds is not { } bounds || MyMapControl.Map?.Navigator is not { } navigator)
            return;

        // Inflate bounds by 10% on each side to provide margin
        var marginX = bounds.Width * 0.10;
        var marginY = bounds.Height * 0.10;
        var paddedBounds = new MRect(
            bounds.MinX - marginX,
            bounds.MinY - marginY,
            bounds.MaxX + marginX,
            bounds.MaxY + marginY);

        navigator.ZoomToBox(paddedBounds, MBoxFit.Fit, 500);
    }

    private void OnZoomInClick(object? sender, RoutedEventArgs e)
    {
        if (MyMapControl.Map?.Navigator is not { } navigator)
            return;

        navigator.ZoomTo(navigator.Viewport.Resolution / 2, 250);
    }

    private void OnZoomOutClick(object? sender, RoutedEventArgs e)
    {
        if (MyMapControl.Map?.Navigator is not { } navigator)
            return;

        navigator.ZoomTo(navigator.Viewport.Resolution * 2, 250);
    }

    private void OnMapMagnify(object? sender, PointerDeltaEventArgs e)
    {
        if (MyMapControl.Map?.Navigator is not { } navigator)
            return;

        var resolution = navigator.Viewport.Resolution;
        var newResolution = resolution / (1 + e.Delta.Y);
        var position = e.GetPosition(MyMapControl);
        var center = new ScreenPosition(position.X, position.Y);
        navigator.ZoomTo(newResolution, center);
        e.Handled = true;
        UpdatePopupPosition();
    }

    private void OnMapPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (MyMapControl.Map?.Navigator is not { } navigator)
            return;

        var viewport = navigator.Viewport;
        var dx = e.Delta.X * viewport.Resolution * 50;
        var dy = e.Delta.Y * viewport.Resolution * 50;
        navigator.CenterOn(viewport.CenterX - dx, viewport.CenterY + dy);
        e.Handled = true;
        UpdatePopupPosition();
    }

    private void OnMapDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (MyMapControl.Map?.Navigator is not { } navigator)
            return;

        var resolution = navigator.Viewport.Resolution;
        var newResolution = resolution / 2;
        var position = e.GetPosition(MyMapControl);
        var center = new ScreenPosition(position.X, position.Y);
        navigator.ZoomTo(newResolution, center, 250);
        e.Handled = true;
        UpdatePopupPosition();
    }

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        if (MyMapControl.Map?.Navigator is not { } navigator)
            return;

        var screenPos = e.GetPosition(MyMapControl);
        var worldPos = navigator.Viewport.ScreenToWorld(new ScreenPosition(screenPos.X, screenPos.Y));
        var point = new Point(worldPos.X, worldPos.Y);

        // Find the smallest chart boundary that contains the pointer
        GeometryFeature? best = null;
        double bestArea = double.MaxValue;

        foreach (var feature in _boundaryFeatures)
        {
            if (feature.Geometry is Polygon polygon && polygon.Contains(point))
            {
                var area = polygon.Area;
                if (area < bestArea)
                {
                    bestArea = area;
                    best = feature;
                }
            }
        }

        // Update highlight state
        if (best != _highlightedBoundaryFeature)
        {
            if (_highlightedBoundaryFeature is not null)
                _highlightedBoundaryFeature["Highlighted"] = false;

            _highlightedBoundaryFeature = best;

            if (best is not null)
                best["Highlighted"] = true;

            MyMapControl.Map?.Refresh();
        }

        UpdatePopupPosition();
    }

    private void UpdatePopupPosition()
    {
        if (MyMapControl.Map?.Navigator is not { } navigator || ViewModel is not { } vm)
            return;

        if (_highlightedBoundaryFeature?.Geometry is Polygon bestPolygon
            && _boundaryFeatureToChart.TryGetValue(_highlightedBoundaryFeature, out var chartVm))
        {
            var env = bestPolygon.EnvelopeInternal;
            var topRightScreen = navigator.Viewport.WorldToScreen(env.MaxX, env.MaxY);
            var topLeftScreen = navigator.Viewport.WorldToScreen(env.MinX, env.MaxY);
            var chartScreenWidth = topRightScreen.X - topLeftScreen.X;

            if (chartScreenWidth >= 200)
            {
                vm.HoveredChart = chartVm;

                ChartPopup.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
                var popupWidth = ChartPopup.DesiredSize.Width;

                Canvas.SetLeft(ChartPopup, topRightScreen.X - popupWidth);
                Canvas.SetTop(ChartPopup, topRightScreen.Y);
            }
            else
            {
                vm.HoveredChart = null;
            }
        }
        else
        {
            vm.HoveredChart = null;
        }
    }

    private async void OnMapFeatureTapped(object? sender, TappedEventArgs e)
    {
        if (MyMapControl.Map is not { } map)
            return;

        var position = e.GetPosition(MyMapControl);
        var screenPos = new ScreenPosition(position.X, position.Y);
        var mapInfo = MyMapControl.GetMapInfo(screenPos, map.Layers);
        if (mapInfo?.Feature is not { } feature)
            return;

        if (feature["IsDefaultRendering"] is not true)
            return;

        var objectCode = feature["ObjectCode"] is S57ObjectCode code ? code.ToString() : "Unknown";
        var objectCodeValue = feature["ObjectCodeValue"] is int codeVal ? codeVal : (int?)null;
        var chartName = feature["ChartName"] as string ?? "Unknown";
        var lat = feature["Latitude"] is double latVal ? latVal.ToString("F6") : "N/A";
        var lon = feature["Longitude"] is double lonVal ? lonVal.ToString("F6") : "N/A";
        var group = feature["Group"] is int g ? g : (int?)null;
        var attributes = feature["FeatureAttributes"] as IReadOnlyList<S57AttributeValue>;

        var dialog = new FeatureInfoWindow(objectCode, objectCodeValue, chartName, lat, lon, group, attributes);
        await dialog.ShowDialog(this);
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // Show setup wizard if no charts have been downloaded yet
        if (!AppDataPaths.HasChartIndex())
        {
            var wizardVm = App.Services.GetRequiredService<SetupWizardViewModel>();
            var wizard = new SetupWizardWindow { DataContext = wizardVm };
            await wizard.ShowDialog(this);
        }

        // Load catalog if chart index exists (either previously or from wizard)
        await LoadCatalogIntoMapAsync();

        // Restore saved viewport position
        if (MyMapControl.Map?.Navigator is { } nav
            && AppDataPaths.LoadViewportState() is var (cx, cy, res))
        {
            nav.CenterOnAndZoomTo(new MPoint(cx, cy), res);
        }

        // Wire up the manage charts command
        if (ViewModel is { } vm2)
        {
            vm2.ManageChartsCommand.Subscribe(async _ => await OpenManageChartsAsync());
            vm2.ResetDataCommand.Subscribe(async _ => await ResetAllDataAsync());
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (MyMapControl.Map?.Navigator is { } nav)
        {
            var vp = nav.Viewport;
            AppDataPaths.SaveViewportState(vp.CenterX, vp.CenterY, vp.Resolution);
        }

        if (ViewModel is { } vm)
        {
            AppDataPaths.SaveFeatureVisibility(
                vm.FeatureCategories
                    .SelectMany(c => c.Features)
                    .ToDictionary(f => f.ObjectCode.ToString(), f => f.IsVisible));

            AppDataPaths.SaveDepthUnit(vm.DepthUnit);
        }
    }

    private async Task LoadCatalogIntoMapAsync()
    {
        if (ViewModel is not { } vm || !AppDataPaths.HasChartIndex())
            return;

        await vm.LoadCatalogAsync();

        // Show chart boundaries on the map
        var boundaryLayer = CreateChartBoundariesLayer(vm.AvailableCharts);
        MyMapControl.Map?.Layers.Add(boundaryLayer);

        // Subscribe to feature visibility changes
        foreach (var featureVm in vm.FeatureCategories)
        {
            featureVm.FeatureVisibilityChanged += OnFeatureItemVisibilityChanged;
        }

        // Subscribe to depth unit changes
        vm.DepthUnitChanged += OnDepthUnitChanged;

        // Restore saved depth unit
        vm.DepthUnit = AppDataPaths.LoadDepthUnit();

        // Restore saved feature visibility
        var savedVisibility = AppDataPaths.LoadFeatureVisibility();
        foreach (var featureVm in vm.FeatureCategories)
        {
            foreach (var featureItem in featureVm.Features)
            {
                if (savedVisibility.TryGetValue(featureItem.ObjectCode.ToString(), out var isVisible))
                {
                    featureItem.IsVisible = isVisible;
                }
                // Backward compat: fall back to category name
                else if (savedVisibility.TryGetValue(featureVm.Name, out var catVisible))
                {
                    featureItem.IsVisible = catVisible;
                }
            }
        }

        // Enable viewport-driven chart loading
        _viewportLoadingActive = true;
        if (MyMapControl.Map?.Navigator is { } nav)
        {
            nav.ViewportChanged += OnNavigatorViewportChanged;
        }

        // Perform initial evaluation after viewport is restored
        ScheduleViewportEvaluation();
    }

    private async Task OpenManageChartsAsync()
    {
        var manageVm = App.Services.GetRequiredService<ManageChartsViewModel>();
        var manageWindow = new ManageChartsWindow { DataContext = manageVm };
        manageVm.BeginFetchCatalog();
        await manageWindow.ShowDialog(this);

        if (manageVm.ChartsChanged)
        {
            await ReloadCatalogAsync();
        }
    }

    private async Task ResetAllDataAsync()
    {
        if (ViewModel is not { } vm)
            return;

        // Stop viewport-driven loading
        _viewportLoadingActive = false;
        _viewportDebounce?.Cancel();
        if (MyMapControl.Map?.Navigator is { } resetNav)
            resetNav.ViewportChanged -= OnNavigatorViewportChanged;

        // Unload all loaded charts from the map
        foreach (var chartVm in _loadedCharts.ToArray())
        {
            UnloadChart(chartVm);
            chartVm.ClearLayerCache();
        }
        _loadedCharts.Clear();
        _loadingCharts.Clear();

        // Remove the old boundary layer
        if (MyMapControl.Map is { } map)
        {
            var boundaryLayer = map.Layers.FirstOrDefault(l => l.Name == "Chart Boundaries");
            if (boundaryLayer is not null)
                map.Layers.Remove(boundaryLayer);
        }

        _boundaryFeatures.Clear();
        _boundaryFeatureToChart.Clear();
        _highlightedBoundaryFeature = null;

        // Unsubscribe from old feature events
        foreach (var featureVm in vm.FeatureCategories)
            featureVm.FeatureVisibilityChanged -= OnFeatureItemVisibilityChanged;
        vm.DepthUnitChanged -= OnDepthUnitChanged;

        // Clear in-memory state
        vm.ClearCatalog();

        // Delete all data from disk
        await Task.Run(AppDataPaths.DeleteAllData);

        // Re-show the setup wizard
        var wizardVm = App.Services.GetRequiredService<SetupWizardViewModel>();
        var wizard = new SetupWizardWindow { DataContext = wizardVm };
        await wizard.ShowDialog(this);

        // Reload catalog if the wizard completed successfully
        await LoadCatalogIntoMapAsync();
    }

    private async Task ReloadCatalogAsync()
    {
        if (ViewModel is not { } vm)
            return;

        // Stop viewport-driven loading
        _viewportLoadingActive = false;
        _viewportDebounce?.Cancel();
        if (MyMapControl.Map?.Navigator is { } reloadNav)
            reloadNav.ViewportChanged -= OnNavigatorViewportChanged;

        // Unload all loaded charts from the map
        foreach (var chartVm in _loadedCharts.ToArray())
        {
            UnloadChart(chartVm);
            chartVm.ClearLayerCache();
        }
        _loadedCharts.Clear();
        _loadingCharts.Clear();

        // Remove the old boundary layer
        if (MyMapControl.Map is { } map)
        {
            var boundaryLayer = map.Layers.FirstOrDefault(l => l.Name == "Chart Boundaries");
            if (boundaryLayer is not null)
                map.Layers.Remove(boundaryLayer);
        }

        _boundaryFeatures.Clear();
        _boundaryFeatureToChart.Clear();
        _highlightedBoundaryFeature = null;

        // Unsubscribe from old feature events
        foreach (var featureVm in vm.FeatureCategories)
            featureVm.FeatureVisibilityChanged -= OnFeatureItemVisibilityChanged;
        vm.DepthUnitChanged -= OnDepthUnitChanged;

        // Clear and reload
        vm.ClearCatalog();
        await LoadCatalogIntoMapAsync();
    }

    /// <summary>
    /// Called when the Mapsui navigator signals a viewport change (pan, zoom, resize).
    /// Debounces and then evaluates which charts should be loaded/unloaded.
    /// </summary>
    private void OnNavigatorViewportChanged(object? sender, ViewportChangedEventArgs e)
    {
        if (!_viewportLoadingActive)
            return;

        ScheduleViewportEvaluation();
    }

    /// <summary>
    /// Schedules a debounced viewport evaluation on the UI thread.
    /// </summary>
    private void ScheduleViewportEvaluation()
    {
        _viewportDebounce?.Cancel();
        var cts = _viewportDebounce = new CancellationTokenSource();

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await Task.Delay(200, cts.Token);
                await EvaluateViewportChartsAsync();
            }
            catch (TaskCanceledException)
            {
                // Superseded by a newer viewport change
            }
        });
    }

    /// <summary>
    /// Maximum number of charts to keep loaded at once. Prevents loading hundreds of
    /// small-scale charts when very zoomed out. Mapsui layer MaxVisible already hides
    /// layers that are beyond their compilation scale, so keeping a bounded set loaded
    /// is safe — excess charts simply won't render.
    /// </summary>
    private const int MaxLoadedCharts = 30;

    /// <summary>
    /// Determines which charts should be visible based on viewport overlap and zoom level,
    /// then loads/unloads charts accordingly.
    /// </summary>
    private async Task EvaluateViewportChartsAsync()
    {
        if (ViewModel is not { } vm || MyMapControl.Map?.Navigator is not { } nav)
            return;

        var evalStopwatch = Stopwatch.StartNew();

        var viewport = nav.Viewport;
        var extent = viewport.ToExtent();
        if (extent is null)
            return;

        var resolution = viewport.Resolution;

        // Determine which charts overlap the viewport.
        // Prefer more-detailed charts (lower CompilationScale) by sorting candidates
        // so they are loaded first and fill the budget.
        var candidates = new List<ChartViewModel>();
        foreach (var chartVm in vm.AvailableCharts)
        {
            if (ChartOverlapsViewport(chartVm, extent))
            {
                candidates.Add(chartVm);
            }
        }

        // Sort candidates so the budget is filled with the most useful charts first.
        // Priority tiers (lower = more important):
        //   0: Already loaded AND renderable at the current viewport resolution
        //   1: Not yet loaded but renderable (candidates that should enter the map)
        //   2: Already loaded but NOT renderable (keep warm if budget allows)
        //   3: Not loaded and not renderable (lowest value at this zoom)
        // Within each tier, prefer lower CompilationScale (more detail).
        // Charts with unknown scale (CompilationScale == 0) are treated as
        // potentially renderable so they get a chance to load.
        candidates.Sort((a, b) =>
        {
            int aPriority = ChartSortPriority(a, resolution);
            int bPriority = ChartSortPriority(b, resolution);
            if (aPriority != bPriority)
                return aPriority.CompareTo(bPriority);

            int aScale = a.CompilationScale > 0 ? a.CompilationScale : int.MaxValue;
            int bScale = b.CompilationScale > 0 ? b.CompilationScale : int.MaxValue;
            return aScale.CompareTo(bScale);
        });

        var shouldBeVisible = new HashSet<ChartViewModel>();
        foreach (var chartVm in candidates)
        {
            if (shouldBeVisible.Count >= MaxLoadedCharts)
                break;
            shouldBeVisible.Add(chartVm);
        }

        // --- Load new charts BEFORE unloading old ones to avoid flashing ---

        // Capture the debounce token so we can bail out if the viewport changes mid-load.
        var token = _viewportDebounce;

        // Load charts one at a time, but offload CPU-heavy parsing + geometry
        // building to the thread pool (inside BuildLayersAsync) so the UI
        // thread stays responsive.  Each chart is inserted as soon as it's ready,
        // giving the user progressive visual feedback.
        foreach (var chartVm in candidates)
        {
            if (!shouldBeVisible.Contains(chartVm))
                continue;

            if (_loadedCharts.Contains(chartVm) || _loadingCharts.Contains(chartVm))
                continue;

            // Bail out early if the viewport changed while we were loading.
            if (token != _viewportDebounce)
                break;

            _loadingCharts.Add(chartVm);
            try
            {
                var chart = await Task.Run(() => vm.GetChartAsync(chartVm.Entry));
                var settings = CreateLayerSettings(vm, chartVm.Name);
                var update = await chartVm.BuildLayersAsync(chart, settings);

                // Re-check after the await — viewport may have changed.
                if (token == _viewportDebounce && update.ActiveLayers.Count > 0)
                {
                    InsertAllChartLayers(chartVm, update, vm);
                    _loadedCharts.Add(chartVm);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading chart {chartVm.Name}: {ex.Message}");
            }
            finally
            {
                _loadingCharts.Remove(chartVm);
            }
        }

        // Now unload charts that should no longer be visible.
        // Because we loaded first, the map never has a "blank" frame.
        foreach (var chartVm in _loadedCharts.ToArray())
        {
            if (!shouldBeVisible.Contains(chartVm))
            {
                UnloadChart(chartVm);
                _loadedCharts.Remove(chartVm);
            }
        }

        // Recompute MinVisible for all loaded charts so that each chart's layers
        // hide once a finer-scale chart's layers take over. This adapts dynamically
        // as charts are loaded and unloaded, ensuring no zoom-level gaps.
        var recalcStopwatch = Stopwatch.StartNew();
        S57LayerFactory.RecalculateMinVisible(_loadedCharts);
        recalcStopwatch.Stop();
        ChartViewerDiagnostics.RecalculateMinVisibleDuration.Record(
            recalcStopwatch.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("loaded.charts", _loadedCharts.Count));

        evalStopwatch.Stop();
        ChartViewerDiagnostics.ViewportEvaluationDuration.Record(
            evalStopwatch.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("loaded.charts", _loadedCharts.Count));
    }

    /// <summary>
    /// Returns true if the chart's pre-computed projected bounds overlap the given viewport extent.
    /// </summary>
    private static bool ChartOverlapsViewport(ChartViewModel chartVm, MRect viewportExtent)
    {
        if (chartVm.ProjectedBounds is not { } bounds)
            return false;

        // Axis-aligned bounding box intersection test
        return bounds.MinX <= viewportExtent.MaxX
            && bounds.MaxX >= viewportExtent.MinX
            && bounds.MinY <= viewportExtent.MaxY
            && bounds.MaxY >= viewportExtent.MinY;
    }

    /// <summary>
    /// Returns a sort priority for a chart candidate (lower = more important).
    /// Charts that are renderable at the current viewport resolution are preferred
    /// over charts that are invisible, preventing the budget from filling with
    /// detailed charts that can't actually render when zoomed out.
    /// </summary>
    private int ChartSortPriority(ChartViewModel chartVm, double resolution)
    {
        bool loaded = _loadedCharts.Contains(chartVm);
        bool renderable = IsRenderableAtResolution(chartVm.CompilationScale, resolution);
        return (loaded, renderable) switch
        {
            (true, true) => 0,   // loaded + renderable: highest priority
            (false, true) => 1,  // not loaded but would render: should enter
            (true, false) => 2,  // loaded but invisible: keep warm if room
            _ => 3,              // not loaded, not renderable: lowest
        };
    }

    /// <summary>
    /// Returns true if a chart with the given compilation scale would have its layers
    /// rendered at the specified viewport resolution.
    /// </summary>
    private static bool IsRenderableAtResolution(int compilationScale, double resolution)
    {
        // Unknown scale (0) — optimistically treat as renderable so it gets a chance to load.
        if (compilationScale <= 0)
            return true;

        double maxVisible = compilationScale * S57LayerFactory.PixelSizeMeters * S57LayerFactory.OverScaleFactor;
        return resolution <= maxVisible;
    }

    // ── Layer pipeline helpers ──────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="ChartLayerSettings"/> snapshot from the current view model state.
    /// </summary>
    private static ChartLayerSettings CreateLayerSettings(MainWindowViewModel vm, string chartName)
    {
        var enabledCodes = vm.FeatureCategories
            .SelectMany(c => c.Features)
            .Where(f => f.IsVisible)
            .Select(f => f.ObjectCode)
            .ToImmutableHashSet();

        return new ChartLayerSettings(vm.DepthUnit, enabledCodes, chartName);
    }

    /// <summary>
    /// Inserts all active layers from a <see cref="LayerUpdate"/> into the Mapsui map.
    /// Used when a chart is first loaded or re-enters the viewport from cache.
    /// Must be called on the UI thread.
    /// </summary>
    private void InsertAllChartLayers(ChartViewModel chartVm, LayerUpdate update, MainWindowViewModel vm)
    {
        if (MyMapControl.Map is not { } map)
            return;

        int insertIndex = FindChartLayerInsertionIndex(map, chartVm.CompilationScale, vm);
        map.Layers.Insert(insertIndex, update.ActiveLayers.ToList());
        EnsureBoundaryLayerOnTop(map);

        var tags = new KeyValuePair<string, object?>("chart.name", chartVm.Name);
        ChartViewerDiagnostics.ChartsLoaded.Add(1, tags);
        Debug.WriteLine($"Loaded chart: {chartVm.Name}");
    }

    /// <summary>
    /// Applies the diff from a <see cref="LayerUpdate"/> to the Mapsui map,
    /// removing old layers and inserting new ones at the correct positions.
    /// Used for in-place updates (feature toggle, depth unit change) on already-loaded charts.
    /// Must be called on the UI thread.
    /// </summary>
    private void ApplyLayerDiff(ChartViewModel chartVm, LayerUpdate update, Map map, MainWindowViewModel vm)
    {
        foreach (var layer in update.Removed)
            map.Layers.Remove(layer);

        foreach (var layer in update.Added)
        {
            int idx = FindLayerInsertionIndex(map, chartVm, layer, vm);
            map.Layers.Insert(idx, layer);
        }

        if (update.Added.Count > 0)
            EnsureBoundaryLayerOnTop(map);
    }

    /// <summary>
    /// Removes a chart's active layers from the Mapsui map without clearing
    /// the layer cache, so re-entering the viewport reuses cached layers.
    /// </summary>
    private void UnloadChart(ChartViewModel chartVm)
    {
        foreach (var layer in chartVm.Layers)
        {
            MyMapControl.Map?.Layers.Remove(layer);
        }

        MyMapControl.Map?.Refresh();
    }

    /// <summary>
    /// Handles a feature visibility toggle by rebuilding layers for all loaded charts
    /// with the updated settings and applying the diff to the map.
    /// </summary>
    private async void OnFeatureItemVisibilityChanged(object? sender, ChartFeatureItemViewModel featureItem)
    {
        if (ViewModel is not { } vm || MyMapControl.Map is not { } map)
            return;

        foreach (var chartVm in _loadedCharts)
        {
            var settings = CreateLayerSettings(vm, chartVm.Name);
            var chart = await vm.GetChartAsync(chartVm.Entry);
            var update = await chartVm.BuildLayersAsync(chart, settings);
            ApplyLayerDiff(chartVm, update, map, vm);
        }

        map.Refresh();
    }

    /// <summary>
    /// Handles a depth unit change by rebuilding layers for all loaded charts
    /// with the updated settings and applying the diff to the map.
    /// Only SOUNDG layers are actually regenerated; all others are cache hits.
    /// </summary>
    private async void OnDepthUnitChanged(object? sender, Models.DepthUnit depthUnit)
    {
        if (ViewModel is not { } vm || MyMapControl.Map is not { } map)
            return;

        foreach (var chartVm in _loadedCharts)
        {
            var settings = CreateLayerSettings(vm, chartVm.Name);
            var chart = await vm.GetChartAsync(chartVm.Entry);
            var update = await chartVm.BuildLayersAsync(chart, settings);
            ApplyLayerDiff(chartVm, update, map, vm);
        }

        map.Refresh();
    }

    /// <summary>
    /// Finds the insertion index in the map's layer collection for a chart with the given
    /// compilation scale. Higher CSCL charts are placed before lower CSCL charts.
    /// </summary>
    private int FindChartLayerInsertionIndex(Map map, int compilationScale, MainWindowViewModel vm)
    {
        int index = 0;
        foreach (var existingLayer in map.Layers)
        {
            var ownerChart = vm.AvailableCharts.FirstOrDefault(
                c => _loadedCharts.Contains(c) && c.Layers.Contains(existingLayer));

            if (ownerChart != null && ownerChart.CompilationScale < compilationScale)
            {
                return index;
            }

            index++;
        }
        return index;
    }

    private int FindMapLayerIndex(ILayer layer)
    {
        if (MyMapControl.Map is not { } map)
            return -1;

        int index = 0;
        foreach (var ml in map.Layers)
        {
            if (ReferenceEquals(ml, layer))
                return index;
            index++;
        }
        return -1;
    }

    /// <summary>
    /// Finds the map insertion index for a single layer being toggled on, respecting both
    /// the chart's compilation-scale ordering and the render order within the chart.
    /// </summary>
    private int FindLayerInsertionIndex(Map map, ChartViewModel chartVm, MemoryLayer layer, MainWindowViewModel vm)
    {
        // Find the position of this layer within the chart's sorted layer list.
        int layerIndex = -1;
        for (int j = 0; j < chartVm.Layers.Count; j++)
        {
            if (ReferenceEquals(chartVm.Layers[j], layer))
            {
                layerIndex = j;
                break;
            }
        }

        // Find the last sibling layer from the same chart that precedes this layer
        // and is currently in the map — insert right after it.
        for (int i = layerIndex - 1; i >= 0; i--)
        {
            int mapIdx = FindMapLayerIndex(chartVm.Layers[i]);
            if (mapIdx >= 0)
                return mapIdx + 1;
        }

        // No preceding sibling found — fall back to the chart-level insertion index.
        return FindChartLayerInsertionIndex(map, chartVm.CompilationScale, vm);
    }

    /// <summary>
    /// Moves the "Chart Boundaries" layer to the end of the map's layer collection
    /// so that outlines and highlights are always rendered on top of chart data.
    /// </summary>
    private static void EnsureBoundaryLayerOnTop(Map map)
    {
        var boundaryLayer = map.Layers.FirstOrDefault(l => l.Name == "Chart Boundaries");
        if (boundaryLayer is not null)
        {
            map.Layers.Remove(boundaryLayer);
            map.Layers.Add(boundaryLayer);
        }
    }

    private MemoryLayer CreateChartBoundariesLayer(
        IEnumerable<ChartViewModel> charts)
    {
        var features = new List<IFeature>();

        foreach (var chartVm in charts)
        {
            if (chartVm.ProjectedBounds is not { } bounds)
                continue;

            var ring = new LinearRing(
            [
                new Coordinate(bounds.MinX, bounds.MinY),
                new Coordinate(bounds.MaxX, bounds.MinY),
                new Coordinate(bounds.MaxX, bounds.MaxY),
                new Coordinate(bounds.MinX, bounds.MaxY),
                new Coordinate(bounds.MinX, bounds.MinY),
            ]);

            var feature = new GeometryFeature(new Polygon(ring));
            features.Add(feature);
            _boundaryFeatures.Add(feature);
            _boundaryFeatureToChart[feature] = chartVm;
        }

        return new MemoryLayer
        {
            Name = "Chart Boundaries",
            Features = features,
            Style = new BoundaryThemeStyle(),
        };
    }

    // ────────────────────────────────────────────────────────────────────
    // Diagnostic capture (Ctrl+Shift+D / Cmd+Shift+D)
    // ────────────────────────────────────────────────────────────────────

    private void OnDiagnosticButtonClick(object? sender, RoutedEventArgs e)
    {
        _ = CaptureDiagnosticsAsync();
    }

    private void OnDiagnosticKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.D)
            return;

        bool hasModifier = e.KeyModifiers.HasFlag(KeyModifiers.Shift)
            && (e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta));

        if (!hasModifier)
            return;

        e.Handled = true;
        _ = CaptureDiagnosticsAsync();
    }

    private async Task CaptureDiagnosticsAsync()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("# ChartViewer Diagnostic Snapshot");
            sb.AppendLine($"- **Timestamp**: {DateTime.UtcNow:O}");
            sb.AppendLine();

            // ── Viewport ──
            if (MyMapControl.Map?.Navigator is { } nav)
            {
                var vp = nav.Viewport;
                var extent = vp.ToExtent();
                sb.AppendLine("## Viewport");
                sb.AppendLine($"- **CenterX**: {vp.CenterX:F2}");
                sb.AppendLine($"- **CenterY**: {vp.CenterY:F2}");

                // Convert center from Mercator to geographic for convenience.
                var (lon, lat) = SphericalMercator.ToLonLat(vp.CenterX, vp.CenterY);
                sb.AppendLine($"- **Center (lon, lat)**: ({lon:F6}, {lat:F6})");
                sb.AppendLine($"- **Resolution**: {vp.Resolution:F6}");
                sb.AppendLine($"- **Rotation**: {vp.Rotation:F2}");
                sb.AppendLine($"- **Width**: {vp.Width:F0}");
                sb.AppendLine($"- **Height**: {vp.Height:F0}");

                if (extent is not null)
                {
                    sb.AppendLine($"- **Extent**: ({extent.MinX:F2}, {extent.MinY:F2}) — ({extent.MaxX:F2}, {extent.MaxY:F2})");
                }

                sb.AppendLine();
            }

            // ── Loaded Charts ──
            sb.AppendLine("## Loaded Charts");
            if (_loadedCharts.Count == 0)
            {
                sb.AppendLine("_(none)_");
            }
            else
            {
                sb.AppendLine("| Chart | CSCL | Layers | Enabled | Visible Range |");
                sb.AppendLine("|---|---|---|---|---|");
                foreach (var chartVm in _loadedCharts.OrderBy(c => c.CompilationScale))
                {
                    int totalLayers = chartVm.Layers.Count;
                    int enabledLayers = chartVm.Layers.Count(l => l.Enabled);
                    double minMin = chartVm.Layers.Count > 0
                        ? chartVm.Layers.Min(l => l.MinVisible)
                        : 0;
                    double maxMax = chartVm.Layers.Count > 0
                        ? chartVm.Layers.Max(l => l.MaxVisible)
                        : 0;
                    sb.AppendLine($"| {chartVm.Name} | {chartVm.CompilationScale} | {totalLayers} | {enabledLayers} | {FormatMaxVisible(minMin)} – {FormatMaxVisible(maxMax)} |");
                }
            }
            sb.AppendLine();

            // ── Loading Charts ──
            if (_loadingCharts.Count > 0)
            {
                sb.AppendLine("## Loading Charts (in progress)");
                foreach (var chartVm in _loadingCharts)
                    sb.AppendLine($"- {chartVm.Name}");
                sb.AppendLine();
            }

            // ── Map Layers (in render order) ──
            sb.AppendLine("## Map Layers (render order, bottom to top)");
            if (MyMapControl.Map is { } map)
            {
                sb.AppendLine("| # | Layer Name | Type | Enabled | MinVisible | MaxVisible |");
                sb.AppendLine("|---|---|---|---|---|---|");
                int idx = 0;
                foreach (var layer in map.Layers)
                {
                    sb.AppendLine($"| {idx} | {layer.Name} | {layer.GetType().Name} | {layer.Enabled} | {FormatMaxVisible(layer.MinVisible)} | {FormatMaxVisible(layer.MaxVisible)} |");
                    idx++;
                }
            }
            sb.AppendLine();

            // ── Screenshot ──
            string? screenshotPath = null;
            try
            {
                var pixelSize = new Avalonia.PixelSize((int)MyMapControl.Bounds.Width, (int)MyMapControl.Bounds.Height);
                if (pixelSize.Width > 0 && pixelSize.Height > 0)
                {
                    var rtb = new RenderTargetBitmap(pixelSize);
                    rtb.Render(MyMapControl);

                    var dir = Path.Combine(Path.GetTempPath(), "EncDotNet-Diagnostics");
                    Directory.CreateDirectory(dir);
                    screenshotPath = Path.Combine(dir, $"diag-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png");
                    rtb.Save(screenshotPath);

                    sb.AppendLine("## Screenshot");
                    sb.AppendLine($"Saved to: `{screenshotPath}`");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("## Screenshot");
                sb.AppendLine($"_(failed: {ex.Message})_");
            }

            // ── Copy to clipboard ──
            var text = sb.ToString();
            if (Clipboard is { } clipboard)
            {
                await clipboard.SetTextAsync(text);
            }

            ShowToast(screenshotPath is not null
                ? $"Diagnostic snapshot copied to clipboard. Screenshot saved to {screenshotPath}"
                : "Diagnostic snapshot copied to clipboard.");

            System.Diagnostics.Debug.WriteLine("Diagnostic snapshot captured to clipboard.");
            System.Diagnostics.Debug.WriteLine(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Diagnostic capture failed: {ex.Message}");
        }
    }

    private static string FormatMaxVisible(double value)
    {
        return value >= double.MaxValue / 2 ? "∞" : $"{value:F4}";
    }

    private CancellationTokenSource? _toastCts;

    private void ShowToast(string message, int durationMs = 3000)
    {
        _toastCts?.Cancel();
        var cts = _toastCts = new CancellationTokenSource();

        ToastText.Text = message;
        ToastBanner.IsVisible = true;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await Task.Delay(durationMs, cts.Token);
                ToastBanner.IsVisible = false;
            }
            catch (TaskCanceledException)
            {
                // Superseded by a newer toast
            }
        });
    }

    private class BoundaryThemeStyle : IThemeStyle
    {
        public double MinVisible { get; set; } = 0;
        public double MaxVisible { get; set; } = double.MaxValue;
        public bool Enabled { get; set; } = true;
        public float Opacity { get; set; } = 1;

        public IStyle? GetStyle(IFeature feature, Viewport viewport)
        {
            return feature["Highlighted"] is true
                ? HighlightBoundaryStyle
                : NormalBoundaryStyle;
        }
    }
}

