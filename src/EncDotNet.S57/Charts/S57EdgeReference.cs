namespace EncDotNet.S57.Charts;

/// <summary>
/// Represents a reference to an edge in a face boundary.
/// </summary>
/// <remarks>
/// Edge references describe how an edge is used within a face's boundary,
/// including its orientation and whether it should be masked during display.
/// </remarks>
public readonly record struct S57EdgeReference
{
    /// <summary>Gets the record name of the referenced edge.</summary>
    public S57RecordName EdgeName { get; }

    /// <summary>Gets the orientation of the edge in this context.</summary>
    public S57Orientation Orientation { get; }

    /// <summary>Gets the usage indicator (exterior or interior boundary).</summary>
    public S57UsageIndicator Usage { get; }

    /// <summary>Gets the masking indicator for display purposes.</summary>
    public S57MaskingIndicator Mask { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="S57EdgeReference"/> struct.
    /// </summary>
    public S57EdgeReference(
        S57RecordName edgeName,
        S57Orientation orientation,
        S57UsageIndicator usage,
        S57MaskingIndicator mask)
    {
        EdgeName = edgeName;
        Orientation = orientation;
        Usage = usage;
        Mask = mask;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Edge {EdgeName}, Orientation={Orientation}, Usage={Usage}";
}
