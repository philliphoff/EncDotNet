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
    private readonly Iso8211FieldLayout _layout;
    private readonly ReadOnlyMemory<byte> _data;
    private readonly int _lexicalLevel;

    // Offsets of each parsed subfield instance. Null when every subfield is fixed-width: the
    // layout then gives any instance's offset from its position, so no table is needed.
    private readonly ParsedSubfield[]? _parsedSubfields;
    private readonly int _subfieldCount;
    private readonly int _groupCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211FieldReader"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The field definition from the DDR describing this field's structure.</param>
    /// <param name="data">The raw field data to read. The reader keeps its own copy.</param>
    /// <param name="lexicalLevel">
    /// The lexical level for character data (0 or 1 = ASCII/ISO 8859, 2 = UCS-2/UTF-16LE).
    /// When set to 2, variable-length character subfields use 2-byte terminators (0x1F 0x00 / 0x1E 0x00).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fieldDefinition"/> is <c>null</c>.
    /// </exception>
    public Iso8211FieldReader(Iso8211FieldDefinition fieldDefinition, ReadOnlySpan<byte> data, int lexicalLevel = 0)
        : this(fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition)), data.ToArray().AsMemory(), lexicalLevel, validated: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211FieldReader"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The field definition from the DDR describing this field's structure.</param>
    /// <param name="data">The raw field data to read. The reader reads the array in place, without copying it.</param>
    /// <param name="lexicalLevel">
    /// The lexical level for character data (0 or 1 = ASCII/ISO 8859, 2 = UCS-2/UTF-16LE).
    /// When set to 2, variable-length character subfields use 2-byte terminators (0x1F 0x00 / 0x1E 0x00).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fieldDefinition"/> or <paramref name="data"/> is <c>null</c>.
    /// </exception>
    public Iso8211FieldReader(Iso8211FieldDefinition fieldDefinition, byte[] data, int lexicalLevel = 0)
        : this(
            fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition)),
            (data ?? throw new ArgumentNullException(nameof(data))).AsMemory(),
            lexicalLevel,
            validated: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211FieldReader"/> class.
    /// </summary>
    /// <param name="fieldDefinition">The field definition from the DDR describing this field's structure.</param>
    /// <param name="data">
    /// The raw field data to read, such as an <see cref="Iso8211Field.Data"/>. The reader reads it in
    /// place, without copying it.
    /// </param>
    /// <param name="lexicalLevel">
    /// The lexical level for character data (0 or 1 = ASCII/ISO 8859, 2 = UCS-2/UTF-16LE).
    /// When set to 2, variable-length character subfields use 2-byte terminators (0x1F 0x00 / 0x1E 0x00).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fieldDefinition"/> is <c>null</c>.
    /// </exception>
    public Iso8211FieldReader(Iso8211FieldDefinition fieldDefinition, ReadOnlyMemory<byte> data, int lexicalLevel = 0)
        : this(fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition)), data, lexicalLevel, validated: true)
    {
    }

    private Iso8211FieldReader(Iso8211FieldDefinition fieldDefinition, ReadOnlyMemory<byte> data, int lexicalLevel, bool validated)
    {
        _ = validated;
        _fieldDefinition = fieldDefinition;
        _layout = Iso8211FieldLayout.For(fieldDefinition);
        _data = data;
        _lexicalLevel = lexicalLevel;

        var span = data.Span;
        if (_layout.IsFixedWidth)
        {
            _subfieldCount = ParseSubfields(span, [], out _groupCount);
        }
        else
        {
            // Count first, then fill an exactly sized table.
            var count = ParseSubfields(span, [], out _);
            _parsedSubfields = count == 0 ? [] : new ParsedSubfield[count];
            _subfieldCount = ParseSubfields(span, _parsedSubfields, out _groupCount);
        }
    }

    /// <summary>
    /// Gets the field definition used by this reader.
    /// </summary>
    public Iso8211FieldDefinition FieldDefinition => _fieldDefinition;

    /// <summary>
    /// Gets the raw field data.
    /// </summary>
    public ReadOnlySpan<byte> Data => _data.Span;

    /// <summary>
    /// Gets the number of parsed subfield instances.
    /// </summary>
    /// <remarks>
    /// For fields with repeating groups, this count includes all instances across all repetitions.
    /// </remarks>
    public int SubfieldCount => _subfieldCount;

    /// <summary>
    /// Gets the number of subfield groups (repetitions) in this field.
    /// </summary>
    /// <remarks>
    /// For fields without repeating groups, this will be 1.
    /// For fields with repeating groups, this is the number of times the group repeats.
    /// A final repetition cut short by the end of the field data is not counted.
    /// </remarks>
    public int GroupCount => _groupCount;

    /// <summary>
    /// Gets whether this field has repeating subfield groups.
    /// </summary>
    public bool HasRepeatingGroups => _fieldDefinition.HasRepeatingGroup && _groupCount > 1;

    /// <summary>
    /// Gets the ordinal of a subfield: its position in the field definition's
    /// <see cref="Iso8211FieldDefinition.SubfieldDefinitions"/>.
    /// </summary>
    /// <param name="name">The subfield name, such as <c>"YCOO"</c>.</param>
    /// <returns>The zero-based ordinal of the first subfield with that name.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the field definition has no subfield with the specified name.
    /// </exception>
    /// <remarks>
    /// Look the ordinal up once and pass it to <see cref="GetSubfield{T}(int, int)"/> to read many
    /// repetitions of a group without repeating the name lookup for each one.
    /// </remarks>
    /// <example>
    /// <code>
    /// var reader = new Iso8211FieldReader(ddr.GetFieldDefinition("SG2D")!, field.Data);
    /// var y = reader.GetOrdinal("YCOO");
    /// var x = reader.GetOrdinal("XCOO");
    /// for (var group = 0; group &lt; reader.GroupCount; group++)
    /// {
    ///     points.Add((reader.GetSubfield&lt;int&gt;(group, y), reader.GetSubfield&lt;int&gt;(group, x)));
    /// }
    /// </code>
    /// </example>
    public int GetOrdinal(string name)
    {
        var ordinal = _layout.IndexOf(name);
        if (ordinal < 0)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field definition '{_fieldDefinition.Tag}'.");
        }

        return ordinal;
    }

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
        // Instances follow definition order, so a subfield's first instance is at its ordinal.
        var ordinal = _layout.IndexOf(name);
        if (ordinal < 0 || ordinal >= _subfieldCount)
        {
            value = default!;
            return false;
        }

        value = ConvertAt<T>(ordinal);
        return true;
    }

    /// <summary>
    /// Gets the value of a subfield in one repetition of the field's repeating group.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield value to.</typeparam>
    /// <param name="groupIndex">
    /// The zero-based repetition, from 0 to <see cref="GroupCount"/> - 1. For a field without a
    /// repeating group, the only repetition is 0.
    /// </param>
    /// <param name="ordinal">The subfield's ordinal, from <see cref="GetOrdinal(string)"/>.</param>
    /// <returns>The subfield value converted to type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ordinal"/> is not a subfield of the repeating group, or the
    /// field data has no such repetition.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the subfield value cannot be converted to the requested type.
    /// </exception>
    public T GetSubfield<T>(int groupIndex, int ordinal)
    {
        if (!TryGetInstanceIndex(groupIndex, ordinal, out var index))
        {
            throw new ArgumentOutOfRangeException(
                nameof(groupIndex),
                $"Field '{_fieldDefinition.Tag}' has no subfield {ordinal} in repetition {groupIndex}.");
        }

        return ConvertAt<T>(index);
    }

    /// <summary>
    /// Tries to get the value of a subfield in one repetition of the field's repeating group.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield value to.</typeparam>
    /// <param name="groupIndex">The zero-based repetition.</param>
    /// <param name="ordinal">The subfield's ordinal, from <see cref="GetOrdinal(string)"/>.</param>
    /// <param name="value">
    /// When this method returns, contains the subfield value if present; otherwise, the default value.
    /// </param>
    /// <returns>
    /// <c>true</c> if the repetition contains the subfield; <c>false</c> if <paramref name="ordinal"/>
    /// is not in the repeating group or the field data ends before it.
    /// </returns>
    public bool TryGetSubfield<T>(int groupIndex, int ordinal, out T value)
    {
        if (!TryGetInstanceIndex(groupIndex, ordinal, out var index))
        {
            value = default!;
            return false;
        }

        value = ConvertAt<T>(index);
        return true;
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
        var ordinal = _layout.IndexOf(name);
        if (ordinal < 0)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field definition '{_fieldDefinition.Tag}'.");
        }

        if (ordinal >= _subfieldCount)
        {
            return [];
        }

        if (!_layout.HasRepeatingGroup || ordinal < _layout.RepeatStart)
        {
            return [ConvertAt<T>(ordinal)];
        }

        // Instances of a repeating subfield sit one group apart.
        var builder = ImmutableArray.CreateBuilder<T>(((_subfieldCount - 1 - ordinal) / _layout.RepeatCount) + 1);
        for (var index = ordinal; index < _subfieldCount; index += _layout.RepeatCount)
        {
            builder.Add(ConvertAt<T>(index));
        }

        return builder.MoveToImmutable();
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
        if (index < 0 || index >= _subfieldCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return ConvertAt<T>(index);
    }

    /// <summary>
    /// Gets the raw bytes of a subfield by name.
    /// </summary>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <returns>A span containing the raw subfield data.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no subfield with the specified name exists.
    /// </exception>
    /// <remarks>
    /// For repeating subfields, this returns the bytes from the first occurrence.
    /// Use <see cref="GetSubfieldBytes(int, int)"/> or
    /// <see cref="Iso8211SubfieldGroup.GetSubfieldBytes(string)"/> to retrieve
    /// bytes for a specific group.
    /// </remarks>
    public ReadOnlySpan<byte> GetSubfieldBytes(string name)
    {
        var ordinal = _layout.IndexOf(name);
        if (ordinal < 0)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field '{_fieldDefinition.Tag}'.");
        }

        if (ordinal >= _subfieldCount)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in parsed field data.");
        }

        return BytesAt(ordinal);
    }

    /// <summary>
    /// Gets the raw bytes of a subfield in one repetition of the field's repeating group.
    /// </summary>
    /// <param name="groupIndex">The zero-based repetition.</param>
    /// <param name="ordinal">The subfield's ordinal, from <see cref="GetOrdinal(string)"/>.</param>
    /// <returns>A span containing the raw subfield data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ordinal"/> is not a subfield of the repeating group, or the
    /// field data has no such repetition.
    /// </exception>
    public ReadOnlySpan<byte> GetSubfieldBytes(int groupIndex, int ordinal)
    {
        if (!TryGetInstanceIndex(groupIndex, ordinal, out var index))
        {
            throw new ArgumentOutOfRangeException(
                nameof(groupIndex),
                $"Field '{_fieldDefinition.Tag}' has no subfield {ordinal} in repetition {groupIndex}.");
        }

        return BytesAt(index);
    }

    /// <summary>
    /// Gets the raw bytes of a subfield at a specific index.
    /// </summary>
    /// <param name="index">The index of the parsed subfield.</param>
    /// <returns>A span containing the raw subfield data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="index"/> is outside the valid range.
    /// </exception>
    public ReadOnlySpan<byte> GetSubfieldBytesAt(int index)
    {
        if (index < 0 || index >= _subfieldCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return BytesAt(index);
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
    /// Unlike <see cref="GroupCount"/>, the enumeration includes a final repetition cut short by
    /// the end of the field data. To read many repetitions without allocating a group object for
    /// each, use <see cref="GetOrdinal(string)"/> with <see cref="GetSubfield{T}(int, int)"/>.
    /// </remarks>
    public IEnumerable<Iso8211SubfieldGroup> GetSubfieldGroups()
    {
        if (!_fieldDefinition.HasRepeatingGroup)
        {
            // Single group containing all subfields
            yield return new Iso8211SubfieldGroup(this, 0, _subfieldCount, 0);
            yield break;
        }

        var repeatStartIndex = _fieldDefinition.RepeatingSubfieldStartIndex;
        var repeatCount = _fieldDefinition.SubfieldDefinitions.Count - repeatStartIndex;
        var fixedCount = repeatStartIndex;

        // First group starts after fixed subfields
        int currentIndex = fixedCount;
        int groupIndex = 0;

        while (currentIndex < _subfieldCount)
        {
            var groupLength = Math.Min(repeatCount, _subfieldCount - currentIndex);
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
    /// Gets the ordinal of a subfield by name, or <c>-1</c>, for <see cref="Iso8211SubfieldGroup"/>.
    /// </summary>
    internal int IndexOfSubfield(string name) => _layout.IndexOf(name);

    /// <summary>
    /// Maps a repetition and subfield ordinal to the index of the parsed subfield instance.
    /// </summary>
    private bool TryGetInstanceIndex(int groupIndex, int ordinal, out int index)
    {
        index = -1;
        if (groupIndex < 0 || ordinal < 0 || ordinal >= _layout.Count)
        {
            return false;
        }

        if (!_layout.HasRepeatingGroup)
        {
            index = groupIndex == 0 ? ordinal : -1;
        }
        else if (ordinal >= _layout.RepeatStart)
        {
            // Instances follow definition order: the leading subfields, then each repetition.
            index = _layout.RepeatStart + (groupIndex * _layout.RepeatCount) + (ordinal - _layout.RepeatStart);
        }

        return index >= 0 && index < _subfieldCount;
    }

    /// <summary>
    /// Locates a parsed subfield instance: its definition ordinal, offset and length.
    /// </summary>
    private void Locate(int index, out int ordinal, out int offset, out int length)
    {
        if (_parsedSubfields is not null)
        {
            var parsed = _parsedSubfields[index];
            ordinal = parsed.DefinitionIndex;
            offset = parsed.Offset;
            length = parsed.Length;
            return;
        }

        // Fixed-width layout: every instance before the last is full width, so offsets follow
        // from position. Only the final instance can be cut short by the end of the data.
        var layout = _layout;
        if (index < layout.PrefixCount)
        {
            ordinal = index;
            offset = layout.Offsets[index];
        }
        else
        {
            var inGroups = index - layout.PrefixCount;
            var group = inGroups / layout.RepeatCount;
            ordinal = layout.RepeatStart + (inGroups - (group * layout.RepeatCount));
            offset = layout.PrefixBytes + (group * layout.Stride) + layout.Offsets[ordinal];
        }

        length = Math.Min(layout.Formats[ordinal].Width, _data.Length - offset);
    }

    private ReadOnlySpan<byte> BytesAt(int index)
    {
        Locate(index, out _, out var offset, out var length);
        return _data.Span.Slice(offset, length);
    }

    private T ConvertAt<T>(int index)
    {
        Locate(index, out var ordinal, out var offset, out var length);
        var format = _layout.Formats[ordinal];
        var span = _data.Span.Slice(offset, length);

        return format.FormatType switch
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
    }

    /// <summary>
    /// Walks the field data, recording each subfield instance in <paramref name="output"/> when
    /// it isn't empty.
    /// </summary>
    /// <returns>The number of subfield instances.</returns>
    private int ParseSubfields(ReadOnlySpan<byte> data, Span<ParsedSubfield> output, out int groupCount)
    {
        var subfields = _layout.Formats;
        groupCount = 0;
        if (subfields.Length == 0)
        {
            return 0;
        }

        var count = 0;
        int offset = 0;
        int subfieldIndex = 0;
        bool inRepeatingGroup = false;

        while (offset < data.Length)
        {
            var format = subfields[subfieldIndex];

            // For variable-length subfields, check if we've hit a field terminator.
            // Fixed-width binary subfields may contain 0x1E (30) as valid data, so we
            // must not treat it as a terminator.
            if (!format.IsFixedWidth && IsFieldTerminator(data, offset))
            {
                break;
            }

            // For fixed-width subfields in repeating groups, check if we've hit the end:
            // If the remaining data is just the field terminator, we're done.
            if (format.IsFixedWidth && _layout.HasRepeatingGroup)
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

            if (!output.IsEmpty)
            {
                output[count] = new ParsedSubfield(subfieldIndex, offset, length);
            }

            count++;
            offset += length;

            // Skip unit terminator if present - only for variable-length subfields
            // Fixed-width binary subfields (b11, b12, b14, b21, b22, b24) don't have UTs
            if (!format.IsFixedWidth && IsUnitTerminator(data, offset))
            {
                offset += TerminatorWidth;
            }

            // A field terminator acts as the final unit terminator; it isn't skipped, so the
            // next iteration's check ends the loop.

            // Advance to next subfield definition
            subfieldIndex++;

            // Handle repeating groups
            if (subfieldIndex >= subfields.Length)
            {
                if (_layout.HasRepeatingGroup)
                {
                    // Reset to the start of the repeating group
                    subfieldIndex = _layout.RepeatStart;
                    groupCount++;
                    inRepeatingGroup = true;
                }
                else
                {
                    // No repeating group, we're done with definitions
                    break;
                }
            }
            else if (!inRepeatingGroup && subfieldIndex == _layout.RepeatStart && _layout.HasRepeatingGroup)
            {
                // Just entered the repeating group for the first time; the wrap-around above
                // counts each group once it is complete.
                groupCount = 0;
                inRepeatingGroup = true;
            }
        }

        // If we never entered a repeating group but have subfields, we have 1 group
        if (!inRepeatingGroup && count > 0)
        {
            groupCount = 1;
        }

        return count;
    }

    /// <summary>
    /// Gets the width of a terminator for the current lexical level.
    /// </summary>
    private int TerminatorWidth => _lexicalLevel >= 2 ? 2 : 1;

    /// <summary>
    /// Checks if the data at the given offset is a field terminator,
    /// accounting for the lexical level.
    /// </summary>
    private bool IsFieldTerminator(ReadOnlySpan<byte> data, int offset)
    {
        if (offset >= data.Length) return false;
        if (data[offset] != FieldTerminator) return false;
        if (_lexicalLevel >= 2)
            return offset + 1 < data.Length && data[offset + 1] == 0x00;
        return true;
    }

    /// <summary>
    /// Checks if the data at the given offset is a unit terminator,
    /// accounting for the lexical level.
    /// </summary>
    private bool IsUnitTerminator(ReadOnlySpan<byte> data, int offset)
    {
        if (offset >= data.Length) return false;
        if (data[offset] != UnitTerminator) return false;
        if (_lexicalLevel >= 2)
            return offset + 1 < data.Length && data[offset + 1] == 0x00;
        return true;
    }

    /// <summary>
    /// Calculates the length of a subfield based on its format.
    /// </summary>
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

    // The conversions below return T directly. Each `(T)(object)value` sits behind a
    // `typeof(T) == typeof(...)` check, which the JIT folds for value types, so optimized code
    // neither boxes nor allocates.

    /// <summary>
    /// Converts character data to the requested type.
    /// </summary>
    private T ConvertCharacterData<T>(ReadOnlySpan<byte> data)
    {
        if (typeof(T) == typeof(byte[]))
        {
            return (T)(object)data.ToArray();
        }

        var str = DecodeCharacterData(data);

        if (typeof(T) == typeof(string))
        {
            return (T)(object)str;
        }
        if (typeof(T) == typeof(int))
        {
            return (T)(object)int.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(long))
        {
            return (T)(object)long.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(double))
        {
            return (T)(object)double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)float.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(byte))
        {
            return (T)(object)byte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(sbyte))
        {
            return (T)(object)sbyte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(short))
        {
            return (T)(object)short.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(ushort))
        {
            return (T)(object)ushort.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert character data to type {typeof(T).Name}.");
    }

    /// <summary>
    /// Decodes character data, dropping trailing terminators, NULs and spaces.
    /// </summary>
    private string DecodeCharacterData(ReadOnlySpan<byte> data)
    {
        if (_lexicalLevel >= 2)
        {
            return Encoding.Unicode.GetString(data).TrimEnd(CharacterPadding);
        }

        // In UTF-8 these four characters are single bytes that never occur inside a multi-byte
        // sequence, so trimming the bytes before decoding matches trimming the decoded string,
        // and decodes once instead of allocating a second, trimmed string.
        var end = data.Length;
        while (end > 0 && data[end - 1] is UnitTerminator or FieldTerminator or 0x00 or (byte)' ')
        {
            end--;
        }

        return Encoding.UTF8.GetString(data[..end]);
    }

    private static readonly char[] CharacterPadding = ['\x1F', '\x1E', '\0', ' '];

    /// <summary>
    /// Converts ASCII integer data (I format) to the requested type.
    /// </summary>
    private static T ConvertAsciiInteger<T>(ReadOnlySpan<byte> data)
    {
        var str = Encoding.ASCII.GetString(data).Trim();

        if (string.IsNullOrEmpty(str))
        {
            return default!;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)str;
        }
        if (typeof(T) == typeof(int))
        {
            return (T)(object)int.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(uint))
        {
            return (T)(object)uint.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(long))
        {
            return (T)(object)long.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(ulong))
        {
            return (T)(object)ulong.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(short))
        {
            return (T)(object)short.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(ushort))
        {
            return (T)(object)ushort.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(byte))
        {
            return (T)(object)byte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(sbyte))
        {
            return (T)(object)sbyte.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(double))
        {
            return (T)(object)double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)float.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert ASCII integer to type {typeof(T).Name}.");
    }

    /// <summary>
    /// Converts ASCII real data (R format) to the requested type.
    /// </summary>
    private static T ConvertAsciiReal<T>(ReadOnlySpan<byte> data)
    {
        var str = Encoding.ASCII.GetString(data).Trim();

        if (string.IsNullOrEmpty(str))
        {
            return default!;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)str;
        }
        if (typeof(T) == typeof(double))
        {
            return (T)(object)double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)float.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(decimal))
        {
            return (T)(object)decimal.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(int))
        {
            return (T)(object)(int)double.Parse(str, CultureInfo.InvariantCulture);
        }
        if (typeof(T) == typeof(long))
        {
            return (T)(object)(long)double.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert ASCII real to type {typeof(T).Name}.");
    }

    /// <summary>
    /// Converts unsigned binary data (b1w format) to the requested type.
    /// </summary>
    private static T ConvertUnsignedBinary<T>(ReadOnlySpan<byte> data, int width)
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
            return (T)(object)(byte)value;
        }
        if (typeof(T) == typeof(ushort))
        {
            return (T)(object)(ushort)value;
        }
        if (typeof(T) == typeof(uint))
        {
            return (T)(object)(uint)value;
        }
        if (typeof(T) == typeof(ulong))
        {
            return (T)(object)value;
        }
        if (typeof(T) == typeof(int))
        {
            return (T)(object)(int)value;
        }
        if (typeof(T) == typeof(long))
        {
            return (T)(object)(long)value;
        }
        if (typeof(T) == typeof(short))
        {
            return (T)(object)(short)value;
        }
        if (typeof(T) == typeof(sbyte))
        {
            return (T)(object)(sbyte)value;
        }
        if (typeof(T) == typeof(double))
        {
            return (T)(object)(double)value;
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)(float)value;
        }
        if (typeof(T) == typeof(string))
        {
            return (T)(object)value.ToString(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert unsigned binary to type {typeof(T).Name}.");
    }

    /// <summary>
    /// Converts signed binary data (b2w format) to the requested type.
    /// </summary>
    private static T ConvertSignedBinary<T>(ReadOnlySpan<byte> data, int width)
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
            return (T)(object)(sbyte)value;
        }
        if (typeof(T) == typeof(short))
        {
            return (T)(object)(short)value;
        }
        if (typeof(T) == typeof(int))
        {
            return (T)(object)(int)value;
        }
        if (typeof(T) == typeof(long))
        {
            return (T)(object)value;
        }
        if (typeof(T) == typeof(byte))
        {
            return (T)(object)(byte)value;
        }
        if (typeof(T) == typeof(ushort))
        {
            return (T)(object)(ushort)value;
        }
        if (typeof(T) == typeof(uint))
        {
            return (T)(object)(uint)value;
        }
        if (typeof(T) == typeof(ulong))
        {
            return (T)(object)(ulong)value;
        }
        if (typeof(T) == typeof(double))
        {
            return (T)(object)(double)value;
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)(float)value;
        }
        if (typeof(T) == typeof(string))
        {
            return (T)(object)value.ToString(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert signed binary to type {typeof(T).Name}.");
    }

    /// <summary>
    /// Converts floating-point binary data (b4w format) to the requested type.
    /// </summary>
    private static T ConvertFloatingBinary<T>(ReadOnlySpan<byte> data, int width)
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
            return (T)(object)value;
        }
        if (typeof(T) == typeof(float))
        {
            return (T)(object)(float)value;
        }
        if (typeof(T) == typeof(decimal))
        {
            return (T)(object)(decimal)value;
        }
        if (typeof(T) == typeof(int))
        {
            return (T)(object)(int)value;
        }
        if (typeof(T) == typeof(long))
        {
            return (T)(object)(long)value;
        }
        if (typeof(T) == typeof(string))
        {
            return (T)(object)value.ToString(CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException($"Cannot convert floating-point binary to type {typeof(T).Name}.");
    }

    /// <summary>
    /// Converts bit string data (B(n) format) to the requested type.
    /// </summary>
    private static T ConvertBitString<T>(ReadOnlySpan<byte> data)
    {
        if (typeof(T) == typeof(byte[]))
        {
            return (T)(object)data.ToArray();
        }

        if (typeof(T) == typeof(string))
        {
            // Return hex representation
            return (T)(object)Convert.ToHexString(data);
        }

        // For integer types, delegate to unsigned binary conversion
        return ConvertUnsignedBinary<T>(data, data.Length);
    }

    /// <summary>
    /// Reads an unsigned little-endian integer of arbitrary width.
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
    /// Reads a signed little-endian integer of arbitrary width.
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
    /// The position of one subfield instance within the field data.
    /// </summary>
    private readonly record struct ParsedSubfield(int DefinitionIndex, int Offset, int Length);
}
