namespace EncDotNet.S57.Charts;

/// <summary>
/// Represents a meta feature (no geometry) in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Meta features (PRIM=255) are objects that carry metadata without any spatial geometry.
/// They are used for information that applies to other features or the entire dataset,
/// such as compilation scale, data quality information, or administrative boundaries
/// that don't require geometry.
/// </para>
/// <para>
/// Examples include: M_QUAL (quality of data), M_SREL (survey reliability),
/// M_CSCL (compilation scale of data).
/// </para>
/// </remarks>
public sealed record S57MetaFeature : S57TypedFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="S57MetaFeature"/> class.
    /// </summary>
    /// <param name="source">The source feature record.</param>
    internal S57MetaFeature(S57FeatureRecord source) : base(source)
    {
    }
}
