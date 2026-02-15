namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Represents a single chart in a pre-generated chart index.
/// </summary>
public sealed record ChartIndexEntry
{
    /// <summary>Gets or sets the unique chart identifier (the chart filename without extension, e.g. "US5WA18M").</summary>
    public string Id { get; init; } = "";

    /// <summary>Gets or sets the chart display name (e.g. "US5WA18M").</summary>
    public required string Name { get; init; }

    /// <summary>Gets or sets the relative path to the chart .000 file within the expanded directory.</summary>
    public required string Path { get; init; }

    /// <summary>Gets or sets the southernmost latitude of the chart's coverage area.</summary>
    public double? SouthLatitude { get; init; }

    /// <summary>Gets or sets the westernmost longitude of the chart's coverage area.</summary>
    public double? WestLongitude { get; init; }

    /// <summary>Gets or sets the northernmost latitude of the chart's coverage area.</summary>
    public double? NorthLatitude { get; init; }

    /// <summary>Gets or sets the easternmost longitude of the chart's coverage area.</summary>
    public double? EastLongitude { get; init; }
}
