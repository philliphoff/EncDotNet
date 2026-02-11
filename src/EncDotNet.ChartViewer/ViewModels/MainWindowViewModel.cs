using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using EncDotNet.ChartViewer.Models;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _hasSelectedCharts;

    /// <summary>
    /// Gets the collection of available charts loaded from the chart index.
    /// </summary>
    public ObservableCollection<ChartViewModel> AvailableCharts { get; } = new();

    /// <summary>
    /// Gets the collection of currently selected (loaded) charts.
    /// </summary>
    public ObservableCollection<ChartViewModel> SelectedCharts { get; } = new();

    /// <summary>
    /// Gets whether any charts are currently selected.
    /// </summary>
    public bool HasSelectedCharts
    {
        get => _hasSelectedCharts;
        private set => this.RaiseAndSetIfChanged(ref _hasSelectedCharts, value);
    }

    /// <summary>
    /// Gets the collection of toggleable chart feature categories.
    /// </summary>
    public ObservableCollection<ChartFeatureViewModel> FeatureCategories { get; } = new();

    /// <summary>
    /// Gets or sets the base directory containing the expanded chart files.
    /// </summary>
    public string ExpandedDirectory { get; set; } = "";

    public MainWindowViewModel()
    {
        foreach (var category in S57FeatureCategory.All)
        {
            FeatureCategories.Add(new ChartFeatureViewModel(category));
        }

        SelectedCharts.CollectionChanged += (_, _) => HasSelectedCharts = SelectedCharts.Count > 0;
    }

    /// <summary>
    /// Loads the chart index from a JSON file and populates <see cref="AvailableCharts"/>.
    /// </summary>
    public void LoadChartIndex(string chartIndexPath)
    {
        if (!File.Exists(chartIndexPath))
        {
            System.Diagnostics.Debug.WriteLine($"Chart index not found: {chartIndexPath}");
            return;
        }

        ExpandedDirectory = Path.GetDirectoryName(chartIndexPath) ?? "";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };

        var json = File.ReadAllText(chartIndexPath);
        var entries = JsonSerializer.Deserialize<List<ChartIndexEntry>>(json, options);

        if (entries is null)
            return;

        foreach (var entry in entries)
        {
            AvailableCharts.Add(new ChartViewModel(entry));
        }

        System.Diagnostics.Debug.WriteLine($"Loaded {AvailableCharts.Count} charts from index");
    }
}