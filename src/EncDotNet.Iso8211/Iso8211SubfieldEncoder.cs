using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Encodes individual ISO 8211 subfield values into their binary representation.
/// </summary>
/// <remarks>
/// <para>
/// This type is the symmetric inverse of the subfield conversion performed by
/// <see cref="Iso8211FieldReader"/>. Each method encodes a single subfield value according to
/// an <see cref="Iso8211SubfieldFormat"/>; unit and field terminators are not written here —
/// they are appended by the field/record writers.
/// </para>
/// </remarks>
internal static class Iso8211SubfieldEncoder
{
    /// <summary>
    /// Encodes a single subfield value according to the supplied format.
    /// </summary>
    /// <param name="value">The value to encode. Accepted CLR types depend on the format type.</param>
    /// <param name="format">The subfield format describing how to encode the value.</param>
    /// <param name="options">The writer options controlling encoding and terminator width.</param>
    /// <returns>The encoded subfield bytes, excluding any terminator.</returns>
    public static byte[] Encode(object? value, Iso8211SubfieldFormat format, Iso8211WriterOptions options)
    {
        return format.FormatType switch
        {
            Iso8211SubfieldFormatType.CharacterData => EncodeCharacter(value, format, options),
            Iso8211SubfieldFormatType.Integer => EncodeAsciiInteger(value, format),
            Iso8211SubfieldFormatType.Real => EncodeAsciiReal(value, format),
            Iso8211SubfieldFormatType.UnsignedInteger => EncodeUnsignedBinary(value, format.Width),
            Iso8211SubfieldFormatType.SignedInteger => EncodeSignedBinary(value, format.Width),
            Iso8211SubfieldFormatType.FloatingPoint => EncodeFloatingBinary(value, format.Width),
            Iso8211SubfieldFormatType.BitString => EncodeBitString(value, format.Width),
            _ => throw new InvalidOperationException($"Unknown subfield format type: {format.FormatType}.")
        };
    }

