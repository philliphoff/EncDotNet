using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Windows.Input;
using EncDotNet.ChartViewer.Models;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _hasSelectedCharts;
    private bool _isChartsPanelExpanded = true;
    private bool _isFeaturesPanelExpanded = true;
    private string _chartSearchText = "";
    private CancellationTokenSource? _filterDebounce;

    /// <summary>
    /// Gets the collection of available charts loaded from the chart index.
    /// </summary>
    public ObservableCollection<ChartViewModel> AvailableCharts { get; } = new();

    /// <summary>
    /// Gets the filtered collection of available charts based on search text.
    /// </summary>
    public ObservableCollection<ChartViewModel> FilteredAvailableCharts { get; } = new();

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
    /// Gets or sets whether the charts panel is expanded.
    /// </summary>
    public bool IsChartsPanelExpanded
    {
        get => _isChartsPanelExpanded;
        set => this.RaiseAndSetIfChanged(ref _isChartsPanelExpanded, value);
    }

    /// <summary>
    /// Gets or sets whether the features panel is expanded.
    /// </summary>
    public bool IsFeaturesPanelExpanded
    {
        get => _isFeaturesPanelExpanded;
        set => this.RaiseAndSetIfChanged(ref _isFeaturesPanelExpanded, value);
    }

    /// <summary>
    /// Gets the collection of toggleable chart feature categories.
    /// </summary>
    public ObservableCollection<ChartFeatureViewModel> FeatureCategories { get; } = new();

    /// <summary>Command to toggle the charts panel.</summary>
    public ICommand ToggleChartsPanelCommand { get; }

    /// <summary>Command to toggle the features panel.</summary>
    public ICommand ToggleFeaturesPanelCommand { get; }

    /// <summary>
    /// Gets or sets the search text used to filter available charts.
    /// </summary>
    public string ChartSearchText
    {
        get => _chartSearchText;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _chartSearchText, value) is not null)
                ScheduleChartFilter();
        }
    }

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

        ToggleChartsPanelCommand = ReactiveCommand.Create(() => IsChartsPanelExpanded = !IsChartsPanelExpanded);
        ToggleFeaturesPanelCommand = ReactiveCommand.Create(() => IsFeaturesPanelExpanded = !IsFeaturesPanelExpanded);
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

        ApplyChartFilter();
    }

    private async void ScheduleChartFilter()
    {
        _filterDebounce?.Cancel();
        var cts = _filterDebounce = new CancellationTokenSource();

        try
        {
            await System.Threading.Tasks.Task.Delay(250, cts.Token);
            ApplyChartFilter();
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            // Superseded by a newer keystroke
        }
    }

    private void ApplyChartFilter()
    {
        FilteredAvailableCharts.Clear();

        var search = _chartSearchText;

        foreach (var chart in AvailableCharts)
        {
            if (search.Length == 0
                || chart.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || chart.Entry.Path.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                FilteredAvailableCharts.Add(chart);
            }
        }
    }
}