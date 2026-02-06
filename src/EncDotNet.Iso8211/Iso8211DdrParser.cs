using System.Collections.Immutable;
using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Parses the Data Descriptive Record (DDR) of an ISO 8211 document and produces
/// a strongly-typed <see cref="Iso8211DataDescriptiveRecord"/>.
/// </summary>
/// <remarks>
/// <para>
/// The DDR is the first record in an ISO 8211 file and has a leader identifier of <c>'L'</c>.
/// Each field in the DDR (except the reserved tag "0000") describes the structure and format
/// of a corresponding field in data records.
/// </para>
/// <para>
/// Each DDR field's data contains:
/// </para>
/// <list type="number">
/// <item><description>
/// <b>Field controls</b> — The first bytes (length specified by the record leader's
/// <c>FieldControlLength</c>) contain the data structure code and data type code.
/// </description></item>
/// <item><description>
/// <b>Field name and subfield descriptors</b> — Following the field controls, a unit-terminator-delimited
/// section contains the field name and subfield names (separated by <c>!</c> for vectors or <c>*</c>
/// for repeating groups).
/// </description></item>
/// <item><description>
/// <b>Format controls</b> — After the subfield descriptors, a format string (e.g., <c>(A,I(10),b11)</c>)
/// describes the encoding of each subfield.
/// </description></item>
/// </list>
/// </remarks>
public static class Iso8211DdrParser
{
    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    /// <summary>
    /// Parses an <see cref="Iso8211Record"/> that represents a DDR and returns a
    /// strongly-typed <see cref="Iso8211DataDescriptiveRecord"/>.
    /// </summary>
    /// <param name="record">The ISO 8211 record to parse. Must be a DDR (leader identifier <c>'L'</c>).</param>
    /// <returns>The parsed DDR containing all field definitions.</returns>
    /// <exception cref="ArgumentException">Thrown when the record is not a DDR.</exception>
    public static Iso8211DataDescriptiveRecord Parse(Iso8211Record record)
    {
        if (!record.IsDataDescriptiveRecord)
        {
            throw new ArgumentException("The record is not a Data Descriptive Record (DDR).", nameof(record));
        }

        var fieldControlLength = record.Leader.FieldControlLength;
        var definitions = ImmutableArray.CreateBuilder<Iso8211FieldDefinition>();

        foreach (var field in record.Fields)
        {
            var definition = ParseFieldDefinition(field, fieldControlLength);
            definitions.Add(definition);
        }

        return new Iso8211DataDescriptiveRecord
        {
            FieldDefinitions = definitions.ToImmutable()
        };
    }

