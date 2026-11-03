namespace EncDotNet.S57;

/// <summary>
/// The positional accuracy units of the DSPM subfield PUNI, which takes its values from the
/// attribute <c>PUNITS</c> (S-57 Edition 3.1, Part 3 §7.3.2.1; Appendix A, chapter 2,
/// attribute 189).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetParameters.PositionalUnits"/> (or a <c>PUNITS</c> attribute value) in
/// either direction. A code S-57 doesn't define casts to an unnamed value rather than failing.
/// </remarks>
public enum S57PositionalAccuracyUnits
{
    /// <summary>Metres (code 1).</summary>
    Metres = 1,

    /// <summary>Degrees of arc (code 2).</summary>
    DegreesOfArc = 2,

    /// <summary>Millimetres (code 3).</summary>
    Millimetres = 3,

    /// <summary>Feet (code 4).</summary>
    Feet = 4,

    /// <summary>Cables (code 5).</summary>
    Cables = 5
}
