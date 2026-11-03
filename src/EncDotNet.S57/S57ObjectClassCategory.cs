namespace EncDotNet.S57;

/// <summary>
/// The category of an S-57 object class.
/// </summary>
/// <remarks>
/// S-57 Edition 3.1, Part 3 §7.5.1.1 marks each object class in the data dictionary as a meta
/// (<c>M</c>), cartographic (<c>$</c>), geo (<c>G</c>) or collection (<c>C</c>) object.
/// </remarks>
public enum S57ObjectClassCategory
{
    /// <summary>The category isn't known.</summary>
    Unknown = 0,

    /// <summary>A geo object, which carries the descriptive characteristics of a real-world entity.</summary>
    Geo,

    /// <summary>A meta object, which carries information about other objects, such as <c>M_COVR</c>.</summary>
    Meta,

    /// <summary>A collection object, which describes relationships between other objects, such as <c>C_AGGR</c>.</summary>
    Collection,

    /// <summary>A cartographic object, which carries cartographic information, such as <c>$TEXTS</c>.</summary>
    Cartographic,
}
