namespace EncDotNet.S57;

/// <summary>
/// The height and length units of the DSPM subfield HUNI, which takes its values from the
/// attribute <c>HUNITS</c> (S-57 Edition 3.1, Part 3 §7.3.2.1; Appendix A, chapter 2,
/// attribute 96).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetParameters.HeightUnits"/> (or an <c>HUNITS</c> attribute value) in either
/// direction. A code S-57 doesn't define casts to an unnamed value rather than failing.
/// </remarks>
public enum S57HeightUnits
{
    /// <summary>Metres (code 1).</summary>
    Metres = 1,

    /// <summary>Feet (code 2).</summary>
    Feet = 2
}
