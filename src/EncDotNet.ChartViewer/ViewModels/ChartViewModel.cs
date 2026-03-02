using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EncDotNet.ChartViewer.Catalogs;
using EncDotNet.ChartViewer.Models;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// ViewModel for a chart in the chart browser panel.
/// </summary>
public sealed class ChartViewModel : ViewModelBase
{
    /// <summary>Gets the chart index entry.</summary>
    public ChartIndexEntry Entry { get; }

    /// <summary>Gets the display name.</summary>
    public string Name => Entry.Name;

    /// <summary>Gets or sets the compilation scale (CSCL) of this chart, set after loading.</summary>
    public int CompilationScale { get; set; }

    /// <summary>Gets the full resolved path to the chart file.</summary>
    public string FullPath => Path.Combine(AppDataPaths.ExpandedDirectory, Entry.Path);

    /// <summary>Command to copy the chart's full path to the clipboard.</summary>
    public ICommand CopyPathCommand { get; }

    /// <summary>Gets or sets the layers created for this chart.</summary>
    public List<MemoryLayer> Layers { get; } = new();

    /// <summary>
    /// Pre-computed Mercator-projected bounds for the chart, or null if the chart has no geographic bounds.
    /// Computed once in the constructor to avoid repeated trigonometry during viewport evaluation.
    /// </summary>
    public MRect? ProjectedBounds { get; }

    public ChartViewModel(ChartIndexEntry entry)
    {
        Entry = entry;

        if (entry.SouthLatitude is { } south
            && entry.NorthLatitude is { } north
            && entry.WestLongitude is { } west
            && entry.EastLongitude is { } east)
        {
            var (minX, minY) = SphericalMercator.FromLonLat(west, south);
            var (maxX, maxY) = SphericalMercator.FromLonLat(east, north);
            ProjectedBounds = new MRect(minX, minY, maxX, maxY);
        }

        CopyPathCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
            {
                await window.Clipboard!.SetTextAsync(FullPath);
            }
        });
    }
}
