using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using EncDotNet.Iso8211;

namespace EncDotNet.S57;

/// <summary>
/// Encodes the subfields of one S-57 field at a time, for the S-57 writers.
/// </summary>
/// <remarks>
/// <para>
/// Binary subfields are little-endian (S-57 Part 3, clause 7.2.2.1). Every variable-length
/// subfield is followed by a unit terminator, including the last one in the field, as S-57
/// requires (S-57 Part 3, clause 2.5). The field terminator is added when the record is
/// written.
/// </para>
/// <para>
/// Character data is encoded strictly: a character the subfield's lexical level can't
/// represent throws <see cref="ArgumentException"/> rather than being replaced.
/// </para>
/// </remarks>
internal sealed class S57FieldEncoder
{
    /// <summary>Lexical level 0: ASCII, the IRV of ISO/IEC 646 (S-57 Part 3, clause 2.4).</summary>
    public static readonly Encoding Ascii = Encoding.GetEncoding("us-ascii", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

    /// <summary>Lexical level 1: ISO 8859-1, Latin alphabet 1 (S-57 Part 3, clause 2.4).</summary>
    public static readonly Encoding Latin1 = Encoding.GetEncoding("iso-8859-1", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

    /// <summary>Lexical level 2: UCS-2, little-endian (S-57 Part 3, clause 2.4).</summary>
    public static readonly Encoding Ucs2 = new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true);

    private const byte UnitTerminator = 0x1F;

    private readonly ArrayBufferWriter<byte> _buffer = new(256);
    private string _context = string.Empty;

    /// <summary>
    /// Starts a field, naming the record and field that error messages refer to.
    /// </summary>
    public S57FieldEncoder Begin(string context)
    {
        _buffer.ResetWrittenCount();
        _context = context;
        return this;
    }

    /// <summary>Writes a <c>b11</c> subfield.</summary>
    public S57FieldEncoder B11(long value, string subfield)
    {
        CheckRange(value, byte.MinValue, byte.MaxValue, subfield, "b11");
        _buffer.GetSpan(1)[0] = (byte)value;
        _buffer.Advance(1);
        return this;
    }

    /// <summary>Writes a <c>b12</c> subfield.</summary>
    public S57FieldEncoder B12(long value, string subfield)
    {
        CheckRange(value, ushort.MinValue, ushort.MaxValue, subfield, "b12");
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.GetSpan(2), (ushort)value);
        _buffer.Advance(2);
        return this;
    }

