using System;
using System.IO;
using Avalonia.Controls;
using EncDotNet.Enc.Charts;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;

namespace EncDotNet.ChartViewer.Views;

public partial class MainWindow : Window
{
    // Hardcoded path to a Puget Sound area chart
    private const string ChartPath = "../../../../../.expanded/US5WA18M/ENC_ROOT/US5WA18M/US5WA18M.000";

    public MainWindow()
    {
        InitializeComponent();

        MyMapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
        
        // Load and display S-57 chart
        LoadS57Chart();
    }

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
            var chart = S57Chart.FromFile(fullPath);

            System.Diagnostics.Debug.WriteLine($"Loaded chart with {chart.PointFeatures.Length} points, {chart.LineFeatures.Length} lines, {chart.AreaFeatures.Length} areas");

            // Create and add the S-57 layer
            var s57Layer = S57LayerFactory.CreateLayer(chart, "Puget Sound Chart");
            MyMapControl.Map?.Layers.Add(s57Layer);

            // Navigate to the chart area (Puget Sound approximate center)
            // Seattle area: approximately -122.35, 47.6
            var (x, y) = SphericalMercator.FromLonLat(-122.35, 47.6);
            MyMapControl.Map?.Navigator.CenterOnAndZoomTo(new MPoint(x, y), 50000);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading chart: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

