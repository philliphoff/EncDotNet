using System.Diagnostics.CodeAnalysis;

namespace EncDotNet.S57;

/// <summary>
/// Describes an S-57 attribute in an <see cref="S57ObjectCatalogue"/>.
/// </summary>
public sealed class S57AttributeDefinition
{
    internal S57AttributeDefinition(
        int code,
        string acronym,
        string name,
        S57AttributeType type,
        IReadOnlyList<S57EnumeratedValue>? values = null,
        S57AttributeClass attributeClass = S57AttributeClass.Feature)
    {
        Code = code;
        Acronym = acronym;
        Name = name;
        Type = type;
        Values = values ?? [];
        Class = attributeClass;
    }

    /// <summary>Gets the attribute code, as it's encoded in <c>ATTL</c>.</summary>
    public int Code { get; }

    /// <summary>Gets the attribute acronym, for example <c>COLOUR</c>.</summary>
    /// <remarks>Acronyms are case-sensitive.</remarks>
    public string Acronym { get; }

    /// <summary>Gets the attribute name, for example <c>Colour</c>.</summary>
    public string Name { get; }

    /// <summary>Gets the type of the attribute's value.</summary>
    public S57AttributeType Type { get; }

    /// <summary>
    /// Gets the class of the attribute: feature, national, spatial or cartographic.
    /// </summary>
    /// <remarks>
    /// The class comes from S-57 Edition 3.1, Appendix A, Chapter 2. The AML catalogues don't
    /// classify their attributes (codes 20000 and up), so those attributes are
    /// <see cref="S57AttributeClass.Feature"/>; the IHO attributes the AML catalogues include
    /// keep their IHO class.
    /// </remarks>
    public S57AttributeClass Class { get; }

    /// <summary>
    /// Gets a value indicating whether the attribute is a national attribute, which a feature
    /// record stores in its <c>NATF</c> field rather than its <c>ATTF</c> field (S-57 Part 3,
    /// clause 4.5).
    /// </summary>
    /// <remarks>
    /// National attributes hold text in a national language, such as <c>NOBJNM</c>, and may use
    /// any lexical level; see <see cref="S57AttributeClass.National"/>.
    /// </remarks>
    public bool IsNational => Class == S57AttributeClass.National;

    /// <summary>
    /// Gets the predefined values of an <see cref="S57AttributeType.Enumerated"/> or
    /// <see cref="S57AttributeType.List"/> attribute, in code order.
    /// </summary>
    /// <remarks>
    /// The list is empty for other attribute types, and for enumerated or list attributes whose
    /// values the catalogue doesn't include.
    /// </remarks>
    public IReadOnlyList<S57EnumeratedValue> Values { get; }

    /// <summary>
    /// Gets the predefined value with the specified code.
    /// </summary>
    /// <param name="code">The value's code, as it's encoded in <c>ATVL</c>.</param>
    /// <returns>The value, or <c>null</c> when the attribute has no value with that code.</returns>
    public S57EnumeratedValue? GetValue(int code)
    {
        TryGetValue(code, out var value);
        return value;
    }

    /// <summary>
    /// Tries to get the predefined value with the specified code.
    /// </summary>
    /// <param name="code">The value's code, as it's encoded in <c>ATVL</c>.</param>
    /// <param name="value">The value, or <c>null</c> when the attribute has no value with that code.</param>
    /// <returns><c>true</c> when the value was found; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(int code, [NotNullWhen(true)] out S57EnumeratedValue? value)
    {
        foreach (var candidate in Values)
        {
            if (candidate.Code == code)
            {
                value = candidate;
                return true;
            }
        }

        value = null;
        return false;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Acronym} ({Code}): {Name}";
}
