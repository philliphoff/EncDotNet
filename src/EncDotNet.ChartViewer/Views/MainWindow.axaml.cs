using System;
using System.IO;
using Avalonia.Controls;
using EncDotNet.ChartViewer.ViewModels;
using EncDotNet.Enc.Charts;
using Mapsui.Tiling;

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

    private void OnDataContextChanged()
    {
        if (ViewModel is not { } vm)
            return;

        // Load chart index
        var basePath = AppContext.BaseDirectory;
        var chartIndexPath = Path.GetFullPath(Path.Combine(basePath, ChartIndexRelativePath));
        vm.LoadChartIndex(chartIndexPath);

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
            LoadChart(chartVm, vm);
        }
        else
        {
            vm.SelectedCharts.Remove(chartVm);
            UnloadChart(chartVm);
        }
    }

    private void LoadChart(ChartViewModel chartVm, MainWindowViewModel vm)
    {
        try
        {
            var chartPath = Path.Combine(vm.ExpandedDirectory, chartVm.Entry.Path);

            if (!File.Exists(chartPath))
            {
                System.Diagnostics.Debug.WriteLine($"Chart file not found: {chartPath}");
                return;
            }

            var chart = S57Chart.FromFile(chartPath);

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
}

