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

/// <summary>
/// Represents a selectable state/territory in the region selection step.
/// </summary>
public sealed class SelectableStateViewModel : ViewModelBase
{
    private bool _isSelected;

    public string Name { get; }

    public int ChartCount { get; }

    public long TotalSize { get; }

    public string DisplayText { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            this.RaiseAndSetIfChanged(ref _isSelected, value);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? SelectionChanged;

    public SelectableStateViewModel(string name, int chartCount, long totalSize)
    {
        Name = name;
        ChartCount = chartCount;
        TotalSize = totalSize;
        DisplayText = $"{name} ({chartCount} charts)";
    }
}

public enum SetupWizardStep
{
    Welcome,
    FetchingCatalog,
    SelectRegions,
    Downloading,
    Complete,
    Error
}

public class SetupWizardViewModel : ViewModelBase
{
    private SetupWizardStep _currentStep = SetupWizardStep.Welcome;
    private SetupWizardStep _errorSource;
    private string _statusText = "";
    private double _progress;
    private string _errorMessage = "";
    private bool _selectAll;
    private int _selectedChartCount;
    private string _selectionSummary = "No charts selected";
    private int _preparedChartCount;
    private bool _updatingSelectAll;
    private EncProductCatalog? _catalog;
    private readonly CancellationTokenSource _cts = new();