    private static byte[] EncodeCharacter(object? value, Iso8211SubfieldFormat format, Iso8211WriterOptions options)
    {
        var text = value switch
        {
            null => string.Empty,
            string s => s,
            byte[] raw => options.EffectiveEncoding.GetString(raw),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

        var encoding = options.EffectiveEncoding;
        var bytes = encoding.GetBytes(text);

        if (!format.IsFixedWidth)
        {
            return bytes;
        }

        // Fixed-width character data: pad with spaces or truncate to the exact width (in code units).
        var charWidth = options.LexicalLevel >= 2 ? 2 : 1;
        var targetBytes = format.Width * charWidth;
        return FitToWidth(bytes, targetBytes, PadByte(encoding, charWidth));
    }

    private static byte[] EncodeAsciiInteger(object? value, Iso8211SubfieldFormat format)
    {
        var number = ToInt64(value);
        string text;

        if (format.IsFixedWidth)
        {
            // Right-justify within the fixed width. The width includes any leading sign, so a
            // negative value is encoded as sign + zero-padded magnitude (e.g. -5 in I(4) => "-005").
            if (number < 0)
            {
                var magnitude = (-number).ToString(CultureInfo.InvariantCulture);
                text = "-" + magnitude.PadLeft(format.Width - 1, '0');
            }
            else
            {
                text = number.ToString(CultureInfo.InvariantCulture).PadLeft(format.Width, '0');
            }

            if (text.Length > format.Width)
            {
                throw new InvalidOperationException(
                    $"Integer value {number} does not fit in an I({format.Width}) subfield.");
            }
        }
        else
        {
            text = number.ToString(CultureInfo.InvariantCulture);
        }

        return Encoding.ASCII.GetBytes(text);
    }

    private static byte[] EncodeAsciiReal(object? value, Iso8211SubfieldFormat format)
    {
        var text = value switch
        {
            null => string.Empty,
            string s => s,
            IFormattable f => f.ToString("R", CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

        if (!format.IsFixedWidth)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        if (text.Length > format.Width)
        {
            throw new InvalidOperationException(
                $"Real value '{text}' does not fit in an R({format.Width}) subfield.");
        }

        // Right-justify within the fixed width, padding with spaces (the reader trims).
        var bytes = Encoding.ASCII.GetBytes(text.PadLeft(format.Width, ' '));
        return bytes;
    }

    private static byte[] EncodeUnsignedBinary(object? value, int width)
    {
        if (width <= 0)
        {
            throw new InvalidOperationException("Unsigned binary subfields require a fixed byte width.");
        }

        var raw = unchecked((ulong)ToInt64(value));
        return WriteLittleEndian(raw, width);
    }

    private static byte[] EncodeSignedBinary(object? value, int width)
    {
        if (width <= 0)
        {
            throw new InvalidOperationException("Signed binary subfields require a fixed byte width.");
        }

        var raw = unchecked((ulong)ToInt64(value));
        return WriteLittleEndian(raw, width);
    }

    private static byte[] EncodeFloatingBinary(object? value, int width)
    {
        var d = ToDouble(value);
        switch (width)
        {
            case 4:
                {
                    var bytes = new byte[4];
                    BinaryPrimitives.WriteSingleLittleEndian(bytes, (float)d);
                    return bytes;
                }
            case 8:
                {
                    var bytes = new byte[8];
                    BinaryPrimitives.WriteDoubleLittleEndian(bytes, d);
                    return bytes;
                }
            default:
                throw new InvalidOperationException($"Unsupported floating-point binary width: {width} byte(s).");
        }
    }

    private static byte[] EncodeBitString(object? value, int width)
    {
        switch (value)
        {
            case byte[] raw:
                if (width <= 0)
                {
                    return (byte[])raw.Clone();
                }
                return FitToWidth(raw, width, 0);
            case string hex:
                {
                    var raw = Convert.FromHexString(hex);
                    return width <= 0 ? raw : FitToWidth(raw, width, 0);
                }
            case null:
                return width <= 0 ? Array.Empty<byte>() : new byte[width];
            default:
                // Treat as an integer packed little-endian into the declared byte width.
                if (width <= 0)
                {
                    throw new InvalidOperationException("Bit string subfields require a fixed byte width for integer values.");
                }
                return WriteLittleEndian(unchecked((ulong)ToInt64(value)), width);
        }
    }

    private static byte[] WriteLittleEndian(ulong value, int width)
    {
        var bytes = new byte[width];
        for (int i = 0; i < width; i++)
        {
            bytes[i] = (byte)(value >> (i * 8));
        }
        return bytes;
    }

    private static byte[] FitToWidth(byte[] source, int width, byte pad)
    {
        if (source.Length == width)
        {
            return source;
        }

        var result = new byte[width];
        if (source.Length > width)
        {
            Array.Copy(source, result, width);
        }
        else
        {
            Array.Copy(source, result, source.Length);
            for (int i = source.Length; i < width; i++)
            {
                result[i] = pad;
            }
        }
        return result;
    }

    private static byte PadByte(Encoding encoding, int charWidth)
    {
        // The space padding byte; for single-byte encodings this is 0x20.
        var spaceBytes = encoding.GetBytes(" ");
        return spaceBytes.Length > 0 ? spaceBytes[0] : (byte)0x20;
    }

    private static long ToInt64(object? value) => value switch
    {
        null => 0L,
        long l => l,
        int i => i,
        uint ui => ui,
        ulong ul => unchecked((long)ul),
        short s => s,
        ushort us => us,
        byte b => b,
        sbyte sb => sb,
        string str => long.Parse(str.Trim(), CultureInfo.InvariantCulture),
        bool boolean => boolean ? 1L : 0L,
        IConvertible c => c.ToInt64(CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Cannot encode value of type {value.GetType().Name} as an integer.")
    };

    private static double ToDouble(object? value) => value switch
    {
        null => 0d,
        double d => d,
        float f => f,
        decimal m => (double)m,
        string str => double.Parse(str.Trim(), CultureInfo.InvariantCulture),
        IConvertible c => c.ToDouble(CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Cannot encode value of type {value.GetType().Name} as a floating-point number.")
    };
}
