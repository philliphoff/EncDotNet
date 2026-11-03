namespace EncDotNet.S57;

/// <summary>
/// The S-57 lexical levels, the character sets that the DSSI subfields AALL (for ATTF) and NALL
/// (for NATF) declare (S-57 Edition 3.1, Part 3, clause 2.4).
/// </summary>
/// <remarks>
/// The values are the exact AALL and NALL codes, so cast between this enum and
/// <see cref="S57DataSetIdentification.AttfLexicalLevel"/> or
/// <see cref="S57DataSetIdentification.NatfLexicalLevel"/> in either direction. ATTF allows
/// levels 0 and 1 only; NATF allows all three. Use <see cref="S57LexicalLevels.Lowest"/> to pick
/// the lowest level that can encode a set of values.
/// </remarks>
public enum S57LexicalLevel
{
    /// <summary>Level 0: ASCII, the IRV of ISO/IEC 646, which S-57 calls the basic character set.</summary>
    Ascii = 0,

    /// <summary>Level 1: ISO 8859-1, Latin alphabet 1.</summary>
    Latin1 = 1,

    /// <summary>
    /// Level 2: UCS-2 (ISO/IEC 10646 implementation level 1), little-endian, which covers the
    /// Basic Multilingual Plane only.
    /// </summary>
    Ucs2 = 2
}
