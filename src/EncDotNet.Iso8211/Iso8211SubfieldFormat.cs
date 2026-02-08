namespace EncDotNet.Iso8211;

/// <summary>
/// Describes the format of an ISO 8211 subfield.
/// </summary>
/// <remarks>
/// <para>
/// The format type and width determine how subfield data is encoded in data records.
/// Common format types used in S-57 ENC files include:
/// </para>
/// <list type="bullet">
/// <item><description><c>A</c> or <c>A(n)</c> — ASCII character data (variable or fixed length)</description></item>
/// <item><description><c>I(n)</c> — Integer as ASCII digits</description></item>
/// <item><description><c>R(n)</c> — Real number as ASCII</description></item>
/// <item><description><c>b11</c> — Unsigned 8-bit integer (1 byte)</description></item>
/// <item><description><c>b12</c> — Unsigned 16-bit integer (2 bytes)</description></item>
/// <item><description><c>b14</c> — Unsigned 32-bit integer (4 bytes)</description></item>
/// <item><description><c>b21</c> — Signed 8-bit integer (1 byte)</description></item>
/// <item><description><c>b22</c> — Signed 16-bit integer (2 bytes)</description></item>
/// <item><description><c>b24</c> — Signed 32-bit integer (4 bytes)</description></item>
/// </list>
/// </remarks>
public readonly struct Iso8211SubfieldFormat : IEquatable<Iso8211SubfieldFormat>
{
    /// <summary>
    /// Gets the format type of this subfield.
    /// </summary>
    public Iso8211SubfieldFormatType FormatType { get; init; }

    /// <summary>
    /// Gets the width (size) of this subfield in characters or bytes, or 0 for variable-length fields.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For character-based formats (<see cref="Iso8211SubfieldFormatType.CharacterData"/>,
    /// <see cref="Iso8211SubfieldFormatType.Integer"/>, <see cref="Iso8211SubfieldFormatType.Real"/>),
    /// this is the number of characters. A value of 0 indicates variable-length data terminated
    /// by a unit terminator (0x1F) or field terminator (0x1E).
    /// </para>
    /// <para>
    /// For binary formats (<see cref="Iso8211SubfieldFormatType.UnsignedInteger"/>,
    /// <see cref="Iso8211SubfieldFormatType.SignedInteger"/>), this is the number of bytes.
    /// </para>
    /// </remarks>
    public int Width { get; init; }

    /// <summary>
    /// Gets whether this subfield has a fixed width.
    /// </summary>
    public readonly bool IsFixedWidth => Width > 0;

    /// <summary>
    /// Gets whether this subfield has variable length (terminated by a delimiter).
    /// </summary>
    public readonly bool IsVariableLength => Width == 0;

    /// <summary>
    /// Gets the size in bytes of this subfield for binary types.
    /// </summary>
    /// <remarks>
    /// For binary types, this equals <see cref="Width"/>.
    /// For character-based types, this equals <see cref="Width"/> when fixed-length,
    /// or 0 when variable-length.
    /// </remarks>
    public readonly int ByteSize => FormatType switch
    {
        Iso8211SubfieldFormatType.UnsignedInteger => Width,
        Iso8211SubfieldFormatType.SignedInteger => Width,
        Iso8211SubfieldFormatType.BitString => Width,
        _ => Width
    };

    /// <inheritdoc/>
    public readonly bool Equals(Iso8211SubfieldFormat other) =>
        FormatType == other.FormatType && Width == other.Width;

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) =>
        obj is Iso8211SubfieldFormat other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() =>
        HashCode.Combine(FormatType, Width);

    /// <inheritdoc/>
    public override readonly string ToString() => FormatType switch
    {
        Iso8211SubfieldFormatType.CharacterData => Width > 0 ? $"A({Width})" : "A",
        Iso8211SubfieldFormatType.Integer => Width > 0 ? $"I({Width})" : "I",
        Iso8211SubfieldFormatType.Real => Width > 0 ? $"R({Width})" : "R",
        Iso8211SubfieldFormatType.UnsignedInteger => $"b1{Width}",
        Iso8211SubfieldFormatType.SignedInteger => $"b2{Width}",
        Iso8211SubfieldFormatType.BitString => $"B({Width * 8})",
        _ => $"?({Width})"
    };

    /// <summary>
    /// Determines whether two <see cref="Iso8211SubfieldFormat"/> values are equal.
    /// </summary>
    public static bool operator ==(Iso8211SubfieldFormat left, Iso8211SubfieldFormat right) =>
        left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Iso8211SubfieldFormat"/> values are not equal.
    /// </summary>
    public static bool operator !=(Iso8211SubfieldFormat left, Iso8211SubfieldFormat right) =>
        !left.Equals(right);
}
