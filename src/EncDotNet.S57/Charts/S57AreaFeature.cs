using System.Collections.Immutable;
using System.Linq;

namespace EncDotNet.S57.Charts;

/// <summary>
/// Represents an area feature in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Area features (PRIM=3) are geographic objects represented by a bounded region,
/// such as depth areas, land areas, anchorage areas, or restricted zones.
/// </para>
/// <para>
/// In full topology (level 3), area features reference one or more faces.
/// In chain-node topology (level 2), area features reference edges directly,
/// with usage indicators distinguishing exterior from interior boundaries.
/// </para>
/// </remarks>
public sealed record S57AreaFeature : S57TypedFeature
{
    /// <summary>
    /// Gets the face references for this area feature (full topology).
    /// </summary>
    public IReadOnlyList<S57SpatialPointer> FaceReferences { get; }

    /// <summary>
    /// Gets the first face reference for this area feature, or <c>null</c> if none exist.
    /// </summary>
    public S57SpatialPointer? FaceReference =>
        FaceReferences.Count == 0 ? null : FaceReferences[0];

    /// <summary>
    /// Gets a value indicating whether this feature has face references.
    /// </summary>
    public bool HasFaceReference => FaceReferences.Count > 0;

    /// <summary>
    /// Gets the exterior boundary edge references (chain-node topology).
    /// </summary>
    public IReadOnlyList<S57EdgeReference> ExteriorEdgeReferences { get; }

    /// <summary>
    /// Gets the interior boundary edge references (chain-node topology).
    /// </summary>
    public IReadOnlyList<S57EdgeReference> InteriorEdgeReferences { get; }

    /// <summary>
    /// Gets a value indicating whether this feature has exterior edge references.
    /// </summary>
    public bool HasExteriorEdgeReferences => ExteriorEdgeReferences.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57AreaFeature"/> class.
    /// </summary>
    /// <param name="source">The source feature record.</param>
    internal S57AreaFeature(S57FeatureRecord source) : base(source)
    {
        if (source.SpatialPointers.Count == 0)
        {
            FaceReferences = [];
            ExteriorEdgeReferences = [];
            InteriorEdgeReferences = [];
            return;
        }

        FaceReferences = source.SpatialPointers
            .Where(p => p.Name.RecordNameCode == S57RecordNameCodes.Face)
            .ToImmutableArray();

        var exterior = ImmutableArray.CreateBuilder<S57EdgeReference>();
        var interior = ImmutableArray.CreateBuilder<S57EdgeReference>();

        foreach (var pointer in source.SpatialPointers)
        {
            if (pointer.Name.RecordNameCode != S57RecordNameCodes.Edge)
                continue;

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

        ExteriorEdgeReferences = exterior.ToImmutable();
        InteriorEdgeReferences = interior.ToImmutable();
    }
}
