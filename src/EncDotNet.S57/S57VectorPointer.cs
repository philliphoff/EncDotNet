namespace EncDotNet.S57;

/// <summary>
/// Represents a spatial record pointer (VRPT) in S-57.
/// </summary>
public readonly record struct S57VectorPointer
{
    /// <summary>Gets the name of the spatial record.</summary>
    public S57RecordName Name { get; init; }

    /// <summary>Gets the orientation.</summary>
    public S57Orientation Orientation { get; init; }

    /// <summary>Gets the usage indicator.</summary>
    public S57UsageIndicator Usage { get; init; }

    /// <summary>Gets the topology indicator.</summary>
    public S57TopologyIndicator Topology { get; init; }

    /// <summary>Gets the masking indicator.</summary>
    public S57MaskingIndicator Mask { get; init; }
}
