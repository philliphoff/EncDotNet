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
using Mapsui;
using Microsoft.Extensions.DependencyInjection;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;

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

        MyMapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Enable pinch-to-zoom on the map control via trackpad magnify gesture
        MyMapControl.AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, OnMapMagnify);

        // Enable double-tap to zoom in
        MyMapControl.DoubleTapped += OnMapDoubleTapped;

        // Enable trackpad scroll/swipe to pan the map (tunnel phase to intercept before MapControl)
        MyMapControl.AddHandler(PointerWheelChangedEvent, OnMapPointerWheelChanged, RoutingStrategies.Tunnel);

        // Enable hover highlighting of chart boundaries
        MyMapControl.PointerMoved += OnMapPointerMoved;
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
        navigator.ZoomTo(newResolution, center);
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
            featureVm.IsVisibleChanged += OnFeatureVisibilityChanged;
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
            featureVm.IsVisibleChanged -= OnFeatureVisibilityChanged;

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
            featureVm.IsVisibleChanged -= OnFeatureVisibilityChanged;

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

            foreach (var featureVm in vm.FeatureCategories)
            {
                var layer = S57LayerFactory.CreateLayerForObjectCodes(
                    chart,
                    featureVm.Category.ObjectCodes,
                    featureVm.Name);

                layer.Enabled = featureVm.IsVisible;
                chartVm.Layers.Add(layer);
                MyMapControl.Map?.Layers.Add(layer);
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

    private void OnFeatureVisibilityChanged(object? sender, bool isVisible)
    {
        if (sender is not ChartFeatureViewModel featureVm || ViewModel is not { } vm)
            return;

        foreach (var chartVm in vm.AvailableCharts)
        {
            if (!chartVm.IsSelected)
                continue;

            foreach (var layer in chartVm.Layers)
            {
                if (layer.Name == featureVm.Name)
                {
                    layer.Enabled = isVisible;
                }
            }
        }

        MyMapControl.Map?.Refresh();
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

