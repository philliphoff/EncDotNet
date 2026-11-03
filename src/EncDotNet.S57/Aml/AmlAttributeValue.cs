using System.Globalization;

namespace EncDotNet.S57.Aml;

/// <summary>
/// Represents an attribute value of an AML feature together with the catalogue definition of its
/// attribute code.
/// </summary>
/// <remarks>
/// Get these from <see cref="AmlDataSet.GetAttributes(Charts.S57TypedFeature)"/> or
/// <see cref="AmlDataSet.GetNationalAttributes(Charts.S57TypedFeature)"/>.
/// </remarks>
/// <param name="Value">The attribute value as encoded (<c>ATTL</c> and <c>ATVL</c>).</param>
/// <param name="Definition">
/// The attribute's definition in the data set's catalogue, or <see langword="null"/> when the
/// catalogue has no attribute with the value's code.
/// </param>
public readonly record struct AmlAttributeValue(S57AttributeValue Value, S57AttributeDefinition? Definition)
{
    /// <summary>Gets the attribute code (<c>ATTL</c>).</summary>
    public int Code => Value.AttributeCode;

    /// <summary>
    /// Gets the attribute acronym from <see cref="Definition"/>, for example <c>OBJNAM</c> or
    /// <c>databa</c>, or <see langword="null"/> when the attribute isn't in the catalogue.
    /// </summary>
    public string? Acronym => Definition?.Acronym;

    /// <inheritdoc/>
    public override string ToString() => $"{Acronym ?? Code.ToString(CultureInfo.InvariantCulture)}={Value.Value}";
}
