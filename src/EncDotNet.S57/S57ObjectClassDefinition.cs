namespace EncDotNet.S57;

/// <summary>
/// Describes an S-57 object class in an <see cref="S57ObjectCatalogue"/>.
/// </summary>
public sealed class S57ObjectClassDefinition
{
    internal S57ObjectClassDefinition(
        int code,
        string acronym,
        string name,
        S57ObjectClassCategory category,
        S57PermittedPrimitives primitives)
    {
        Code = code;
        Acronym = acronym;
        Name = name;
        Category = category;
        Primitives = primitives;
    }

    /// <summary>Gets the object class code, as it's encoded in a feature record's <c>OBJL</c>.</summary>
    public int Code { get; }

    /// <summary>Gets the object class acronym, for example <c>LIGHTS</c>.</summary>
    /// <remarks>
    /// Acronyms are case-sensitive. Some product catalogues use lowercase acronyms for their own
    /// classes; Inland ENC, for example, has both <c>ACHBRT</c> and <c>achbrt</c>.
    /// </remarks>
    public string Acronym { get; }

    /// <summary>Gets the object class name, for example <c>Light</c>.</summary>
    public string Name { get; }

    /// <summary>Gets whether the class is a geo, meta, collection or cartographic object.</summary>
    public S57ObjectClassCategory Category { get; }

    /// <summary>
    /// Gets the geometric primitives the class may be encoded with, or
    /// <see cref="S57PermittedPrimitives.None"/> when it has none or the catalogue doesn't say.
    /// </summary>
    public S57PermittedPrimitives Primitives { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{Acronym} ({Code}): {Name}";
}
