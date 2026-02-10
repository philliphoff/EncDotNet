using System;
using EncDotNet.ChartViewer.Models;
using Mapsui.Layers;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// ViewModel for a toggleable S-57 chart feature category.
/// </summary>
public sealed class ChartFeatureViewModel : ViewModelBase
{
    private bool _isVisible;

    /// <summary>Gets the feature category definition.</summary>
    public S57FeatureCategory Category { get; }

    /// <summary>Gets the display name.</summary>
    public string Name => Category.Name;

    /// <summary>Gets or sets the associated map layer (set after chart loading).</summary>
    public MemoryLayer? Layer { get; set; }

    /// <summary>
    /// Gets or sets whether this feature category is visible on the map.
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

    public ChartFeatureViewModel(S57FeatureCategory category)
    {
        Category = category;
        _isVisible = category.DefaultEnabled;
    }
}
