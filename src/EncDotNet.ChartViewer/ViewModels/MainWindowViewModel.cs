using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using EncDotNet.ChartViewer.Catalogs;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57.Charts;
using ReactiveUI;
using System.Linq;

namespace EncDotNet.ChartViewer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _isChartsPanelExpanded = true;
    private bool _isFeaturesPanelExpanded = true;
    private string _chartSearchText = "";
    private CancellationTokenSource? _filterDebounce;
    private DepthUnit _depthUnit = DepthUnit.Feet;

    /// <summary>
    /// Gets the collection of available charts loaded from the chart index.
    /// </summary>
    public ObservableCollection<ChartViewModel> AvailableCharts { get; } = new();

    /// <summary>
    /// Gets the filtered collection of available charts based on search text, grouped by state.
    /// Contains interleaved <see cref="ChartGroupHeaderViewModel"/> and <see cref="ChartViewModel"/> items.
    /// </summary>
    public ObservableCollection<object> FilteredAvailableCharts { get; } = new();

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

    /// <summary>
    /// Gets the available depth unit options for the ComboBox.
    /// </summary>
    public DepthUnit[] DepthUnitOptions { get; } = Enum.GetValues<DepthUnit>();

    /// <summary>
    /// Gets or sets the unit used for displaying sounding depths.
    /// </summary>
    public DepthUnit DepthUnit
    {
        get => _depthUnit;
        set
        {
            if (_depthUnit == value)
                return;

            this.RaiseAndSetIfChanged(ref _depthUnit, value);
            DepthUnitChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// Raised when the depth unit changes.
    /// </summary>
    public event EventHandler<DepthUnit>? DepthUnitChanged;

    /// <summary>Command to toggle the charts panel.</summary>
    public ICommand ToggleChartsPanelCommand { get; }

    /// <summary>Command to toggle the features panel.</summary>
    public ICommand ToggleFeaturesPanelCommand { get; }

    /// <summary>Command to open the manage charts dialog.</summary>
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ManageChartsCommand { get; }

    /// <summary>Command to reset all data and return to the setup wizard.</summary>
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> ResetDataCommand { get; }

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

    private ChartViewModel? _hoveredChart;

    /// <summary>
    /// Gets the chart currently being hovered over on the map (for the popup panel).
    /// </summary>
    public ChartViewModel? HoveredChart
    {
        get => _hoveredChart;
        set
        {
            var old = _hoveredChart;
            this.RaiseAndSetIfChanged(ref _hoveredChart, value);
            if (!ReferenceEquals(old, value))
                this.RaisePropertyChanged(nameof(HasHoveredChart));
        }
    }

    /// <summary>
    /// Gets whether a chart is currently being hovered over.
    /// </summary>
    public bool HasHoveredChart => _hoveredChart is not null;

    private readonly ICatalogSource _catalogSource;

    public MainWindowViewModel(ICatalogSource catalogSource)
    {
        _catalogSource = catalogSource;
        foreach (var category in S57FeatureCategory.All)
        {
            FeatureCategories.Add(new ChartFeatureViewModel(category));
        }

        ToggleChartsPanelCommand = ReactiveCommand.Create(() => IsChartsPanelExpanded = !IsChartsPanelExpanded);
        ToggleFeaturesPanelCommand = ReactiveCommand.Create(() => IsFeaturesPanelExpanded = !IsFeaturesPanelExpanded);
        ManageChartsCommand = ReactiveCommand.Create(() => { });
        ResetDataCommand = ReactiveCommand.Create(() => { });
    }

    /// <summary>
    /// Clears all loaded charts and chart state so the catalog can be reloaded.
    /// </summary>
    internal void ClearCatalog()
    {
        FilteredAvailableCharts.Clear();
        AvailableCharts.Clear();
        _chartSearchText = "";
        this.RaisePropertyChanged(nameof(ChartSearchText));
    }

    /// <summary>
    /// Loads the catalog entries from the given source and populates <see cref="AvailableCharts"/>.
    /// </summary>
    internal async Task LoadCatalogAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        await foreach (var entry in _catalogSource.GetCatalogAsync(cancellationToken))
        {
            AvailableCharts.Add(new ChartViewModel(entry));
        }

        stopwatch.Stop();
        ChartViewerDiagnostics.CatalogLoadDuration.Record(stopwatch.Elapsed.TotalMilliseconds);

        System.Diagnostics.Debug.WriteLine($"Loaded {AvailableCharts.Count} charts from catalog");

        ApplyChartFilter();
    }

    /// <summary>
    /// Loads a chart from the catalog source for the given entry.
    /// </summary>
    internal Task<S57Chart> GetChartAsync(ChartIndexEntry entry, CancellationToken cancellationToken = default)
    {
        return _catalogSource.GetChartAsync(entry, cancellationToken);
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

        var filtered = AvailableCharts
            .Where(chart =>
                search.Length == 0
                || chart.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || chart.Entry.Id.Contains(search, StringComparison.OrdinalIgnoreCase)
                || chart.Entry.Path.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (chart.Entry.State is { } state && state.Contains(search, StringComparison.OrdinalIgnoreCase)));

        var groups = filtered
            .GroupBy(chart => chart.Entry.State ?? "Other")
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups)
        {
            FilteredAvailableCharts.Add(new ChartGroupHeaderViewModel(group.Key, group.Count()));

            foreach (var chart in group)
            {
                FilteredAvailableCharts.Add(chart);
            }
        }
    }
}