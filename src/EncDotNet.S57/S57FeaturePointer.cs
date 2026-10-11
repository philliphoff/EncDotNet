namespace EncDotNet.S57;

/// <summary>
/// Represents a feature-to-feature object pointer (FFPT) in S-57.
/// </summary>
public readonly record struct S57FeaturePointer
{
    /// <summary>
    /// Gets the long name (LNAM) of the related feature object (S-57 Part 3, 4.6). Look the
    /// feature up with <see cref="Charts.S57Chart.GetFeature(S57LongName)"/> or
    /// <see cref="S57Document.GetFeatureRecord(S57LongName)"/>.
    /// </summary>
    public S57LongName Name { get; init; }

    /// <summary>Gets the relationship indicator.</summary>
    public S57RelationshipIndicator Relationship { get; init; }

    /// <summary>Gets the comment.</summary>
    public string Comment { get; init; }
}