    /// <summary>Writes a <c>b14</c> subfield.</summary>
    public S57FieldEncoder B14(long value, string subfield)
    {
        CheckRange(value, uint.MinValue, uint.MaxValue, subfield, "b14");
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.GetSpan(4), (uint)value);
        _buffer.Advance(4);
        return this;
    }

    /// <summary>Writes a binary <c>LNAM</c> subfield (<c>B(64)</c>, S-57 Part 3, clause 4.3).</summary>
    public S57FieldEncoder Lnam(S57LongName name)
    {
        name.TryWriteLnam(_buffer.GetSpan(S57LongName.LnamLength));
        _buffer.Advance(S57LongName.LnamLength);
        return this;
    }

    /// <summary>Writes a <c>b24</c> subfield.</summary>
    public S57FieldEncoder B24(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.GetSpan(4), value);
        _buffer.Advance(4);
        return this;
    }

    /// <summary>
    /// Writes a variable-length character subfield and its unit terminator.
    /// </summary>
    /// <param name="value">The text; <c>null</c> writes an empty subfield.</param>
    /// <param name="subfield">The subfield name, for error messages.</param>
    /// <param name="encoding">The encoding of the subfield's lexical level.</param>
    public S57FieldEncoder Text(string? value, string subfield, Encoding encoding)
    {
        Encode(value ?? string.Empty, subfield, encoding);
        if (ReferenceEquals(encoding, Ucs2))
        {
            var span = _buffer.GetSpan(2);
            span[0] = UnitTerminator;
            span[1] = 0x00;
            _buffer.Advance(2);
        }
        else
        {
            _buffer.GetSpan(1)[0] = UnitTerminator;
            _buffer.Advance(1);
        }

        return this;
    }

    /// <summary>
    /// Writes a fixed-width <c>A(n)</c> subfield in ASCII, padded on the right with spaces.
    /// </summary>
    public S57FieldEncoder FixedText(string? value, int width, string subfield)
    {
        value ??= string.Empty;
        if (value.Length > width)
        {
            throw Invalid($"{subfield} value '{value}' is longer than its A({width}) format allows");
        }

        Encode(value.PadRight(width), subfield, Ascii);
        return this;
    }

    /// <summary>
    /// Writes a fixed-width <c>R(n)</c> subfield, right-justified with spaces.
    /// </summary>
    public S57FieldEncoder FixedReal(string? value, int width, string subfield)
    {
        value ??= string.Empty;
        if (value.Length > width)
        {
            throw Invalid($"{subfield} value '{value}' is longer than its R({width}) format allows");
        }

        Encode(value.PadLeft(width), subfield, Ascii);
        return this;
    }

    /// <summary>
    /// Writes a fixed-width <c>I(n)</c> subfield as zero-padded decimal digits.
    /// </summary>
    public S57FieldEncoder FixedInteger(long value, int width, string subfield)
    {
        var text = value.ToString(CultureInfo.InvariantCulture);
        if (value < 0 || text.Length > width)
        {
            throw Invalid($"{subfield} value {value} does not fit its I({width}) format");
        }

        Encode(text.PadLeft(width, '0'), subfield, Ascii);
        return this;
    }

    /// <summary>
    /// Completes the field and copies it out.
    /// </summary>
    /// <param name="tag">The field tag.</param>
    /// <param name="twoByteTerminator">
    /// Whether the field is encoded at lexical level 2, whose field terminator is
    /// <c>0x1E 0x00</c>. Its data then ends with the terminator's first byte, and the
    /// ISO 8211 writer adds the second (see <see cref="Iso8211WriterOptions.TwoByteTerminatorFieldTags"/>).
    /// </param>
    public Iso8211Field ToField(string tag, bool twoByteTerminator = false)
    {
        if (twoByteTerminator)
        {
            _buffer.GetSpan(1)[0] = 0x1E;
            _buffer.Advance(1);
        }

        return new Iso8211Field { Tag = tag, Data = _buffer.WrittenSpan.ToArray() };
    }

    /// <summary>
    /// Gets whether <see cref="Text"/> can encode a value with an encoding, without throwing.
    /// </summary>
    /// <remarks>
    /// This makes the same checks as the encoder, so <see cref="S57LexicalLevels"/> and the
    /// writers can't disagree about which lexical level a string needs.
    /// </remarks>
    public static bool CanEncode(string value, Encoding encoding)
    {
        if (ReferenceEquals(encoding, Ucs2))
        {
            return !HasSurrogate(value);
        }

        try
        {
            encoding.GetByteCount(value);
            return true;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    /// <summary>Gets the encoding of a lexical level (S-57 Part 3, clause 2.4).</summary>
    public static Encoding ForLexicalLevel(S57LexicalLevel level) => level switch
    {
        S57LexicalLevel.Ascii => Ascii,
        S57LexicalLevel.Latin1 => Latin1,
        S57LexicalLevel.Ucs2 => Ucs2,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "S-57 defines lexical levels 0, 1 and 2 only.")
    };

    // UCS-2 implementation level 1 covers the Basic Multilingual Plane only, so neither half of
    // a UTF-16 surrogate pair (nor an unpaired surrogate) can be encoded.
    private static bool HasSurrogate(string value) =>
        value.AsSpan().IndexOfAnyInRange('\uD800', '\uDFFF') >= 0;

    private void Encode(string value, string subfield, Encoding encoding)
    {
        if (ReferenceEquals(encoding, Ucs2) && HasSurrogate(value))
        {
            throw Invalid($"{subfield} value '{value}' has a character outside the Basic Multilingual Plane, which UCS-2 (lexical level 2) can't encode");
        }

        try
        {
            var count = encoding.GetByteCount(value);
            var written = encoding.GetBytes(value, _buffer.GetSpan(count));
            _buffer.Advance(written);
        }
        catch (EncoderFallbackException ex)
        {
            throw Invalid($"{subfield} value '{value}' has a character that {encoding.WebName} can't encode", ex);
        }
    }

    private void CheckRange(long value, long min, long max, string subfield, string format)
    {
        if (value < min || value > max)
        {
            throw Invalid($"{subfield} value {value} is outside the range of its {format} format ({min} to {max})");
        }
    }

    private ArgumentException Invalid(string message, Exception? inner = null) =>
        new($"{_context}: {message}.", inner);
}