    /// <summary>
    /// Parses a single DDR field into an <see cref="Iso8211FieldDefinition"/>.
    /// </summary>
    private static Iso8211FieldDefinition ParseFieldDefinition(Iso8211Field field, int fieldControlLength)
    {
        var data = field.Data.AsSpan();

        // Parse field controls (if present)
        var dataStructureCode = Iso8211DataStructureCode.Elementary;
        var dataTypeCode = Iso8211DataTypeCode.CharacterString;

        if (fieldControlLength >= 2 && data.Length >= 2)
        {
            dataStructureCode = (Iso8211DataStructureCode)(data[0] - '0');
            dataTypeCode = (Iso8211DataTypeCode)(data[1] - '0');
        }

        // Skip past field controls
        var remaining = fieldControlLength < data.Length ? data.Slice(fieldControlLength) : ReadOnlySpan<byte>.Empty;

        // Find the unit terminator that separates the descriptor area from the format controls.
        // The descriptor area contains: FieldName [! SubfieldName1 ! SubfieldName2 ...] UT FormatControls
        //
        // For a field like "0000" (record directory entry), the field may just contain raw data
        // without the standard descriptor/format structure.
        string fieldName = string.Empty;
        string? arrayDescriptor = null;
        string formatControls = string.Empty;
        var subfieldNames = ImmutableArray<string>.Empty;
        int repeatingGroupStartIndex = -1;

        if (!remaining.IsEmpty)
        {
            // Find the unit terminator(s) in the remaining data.
            // Structure: [FieldName!SF1!SF2!...] UT [FormatControls]
            // Or for arrays: [FieldName] UT [ArrayDescriptor] UT [FormatControls]
            var utPositions = FindAllPositions(remaining, UnitTerminator);

            if (utPositions.Length == 0)
            {
                // No unit terminators — treat entire remaining data as field name
                fieldName = Encoding.ASCII.GetString(remaining).TrimEnd((char)FieldTerminator);
            }
            else if (utPositions.Length == 1)
            {
                // One UT: [NameAndSubfields] UT [FormatControls]
                var descriptorSpan = remaining.Slice(0, utPositions[0]);
                var formatSpan = remaining.Slice(utPositions[0] + 1);

                ParseDescriptors(descriptorSpan, out fieldName, out subfieldNames, out repeatingGroupStartIndex);
                formatControls = Encoding.ASCII.GetString(formatSpan).TrimEnd((char)FieldTerminator);
            }
            else
            {
                // Two or more UTs: [NameAndSubfields] UT [ArrayDescriptor] UT [FormatControls]
                var descriptorSpan = remaining.Slice(0, utPositions[0]);
                var arraySpan = remaining.Slice(utPositions[0] + 1, utPositions[1] - utPositions[0] - 1);
                var formatSpan = remaining.Slice(utPositions[1] + 1);

                ParseDescriptors(descriptorSpan, out fieldName, out subfieldNames, out repeatingGroupStartIndex);
                arrayDescriptor = Encoding.ASCII.GetString(arraySpan);
                formatControls = Encoding.ASCII.GetString(formatSpan).TrimEnd((char)FieldTerminator);
            }
        }

        // Parse format controls into subfield formats
        var formats = !string.IsNullOrEmpty(formatControls)
            ? ParseFormatControls(formatControls)
            : ImmutableArray<Iso8211SubfieldFormat>.Empty;

        // Build subfield definitions by pairing names with formats
        var subfieldDefinitions = BuildSubfieldDefinitions(subfieldNames, formats, repeatingGroupStartIndex);

        return new Iso8211FieldDefinition
        {
            Tag = field.Tag,
            DataStructureCode = dataStructureCode,
            DataTypeCode = dataTypeCode,
            FieldName = fieldName,
            ArrayDescriptor = arrayDescriptor,
            FormatControls = formatControls,
            SubfieldDefinitions = subfieldDefinitions,
            RepeatingSubfieldStartIndex = repeatingGroupStartIndex
        };
    }

    /// <summary>
    /// Parses the descriptor portion of a DDR field into a field name and subfield names.
    /// </summary>
    /// <remarks>
    /// The descriptor has the form: <c>FieldName</c> or <c>SF1!SF2!SF3</c> (where the field
    /// name may be empty). Subfield names are separated by <c>!</c> delimiters. The special
    /// <c>*</c> prefix indicates a repeating group of subfields.
    /// </remarks>
    private static void ParseDescriptors(
        ReadOnlySpan<byte> descriptorSpan,
        out string fieldName,
        out ImmutableArray<string> subfieldNames,
        out int repeatingGroupStartIndex)
    {
        var descriptorStr = Encoding.ASCII.GetString(descriptorSpan);
        repeatingGroupStartIndex = -1;

        // Split by '!' delimiter
        var parts = descriptorStr.Split('!');

        if (parts.Length == 0 || (parts.Length == 1 && string.IsNullOrEmpty(parts[0])))
        {
            fieldName = string.Empty;
            subfieldNames = ImmutableArray<string>.Empty;
            return;
        }

        // If there's only one part and no '!' delimiters, it could be just a field name
        // with no subfields, or a single subfield name. By convention, when field controls
        // indicate subfields exist, the first part before '!' is the field name.
        if (parts.Length == 1)
        {
            fieldName = parts[0];
            subfieldNames = ImmutableArray<string>.Empty;
            return;
        }

        // First part is the field name, remaining are subfield names.
        // A '*' prefix on a subfield name marks the start of the repeating group.
        fieldName = parts[0];
        var names = ImmutableArray.CreateBuilder<string>(parts.Length - 1);
        int subfieldIndex = 0;

        for (int i = 1; i < parts.Length; i++)
        {
            var rawName = parts[i];
            if (rawName.StartsWith('*'))
            {
                repeatingGroupStartIndex = subfieldIndex;
                rawName = rawName.Substring(1);
            }

            if (!string.IsNullOrEmpty(rawName))
            {
                names.Add(rawName);
                subfieldIndex++;
            }
        }

        subfieldNames = names.ToImmutable();
    }

