using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using EncDotNet.ChartViewer.Catalogs;
using EncDotNet.ChartViewer.Models;
using EncDotNet.Enc.Catalogs;
using EncDotNet.ProductCatalog;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

public enum ManageChartsStep
{
    FetchingCatalog,
    SelectRegions,
    Applying,
    Complete,
    Error
}

public class ManageChartsViewModel : ViewModelBase
{
    private ManageChartsStep _currentStep = ManageChartsStep.FetchingCatalog;
    private ManageChartsStep _errorSource;
    private string _statusText = "";
    private double _progress;
    private string _errorMessage = "";
    private bool _selectAll;
    private int _selectedChartCount;
    private string _selectionSummary = "No charts selected";
    private string _changeSummary = "";
    private int _preparedChartCount;
    private bool _updatingSelectAll;
    private bool _hasChanges;
    private EncProductCatalog? _catalog;
    private HashSet<string> _previouslySelectedStates = [];
    private readonly CancellationTokenSource _cts = new();

    public ManageChartsStep CurrentStep
    {
        get => _currentStep;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentStep, value);
            this.RaisePropertyChanged(nameof(IsFetchingStep));
            this.RaisePropertyChanged(nameof(IsSelectStep));
            this.RaisePropertyChanged(nameof(IsApplyingStep));
            this.RaisePropertyChanged(nameof(IsCompleteStep));
            this.RaisePropertyChanged(nameof(IsErrorStep));
            this.RaisePropertyChanged(nameof(ShowBackButton));
            this.RaisePropertyChanged(nameof(ShowApplyButton));
            this.RaisePropertyChanged(nameof(ShowCancelButton));
            this.RaisePropertyChanged(nameof(ShowFinishButton));
            this.RaisePropertyChanged(nameof(IsProgressIndeterminate));
        }
    }

    public bool IsFetchingStep => _currentStep == ManageChartsStep.FetchingCatalog;
    public bool IsSelectStep => _currentStep == ManageChartsStep.SelectRegions;
    public bool IsApplyingStep => _currentStep == ManageChartsStep.Applying;
    public bool IsCompleteStep => _currentStep == ManageChartsStep.Complete;
    public bool IsErrorStep => _currentStep == ManageChartsStep.Error;

    public bool ShowBackButton => _currentStep == ManageChartsStep.Error;
    public bool ShowApplyButton => _currentStep == ManageChartsStep.SelectRegions;
    public bool ShowCancelButton => _currentStep is not ManageChartsStep.Complete;
    public bool ShowFinishButton => _currentStep == ManageChartsStep.Complete;
    public bool IsProgressIndeterminate => _currentStep == ManageChartsStep.FetchingCatalog;

    public bool HasChanges
    {
        get => _hasChanges;
        private set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public double Progress
    {
        get => _progress;
        private set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool SelectAll
    {
        get => _selectAll;
        set
        {
            if (_updatingSelectAll || _selectAll == value)
                return;

            _updatingSelectAll = true;
            this.RaiseAndSetIfChanged(ref _selectAll, value);

            foreach (var state in States)
                state.IsSelected = value;

            _updatingSelectAll = false;
            UpdateSelectionSummary();
        }
    }

    public int SelectedChartCount
    {
        get => _selectedChartCount;
        private set => this.RaiseAndSetIfChanged(ref _selectedChartCount, value);
    }

    public string SelectionSummary
    {
        get => _selectionSummary;
        private set => this.RaiseAndSetIfChanged(ref _selectionSummary, value);
    }

    public string ChangeSummary
    {
        get => _changeSummary;
        private set => this.RaiseAndSetIfChanged(ref _changeSummary, value);
    }

    public int PreparedChartCount
    {
        get => _preparedChartCount;
        private set => this.RaiseAndSetIfChanged(ref _preparedChartCount, value);
    }

    /// <summary>
    /// Whether chart data was actually modified (additions or removals were applied).
    /// </summary>
    public bool ChartsChanged { get; private set; }

    public ObservableCollection<SelectableStateViewModel> States { get; } = new();

    public ICommand ApplyCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand FinishCommand { get; }

    public event Action? RequestClose;

    public ManageChartsViewModel()
    {
        ApplyCommand = ReactiveCommand.Create(OnApply);
        BackCommand = ReactiveCommand.Create(OnBack);
        CancelCommand = ReactiveCommand.Create(OnCancel);
        FinishCommand = ReactiveCommand.Create(OnFinish);
    }

    public void Cancel()
    {
        _cts.Cancel();
    }

    public async void BeginFetchCatalog()
    {
        await FetchCatalogAsync();
    }

    private async void OnApply()
    {
        CurrentStep = ManageChartsStep.Applying;
        await ApplyChangesAsync();
    }

    private void OnBack()
    {
        CurrentStep = _errorSource == ManageChartsStep.FetchingCatalog
            ? ManageChartsStep.FetchingCatalog
            : ManageChartsStep.SelectRegions;
    }

    private void OnCancel()
    {
        _cts.Cancel();
        RequestClose?.Invoke();
    }

    private void OnFinish()
    {
        RequestClose?.Invoke();
    }

    private async Task FetchCatalogAsync()
    {
        StatusText = "Fetching NOAA ENC catalog...";

        try
        {
            _previouslySelectedStates = AppDataPaths.LoadDownloadedStates();

            using var client = new EncProductCatalogClient();
            _catalog = await client.GetNoaaCatalogAsync(_cts.Token);

            var stateGroups = new Dictionary<string, (int Count, long Size)>();

            foreach (var cell in _catalog.Cells)
            {
                var cellStates = cell.States?.StateList ?? [];
                var stateKeys = cellStates.Count > 0 ? cellStates : ["Other"];

                foreach (var state in stateKeys)
                {
                    if (!stateGroups.TryGetValue(state, out var group))
                        group = (0, 0);

                    stateGroups[state] = (group.Count + 1, group.Size + cell.ZipfileSize);
                }
            }

            States.Clear();

            foreach (var (state, (count, size)) in stateGroups.OrderBy(kv => kv.Key))
            {
                var vm = new SelectableStateViewModel(state, count, size);
                vm.IsSelected = _previouslySelectedStates.Contains(state);
                vm.SelectionChanged += (_, _) => OnStateSelectionChanged();
                States.Add(vm);
            }

            _updatingSelectAll = true;
            _selectAll = States.All(s => s.IsSelected);
            this.RaisePropertyChanged(nameof(SelectAll));
            _updatingSelectAll = false;

            CurrentStep = ManageChartsStep.SelectRegions;
            UpdateSelectionSummary();
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to fetch the NOAA catalog: {ex.Message}";
            _errorSource = ManageChartsStep.FetchingCatalog;
            CurrentStep = ManageChartsStep.Error;
        }
    }

    private void OnStateSelectionChanged()
    {
        if (_updatingSelectAll)
            return;

        _updatingSelectAll = true;
        _selectAll = States.All(s => s.IsSelected);
        this.RaisePropertyChanged(nameof(SelectAll));
        _updatingSelectAll = false;

        UpdateSelectionSummary();
    }

    private void UpdateSelectionSummary()
    {
        var selectedStates = new HashSet<string>(
            States.Where(s => s.IsSelected).Select(s => s.Name));

        var selectedCells = GetCellsForStates(selectedStates);
        SelectedChartCount = selectedCells.Count;

        if (selectedStates.Count == 0)
        {
            SelectionSummary = "No charts selected";
        }
        else
        {
            var totalBytes = selectedCells.Sum(c => c.ZipfileSize);
            SelectionSummary = $"{selectedCells.Count} charts selected ({FormatSize(totalBytes)})";
        }

        // Compute change summary
        var added = selectedStates.Except(_previouslySelectedStates).ToList();
        var removed = _previouslySelectedStates.Except(selectedStates).ToList();

        HasChanges = added.Count > 0 || removed.Count > 0;

        var parts = new List<string>();
        if (added.Count > 0)
            parts.Add($"{added.Count} state(s) to add");
        if (removed.Count > 0)
            parts.Add($"{removed.Count} state(s) to remove");

        ChangeSummary = parts.Count > 0
            ? string.Join(", ", parts)
            : "No changes";
    }

    private List<Cell> GetCellsForStates(HashSet<string> states)
    {
        if (_catalog is null)
            return [];

        var cells = new List<Cell>();
        var seen = new HashSet<string>();

        foreach (var cell in _catalog.Cells)
        {
            if (seen.Contains(cell.Name))
                continue;

            var cellStates = cell.States?.StateList ?? [];
            bool match = cellStates.Count == 0
                ? states.Contains("Other")
                : cellStates.Any(states.Contains);

            if (match)
            {
                cells.Add(cell);
                seen.Add(cell.Name);
            }
        }

        return cells;
    }

    private async Task ApplyChangesAsync()
    {
        try
        {
            AppDataPaths.EnsureDirectories();

            var newSelectedStates = new HashSet<string>(
                States.Where(s => s.IsSelected).Select(s => s.Name));

            var removedStates = _previouslySelectedStates.Except(newSelectedStates).ToHashSet();
            var addedStates = newSelectedStates.Except(_previouslySelectedStates).ToHashSet();

            // Phase 1: Remove cells belonging to deselected states
            if (removedStates.Count > 0)
            {
                var cellsToRemove = GetCellsForStates(removedStates);

                // Only remove cells that aren't also needed by still-selected states
                var cellsToKeep = new HashSet<string>(
                    GetCellsForStates(newSelectedStates).Select(c => c.Name));

                int removeCount = 0;
                foreach (var cell in cellsToRemove)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    if (cellsToKeep.Contains(cell.Name))
                        continue;

                    removeCount++;
                    StatusText = $"Removing {cell.Name}...";

                    // Remove expanded directory
                    var zipUrl = cell.ZipfileLocation;
                    var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
                    var folderName = Path.GetFileNameWithoutExtension(fileName);
                    var expandedDir = Path.Combine(AppDataPaths.ExpandedDirectory, folderName);

                    if (Directory.Exists(expandedDir))
                        await Task.Run(() => Directory.Delete(expandedDir, recursive: true), _cts.Token);

                    // Remove zip file
                    var zipPath = Path.Combine(AppDataPaths.CatalogDirectory, fileName);

                    if (File.Exists(zipPath))
                        File.Delete(zipPath);
                }
            }

            // Phase 2: Download & extract cells for newly-added states
            if (addedStates.Count > 0)
            {
                var cellsToAdd = GetCellsForStates(addedStates);

                // Exclude cells that are already downloaded (from other states)
                var alreadyDownloadedCells = new HashSet<string>(
                    GetCellsForStates(_previouslySelectedStates.Intersect(newSelectedStates).ToHashSet())
                        .Select(c => c.Name));

                var newCells = cellsToAdd.Where(c => !alreadyDownloadedCells.Contains(c.Name)).ToList();

                using var httpClient = new HttpClient();
                int total = newCells.Count;
                int completed = 0;

                foreach (var cell in newCells)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    var zipUrl = cell.ZipfileLocation;
                    var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
                    var outputPath = Path.Combine(AppDataPaths.CatalogDirectory, fileName);

                    if (!File.Exists(outputPath))
                    {
                        StatusText = $"Downloading {cell.Name} ({completed + 1} of {total})...";
                        Progress = (double)completed / total * 60;

                        using var response = await httpClient.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                        response.EnsureSuccessStatusCode();

                        using var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
                        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
                        await stream.CopyToAsync(fileStream, _cts.Token);
                    }

                    // Extract
                    var folderName = Path.GetFileNameWithoutExtension(fileName);
                    var expandedDir = Path.Combine(AppDataPaths.ExpandedDirectory, folderName);

                    if (!Directory.Exists(expandedDir) && File.Exists(outputPath))
                    {
                        StatusText = $"Extracting {cell.Name} ({completed + 1} of {total})...";
                        await Task.Run(() => ZipFile.ExtractToDirectory(outputPath, expandedDir), _cts.Token);
                    }

                    completed++;
                    Progress = (double)completed / total * 90;
                }
            }

            // Phase 3: Rebuild chart index
            StatusText = "Building chart index...";
            Progress = 90;
            var chartCount = await Task.Run(BuildChartIndex, _cts.Token);
            Progress = 100;

            // Save the new state selection
            AppDataPaths.SaveDownloadedStates(newSelectedStates);

            ChartsChanged = true;
            PreparedChartCount = chartCount;
            CurrentStep = ManageChartsStep.Complete;
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while applying changes: {ex.Message}";
            _errorSource = ManageChartsStep.Applying;
            CurrentStep = ManageChartsStep.Error;
        }
    }

    private static int BuildChartIndex()
    {
        var expandedDir = AppDataPaths.ExpandedDirectory;
        var entries = new List<ChartIndexEntry>();

        foreach (var subDir in Directory.EnumerateDirectories(expandedDir)
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            var catalogPath = Path.Combine(subDir, "ENC_ROOT", "CATALOG.031");

            if (!File.Exists(catalogPath))
                continue;

            try
            {
                var catalog = S57CatalogReader.ReadFromFile(catalogPath);
                var folderName = Path.GetFileName(subDir);

                foreach (var catEntry in catalog.Entries)
                {
                    if (!catEntry.FileName.EndsWith(".000", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var relativePath = Path.Combine(folderName, "ENC_ROOT", catEntry.FileName)
                        .Replace('\\', '/');

                    var chartName = !string.IsNullOrEmpty(catEntry.LongFileName)
                        ? catEntry.LongFileName
                        : Path.GetFileNameWithoutExtension(catEntry.FileName);

                    entries.Add(new ChartIndexEntry
                    {
                        Name = chartName,
                        Path = relativePath,
                        SouthLatitude = catEntry.SouthernmostLatitude,
                        WestLongitude = catEntry.WesternmostLongitude,
                        NorthLatitude = catEntry.NorthernmostLatitude,
                        EastLongitude = catEntry.EasternmostLongitude,
                    });
                }
            }
            catch
            {
                // Skip catalogs that fail to parse
            }
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };

        var json = JsonSerializer.Serialize(entries, options);
        File.WriteAllText(AppDataPaths.ChartIndexPath, json);

        return entries.Count;
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }
}
