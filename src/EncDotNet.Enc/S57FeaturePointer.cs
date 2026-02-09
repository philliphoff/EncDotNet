namespace EncDotNet.Enc;

/// <summary>
/// Represents a feature-to-feature object pointer (FFPT) in S-57.
/// </summary>
public readonly record struct S57FeaturePointer
{
    /// <summary>Gets the name of the related feature record.</summary>
    public S57RecordName Name { get; init; }

    /// <summary>Gets the relationship indicator.</summary>
    public S57RelationshipIndicator Relationship { get; init; }

    /// <summary>Gets the comment.</summary>
    public string Comment { get; init; }
}
