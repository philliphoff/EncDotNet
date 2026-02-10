using System;
using System.IO;
using Avalonia.Controls;
using EncDotNet.ChartViewer.ViewModels;
using EncDotNet.Enc.Charts;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;

namespace EncDotNet.ChartViewer.Views;

public partial class MainWindow : Window
{
    // Hardcoded path to a Puget Sound area chart
    private const string ChartPath = "../../../../../.expanded/US5WA18M/ENC_ROOT/US5WA18M/US5WA18M.000";

    private S57Chart? _chart;

    public MainWindow()
    {
        InitializeComponent();

        MyMapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
        
        // Load the S-57 chart data
        LoadS57Chart();

        DataContextChanged += (_, _) => WireUpFeatureLayers();
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void LoadS57Chart()
    {
        try
        {
            // Get the full path relative to the executable
            var basePath = AppContext.BaseDirectory;
            var fullPath = Path.GetFullPath(Path.Combine(basePath, ChartPath));

            if (!File.Exists(fullPath))
            {
                System.Diagnostics.Debug.WriteLine($"Chart file not found: {fullPath}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Loading chart from: {fullPath}");

            // Load the S-57 chart
            _chart = S57Chart.FromFile(fullPath);

            System.Diagnostics.Debug.WriteLine($"Loaded chart with {_chart.PointFeatures.Length} points, {_chart.LineFeatures.Length} lines, {_chart.AreaFeatures.Length} areas");

            // Defer navigation until the window is fully loaded and the map control has a valid size
            Loaded += (_, _) =>
            {
                // Navigate to the chart area (Puget Sound approximate center)
                // Seattle area: approximately -122.35, 47.6
                var (x, y) = SphericalMercator.FromLonLat(-122.35, 47.6);
                MyMapControl.Map?.Navigator.CenterOnAndZoomTo(new MPoint(x, y), 150);
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading chart: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void WireUpFeatureLayers()
    {
        if (_chart is not { } chart || ViewModel is not { } vm)
        {
            return;
        }

        foreach (var featureVm in vm.FeatureCategories)
        {
            var layer = S57LayerFactory.CreateLayerForObjectCodes(
                chart,
                featureVm.Category.ObjectCodes,
                featureVm.Name);

            layer.Enabled = featureVm.IsVisible;
            featureVm.Layer = layer;

            MyMapControl.Map?.Layers.Add(layer);

            // Subscribe to visibility changes
            featureVm.IsVisibleChanged += (_, isVisible) =>
            {
                layer.Enabled = isVisible;
                MyMapControl.Map?.Refresh();
            };
        }
    }
}

