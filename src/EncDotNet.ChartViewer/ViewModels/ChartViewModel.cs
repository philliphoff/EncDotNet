using System;
using System.Collections.Generic;
using EncDotNet.ChartViewer.Models;
using Mapsui.Layers;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// ViewModel for a selectable chart in the chart browser panel.
/// </summary>
public sealed class ChartViewModel : ViewModelBase
{
    private bool _isSelected;

    /// <summary>Gets the chart index entry.</summary>
    public ChartIndexEntry Entry { get; }

    /// <summary>Gets the display name.</summary>
    public string Name => Entry.Name;

    /// <summary>Gets or sets the layers created for this chart.</summary>
    public List<MemoryLayer> Layers { get; } = new();

    /// <summary>
    /// Gets or sets whether this chart is selected (loaded on the map).
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            this.RaiseAndSetIfChanged(ref _isSelected, value);
            IsSelectedChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// Raised when selection changes. The bool argument is the new selection value.
    /// </summary>
    public event EventHandler<bool>? IsSelectedChanged;

    public ChartViewModel(ChartIndexEntry entry)
    {
        Entry = entry;
    }
}
