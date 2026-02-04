using System.Collections.Immutable;

namespace EncDotNet.Enc.Charts;

/// <summary>
/// Represents a line feature in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Line features (PRIM=2) are geographic objects represented by a series of connected points,
/// such as coastlines, depth contours, cables, pipelines, or traffic separation lines.
/// </para>
/// <para>
/// Line features reference one or more edges for their geometry.
/// </para>
/// </remarks>
public sealed class S57LineFeature : S57TypedFeature
{
    /// <summary>
    /// Gets the edge references that make up this line feature.
    /// </summary>
    public ImmutableArray<S57SpatialPointer> EdgeReferences { get; }

    /// <summary>
    /// Gets a value indicating whether this feature has edge references.
    /// </summary>
    public bool HasEdgeReferences => !EdgeReferences.IsDefaultOrEmpty;

    /// <summary>
    /// Gets the number of edges that make up this feature.
    /// </summary>
    public int EdgeCount => EdgeReferences.IsDefaultOrEmpty ? 0 : EdgeReferences.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57LineFeature"/> class.
    /// </summary>
    /// <param name="source">The source feature record.</param>
    internal S57LineFeature(S57FeatureRecord source) : base(source)
    {
        EdgeReferences = source.SpatialPointers;
    }
}
