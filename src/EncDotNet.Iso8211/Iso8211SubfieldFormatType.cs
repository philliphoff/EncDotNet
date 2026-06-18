namespace EncDotNet.Iso8211;

/// <summary>
/// Specifies the format type of an ISO 8211 subfield.
/// </summary>
public enum Iso8211SubfieldFormatType : byte
{
    /// <summary>
    /// Character (alphanumeric) data. Corresponds to format code <c>A</c>.
    /// </summary>
    CharacterData = 0,

    /// <summary>
    /// Integer data encoded as ASCII digits. Corresponds to format code <c>I</c>.
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Real (floating-point) data encoded as ASCII. Corresponds to format code <c>R</c>.
    /// </summary>
    Real = 2,

    /// <summary>
    /// Unsigned binary integer data. Corresponds to format code <c>b1x</c> where x is the byte width.
    /// </summary>
    UnsignedInteger = 3,

    /// <summary>
    /// Signed binary integer data. Corresponds to format code <c>b2x</c> where x is the byte width.
    /// </summary>
    SignedInteger = 4,

    /// <summary>
    /// Bit string data. Corresponds to format code <c>B(n)</c> where n is the width in bits.
    /// The <see cref="Iso8211SubfieldFormat.Width"/> is stored in bytes (n / 8).
    /// </summary>
    BitString = 5,

    /// <summary>
    /// Floating-point binary (IEEE 754) data. Corresponds to format codes <c>b4x</c> and
    /// <c>b5x</c> where x is the byte width (4 for single precision, 8 for double precision).
    /// Used, for example, by the S-100 DSSI origin-shift subfields (<c>b48</c>).
    /// </summary>
    FloatingPoint = 6
}
