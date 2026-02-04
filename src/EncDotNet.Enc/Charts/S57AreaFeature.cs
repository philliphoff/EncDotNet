namespace EncDotNet.Enc.Charts;

/// <summary>
/// Represents an area feature in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Area features (PRIM=3) are geographic objects represented by a bounded region,
/// such as depth areas, land areas, anchorage areas, or restricted zones.
/// </para>
/// <para>
/// Area features reference a face for their geometry.
/// </para>
/// </remarks>
public sealed class S57AreaFeature : S57TypedFeature
{
    /// <summary>
    /// Gets the face reference for this area feature.
    /// </summary>
    public S57SpatialPointer? FaceReference { get; }

    /// <summary>
    /// Gets a value indicating whether this feature has a face reference.
    /// </summary>
    public bool HasFaceReference => FaceReference.HasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57AreaFeature"/> class.
    /// </summary>
    /// <param name="source">The source feature record.</param>
    internal S57AreaFeature(S57FeatureRecord source) : base(source)
    {
        FaceReference = source.SpatialPointers.IsDefaultOrEmpty
            ? null
            : source.SpatialPointers[0];
    }
}
