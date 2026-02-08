namespace EncDotNet.Enc;

/// <summary>
/// Represents a feature-to-spatial object pointer (FSPT) in S-57.
/// </summary>
public readonly struct S57SpatialPointer
{
    /// <summary>Gets the name of the spatial record.</summary>
    public S57RecordName Name { get; init; }

    /// <summary>Gets the orientation.</summary>
    public S57Orientation Orientation { get; init; }

    /// <summary>Gets the usage indicator.</summary>
    public S57UsageIndicator Usage { get; init; }

    /// <summary>Gets the masking indicator.</summary>
    public S57MaskingIndicator Mask { get; init; }
}