    public SetupWizardStep CurrentStep
    {
        get => _currentStep;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentStep, value);
            this.RaisePropertyChanged(nameof(IsWelcomeStep));
            this.RaisePropertyChanged(nameof(IsFetchingStep));
            this.RaisePropertyChanged(nameof(IsSelectStep));
            this.RaisePropertyChanged(nameof(IsDownloadingStep));
            this.RaisePropertyChanged(nameof(IsCompleteStep));
            this.RaisePropertyChanged(nameof(IsErrorStep));
            this.RaisePropertyChanged(nameof(ShowBackButton));
            this.RaisePropertyChanged(nameof(ShowNextButton));
            this.RaisePropertyChanged(nameof(ShowCancelButton));
            this.RaisePropertyChanged(nameof(ShowFinishButton));
            this.RaisePropertyChanged(nameof(NextButtonText));
            this.RaisePropertyChanged(nameof(IsProgressIndeterminate));
            this.RaisePropertyChanged(nameof(CanProceed));
        }
    }

    public bool IsWelcomeStep => _currentStep == SetupWizardStep.Welcome;
    public bool IsFetchingStep => _currentStep == SetupWizardStep.FetchingCatalog;
    public bool IsSelectStep => _currentStep == SetupWizardStep.SelectRegions;
    public bool IsDownloadingStep => _currentStep == SetupWizardStep.Downloading;
    public bool IsCompleteStep => _currentStep == SetupWizardStep.Complete;
    public bool IsErrorStep => _currentStep == SetupWizardStep.Error;

    public bool ShowBackButton => _currentStep is SetupWizardStep.SelectRegions or SetupWizardStep.Error;
    public bool ShowNextButton => _currentStep is SetupWizardStep.Welcome or SetupWizardStep.SelectRegions;
    public bool ShowCancelButton => _currentStep is not SetupWizardStep.Complete;
    public bool ShowFinishButton => _currentStep is SetupWizardStep.Complete;

    public string NextButtonText => _currentStep == SetupWizardStep.SelectRegions ? "Download" : "Next";
    public bool IsProgressIndeterminate => _currentStep == SetupWizardStep.FetchingCatalog;
    public bool CanProceed => _currentStep != SetupWizardStep.SelectRegions || _selectedChartCount > 0;

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

    public int PreparedChartCount
    {
        get => _preparedChartCount;
        private set => this.RaiseAndSetIfChanged(ref _preparedChartCount, value);
    }

    public ObservableCollection<SelectableStateViewModel> States { get; } = new();

    public ICommand NextCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand FinishCommand { get; }

    public event Action? RequestClose;

    public SetupWizardViewModel()
    {
        NextCommand = ReactiveCommand.Create(OnNext);
        BackCommand = ReactiveCommand.Create(OnBack);
        CancelCommand = ReactiveCommand.Create(OnCancel);
        FinishCommand = ReactiveCommand.Create(OnFinish);
    }

    public void Cancel()
    {
        _cts.Cancel();
    }

    private async void OnNext()
    {
        switch (_currentStep)
        {
            case SetupWizardStep.Welcome:
                CurrentStep = SetupWizardStep.FetchingCatalog;
                await FetchCatalogAsync();
                break;

            case SetupWizardStep.SelectRegions:
                CurrentStep = SetupWizardStep.Downloading;
                await DownloadAndPrepareAsync();
                break;
        }
    }

    private void OnBack()
    {
        switch (_currentStep)
        {
            case SetupWizardStep.SelectRegions:
                CurrentStep = SetupWizardStep.Welcome;
                break;

            case SetupWizardStep.Error:
                CurrentStep = _errorSource == SetupWizardStep.FetchingCatalog
                    ? SetupWizardStep.Welcome
                    : SetupWizardStep.SelectRegions;
                break;
        }
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
                vm.SelectionChanged += (_, _) => OnStateSelectionChanged();
                States.Add(vm);
            }

            CurrentStep = SetupWizardStep.SelectRegions;
            UpdateSelectionSummary();
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to fetch the NOAA catalog: {ex.Message}";
            _errorSource = SetupWizardStep.FetchingCatalog;
            CurrentStep = SetupWizardStep.Error;
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

        if (selectedStates.Count == 0)
        {
            SelectedChartCount = 0;
            SelectionSummary = "No charts selected";
            this.RaisePropertyChanged(nameof(CanProceed));
            return;
        }

        var selectedCells = GetSelectedCells(selectedStates);
        SelectedChartCount = selectedCells.Count;
        var totalBytes = selectedCells.Sum(c => c.ZipfileSize);
        SelectionSummary = $"{selectedCells.Count} charts selected ({FormatSize(totalBytes)} total download)";
        this.RaisePropertyChanged(nameof(CanProceed));
    }

    private List<Cell> GetSelectedCells(HashSet<string> selectedStates)
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
                ? selectedStates.Contains("Other")
                : cellStates.Any(selectedStates.Contains);

            if (match)
            {
                cells.Add(cell);
                seen.Add(cell.Name);
            }
        }

        return cells;
    }

    private async Task DownloadAndPrepareAsync()
    {
        try
        {
            AppDataPaths.EnsureDirectories();

            var selectedStates = new HashSet<string>(
                States.Where(s => s.IsSelected).Select(s => s.Name));
            var cells = GetSelectedCells(selectedStates);

            if (cells.Count == 0)
            {
                ErrorMessage = "No charts selected.";
                _errorSource = SetupWizardStep.Downloading;
                CurrentStep = SetupWizardStep.Error;
                return;
            }

            using var httpClient = new HttpClient();
            int total = cells.Count;
            int completed = 0;

            // Phase 1: Download zip files
            foreach (var cell in cells)
            {
                _cts.Token.ThrowIfCancellationRequested();

                var zipUrl = cell.ZipfileLocation;
                var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
                var outputPath = Path.Combine(AppDataPaths.CatalogDirectory, fileName);

                if (!File.Exists(outputPath))
                {
                    StatusText = $"Downloading {cell.Name} ({completed + 1} of {total})...";

                    using var response = await httpClient.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
                    using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
                    await stream.CopyToAsync(fileStream, _cts.Token);
                }

                completed++;
                Progress = (double)completed / total * 60;
            }

            // Phase 2: Extract zip files
            completed = 0;
            foreach (var cell in cells)
            {
                _cts.Token.ThrowIfCancellationRequested();

                var zipUrl = cell.ZipfileLocation;
                var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
                var zipPath = Path.Combine(AppDataPaths.CatalogDirectory, fileName);
                var folderName = Path.GetFileNameWithoutExtension(zipPath);
                var outputDir = Path.Combine(AppDataPaths.ExpandedDirectory, folderName);

                if (!Directory.Exists(outputDir) && File.Exists(zipPath))
                {
                    StatusText = $"Extracting {cell.Name} ({completed + 1} of {total})...";
                    await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, outputDir), _cts.Token);
                }

                completed++;
                Progress = 60 + (double)completed / total * 30;
            }

            // Phase 3: Build chart index
            StatusText = "Building chart index...";
            Progress = 90;
            var chartCount = await Task.Run(BuildChartIndex, _cts.Token);
            Progress = 100;

            PreparedChartCount = chartCount;
            CurrentStep = SetupWizardStep.Complete;
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred during download: {ex.Message}";
            _errorSource = SetupWizardStep.Downloading;
            CurrentStep = SetupWizardStep.Error;
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
