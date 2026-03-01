using System.Collections.Immutable;

namespace EncDotNet.S57.Charts;

/// <summary>
/// Represents a point feature in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Point features (PRIM=1) are geographic objects represented by a single location,
/// such as buoys, lights, beacons, or individual landmarks.
/// </para>
/// <para>
/// Point features reference isolated nodes or connected nodes for their geometry.
/// </para>
/// </remarks>
public sealed record S57PointFeature : S57TypedFeature
{
    /// <summary>
    /// Gets the spatial references (typically isolated or connected nodes).
    /// </summary>
    public IReadOnlyList<S57SpatialPointer> SpatialReferences { get; }

    /// <summary>
    /// Gets a value indicating whether this feature has spatial references.
    /// </summary>
    public bool HasSpatialReferences => SpatialReferences.Count > 0;

    /// <summary>
    /// Gets the primary spatial reference (the first one, if any).
    /// </summary>
    public S57SpatialPointer? PrimarySpatialReference =>
        SpatialReferences.Count == 0 ? null : SpatialReferences[0];

    /// <summary>
    /// Initializes a new instance of the <see cref="S57PointFeature"/> class.
    /// </summary>
    /// <param name="source">The source feature record.</param>
    internal S57PointFeature(S57FeatureRecord source) : base(source)
    {
        SpatialReferences = source.SpatialPointers;
    }
}
