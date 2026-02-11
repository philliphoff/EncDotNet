namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Represents a single chart in a pre-generated chart index.
/// </summary>
public sealed class ChartIndexEntry
{
    /// <summary>Gets or sets the chart name (e.g. "US5WA18M").</summary>
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the relative path to the chart .000 file within the expanded directory.</summary>
    public string Path { get; set; } = "";

    /// <summary>Gets or sets the southernmost latitude of the chart's coverage area.</summary>
    public double? SouthLatitude { get; set; }

    /// <summary>Gets or sets the westernmost longitude of the chart's coverage area.</summary>
    public double? WestLongitude { get; set; }

    /// <summary>Gets or sets the northernmost latitude of the chart's coverage area.</summary>
    public double? NorthLatitude { get; set; }

    /// <summary>Gets or sets the easternmost longitude of the chart's coverage area.</summary>
    public double? EastLongitude { get; set; }
}
