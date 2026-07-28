using EncDotNet.S57;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// ViewModel for an individual S-57 feature (object code) within a category.
/// </summary>
public sealed class ChartFeatureItemViewModel : ViewModelBase
{
    private bool _isVisible;

    /// <summary>Gets the S-57 object code for this feature.</summary>
    public S57ObjectCode ObjectCode { get; }

    /// <summary>Gets the display name.</summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets whether this feature is visible on the map.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value)
                return;

            this.RaiseAndSetIfChanged(ref _isVisible, value);
            IsVisibleChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// Raised when visibility changes. The bool argument is the new visibility value.
    /// </summary>
    public event EventHandler<bool>? IsVisibleChanged;

    public ChartFeatureItemViewModel(S57ObjectCode objectCode, string name, bool defaultEnabled)
    {
        ObjectCode = objectCode;
        Name = name;
        _isVisible = defaultEnabled;
    }
}
