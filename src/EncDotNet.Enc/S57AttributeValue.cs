namespace EncDotNet.Enc;

/// <summary>
/// Represents an S-57 attribute value.
/// </summary>
public readonly struct S57AttributeValue
{
    /// <summary>Gets the attribute code (ATTL).</summary>
    public int AttributeCode { get; init; }

    /// <summary>Gets the attribute value (ATVL).</summary>
    public string Value { get; init; }

    /// <summary>
    /// Creates an attribute value with the specified code and value.
    /// </summary>
    public S57AttributeValue(int attributeCode, string value)
    {
        AttributeCode = attributeCode;
        Value = value;
    }

    /// <inheritdoc/>
    public override string ToString() => $"ATTL={AttributeCode}, ATVL={Value}";
}
