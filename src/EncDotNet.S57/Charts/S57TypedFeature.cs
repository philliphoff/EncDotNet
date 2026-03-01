using System.Collections.Immutable;

namespace EncDotNet.S57.Charts;

/// <summary>
/// Base class for all strongly-typed feature records in an S-57 chart.
/// </summary>
public abstract record S57TypedFeature
{
    /// <summary>Gets the record name that uniquely identifies this feature.</summary>
    public S57RecordName RecordName { get; }

    /// <summary>Gets the object code (OBJL) identifying the feature type.</summary>
    public S57ObjectCode ObjectCode { get; }

    /// <summary>Gets the group code (GRUP) for display ordering.</summary>
    public int Group { get; }

    /// <summary>Gets the record version.</summary>
    public int RecordVersion { get; }

    /// <summary>Gets the update instruction for this record.</summary>
    public S57UpdateInstruction UpdateInstruction { get; }

    /// <summary>Gets the feature attributes from the ATTF field.</summary>
    public IReadOnlyList<S57AttributeValue> Attributes { get; }

    /// <summary>Gets the national language attributes from the NATF field.</summary>
    public IReadOnlyList<S57AttributeValue> NationalAttributes { get; }

    /// <summary>Gets the feature-to-feature relationships from the FFPT field.</summary>
    public IReadOnlyList<S57FeaturePointer> RelatedFeatures { get; }

    /// <summary>
    /// Gets a value indicating whether this feature has any attributes.
    /// </summary>
    public bool HasAttributes => Attributes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this feature has national attributes.
    /// </summary>
    public bool HasNationalAttributes => NationalAttributes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this feature has related features.
    /// </summary>
    public bool HasRelatedFeatures => RelatedFeatures.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57TypedFeature"/> class from a generic feature record.
    /// </summary>
    protected S57TypedFeature(S57FeatureRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        RecordName = source.RecordName;
        ObjectCode = source.ObjectCode;
        Group = source.Group;
        RecordVersion = source.RecordVersion;
        UpdateInstruction = source.UpdateInstruction;
        Attributes = source.Attributes;
        NationalAttributes = source.NationalAttributes;
        RelatedFeatures = source.FeaturePointers;
    }

    /// <summary>
    /// Creates the appropriate strongly-typed feature from a generic feature record.
    /// </summary>
    /// <param name="source">The source feature record.</param>
    /// <returns>A strongly-typed feature record.</returns>
    public static S57TypedFeature Create(S57FeatureRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Primitive switch
        {
            S57GeometricPrimitive.Point => new S57PointFeature(source),
            S57GeometricPrimitive.Line => new S57LineFeature(source),
            S57GeometricPrimitive.Area => new S57AreaFeature(source),
            S57GeometricPrimitive.None => new S57MetaFeature(source),
            _ => throw new ArgumentException(
                $"Unknown geometric primitive: {source.Primitive}",
                nameof(source))
        };
    }

    /// <summary>
    /// Gets the value of an attribute by its code.
    /// </summary>
    /// <param name="attributeCode">The attribute code to find.</param>
    /// <returns>The attribute value, or <c>null</c> if not found.</returns>
    public string? GetAttributeValue(int attributeCode)
    {
        foreach (var attr in Attributes)
        {
            if (attr.AttributeCode == attributeCode)
            {
                return attr.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all values for a specific attribute code (for repeating attributes).
    /// </summary>
    /// <param name="attributeCode">The attribute code to find.</param>
    /// <returns>All values for the specified attribute.</returns>
    public IEnumerable<string> GetAttributeValues(int attributeCode)
    {
        foreach (var attr in Attributes)
        {
            if (attr.AttributeCode == attributeCode)
            {
                yield return attr.Value;
            }
        }
    }
}
