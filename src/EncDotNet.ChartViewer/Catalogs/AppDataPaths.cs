using System;
using System.IO;

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

    public static bool HasChartIndex() => File.Exists(ChartIndexPath);

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(CatalogDirectory);
        Directory.CreateDirectory(ExpandedDirectory);
    }
}
