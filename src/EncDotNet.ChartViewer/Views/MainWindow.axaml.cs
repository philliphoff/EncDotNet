using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EncDotNet.ChartViewer.Catalogs;
using EncDotNet.ChartViewer.Models;
using EncDotNet.ChartViewer.ViewModels;
using EncDotNet.Enc;
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

        ZoomInButton.Click += OnZoomInClick;
        ZoomOutButton.Click += OnZoomOutClick;
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
            AppDataPaths.SaveSelectedCharts(
                vm.SelectedCharts.Select(c => c.Entry.Id));

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

        // Subscribe to chart selection changes
        foreach (var chartVm in vm.AvailableCharts)
        {
            chartVm.IsSelectedChanged += OnChartSelectionChanged;
        }

        // Subscribe to feature visibility changes
        foreach (var featureVm in vm.FeatureCategories)
        {
            featureVm.FeatureVisibilityChanged += OnFeatureItemVisibilityChanged;
        }

        // Subscribe to depth unit changes
        vm.DepthUnitChanged += OnDepthUnitChanged;

        // Restore saved depth unit
        vm.DepthUnit = AppDataPaths.LoadDepthUnit();

        // Restore saved feature visibility (before selecting charts so layers get correct initial state)
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

        // Restore saved chart selections
        var savedCharts = new HashSet<string>(AppDataPaths.LoadSelectedCharts());
        foreach (var chartVm in vm.AvailableCharts)
        {
            if (!string.IsNullOrEmpty(chartVm.Entry.Id) && savedCharts.Contains(chartVm.Entry.Id))
            {
                chartVm.IsSelected = true;
            }
        }
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

        // Unload all selected charts from the map
        foreach (var chartVm in vm.SelectedCharts.ToArray())
        {
            UnloadChart(chartVm);
        }

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

        // Unsubscribe from old chart/feature events
        foreach (var chartVm in vm.AvailableCharts)
            chartVm.IsSelectedChanged -= OnChartSelectionChanged;
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

        // Unload all selected charts from the map
        foreach (var chartVm in vm.SelectedCharts.ToArray())
        {
            UnloadChart(chartVm);
        }

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

        // Unsubscribe from old chart/feature events
        foreach (var chartVm in vm.AvailableCharts)
            chartVm.IsSelectedChanged -= OnChartSelectionChanged;
        foreach (var featureVm in vm.FeatureCategories)
            featureVm.FeatureVisibilityChanged -= OnFeatureItemVisibilityChanged;
        vm.DepthUnitChanged -= OnDepthUnitChanged;

        // Clear and reload
        vm.ClearCatalog();
        await LoadCatalogIntoMapAsync();
    }

    private void OnChartSelectionChanged(object? sender, bool isSelected)
    {
        if (sender is not ChartViewModel chartVm || ViewModel is not { } vm)
            return;

        if (isSelected)
        {
            vm.SelectedCharts.Add(chartVm);
            _ = LoadChartAsync(chartVm, vm);
        }
        else
        {
            vm.SelectedCharts.Remove(chartVm);
            UnloadChart(chartVm);
        }
    }

    private async Task LoadChartAsync(ChartViewModel chartVm, MainWindowViewModel vm)
    {
        try
        {
            var chart = await vm.GetChartAsync(chartVm.Entry);
            chartVm.CompilationScale = chart.CompilationScale;

            foreach (var featureVm in vm.FeatureCategories)
            {
                foreach (var featureItem in featureVm.Features)
                {
                    var layer = S57LayerFactory.CreateLayerForObjectCodes(
                        chart,
                        ImmutableArray.Create(featureItem.ObjectCode),
                        featureItem.ObjectCode.ToString(),
                        vm.DepthUnit);

                    layer.Enabled = featureItem.IsVisible;
                    chartVm.Layers.Add(layer);
                }
            }

            // Insert layers ordered by compilation scale (higher CSCL before lower CSCL)
            if (MyMapControl.Map is { } map)
            {
                int insertIndex = FindChartLayerInsertionIndex(map, chartVm.CompilationScale, vm);
                map.Layers.Insert(insertIndex, chartVm.Layers);
            }

            System.Diagnostics.Debug.WriteLine($"Loaded chart: {chartVm.Name}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading chart {chartVm.Name}: {ex.Message}");
        }
    }

    private void UnloadChart(ChartViewModel chartVm)
    {
        foreach (var layer in chartVm.Layers)
        {
            MyMapControl.Map?.Layers.Remove(layer);
        }

        chartVm.Layers.Clear();
        MyMapControl.Map?.Refresh();
    }

    private void OnFeatureItemVisibilityChanged(object? sender, ChartFeatureItemViewModel featureItem)
    {
        if (ViewModel is not { } vm)
            return;

        var objectCodeName = featureItem.ObjectCode.ToString();
        foreach (var chartVm in vm.AvailableCharts)
        {
            if (!chartVm.IsSelected)
                continue;

            foreach (var layer in chartVm.Layers)
            {
                if (layer.Name == objectCodeName)
                {
                    layer.Enabled = featureItem.IsVisible;
                }
            }
        }

        MyMapControl.Map?.Refresh();
    }

    private void OnDepthUnitChanged(object? sender, Models.DepthUnit depthUnit)
    {
        if (ViewModel is not { } vm)
            return;

        // Reload sounding layers for all selected charts
        foreach (var chartVm in vm.AvailableCharts)
        {
            if (!chartVm.IsSelected)
                continue;

            for (int i = chartVm.Layers.Count - 1; i >= 0; i--)
            {
                var layer = chartVm.Layers[i];
                if (layer.Name != S57ObjectCode.SOUNDG.ToString())
                    continue;

                // Find position before removing so we can re-insert at the same spot
                int mapIndex = FindMapLayerIndex(layer);

                // Remove old sounding layer
                MyMapControl.Map?.Layers.Remove(layer);
                chartVm.Layers.RemoveAt(i);

                // Create new sounding layer with updated depth unit
                var chart = vm.GetChartAsync(chartVm.Entry).GetAwaiter().GetResult();
                var newLayer = S57LayerFactory.CreateLayerForObjectCodes(
                    chart,
                    ImmutableArray.Create(S57ObjectCode.SOUNDG),
                    S57ObjectCode.SOUNDG.ToString(),
                    depthUnit);

                var soundingFeatureItem = vm.FeatureCategories
                    .SelectMany(c => c.Features)
                    .FirstOrDefault(f => f.ObjectCode == S57ObjectCode.SOUNDG);
                newLayer.Enabled = soundingFeatureItem?.IsVisible ?? false;

                chartVm.Layers.Insert(i, newLayer);

                // Re-insert at the same position to preserve CSCL ordering
                if (mapIndex >= 0)
                    MyMapControl.Map?.Layers.Insert(mapIndex, newLayer);
                else
                    MyMapControl.Map?.Layers.Add(newLayer);
            }
        }

        MyMapControl.Map?.Refresh();
    }

    /// <summary>
    /// Finds the insertion index in the map's layer collection for a chart with the given
    /// compilation scale. Higher CSCL charts are placed before lower CSCL charts.
    /// </summary>
    private static int FindChartLayerInsertionIndex(Map map, int compilationScale, MainWindowViewModel vm)
    {
        int index = 0;
        foreach (var existingLayer in map.Layers)
        {
            var ownerChart = vm.AvailableCharts.FirstOrDefault(
                c => c.IsSelected && c.Layers.Contains(existingLayer));

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

    private MemoryLayer CreateChartBoundariesLayer(
        IEnumerable<ChartViewModel> charts)
    {
        var features = new List<IFeature>();

        foreach (var chartVm in charts)
        {
            var entry = chartVm.Entry;

            if (entry.SouthLatitude is not { } south
                || entry.NorthLatitude is not { } north
                || entry.WestLongitude is not { } west
                || entry.EastLongitude is not { } east)
            {
                continue;
            }

            var (minX, minY) = SphericalMercator.FromLonLat(west, south);
            var (maxX, maxY) = SphericalMercator.FromLonLat(east, north);

            var ring = new LinearRing(
            [
                new Coordinate(minX, minY),
                new Coordinate(maxX, minY),
                new Coordinate(maxX, maxY),
                new Coordinate(minX, maxY),
                new Coordinate(minX, minY),
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