    /// <summary>
    /// Parses an ISO 8211 format controls string into individual subfield format descriptions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Format controls are enclosed in parentheses and contain comma-separated format descriptors.
    /// Examples: <c>(A)</c>, <c>(A,I(10),b11,b24)</c>, <c>(3A,2b14)</c>.
    /// </para>
    /// <para>
    /// Supported format types:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>A</c> or <c>A(n)</c> — Character data</description></item>
    /// <item><description><c>I</c> or <c>I(n)</c> — ASCII integer</description></item>
    /// <item><description><c>R</c> or <c>R(n)</c> — ASCII real</description></item>
    /// <item><description><c>b1n</c> — Unsigned integer of <c>n</c> bytes</description></item>
    /// <item><description><c>b2n</c> — Signed integer of <c>n</c> bytes</description></item>
    /// </list>
    /// <para>
    /// A numeric prefix (e.g., <c>3A</c>) is a repeat count, producing that many copies of the format.
    /// </para>
    /// </remarks>
    internal static ImmutableArray<Iso8211SubfieldFormat> ParseFormatControls(string formatControls)
    {
        // Strip outer parentheses
        var trimmed = formatControls.Trim();
        if (trimmed.StartsWith('(') && trimmed.EndsWith(')'))
        {
            trimmed = trimmed.Substring(1, trimmed.Length - 2);
        }

        if (string.IsNullOrEmpty(trimmed))
        {
            return ImmutableArray<Iso8211SubfieldFormat>.Empty;
        }

        var formats = ImmutableArray.CreateBuilder<Iso8211SubfieldFormat>();
        var parts = SplitFormatParts(trimmed);

        foreach (var part in parts)
        {
            var parsed = ParseSingleFormat(part.Trim(), out int repeatCount);
            if (parsed.HasValue)
            {
                // Expand repeat counts: e.g., "2b24" produces two b24 entries
                int count = repeatCount > 0 ? repeatCount : 1;
                for (int r = 0; r < count; r++)
                {
                    formats.Add(parsed.Value);
                }
            }
        }

        return formats.ToImmutable();
    }

    /// <summary>
    /// Splits a format controls string by commas, respecting nested parentheses.
    /// </summary>
    private static List<string> SplitFormatParts(string input)
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;

