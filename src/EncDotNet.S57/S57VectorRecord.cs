namespace EncDotNet.S57;

/// <summary>
/// Represents an S-57 vector (spatial) record.
/// </summary>
public sealed record S57VectorRecord
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the record version (RVER).</summary>
    public int RecordVersion { get; init; }

    /// <summary>Gets the record update instruction (RUIN).</summary>
    public S57UpdateInstruction UpdateInstruction { get; init; }

    /// <summary>Gets the attributes from ATTV field.</summary>
    public IReadOnlyList<S57AttributeValue> Attributes { get; init; } = [];

    /// <summary>Gets the vector record pointers from VRPT field.</summary>
    public IReadOnlyList<S57VectorPointer> VectorPointers { get; init; } = [];

    /// <summary>Gets the vector pointer update control from VRPC field, if present.</summary>
    public S57FieldUpdateControl? VectorPointerControl { get; init; }

    /// <summary>Gets the 2D coordinates from SG2D field.</summary>
    public IReadOnlyList<S57Coordinate2D> Coordinates2D { get; init; } = [];

    /// <summary>Gets the 3D sounding coordinates from SG3D field.</summary>
    public IReadOnlyList<S57Sounding> Soundings { get; init; } = [];

    /// <summary>
    /// Gets the arc/curve definitions from the ARCC fields, each with the AR2D, EL2D or CT2D
    /// coordinates that follow it.
    /// </summary>
    /// <remarks>
    /// An edge has either <see cref="Coordinates2D"/> or arc/curve definitions, not both
    /// (S-57 Part 3, clause 5.1.4.4).
    /// </remarks>
    public IReadOnlyList<S57ArcCurve> ArcCurves { get; init; } = [];

    /// <summary>
    /// Gets the coordinate update control from SGCC field, if present. It addresses the
    /// coordinates of the SG2D, SG3D, AR2D, EL2D and CT2D fields.
    /// </summary>
    public S57FieldUpdateControl? CoordinateControl { get; init; }
}
