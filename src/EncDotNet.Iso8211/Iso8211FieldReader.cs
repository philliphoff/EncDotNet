using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Reads and extracts subfield values from ISO 8211 field data using the field's definition from the DDR.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Iso8211FieldReader"/> provides a high-level interface for extracting typed
/// subfield values from raw ISO 8211 field data. It uses the field definition from the
/// Data Descriptive Record (DDR) to understand the structure and format of each subfield.
/// </para>
/// <para>
/// The reader eagerly parses all subfield boundaries upon construction, which simplifies
/// handling of variable-width subfields. Actual value conversion is performed lazily when
/// <see cref="GetSubfield{T}(string)"/> or similar methods are called.
/// </para>
/// <para>
/// For fields with repeating subfield groups, use <see cref="GetSubfieldValues{T}(string)"/>
/// to retrieve all values, or <see cref="GetSubfieldGroups"/> to iterate over each group.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var ddr = Iso8211DdrParser.Parse(ddrRecord);
/// var fieldDef = ddr.GetFieldDefinition("DSID");
/// var reader = new Iso8211FieldReader(fieldDef, fieldData);
/// 
/// var recordName = reader.GetSubfield&lt;byte&gt;("RCNM");
/// var recordId = reader.GetSubfield&lt;int&gt;("RCID");
/// var dataSetName = reader.GetSubfield&lt;string&gt;("DSNM");
/// </code>
/// </example>
public sealed class Iso8211FieldReader
{
    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    private readonly Iso8211FieldDefinition _fieldDefinition;
    private readonly byte[] _data;
    private readonly ImmutableArray<ParsedSubfield> _parsedSubfields;
    private readonly int _groupCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211FieldReader"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The field definition from the DDR describing this field's structure.</param>
    /// <param name="data">The raw field data to read.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fieldDefinition"/> is <c>null</c>.
    /// </exception>
    public Iso8211FieldReader(Iso8211FieldDefinition fieldDefinition, ReadOnlySpan<byte> data)
    {
        _fieldDefinition = fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition));
        _data = data.ToArray();
        (_parsedSubfields, _groupCount) = ParseSubfields();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211FieldReader"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The field definition from the DDR describing this field's structure.</param>
    /// <param name="data">The raw field data to read.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fieldDefinition"/> or <paramref name="data"/> is <c>null</c>.
    /// </exception>
    public Iso8211FieldReader(Iso8211FieldDefinition fieldDefinition, byte[] data)
    {
        _fieldDefinition = fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition));
        _data = data ?? throw new ArgumentNullException(nameof(data));
        (_parsedSubfields, _groupCount) = ParseSubfields();
    }

    /// <summary>
    /// Gets the field definition used by this reader.
    /// </summary>
    public Iso8211FieldDefinition FieldDefinition => _fieldDefinition;

    /// <summary>
    /// Gets the raw field data.
    /// </summary>
    public ReadOnlySpan<byte> Data => _data;

    /// <summary>
    /// Gets the number of parsed subfield instances.
    /// </summary>
    /// <remarks>
    /// For fields with repeating groups, this count includes all instances across all repetitions.
    /// </remarks>
    public int SubfieldCount => _parsedSubfields.Length;

    /// <summary>
    /// Gets the number of subfield groups (repetitions) in this field.
    /// </summary>
    /// <remarks>
    /// For fields without repeating groups, this will be 1.
    /// For fields with repeating groups, this is the number of times the group repeats.
    /// </remarks>
    public int GroupCount => _groupCount;

    /// <summary>
    /// Gets whether this field has repeating subfield groups.
    /// </summary>
    public bool HasRepeatingGroups => _fieldDefinition.HasRepeatingGroup && _groupCount > 1;

    /// <summary>
    /// Gets the value of a subfield by name.
    /// </summary>
    /// <typeparam name="T">
    /// The type to convert the subfield value to. Supported types include:
    /// <see cref="byte"/>, <see cref="sbyte"/>, <see cref="short"/>, <see cref="ushort"/>,
    /// <see cref="int"/>, <see cref="uint"/>, <see cref="long"/>, <see cref="ulong"/>,
    /// <see cref="float"/>, <see cref="double"/>, and <see cref="string"/>.
    /// </typeparam>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <returns>The subfield value converted to type <typeparamref name="T"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no subfield with the specified name exists.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the subfield value cannot be converted to the requested type.
    /// </exception>
    /// <remarks>
    /// For repeating subfields, this returns the value from the first occurrence.
    /// Use <see cref="GetSubfieldValues{T}(string)"/> to retrieve all values.
    /// </remarks>
    public T GetSubfield<T>(string name)
    {
        if (!TryGetSubfield<T>(name, out var value))
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field '{_fieldDefinition.Tag}'.");
        }
        return value;
    }

    /// <summary>
    /// Tries to get the value of a subfield by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield value to.</typeparam>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <param name="value">
    /// When this method returns, contains the subfield value if found; otherwise, the default value.
    /// </param>
    /// <returns><c>true</c> if the subfield was found; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Gets all values of a repeating subfield by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield values to.</typeparam>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <returns>An immutable array containing all values of the specified subfield.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no subfield with the specified name exists in the field definition.
    /// </exception>
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

    /// <summary>
    /// Gets the value of a subfield at a specific index.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield value to.</typeparam>
    /// <param name="index">The index of the parsed subfield.</param>
    /// <returns>The subfield value converted to type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="index"/> is outside the valid range.
    /// </exception>
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

    /// <summary>
    /// Gets the raw bytes of a subfield by name.
    /// </summary>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <returns>A span containing the raw subfield data.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no subfield with the specified name exists.
    /// </exception>
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

    /// <summary>
    /// Enumerates groups of repeating subfields.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="Iso8211SubfieldGroup"/> instances, one for each repetition
    /// of the repeating subfield group.
    /// </returns>
    /// <remarks>
    /// For fields without repeating groups, this returns a single group containing all subfields.
    /// </remarks>
    public IEnumerable<Iso8211SubfieldGroup> GetSubfieldGroups()
    {
        if (!_fieldDefinition.HasRepeatingGroup)
        {
            // Single group containing all subfields
            yield return new Iso8211SubfieldGroup(this, 0, _parsedSubfields.Length, 0);
            yield break;
        }

        var repeatStartIndex = _fieldDefinition.RepeatingSubfieldStartIndex;
        var repeatCount = _fieldDefinition.SubfieldDefinitions.Length - repeatStartIndex;
        var fixedCount = repeatStartIndex;

        // First group starts after fixed subfields
        int currentIndex = fixedCount;
        int groupIndex = 0;

        while (currentIndex < _parsedSubfields.Length)
        {
            var groupLength = Math.Min(repeatCount, _parsedSubfields.Length - currentIndex);
            yield return new Iso8211SubfieldGroup(this, currentIndex, groupLength, groupIndex);
            currentIndex += groupLength;
            groupIndex++;
        }
    }

    /// <summary>
    /// Gets the value of a non-repeating (fixed) subfield by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield value to.</typeparam>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <returns>The subfield value converted to type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method is equivalent to <see cref="GetSubfield{T}(string)"/> but is semantically
    /// clearer when used with fields that have both fixed and repeating subfields.
    /// </remarks>
    public T GetFixedSubfield<T>(string name) => GetSubfield<T>(name);

    /// <summary>
    /// Parses all subfields in the field data.
    /// </summary>
    /// <returns>
    /// A tuple containing the array of parsed subfields and the number of groups.
    /// </returns>
    private (ImmutableArray<ParsedSubfield> subfields, int groupCount) ParseSubfields()
    {
        var subfields = _fieldDefinition.SubfieldDefinitions;
        if (subfields.IsDefaultOrEmpty)
        {
            return (ImmutableArray<ParsedSubfield>.Empty, 0);
        }

        var parsed = ImmutableArray.CreateBuilder<ParsedSubfield>();
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
            // HOWEVER, for repeating groups, if we're at the field terminator position
            // (last byte and it's 0x1E), that IS the real terminator even for fixed-width.
            if (!format.IsFixedWidth && data[offset] == FieldTerminator)
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
            if (!format.IsFixedWidth && offset < data.Length && data[offset] == UnitTerminator)
            {
                offset++;
            }

            // Advance to next subfield definition
            subfieldIndex++;

            // Handle repeating groups
            if (subfieldIndex >= subfields.Length)
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
                // Just entered the repeating group for the first time
                groupCount = 1;
                inRepeatingGroup = true;
            }
        }

        // If we never entered a repeating group but have subfields, we have 1 group
        if (!inRepeatingGroup && parsed.Count > 0)
        {
            groupCount = 1;
        }

        return (parsed.ToImmutable(), groupCount);
    }

    /// <summary>
    /// Calculates the length of a subfield based on its format.
    /// </summary>
    private static int CalculateSubfieldLength(ReadOnlySpan<byte> data, int offset, Iso8211SubfieldFormat format)
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
            byte b = data[offset + length];
            if (b == UnitTerminator || b == FieldTerminator)
            {
                break;
            }
            length++;
        }

        return length;
    }

    /// <summary>
    /// Converts a parsed subfield value to the requested type.
    /// </summary>
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
            Iso8211SubfieldFormatType.BitString => ConvertBitString<T>(span),
            _ => throw new InvalidOperationException($"Unknown format type: {format.FormatType}")
        };

        return (T)result;
    }

    /// <summary>
    /// Converts character data to the requested type.
    /// </summary>
    private static object ConvertCharacterData<T>(ReadOnlySpan<byte> data)
    {
        var str = Encoding.ASCII.GetString(data).TrimEnd('\x1F', '\x1E', '\0', ' ');

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

    /// <summary>
    /// Converts ASCII integer data to the requested type.
    /// </summary>
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

    /// <summary>
    /// Converts ASCII real number data to the requested type.
    /// </summary>
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

    /// <summary>
    /// Converts unsigned binary integer data to the requested type.
    /// </summary>
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

    /// <summary>
    /// Converts signed binary integer data to the requested type.
    /// </summary>
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

    /// <summary>
    /// Converts bit string data (ISO 8211 <c>B(n)</c> format) to the requested type.
    /// </summary>
    /// <remarks>
    /// Bit strings are fixed-width binary data. They can be returned as <c>byte[]</c>
    /// for raw access, or as integer types when the width matches.
    /// </remarks>
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

    /// <summary>
    /// Reads an unsigned integer with a non-standard width.
    /// </summary>
    private static ulong ReadUnsignedLittleEndian(ReadOnlySpan<byte> data, int width)
    {
        ulong value = 0;
        for (int i = 0; i < width && i < data.Length; i++)
        {
            value |= (ulong)data[i] << (i * 8);
        }
        return value;
    }

    /// <summary>
    /// Reads a signed integer with a non-standard width.
    /// </summary>
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

    /// <summary>
    /// Represents a parsed subfield with its location in the field data.
    /// </summary>
    private readonly struct ParsedSubfield
    {
        /// <summary>
        /// The index of the subfield definition this parsed subfield corresponds to.
        /// </summary>
        public int DefinitionIndex { get; init; }

        /// <summary>
        /// The byte offset of this subfield within the field data.
        /// </summary>
        public int Offset { get; init; }

        /// <summary>
        /// The length of this subfield's data in bytes.
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// The group index for repeating subfields.
        /// </summary>
        public int GroupIndex { get; init; }
    }
}