        for (int i = 0; i < input.Length; i++)
        {
            switch (input[i])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case ',' when depth == 0:
                    parts.Add(input.Substring(start, i - start));
                    start = i + 1;
                    break;
            }
        }

        if (start < input.Length)
        {
            parts.Add(input.Substring(start));
        }

        return parts;
    }

    /// <summary>
    /// Parses a single format descriptor string (e.g., "A", "I(10)", "b11", "3A").
    /// </summary>
    /// <param name="format">The format descriptor string.</param>
    /// <param name="repeatCount">The repeat count prefix, or <c>0</c> if no repeat count was specified.</param>
    /// <returns>The parsed subfield format, or <c>null</c> if the format could not be parsed.</returns>
    private static Iso8211SubfieldFormat? ParseSingleFormat(string format, out int repeatCount)
    {
        repeatCount = 0;

        if (string.IsNullOrEmpty(format))
        {
            return null;
        }

        var pos = 0;

        // Parse leading repeat count (e.g., "3" in "3A", "2" in "2b24")
        while (pos < format.Length && char.IsDigit(format[pos]))
        {
            repeatCount = repeatCount * 10 + (format[pos] - '0');
            pos++;
        }

        if (pos >= format.Length)
        {
            return null;
        }

        // Parse the format type
        var typeChar = format[pos];

        switch (typeChar)
        {
            case 'A':
            case 'a':
                return ParseCharacterFormat(format, pos, Iso8211SubfieldFormatType.CharacterData);

            case 'I':
            case 'i':
                return ParseCharacterFormat(format, pos, Iso8211SubfieldFormatType.Integer);

            case 'R':
            case 'r':
                return ParseCharacterFormat(format, pos, Iso8211SubfieldFormatType.Real);

            case 'B':
            case 'b':
                return ParseBinaryFormat(format, pos);

            default:
                return null;
        }
    }

    /// <summary>
    /// Parses a character-based format like <c>A</c>, <c>A(10)</c>, <c>I(5)</c>.
    /// </summary>
    private static Iso8211SubfieldFormat ParseCharacterFormat(string format, int pos, Iso8211SubfieldFormatType formatType)
    {
        pos++; // Skip the type character

        int width = 0;

        if (pos < format.Length && format[pos] == '(')
        {
            // Parse width from parenthesized value: A(10)
            pos++; // Skip '('
            while (pos < format.Length && char.IsDigit(format[pos]))
            {
                width = width * 10 + (format[pos] - '0');
                pos++;
            }
            // Skip closing ')'
        }

        return new Iso8211SubfieldFormat
        {
            FormatType = formatType,
            Width = width
        };
    }

    /// <summary>
    /// Parses a binary format like <c>b11</c>, <c>b12</c>, <c>b14</c>, <c>b21</c>, <c>b22</c>, <c>b24</c>.
    /// </summary>
    private static Iso8211SubfieldFormat? ParseBinaryFormat(string format, int pos)
    {
        pos++; // Skip 'b'

        if (pos >= format.Length)
        {
            return null;
        }

        // Parse sign indicator: 1 = unsigned, 2 = signed
        var signIndicator = format[pos] - '0';
        pos++;

        if (pos >= format.Length)
        {
            return null;
        }

        // Parse byte width
        int width = 0;
        while (pos < format.Length && char.IsDigit(format[pos]))
        {
            width = width * 10 + (format[pos] - '0');
            pos++;
        }

        Iso8211SubfieldFormatType formatType;
        switch (signIndicator)
        {
            case 1:
                formatType = Iso8211SubfieldFormatType.UnsignedInteger;
                break;
            case 2:
                formatType = Iso8211SubfieldFormatType.SignedInteger;
                break;
            default:
                return null;
        }

        return new Iso8211SubfieldFormat
        {
            FormatType = formatType,
            Width = width
        };
    }

    /// <summary>
    /// Builds subfield definitions by pairing subfield names with their corresponding formats.
    /// </summary>
    /// <param name="subfieldNames">The subfield names parsed from the descriptor area.</param>
    /// <param name="formats">The subfield formats parsed from the format controls string.</param>
    /// <param name="repeatingGroupStartIndex">
    /// The index at which the repeating group begins (from the <c>*</c> marker), or <c>-1</c> if none.
    /// Subfields at and after this index will have <see cref="Iso8211SubfieldDefinition.IsRepeating"/> set to <c>true</c>.
    /// </param>
    private static ImmutableArray<Iso8211SubfieldDefinition> BuildSubfieldDefinitions(
        ImmutableArray<string> subfieldNames,
        ImmutableArray<Iso8211SubfieldFormat> formats,
        int repeatingGroupStartIndex)
    {
        if (subfieldNames.IsDefaultOrEmpty)
        {
            return ImmutableArray<Iso8211SubfieldDefinition>.Empty;
        }

        var definitions = ImmutableArray.CreateBuilder<Iso8211SubfieldDefinition>(subfieldNames.Length);

        for (int i = 0; i < subfieldNames.Length; i++)
        {
            var format = i < formats.Length
                ? formats[i]
                : new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 0 };

            definitions.Add(new Iso8211SubfieldDefinition
            {
                Name = subfieldNames[i],
                Format = format,
                Index = i,
                IsRepeating = repeatingGroupStartIndex >= 0 && i >= repeatingGroupStartIndex
            });
        }

        return definitions.ToImmutable();
    }

    /// <summary>
    /// Finds all positions of the specified byte value in a span.
    /// </summary>
    private static int[] FindAllPositions(ReadOnlySpan<byte> span, byte value)
    {
        var positions = new List<int>();
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == value)
            {
                positions.Add(i);
            }
        }
        return positions.ToArray();
    }
}
