namespace EncDotNet.S57;

/// <summary>
/// The class of an S-57 attribute, as given in an object catalogue.
/// </summary>
/// <remarks>
/// <para>
/// S-57 Edition 3.1, Appendix A, Chapter 2 groups attributes into feature object attributes
/// (§2.2), national language attributes (§2.3) and spatial and meta object attributes (§2.4).
/// The catalogue tables mark the feature object attributes used for cartographic presentation,
/// whose acronyms start with <c>$</c>, as a fourth class.
/// </para>
/// <para>
/// The class decides where a feature record stores an attribute: national attributes are
/// encoded in the <c>NATF</c> field, at the data set's national lexical level (<c>NALL</c>), and
/// the other attributes of a feature in the <c>ATTF</c> field (S-57 Part 3, clauses 4.4, 4.5,
/// 7.6.3 and 7.6.4). Spatial attributes are encoded in the <c>ATTV</c> field of a vector record
/// (S-57 Part 3, clauses 5.1.2 and 7.7.1.2).
/// </para>
/// </remarks>
public enum S57AttributeClass
{
    /// <summary>
    /// A feature object attribute (<c>F</c>), such as <c>COLOUR</c>, encoded in <c>ATTF</c>.
    /// </summary>
    Feature = 0,

    /// <summary>
    /// A national language attribute (<c>N</c>), such as <c>NOBJNM</c>, encoded in <c>NATF</c>.
    /// </summary>
    National,

    /// <summary>
    /// A spatial object attribute (<c>S</c>), such as <c>POSACC</c>, encoded in a vector
    /// record's <c>ATTV</c>.
    /// </summary>
    Spatial,

    /// <summary>
    /// A cartographic attribute (<c>$</c>), such as <c>$SCODE</c>, encoded in <c>ATTF</c>.
    /// </summary>
    Cartographic,
}
