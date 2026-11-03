using System.Diagnostics.CodeAnalysis;

namespace EncDotNet.S57;

/// <summary>
/// Looks up S-57 object classes and attributes by code or acronym in an object catalogue.
/// </summary>
/// <remarks>
/// <para>
/// An S-57 data set encodes object classes (<c>OBJL</c>) and attributes (<c>ATTL</c>) as numbers.
/// The product the data set belongs to decides which catalogue gives those numbers their meaning.
/// Use <see cref="For(S57DataSetIdentification)"/> to choose the catalogue for a data set, or one of
/// the built-in catalogues directly:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="Iho"/>: the IHO S-57 Edition 3.1 Object Catalogue (Appendix A).</description></item>
/// <item><description><see cref="InlandEnc"/>: the IHO catalogue plus the Inland ENC classes and attributes, codes 17000 and up.</description></item>
/// </list>
/// <para>
/// A catalogue holds codes, acronyms, names, attribute types and enumerated values. It doesn't hold
/// definitions or the attributes each class may carry. The built-in catalogues are built the first
/// time they're used.
/// </para>
/// </remarks>
public sealed partial class S57ObjectCatalogue
{
    /// <summary>
    /// The product specification code (<c>PRSP</c>) that Inland ENC producers declare.
    /// </summary>
    private const int InlandEncProductSpecification = 10;

    private static readonly Lazy<S57ObjectCatalogue> LazyIho =
        new(() => new("IHO S-57 Edition 3.1", null, CreateIhoObjectClasses(), CreateIhoAttributes()));

    private static readonly Lazy<S57ObjectCatalogue> LazyInlandEnc =
        new(() => new("Inland ENC", Iho, CreateInlandEncObjectClasses(), CreateInlandEncAttributes()));

    private readonly Dictionary<int, S57ObjectClassDefinition> _objectClassesByCode;
    private readonly Dictionary<string, S57ObjectClassDefinition> _objectClassesByAcronym;
    private readonly Dictionary<int, S57AttributeDefinition> _attributesByCode;
    private readonly Dictionary<string, S57AttributeDefinition> _attributesByAcronym;

    /// <summary>
    /// Creates a catalogue from a base catalogue and the entries a product adds to it.
    /// </summary>
    /// <param name="name">The catalogue name.</param>
    /// <param name="baseCatalogue">The catalogue the product extends, or <c>null</c>.</param>
    /// <param name="objectClasses">The product's object classes. A class replaces a base class with the same code.</param>
    /// <param name="attributes">The product's attributes. An attribute replaces a base attribute with the same code.</param>
    internal S57ObjectCatalogue(
        string name,
        S57ObjectCatalogue? baseCatalogue,
        IEnumerable<S57ObjectClassDefinition> objectClasses,
        IEnumerable<S57AttributeDefinition> attributes)
    {
        Name = name;
        _objectClassesByCode = baseCatalogue is null ? [] : new(baseCatalogue._objectClassesByCode);
        _attributesByCode = baseCatalogue is null ? [] : new(baseCatalogue._attributesByCode);

        foreach (var objectClass in objectClasses)
        {
            _objectClassesByCode[objectClass.Code] = objectClass;
        }

        foreach (var attribute in attributes)
        {
            _attributesByCode[attribute.Code] = attribute;
        }

        _objectClassesByAcronym = IndexByAcronym(_objectClassesByCode.Values, c => c.Acronym);
        _attributesByAcronym = IndexByAcronym(_attributesByCode.Values, a => a.Acronym);
    }

    /// <summary>
    /// Gets the IHO S-57 Edition 3.1 Object Catalogue.
    /// </summary>
    /// <remarks>
    /// It holds the object classes and attributes of S-57 Edition 3.1, Appendix A, including those
    /// added by its supplements, such as <c>ARCSLN</c> and <c>CAT_TS</c>.
    /// </remarks>
    public static S57ObjectCatalogue Iho => LazyIho.Value;

    /// <summary>
    /// Gets the Inland ENC catalogue: the <see cref="Iho"/> catalogue plus the Inland ENC object
    /// classes and attributes, codes 17000 and up.
    /// </summary>
    /// <remarks>
    /// The Inland ENC entries give codes, acronyms, names and attribute types. They don't include
    /// the enumerated values of Inland ENC attributes, so <see cref="S57AttributeDefinition.Values"/>
    /// is empty for them.
    /// </remarks>
    public static S57ObjectCatalogue InlandEnc => LazyInlandEnc.Value;

    /// <summary>Gets the catalogue name, for example <c>IHO S-57 Edition 3.1</c>.</summary>
    public string Name { get; }

    /// <summary>Gets every object class in the catalogue.</summary>
    public IReadOnlyCollection<S57ObjectClassDefinition> ObjectClasses => _objectClassesByCode.Values;

    /// <summary>Gets every attribute in the catalogue.</summary>
    public IReadOnlyCollection<S57AttributeDefinition> Attributes => _attributesByCode.Values;

