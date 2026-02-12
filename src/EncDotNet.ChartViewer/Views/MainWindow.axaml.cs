using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using EncDotNet.ChartViewer.Catalogs;
using EncDotNet.ChartViewer.Models;
using EncDotNet.ChartViewer.ViewModels;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer.Views;

public partial class MainWindow : Window
{
    private const string ChartIndexRelativePath = "../../../../../.expanded/chart-index.json";

    public MainWindow()
    {
        InitializeComponent();

        MyMapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

        DataContextChanged += (_, _) => OnDataContextChanged();
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private async void OnDataContextChanged()
    {
        if (ViewModel is not { } vm)
            return;

        // Load catalog via ICatalogSource
        var basePath = AppContext.BaseDirectory;
        var chartIndexPath = Path.GetFullPath(Path.Combine(basePath, ChartIndexRelativePath));
        var catalogSource = new FileSystemCatalogSource(chartIndexPath);
        await vm.LoadCatalogAsync(catalogSource);

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

    private static MemoryLayer CreateChartBoundariesLayer(
        IEnumerable<ChartViewModel> charts)
    {
        var features = new List<IFeature>();
        var style = new VectorStyle
        {
            Fill = null,
            Line = new Pen(Color.Red, 1),
        };

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
            feature.Styles.Add(style);
            features.Add(feature);
        }

        return new MemoryLayer
        {
            Name = "Chart Boundaries",
            Features = features,
            Style = null,
        };
    }
}

