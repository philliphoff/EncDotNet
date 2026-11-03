namespace EncDotNet.S57;

/// <summary>
/// The type of an S-57 attribute's value, as given in an object catalogue.
/// </summary>
/// <remarks>
/// S-57 Edition 3.1, Appendix A, Chapter 2 §2.1 gives each attribute one of six types, written as
/// the letters <c>E</c>, <c>L</c>, <c>F</c>, <c>I</c>, <c>A</c> and <c>S</c>. Each member's value
/// is the code of that letter.
/// </remarks>
public enum S57AttributeType
{
    /// <summary>The type isn't known.</summary>
    Unknown = 0,

    /// <summary>Enumerated (<c>E</c>): one value from a list of predefined values.</summary>
    Enumerated = 'E',

    /// <summary>List (<c>L</c>): one or more values from a list of predefined values.</summary>
    List = 'L',

    /// <summary>Float (<c>F</c>): a floating point number with defined range, resolution, units and format.</summary>
    Float = 'F',

    /// <summary>Integer (<c>I</c>): an integer with defined range, units and format.</summary>
    Integer = 'I',

    /// <summary>Coded string (<c>A</c>): a string of ASCII characters in a predefined format.</summary>
    CodedString = 'A',

    /// <summary>Free text (<c>S</c>): a free-format alphanumeric string.</summary>
    FreeText = 'S',
}
