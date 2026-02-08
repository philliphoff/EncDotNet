using System.Collections.Immutable;

namespace EncDotNet.Enc;

/// <summary>
/// Represents an S-57 vector (spatial) record.
/// </summary>
public sealed class S57VectorRecord
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the record version (RVER).</summary>
    public int RecordVersion { get; init; }

    /// <summary>Gets the record update instruction (RUIN).</summary>
    public S57UpdateInstruction UpdateInstruction { get; init; }

    /// <summary>Gets the attributes from ATTV field.</summary>
    public ImmutableArray<S57AttributeValue> Attributes { get; init; }

    /// <summary>Gets the vector record pointers from VRPT field.</summary>
    public ImmutableArray<S57VectorPointer> VectorPointers { get; init; }

    /// <summary>Gets the 2D coordinates from SG2D field.</summary>
    public ImmutableArray<S57Coordinate2D> Coordinates2D { get; init; }

    /// <summary>Gets the 3D sounding coordinates from SG3D field.</summary>
    public ImmutableArray<S57Sounding> Soundings { get; init; }
}
