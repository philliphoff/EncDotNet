using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EncDotNet.ChartViewer.Catalogs;

/// <summary>
/// Provides paths to the app-local storage locations for chart data.
/// </summary>
internal static class AppDataPaths
{
    private static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EncDotNet");

    public static string CatalogDirectory => Path.Combine(Root, "catalog");

    public static string ExpandedDirectory => Path.Combine(Root, "expanded");

    public static string ChartIndexPath => Path.Combine(Root, "expanded", "chart-index.json");

    public static string DownloadedStatesPath => Path.Combine(Root, "downloaded-states.json");

    public static string ViewportStatePath => Path.Combine(Root, "viewport-state.json");

    public static bool HasChartIndex() => File.Exists(ChartIndexPath);

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(CatalogDirectory);
        Directory.CreateDirectory(ExpandedDirectory);
    }

    /// <summary>
    /// Deletes all downloaded chart data, returning the application to its initial state.
    /// </summary>
    public static void DeleteAllData()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    public static HashSet<string> LoadDownloadedStates()
    {
        if (!File.Exists(DownloadedStatesPath))
            return [];

        try
        {
            var json = File.ReadAllText(DownloadedStatesPath);
            var list = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
            return list is not null ? new HashSet<string>(list) : [];
        }
        catch
        {
            return [];
        }
    }

    public static void SaveDownloadedStates(IEnumerable<string> states)
    {
        Directory.CreateDirectory(Root);
        var json = JsonSerializer.Serialize(states, JsonOptions);
        File.WriteAllText(DownloadedStatesPath, json);
    }

    public static (double CenterX, double CenterY, double Resolution)? LoadViewportState()
    {
        if (!File.Exists(ViewportStatePath))
            return null;

        try
        {
            var json = File.ReadAllText(ViewportStatePath);
            var values = JsonSerializer.Deserialize<double[]>(json, JsonOptions);
            if (values is { Length: 3 })
                return (values[0], values[1], values[2]);
        }
        catch
        {
            // Ignore corrupt state
        }

        return null;
    }

    public static void SaveViewportState(double centerX, double centerY, double resolution)
    {
        Directory.CreateDirectory(Root);
        var json = JsonSerializer.Serialize(new[] { centerX, centerY, resolution }, JsonOptions);
        File.WriteAllText(ViewportStatePath, json);
    }
}
