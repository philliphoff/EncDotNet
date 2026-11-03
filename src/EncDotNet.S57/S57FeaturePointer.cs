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

    /// <summary>Gets the comment (COMT).</summary>
    /// <remarks>
    /// S-57 makes this basic text (ASCII), but some producers, such as NATO AML cells, put
    /// ISO 8859-1 characters in it, so it's read and written as ISO 8859-1, a superset of ASCII.
    /// </remarks>
    public string Comment { get; init; }
}
