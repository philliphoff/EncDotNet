using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;

using EncDotNet.Iso8211;

namespace EncDotNet.UnitTests.Reference;

/// <summary>
/// A frozen copy of <see cref="Iso8211FieldReader"/> as it was before its performance rewrite,
/// used only as the reference that the differential tests compare the current reader against.
/// </summary>
internal sealed class LegacyIso8211FieldReader
{
    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    private readonly Iso8211FieldDefinition _fieldDefinition;
    private readonly byte[] _data;
    private readonly ParsedSubfield[] _parsedSubfields;
    private readonly int _groupCount;
    private readonly int _lexicalLevel;

    public LegacyIso8211FieldReader(Iso8211FieldDefinition fieldDefinition, ReadOnlySpan<byte> data, int lexicalLevel = 0)
    {
        _fieldDefinition = fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition));
        _data = data.ToArray();
        _lexicalLevel = lexicalLevel;
        (_parsedSubfields, _groupCount) = ParseSubfields();
    }

    public LegacyIso8211FieldReader(Iso8211FieldDefinition fieldDefinition, byte[] data, int lexicalLevel = 0)
    {
        _fieldDefinition = fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition));
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _lexicalLevel = lexicalLevel;
        (_parsedSubfields, _groupCount) = ParseSubfields();
    }

    public Iso8211FieldDefinition FieldDefinition => _fieldDefinition;

    public ReadOnlySpan<byte> Data => _data;

    public int SubfieldCount => _parsedSubfields.Length;

    public int GroupCount => _groupCount;

    public bool HasRepeatingGroups => _fieldDefinition.HasRepeatingGroup && _groupCount > 1;

    public T GetSubfield<T>(string name)
    {
        if (!TryGetSubfield<T>(name, out var value))
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field '{_fieldDefinition.Tag}'.");
        }
        return value;
    }

    public bool TryGetSubfield<T>(string name, out T value)
    {
        var subfieldDef = _fieldDefinition.GetSubfieldDefinition(name);
        if (subfieldDef is null)
        {
            value = default!;
            return false;
        }

        // Find the first parsed subfield matching this definition
        foreach (var parsed in _parsedSubfields)
        {
            if (parsed.DefinitionIndex == subfieldDef.Index)
            {
                value = ConvertValue<T>(parsed, subfieldDef);
                return true;
            }
        }

        value = default!;
        return false;
    }

    public ImmutableArray<T> GetSubfieldValues<T>(string name)
    {
        var subfieldDef = _fieldDefinition.GetSubfieldDefinition(name);
        if (subfieldDef is null)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field definition '{_fieldDefinition.Tag}'.");
        }

        var builder = ImmutableArray.CreateBuilder<T>();

        foreach (var parsed in _parsedSubfields)
        {
            if (parsed.DefinitionIndex == subfieldDef.Index)
            {
                builder.Add(ConvertValue<T>(parsed, subfieldDef));
            }
        }

        return builder.ToImmutable();
    }

    public T GetSubfieldAt<T>(int index)
    {
        if (index < 0 || index >= _parsedSubfields.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var parsed = _parsedSubfields[index];
        var subfieldDef = _fieldDefinition.SubfieldDefinitions[parsed.DefinitionIndex];
        return ConvertValue<T>(parsed, subfieldDef);
    }

    public ReadOnlySpan<byte> GetSubfieldBytes(string name)
    {
        var subfieldDef = _fieldDefinition.GetSubfieldDefinition(name);
        if (subfieldDef is null)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field '{_fieldDefinition.Tag}'.");
        }

        foreach (var parsed in _parsedSubfields)
        {
            if (parsed.DefinitionIndex == subfieldDef.Index)
            {
                return _data.AsSpan(parsed.Offset, parsed.Length);
            }
        }

        throw new KeyNotFoundException($"Subfield '{name}' not found in parsed field data.");
    }

    public ReadOnlySpan<byte> GetSubfieldBytesAt(int index)
    {
        if (index < 0 || index >= _parsedSubfields.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var parsed = _parsedSubfields[index];
        return _data.AsSpan(parsed.Offset, parsed.Length);
    }

    private (ParsedSubfield[] subfields, int groupCount) ParseSubfields()
    {
        var subfields = _fieldDefinition.SubfieldDefinitions;
        if (subfields.Count == 0)
        {
            return ([], 0);
        }

        // Pre-size: for repeating groups, estimate ~4 bytes per subfield
        int initialCapacity = subfields.Count;
        if (_fieldDefinition.HasRepeatingGroup && _data.Length > 0)
        {
            initialCapacity = Math.Max(initialCapacity, _data.Length / 4);
        }

        var parsed = new List<ParsedSubfield>(initialCapacity);
        var data = _data.AsSpan();
        int offset = 0;
        int subfieldIndex = 0;
        int groupCount = 0;
        bool inRepeatingGroup = false;

        while (offset < data.Length)
        {
            var subfieldDef = subfields[subfieldIndex];
            var format = subfieldDef.Format;

            // For variable-length subfields, check if we've hit a field terminator.
            // Fixed-width binary subfields may contain 0x1E (30) as valid data, so we
            // must not treat it as a terminator.
            if (!format.IsFixedWidth && IsFieldTerminator(data, offset))
            {
                break;
            }

            // For fixed-width subfields in repeating groups, check if we've hit the end:
            // If the remaining data is just the field terminator, we're done.
            if (format.IsFixedWidth && _fieldDefinition.HasRepeatingGroup)
            {
                // If remaining bytes < expected width AND the first remaining byte is FT, stop
                int remaining = data.Length - offset;
                if (remaining < format.Width && remaining > 0 && data[offset] == FieldTerminator)
                {
                    break;
                }
            }

            // Calculate the length of this subfield
            int length = CalculateSubfieldLength(data, offset, format);

            parsed.Add(new ParsedSubfield
            {
                DefinitionIndex = subfieldIndex,
                Offset = offset,
                Length = length,
                GroupIndex = groupCount
            });

            offset += length;

            // Skip unit terminator if present - only for variable-length subfields
            // Fixed-width binary subfields (b11, b12, b14, b21, b22, b24) don't have UTs
            if (!format.IsFixedWidth)
            {
                if (IsUnitTerminator(data, offset))
                {
                    offset += TerminatorWidth;
                }
                else if (IsFieldTerminator(data, offset))
                {
                    // Field terminator acts as the final unit terminator; don't advance
                    // past it so the next iteration's FT check can break the loop.
                }
            }

            // Advance to next subfield definition
            subfieldIndex++;

            // Handle repeating groups
            if (subfieldIndex >= subfields.Count)
            {
                if (_fieldDefinition.HasRepeatingGroup)
                {
                    // Reset to the start of the repeating group
                    subfieldIndex = _fieldDefinition.RepeatingSubfieldStartIndex;
                    groupCount++;
                    inRepeatingGroup = true;
                }
                else
                {
                    // No repeating group, we're done with definitions
                    break;
                }
            }
            else if (!inRepeatingGroup && subfieldIndex == _fieldDefinition.RepeatingSubfieldStartIndex && _fieldDefinition.HasRepeatingGroup)
            {
                // Just entered the repeating group for the first time.
                // Set to 0; the wrap-around at line ~424 will increment to 1
                // after the first complete group is parsed.
                groupCount = 0;
                inRepeatingGroup = true;
            }
        }

        // If we never entered a repeating group but have subfields, we have 1 group
        if (!inRepeatingGroup && parsed.Count > 0)
        {
            groupCount = 1;
        }

        return (parsed.ToArray(), groupCount);
    }

    private int TerminatorWidth => _lexicalLevel >= 2 ? 2 : 1;

    private bool IsFieldTerminator(ReadOnlySpan<byte> data, int offset)
    {
        if (offset >= data.Length) return false;
        if (data[offset] != FieldTerminator) return false;
        if (_lexicalLevel >= 2)
            return offset + 1 < data.Length && data[offset + 1] == 0x00;
        return true;
    }

    private bool IsUnitTerminator(ReadOnlySpan<byte> data, int offset)
    {
        if (offset >= data.Length) return false;
        if (data[offset] != UnitTerminator) return false;
        if (_lexicalLevel >= 2)
            return offset + 1 < data.Length && data[offset + 1] == 0x00;
        return true;
    }

    private int CalculateSubfieldLength(ReadOnlySpan<byte> data, int offset, Iso8211SubfieldFormat format)
    {
        if (format.IsFixedWidth)
        {
            // Fixed-width subfield
            return Math.Min(format.Width, data.Length - offset);
        }

        // Variable-length subfield - scan for unit terminator or field terminator
        int length = 0;
        while (offset + length < data.Length)
        {
            if (IsUnitTerminator(data, offset + length) || IsFieldTerminator(data, offset + length))
            {
                break;
            }
            length++;
        }

        return length;
    }

    private T ConvertValue<T>(ParsedSubfield parsed, Iso8211SubfieldDefinition subfieldDef)
    {
        var span = _data.AsSpan(parsed.Offset, parsed.Length);
        var format = subfieldDef.Format;

        object result = format.FormatType switch
        {
            Iso8211SubfieldFormatType.CharacterData => ConvertCharacterData<T>(span),
            Iso8211SubfieldFormatType.Integer => ConvertAsciiInteger<T>(span),
            Iso8211SubfieldFormatType.Real => ConvertAsciiReal<T>(span),
            Iso8211SubfieldFormatType.UnsignedInteger => ConvertUnsignedBinary<T>(span, format.Width),
            Iso8211SubfieldFormatType.SignedInteger => ConvertSignedBinary<T>(span, format.Width),
            Iso8211SubfieldFormatType.FloatingPoint => ConvertFloatingBinary<T>(span, format.Width),
            Iso8211SubfieldFormatType.BitString => ConvertBitString<T>(span),
            _ => throw new InvalidOperationException($"Unknown format type: {format.FormatType}")
        };

        return (T)result;
    }

    private object ConvertCharacterData<T>(ReadOnlySpan<byte> data)
    {
        var encoding = _lexicalLevel >= 2 ? Encoding.Unicode : Encoding.UTF8;
        var str = encoding.GetString(data).TrimEnd('\x1F', '\x1E', '\0', ' ');

        if (typeof(T) == typeof(string))
        {
            return str;
        }
        if (typeof(T) == typeof(int))
        {
            return int.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(long))
        {
            return long.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(double))
        {
            return double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(float))
        {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(byte[]))
        {
            return data.ToArray();
        }
        if (typeof(T) == typeof(byte))
        {
            return byte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(sbyte))
        {
            return sbyte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(short))
        {
            return short.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(ushort))
        {
            return ushort.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert character data to type {typeof(T).Name}.");
    }

    private static object ConvertAsciiInteger<T>(ReadOnlySpan<byte> data)
    {
        var str = Encoding.ASCII.GetString(data).Trim();

        if (string.IsNullOrEmpty(str))
        {
            return default(T)!;
        }

        if (typeof(T) == typeof(string))
        {
            return str;
        }
        if (typeof(T) == typeof(int))
        {
            return int.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(uint))
        {
            return uint.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(long))
        {
            return long.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(ulong))
        {
            return ulong.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(short))
        {
            return short.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(ushort))
        {
            return ushort.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(byte))
        {
            return byte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(sbyte))
        {
            return sbyte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(double))
        {
            return double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(float))
        {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert ASCII integer to type {typeof(T).Name}.");
    }

    private static object ConvertAsciiReal<T>(ReadOnlySpan<byte> data)
    {
        var str = Encoding.ASCII.GetString(data).Trim();

        if (string.IsNullOrEmpty(str))
        {
            return default(T)!;
        }

        if (typeof(T) == typeof(string))
        {
            return str;
        }
        if (typeof(T) == typeof(double))
        {
            return double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(float))
        {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(decimal))
        {
            return decimal.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(int))
        {
            return (int)double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(long))
        {
            return (long)double.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert ASCII real to type {typeof(T).Name}.");
    }

    private static object ConvertUnsignedBinary<T>(ReadOnlySpan<byte> data, int width)
    {
        // Use actual data length if shorter than expected width
        int actualWidth = Math.Min(width, data.Length);

        ulong value = actualWidth switch
        {
            1 => data[0],
            2 => BinaryPrimitives.ReadUInt16LittleEndian(data),
            4 => BinaryPrimitives.ReadUInt32LittleEndian(data),
            8 => BinaryPrimitives.ReadUInt64LittleEndian(data),
            _ => ReadUnsignedLittleEndian(data, actualWidth)
        };

        if (typeof(T) == typeof(byte))
        {
            return (byte)value;
        }
        if (typeof(T) == typeof(ushort))
        {
            return (ushort)value;
        }
        if (typeof(T) == typeof(uint))
        {
            return (uint)value;
        }
        if (typeof(T) == typeof(ulong))
        {
            return value;
        }
        if (typeof(T) == typeof(int))
        {
            return (int)value;
        }
        if (typeof(T) == typeof(long))
        {
            return (long)value;
        }
        if (typeof(T) == typeof(short))
        {
            return (short)value;
        }
        if (typeof(T) == typeof(sbyte))
        {
            return (sbyte)value;
        }
        if (typeof(T) == typeof(double))
        {
            return (double)value;
        }
        if (typeof(T) == typeof(float))
        {
            return (float)value;
        }
        if (typeof(T) == typeof(string))
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert unsigned binary to type {typeof(T).Name}.");
    }

    private static object ConvertSignedBinary<T>(ReadOnlySpan<byte> data, int width)
    {
        // Use actual data length if shorter than expected width
        int actualWidth = Math.Min(width, data.Length);

        long value = actualWidth switch
        {
            1 => (sbyte)data[0],
            2 => BinaryPrimitives.ReadInt16LittleEndian(data),
            4 => BinaryPrimitives.ReadInt32LittleEndian(data),
            8 => BinaryPrimitives.ReadInt64LittleEndian(data),
            _ => ReadSignedLittleEndian(data, actualWidth)
        };

        if (typeof(T) == typeof(sbyte))
        {
            return (sbyte)value;
        }
        if (typeof(T) == typeof(short))
        {
            return (short)value;
        }
        if (typeof(T) == typeof(int))
        {
            return (int)value;
        }
        if (typeof(T) == typeof(long))
        {
            return value;
        }
        if (typeof(T) == typeof(byte))
        {
            return (byte)value;
        }
        if (typeof(T) == typeof(ushort))
        {
            return (ushort)value;
        }
        if (typeof(T) == typeof(uint))
        {
            return (uint)value;
        }
        if (typeof(T) == typeof(ulong))
        {
            return (ulong)value;
        }
        if (typeof(T) == typeof(double))
        {
            return (double)value;
        }
        if (typeof(T) == typeof(float))
        {
            return (float)value;
        }
        if (typeof(T) == typeof(string))
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert signed binary to type {typeof(T).Name}.");
    }

    private static object ConvertFloatingBinary<T>(ReadOnlySpan<byte> data, int width)
    {
        int actualWidth = Math.Min(width, data.Length);

        double value = actualWidth switch
        {
            4 => BinaryPrimitives.ReadSingleLittleEndian(data),
            8 => BinaryPrimitives.ReadDoubleLittleEndian(data),
            _ => throw new InvalidOperationException(
                $"Unsupported floating-point binary width: {actualWidth} byte(s).")
        };

        if (typeof(T) == typeof(double))
        {
            return value;
        }
        if (typeof(T) == typeof(float))
        {
            return (float)value;
        }
        if (typeof(T) == typeof(decimal))
        {
            return (decimal)value;
        }
        if (typeof(T) == typeof(int))
        {
            return (int)value;
        }
        if (typeof(T) == typeof(long))
        {
            return (long)value;
        }
        if (typeof(T) == typeof(string))
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert floating-point binary to type {typeof(T).Name}.");
    }

    private static object ConvertBitString<T>(ReadOnlySpan<byte> data)
    {
        if (typeof(T) == typeof(byte[]))
        {
            return data.ToArray();
        }
        if (typeof(T) == typeof(string))
        {
            // Return hex representation
            return Convert.ToHexString(data);
        }

        // For integer types, delegate to unsigned binary conversion
        return ConvertUnsignedBinary<T>(data, data.Length);
    }

    private static ulong ReadUnsignedLittleEndian(ReadOnlySpan<byte> data, int width)
    {
        ulong value = 0;
        for (int i = 0; i < width && i < data.Length; i++)
        {
            value |= (ulong)data[i] << (i * 8);
        }
        return value;
    }

    private static long ReadSignedLittleEndian(ReadOnlySpan<byte> data, int width)
    {
        var unsigned = ReadUnsignedLittleEndian(data, width);

        // Sign extend if the high bit is set
        var signBit = 1UL << (width * 8 - 1);
        if ((unsigned & signBit) != 0)
        {
            // Set all bits above the width to 1
            var mask = ~((1UL << (width * 8)) - 1);
            unsigned |= mask;
        }

        return (long)unsigned;
    }

    private readonly struct ParsedSubfield
    {
        public int DefinitionIndex { get; init; }

        public int Offset { get; init; }

        public int Length { get; init; }

        public int GroupIndex { get; init; }
    }
}
