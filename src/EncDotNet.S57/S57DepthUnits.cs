namespace EncDotNet.S57;

/// <summary>
/// The depth units of the DSPM subfield DUNI, which takes its values from the attribute
/// <c>DUNITS</c> (S-57 Edition 3.1, Part 3 §7.3.2.1; Appendix A, chapter 2, attribute 89).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetParameters.DepthUnits"/> (or a <c>DUNITS</c> attribute value) in either
/// direction. A code S-57 doesn't define casts to an unnamed value rather than failing.
/// </remarks>
public enum S57DepthUnits
{
    /// <summary>Metres (code 1).</summary>
    Metres = 1,

    /// <summary>Fathoms and feet (code 2).</summary>
    FathomsAndFeet = 2,

    /// <summary>Feet (code 3).</summary>
    Feet = 3,

    /// <summary>Fathoms and fractions of fathoms (code 4).</summary>
    FathomsAndFractions = 4
}
