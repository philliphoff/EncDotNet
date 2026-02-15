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

    public static string SelectedChartsPath => Path.Combine(Root, "selected-charts.json");

    public static string FeatureVisibilityPath => Path.Combine(Root, "feature-visibility.json");

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

    public static List<string> LoadSelectedCharts()
    {
        if (!File.Exists(SelectedChartsPath))
            return [];

        try
        {
            var json = File.ReadAllText(SelectedChartsPath);
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void SaveSelectedCharts(IEnumerable<string> chartNames)
    {
        Directory.CreateDirectory(Root);
        var json = JsonSerializer.Serialize(chartNames, JsonOptions);
        File.WriteAllText(SelectedChartsPath, json);
    }

    public static Dictionary<string, bool> LoadFeatureVisibility()
    {
        if (!File.Exists(FeatureVisibilityPath))
            return new();

        try
        {
            var json = File.ReadAllText(FeatureVisibilityPath);
            return JsonSerializer.Deserialize<Dictionary<string, bool>>(json, JsonOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public static void SaveFeatureVisibility(IDictionary<string, bool> visibility)
    {
        Directory.CreateDirectory(Root);
        var json = JsonSerializer.Serialize(visibility, JsonOptions);
        File.WriteAllText(FeatureVisibilityPath, json);
    }
}
