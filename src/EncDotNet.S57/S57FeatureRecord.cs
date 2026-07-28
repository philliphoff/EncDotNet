namespace EncDotNet.S57;

/// <summary>
/// Base class for S-57 feature records.
/// </summary>
public sealed record S57FeatureRecord
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the object geometric primitive (PRIM).</summary>
    public S57GeometricPrimitive Primitive { get; init; }

    /// <summary>Gets the group code (GRUP).</summary>
    public int Group { get; init; }

    /// <summary>Gets the object label/code (OBJL).</summary>
    public S57ObjectCode ObjectCode { get; init; }

    /// <summary>Gets the record version (RVER).</summary>
    public int RecordVersion { get; init; }

    /// <summary>Gets the record update instruction (RUIN).</summary>
    public S57UpdateInstruction UpdateInstruction { get; init; }

    /// <summary>Gets the object attributes from ATTF field.</summary>
    public IReadOnlyList<S57AttributeValue> Attributes { get; init; } = [];

    /// <summary>Gets the national attributes from NATF field.</summary>
    public IReadOnlyList<S57AttributeValue> NationalAttributes { get; init; } = [];

    /// <summary>Gets the spatial pointers from FSPT field.</summary>
    public IReadOnlyList<S57SpatialPointer> SpatialPointers { get; init; } = [];

    /// <summary>Gets the spatial pointer update control from FSPC field, if present.</summary>
    public S57FieldUpdateControl? SpatialPointerControl { get; init; }

    /// <summary>Gets the feature pointers from FFPT field.</summary>
    public IReadOnlyList<S57FeaturePointer> FeaturePointers { get; init; } = [];

    /// <summary>Gets the feature pointer update control from FFPC field, if present.</summary>
    public S57FieldUpdateControl? FeaturePointerControl { get; init; }
}