    /// <summary>
    /// Chooses the catalogue for a data set from its product specification.
    /// </summary>
    /// <param name="identification">The data set's identification (<c>DSID</c>) field.</param>
    /// <returns>
    /// <see cref="InlandEnc"/> when the product specification (<c>PRSP</c>) is 10; otherwise,
    /// <see cref="Iho"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="identification"/> is <c>null</c>.</exception>
    public static S57ObjectCatalogue For(S57DataSetIdentification identification)
    {
        ArgumentNullException.ThrowIfNull(identification);

        return identification.ProductSpecification switch
        {
            InlandEncProductSpecification => InlandEnc,
            _ => Iho,
        };
    }

    /// <summary>
    /// Gets the object class with the specified code.
    /// </summary>
    /// <param name="code">The object class code (<c>OBJL</c>).</param>
    /// <returns>The object class, or <c>null</c> when the catalogue has no class with that code.</returns>
    public S57ObjectClassDefinition? GetObjectClass(int code) =>
        _objectClassesByCode.GetValueOrDefault(code);

    /// <summary>
    /// Gets the object class with the specified code.
    /// </summary>
    /// <param name="code">The object class code (<c>OBJL</c>).</param>
    /// <returns>The object class, or <c>null</c> when the catalogue has no class with that code.</returns>
    public S57ObjectClassDefinition? GetObjectClass(S57ObjectCode code) => GetObjectClass((int)code);

    /// <summary>
    /// Gets the object class with the specified acronym.
    /// </summary>
    /// <param name="acronym">The object class acronym, for example <c>LIGHTS</c>. The match is case-sensitive.</param>
    /// <returns>The object class, or <c>null</c> when the catalogue has no class with that acronym.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="acronym"/> is <c>null</c>.</exception>
    public S57ObjectClassDefinition? GetObjectClass(string acronym)
    {
        ArgumentNullException.ThrowIfNull(acronym);
        return _objectClassesByAcronym.GetValueOrDefault(acronym);
    }

    /// <summary>
    /// Tries to get the object class with the specified code.
    /// </summary>
    /// <param name="code">The object class code (<c>OBJL</c>).</param>
    /// <param name="objectClass">The object class, or <c>null</c> when the catalogue has no class with that code.</param>
    /// <returns><c>true</c> when the object class was found; otherwise, <c>false</c>.</returns>
    public bool TryGetObjectClass(int code, [NotNullWhen(true)] out S57ObjectClassDefinition? objectClass) =>
        _objectClassesByCode.TryGetValue(code, out objectClass);

    /// <summary>
    /// Tries to get the object class with the specified acronym.
    /// </summary>
    /// <param name="acronym">The object class acronym, for example <c>LIGHTS</c>. The match is case-sensitive.</param>
    /// <param name="objectClass">The object class, or <c>null</c> when the catalogue has no class with that acronym.</param>
    /// <returns><c>true</c> when the object class was found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="acronym"/> is <c>null</c>.</exception>
    public bool TryGetObjectClass(string acronym, [NotNullWhen(true)] out S57ObjectClassDefinition? objectClass)
    {
        ArgumentNullException.ThrowIfNull(acronym);
        return _objectClassesByAcronym.TryGetValue(acronym, out objectClass);
    }

    /// <summary>
    /// Gets the attribute with the specified code.
    /// </summary>
    /// <param name="code">The attribute code (<c>ATTL</c>).</param>
    /// <returns>The attribute, or <c>null</c> when the catalogue has no attribute with that code.</returns>
    public S57AttributeDefinition? GetAttribute(int code) => _attributesByCode.GetValueOrDefault(code);

    /// <summary>
    /// Gets the attribute with the specified acronym.
    /// </summary>
    /// <param name="acronym">The attribute acronym, for example <c>COLOUR</c>. The match is case-sensitive.</param>
    /// <returns>The attribute, or <c>null</c> when the catalogue has no attribute with that acronym.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="acronym"/> is <c>null</c>.</exception>
    public S57AttributeDefinition? GetAttribute(string acronym)
    {
        ArgumentNullException.ThrowIfNull(acronym);
        return _attributesByAcronym.GetValueOrDefault(acronym);
    }

    /// <summary>
    /// Tries to get the attribute with the specified code.
    /// </summary>
    /// <param name="code">The attribute code (<c>ATTL</c>).</param>
    /// <param name="attribute">The attribute, or <c>null</c> when the catalogue has no attribute with that code.</param>
    /// <returns><c>true</c> when the attribute was found; otherwise, <c>false</c>.</returns>
    public bool TryGetAttribute(int code, [NotNullWhen(true)] out S57AttributeDefinition? attribute) =>
        _attributesByCode.TryGetValue(code, out attribute);

    /// <summary>
    /// Tries to get the attribute with the specified acronym.
    /// </summary>
    /// <param name="acronym">The attribute acronym, for example <c>COLOUR</c>. The match is case-sensitive.</param>
    /// <param name="attribute">The attribute, or <c>null</c> when the catalogue has no attribute with that acronym.</param>
    /// <returns><c>true</c> when the attribute was found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="acronym"/> is <c>null</c>.</exception>
    public bool TryGetAttribute(string acronym, [NotNullWhen(true)] out S57AttributeDefinition? attribute)
    {
        ArgumentNullException.ThrowIfNull(acronym);
        return _attributesByAcronym.TryGetValue(acronym, out attribute);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    private static Dictionary<string, T> IndexByAcronym<T>(IEnumerable<T> entries, Func<T, string> acronym)
    {
        var index = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            index[acronym(entry)] = entry;
        }

        return index;
    }
}
