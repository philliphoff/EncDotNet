using System.Collections.Immutable;

namespace EncDotNet.S57.Charts;

/// <summary>
/// Represents a face in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Faces (RCNM=140) are area spatial objects defined by one or more edge boundaries.
/// They represent bounded 2D regions and are used for area features like sea areas,
/// land areas, or depth zones.
/// </para>
/// <para>
/// A face has an exterior boundary (the outer ring) and may have interior boundaries
/// (holes or islands within the area).
/// </para>
/// </remarks>
public sealed record S57Face : S57SpatialRecord
{
    /// <summary>
    /// Gets the exterior boundary edge references.
    /// </summary>
    /// <remarks>
    /// The exterior boundary defines the outer ring of the face.
    /// Edges are listed in order and their orientation indicates the direction
    /// they should be traversed.
    /// </remarks>
    public ImmutableArray<S57EdgeReference> ExteriorBoundary { get; }

    /// <summary>
    /// Gets the interior boundary edge references (holes).
    /// </summary>
    /// <remarks>
    /// Interior boundaries define holes or islands within the face.
    /// Each interior boundary is a ring of edges.
    /// </remarks>
    public ImmutableArray<S57EdgeReference> InteriorBoundaries { get; }

    /// <summary>
    /// Gets a value indicating whether this face has an exterior boundary.
    /// </summary>
    public bool HasExteriorBoundary => !ExteriorBoundary.IsDefaultOrEmpty;

    /// <summary>
    /// Gets a value indicating whether this face has any interior boundaries (holes).
    /// </summary>
    public bool HasInteriorBoundaries => !InteriorBoundaries.IsDefaultOrEmpty;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57Face"/> class.
    /// </summary>
    /// <param name="source">The source vector record.</param>
    internal S57Face(S57VectorRecord source) : base(source)
    {
        var exterior = ImmutableArray.CreateBuilder<S57EdgeReference>();
        var interior = ImmutableArray.CreateBuilder<S57EdgeReference>();

        foreach (var pointer in source.VectorPointers)
        {
            var edgeRef = new S57EdgeReference(
                pointer.Name,
                pointer.Orientation,
                pointer.Usage,
                pointer.Mask);

            switch (pointer.Usage)
            {
                case S57UsageIndicator.Exterior:
                case S57UsageIndicator.ExteriorTruncated:
                    exterior.Add(edgeRef);
                    break;
                case S57UsageIndicator.Interior:
                    interior.Add(edgeRef);
                    break;
            }
        }

        ExteriorBoundary = exterior.ToImmutable();
        InteriorBoundaries = interior.ToImmutable();
    }
}
