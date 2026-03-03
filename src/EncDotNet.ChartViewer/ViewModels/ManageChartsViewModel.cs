using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using EncDotNet.ChartViewer.Catalogs;
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
    private HashSet<string> _previouslySelectedStates = [];
    private readonly IChartPackageManager _packageManager;
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
    public ICommand ReloadCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand FinishCommand { get; }

    public event Action? RequestClose;

    public ManageChartsViewModel(IChartPackageManager packageManager)
    {
        _packageManager = packageManager;
        ApplyCommand = ReactiveCommand.Create(OnApply);
        ReloadCommand = ReactiveCommand.Create(OnReload);
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

    private async void OnReload()
    {
        CurrentStep = ManageChartsStep.Applying;
        await ReloadIndexAsync();
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

            States.Clear();

            await foreach (var package in _packageManager.GetPackagesAsync(_cts.Token))
            {
                var vm = new SelectableStateViewModel(package.PackageName, package.ChartCount);
                vm.IsSelected = package.IsInstalled;
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

        var chartCount = States.Where(s => s.IsSelected).Sum(s => s.ChartCount);
        SelectedChartCount = chartCount;

        if (selectedStates.Count == 0)
        {
            SelectionSummary = "No charts selected";
        }
        else
        {
            SelectionSummary = $"{chartCount} charts selected";
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

    private async Task ReloadIndexAsync()
    {
        try
        {
            var progress = new Progress<InstallationUpdate>(update =>
            {
                StatusText = update.Message;
                Progress = update.ProgressPercentage;
            });

            await _packageManager.ReloadIndexAsync(progress, _cts.Token);

            ChartsChanged = true;
            PreparedChartCount = SelectedChartCount;
            CurrentStep = ManageChartsStep.Complete;
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while reloading charts: {ex.Message}";
            _errorSource = ManageChartsStep.Applying;
            CurrentStep = ManageChartsStep.Error;
        }
    }

    private async Task ApplyChangesAsync()
    {
        try
        {
            var newSelectedStates = new HashSet<string>(
                States.Where(s => s.IsSelected).Select(s => s.Name));

            var progress = new Progress<InstallationUpdate>(update =>
            {
                StatusText = update.Message;
                Progress = update.ProgressPercentage;
            });

            await _packageManager.InstallPackagesAsync(newSelectedStates, progress, _cts.Token);

            ChartsChanged = true;
            PreparedChartCount = SelectedChartCount;
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
}
