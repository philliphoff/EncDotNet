using System.Collections.ObjectModel;
using System.Windows.Input;
using EncDotNet.ChartViewer.Charts;
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

    public SelectableStateViewModel(string name, int chartCount)
    {
        Name = name;
        ChartCount = chartCount;
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
    private readonly IChartPackageManager _packageManager;
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

    public SetupWizardViewModel(IChartPackageManager packageManager)
    {
        _packageManager = packageManager;
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
            States.Clear();

            await foreach (var package in _packageManager.GetPackagesAsync(_cts.Token))
            {
                var vm = new SelectableStateViewModel(package.PackageName, package.ChartCount);
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
        var selectedStates = States.Where(s => s.IsSelected).ToList();

        if (selectedStates.Count == 0)
        {
            SelectedChartCount = 0;
            SelectionSummary = "No charts selected";
            this.RaisePropertyChanged(nameof(CanProceed));
            return;
        }

        var chartCount = selectedStates.Sum(s => s.ChartCount);
        SelectedChartCount = chartCount;
        SelectionSummary = $"{chartCount} charts selected";
        this.RaisePropertyChanged(nameof(CanProceed));
    }

    private async Task DownloadAndPrepareAsync()
    {
        try
        {
            var selectedStateIds = new HashSet<string>(
                States.Where(s => s.IsSelected).Select(s => s.Name));

            if (selectedStateIds.Count == 0)
            {
                ErrorMessage = "No charts selected.";
                _errorSource = SetupWizardStep.Downloading;
                CurrentStep = SetupWizardStep.Error;
                return;
            }

            var progress = new Progress<InstallationUpdate>(update =>
            {
                StatusText = update.Message;
                Progress = update.ProgressPercentage;
            });

            await _packageManager.InstallPackagesAsync(selectedStateIds, progress, _cts.Token);

            PreparedChartCount = SelectedChartCount;
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
}
