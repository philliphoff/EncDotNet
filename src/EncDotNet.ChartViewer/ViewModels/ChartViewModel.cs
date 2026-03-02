using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EncDotNet.ChartViewer.Catalogs;
using EncDotNet.ChartViewer.Models;
using Mapsui.Layers;
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

    public ChartViewModel(ChartIndexEntry entry)
    {
        Entry = entry;
        CopyPathCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
            {
                await window.Clipboard!.SetTextAsync(FullPath);
            }
        });
    }
}
