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
        IReadOnlyList<S57EnumeratedValue>? values = null)
    {
        Code = code;
        Acronym = acronym;
        Name = name;
        Type = type;
        Values = values ?? [];
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
