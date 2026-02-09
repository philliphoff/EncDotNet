using System.Collections.Concurrent;
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
public static class Iso8211DataDescriptiveRecordReader
{
    /// <summary>
    /// Cache of DDR field definitions keyed by field control length.
    /// Since every DDR field shares the same internal layout for a given field control length,
    /// we avoid repeated allocations by caching the definition.
    /// </summary>
    private static readonly ConcurrentDictionary<int, Iso8211FieldDefinition> s_ddrFieldDefinitionCache = new();

    /// <summary>
    /// Parses an <see cref="Iso8211Record"/> that represents a DDR and returns a
    /// strongly-typed <see cref="Iso8211DataDescriptiveRecord"/>.
    /// </summary>
    /// <param name="record">The ISO 8211 record to parse. Must be a DDR (leader identifier <c>'L'</c>).</param>
    /// <returns>The parsed DDR containing all field definitions.</returns>
    /// <exception cref="ArgumentException">Thrown when the record is not a DDR.</exception>
    public static Iso8211DataDescriptiveRecord Read(Iso8211Record record)
    {
        if (!record.IsDataDescriptiveRecord)
        {
            throw new ArgumentException("The record is not a Data Descriptive Record (DDR).", nameof(record));
        }

        // Create a meta-DDR field definition that describes the structure of DDR field
        // entries themselves. Since the DDR is a standard ISO 8211 record, we can use the
        // same Iso8211FieldReader infrastructure to parse its fields. Every DDR field shares
        // the same internal layout, determined solely by the leader's FieldControlLength.
        var ddrFieldDefinition = GetDdrFieldDefinition(record.Leader.FieldControlLength);
        var definitions = ImmutableArray.CreateBuilder<Iso8211FieldDefinition>();

        foreach (var field in record.Fields)
        {
            var reader = new Iso8211FieldReader(ddrFieldDefinition, field.Data);
            var definition = ParseFieldDefinition(field.Tag, reader);
            definitions.Add(definition);
        }

        return new Iso8211DataDescriptiveRecord
        {
            FieldDefinitions = definitions.ToImmutable()
        };
    }

    /// <summary>
    /// Gets (or creates and caches) an <see cref="Iso8211FieldDefinition"/> that describes
    /// the internal structure of any DDR field entry for the given field control length.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Since the DDR is itself a standard ISO 8211 record, its fields follow a known structure
    /// defined by the ISO 8211 standard. This method returns a meta-DDR field definition —
    /// a single definition that describes the layout shared by all DDR fields — enabling the
    /// use of <see cref="Iso8211FieldReader"/> to parse DDR field data.
    /// </para>
    /// <para>
    /// Every DDR field has the same internal layout, determined solely by
    /// <paramref name="fieldControlLength"/>:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Field controls (fixed-width, size determined by
    /// <paramref name="fieldControlLength"/>): data structure code (1 byte) and data type code
    /// (1 byte), plus any additional control bytes.</description></item>
    /// <item><description>Field name / descriptors (variable-length, unit-terminated).</description></item>
    /// <item><description>Subfield labels or array descriptor (variable-length, unit-terminated).</description></item>
    /// <item><description>Format controls (variable-length, terminated by end of field).</description></item>
    /// </list>
    /// <para>
    /// The returned definition is cached by <paramref name="fieldControlLength"/>, so repeated
    /// calls with the same value return the same instance without additional allocations.
    /// </para>
    /// </remarks>
    /// <param name="fieldControlLength">The field control length from the DDR leader.</param>
    /// <returns>An <see cref="Iso8211FieldDefinition"/> describing the DDR's own field structure.</returns>
    public static Iso8211FieldDefinition GetDdrFieldDefinition(int fieldControlLength)
    {
        return s_ddrFieldDefinitionCache.GetOrAdd(fieldControlLength, static fcl =>
        {
            var subfieldDefinitions = CreateDdrSubfieldDefinitions(fcl);

            return new Iso8211FieldDefinition
            {
                Tag = string.Empty,
                DataStructureCode = Iso8211DataStructureCode.Elementary,
                DataTypeCode = Iso8211DataTypeCode.CharacterString,
                FieldName = string.Empty,
                SubfieldDefinitions = subfieldDefinitions,
                FormatControls = string.Empty
            };
        });
    }

    /// <summary>
    /// Creates the subfield definitions that describe the internal structure of a DDR field entry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The subfield layout depends on the <paramref name="fieldControlLength"/> from the DDR leader:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>FCDS</c> (1 byte): data structure code — present when <paramref name="fieldControlLength"/> ≥ 1.</description></item>
    /// <item><description><c>FCDT</c> (1 byte): data type code — present when <paramref name="fieldControlLength"/> ≥ 2.</description></item>
    /// <item><description><c>FCEX</c> (remaining bytes): extended field controls — present when <paramref name="fieldControlLength"/> &gt; 2.</description></item>
    /// <item><description><c>NAME</c> (variable): field name, or combined field name and subfield descriptors.</description></item>
    /// <item><description><c>LBLS</c> (variable): subfield labels, array descriptor, or format controls.</description></item>
    /// <item><description><c>FMTS</c> (variable): format controls string.</description></item>
    /// </list>
    /// </remarks>
    private static ImmutableArray<Iso8211SubfieldDefinition> CreateDdrSubfieldDefinitions(int fieldControlLength)
    {
        var builder = ImmutableArray.CreateBuilder<Iso8211SubfieldDefinition>();
        int index = 0;

        if (fieldControlLength >= 1)
        {
            builder.Add(new Iso8211SubfieldDefinition
            {
                Name = "FCDS",
                Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 1 },
                Index = index++
            });
        }

        if (fieldControlLength >= 2)
        {
            builder.Add(new Iso8211SubfieldDefinition
            {
                Name = "FCDT",
                Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 1 },
                Index = index++
            });
        }

        if (fieldControlLength > 2)
        {
            builder.Add(new Iso8211SubfieldDefinition
            {
                Name = "FCEX",
                Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = fieldControlLength - 2 },
                Index = index++
            });
        }

        // Variable-length sections, each terminated by unit terminator (0x1F).
        // NAME: field name and/or combined descriptors.
        // LBLS: subfield labels, array descriptor, or (in the 1-UT case) format controls.
        // FMTS: format controls string (populated only in the 2-UT case).
        builder.Add(new Iso8211SubfieldDefinition
        {
            Name = "NAME",
            Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 0 },
            Index = index++
        });

        builder.Add(new Iso8211SubfieldDefinition
        {
            Name = "LBLS",
            Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 0 },
            Index = index++
        });

        builder.Add(new Iso8211SubfieldDefinition
        {
            Name = "FMTS",
            Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 0 },
            Index = index++
        });

        return builder.ToImmutable();
    }

    /// <summary>
    /// Parses a single DDR field into an <see cref="Iso8211FieldDefinition"/> using the
    /// provided <see cref="Iso8211FieldReader"/> that has been configured with the DDR's
    /// own field definition.
    /// </summary>
    /// <param name="tag">The field tag.</param>
    /// <param name="reader">
    /// A field reader configured with the meta-DDR field definition produced by
    /// <see cref="CreateDdrSubfieldDefinitions"/>.
    /// </param>
    private static Iso8211FieldDefinition ParseFieldDefinition(string tag, Iso8211FieldReader reader)
    {
        // Extract data structure code and data type code from field controls.
        var dataStructureCode = Iso8211DataStructureCode.Elementary;
        var dataTypeCode = Iso8211DataTypeCode.CharacterString;

        if (reader.TryGetSubfield<string>("FCDS", out var dsStr) && dsStr.Length > 0)
        {
            dataStructureCode = (Iso8211DataStructureCode)(dsStr[0] - '0');
        }

        if (reader.TryGetSubfield<string>("FCDT", out var dtStr) && dtStr.Length > 0)
        {
            dataTypeCode = (Iso8211DataTypeCode)(dtStr[0] - '0');
        }

        // Extract the three variable-length sections from the DDR field.
        //
        // Depending on the number of unit terminators (UTs) in the original data,
        // these reader subfields map differently:
        //
        //   2-UT case: NAME = fieldName, LBLS = labels/descriptor, FMTS = formatControls
        //   1-UT case: NAME = combined name+labels, LBLS = formatControls, FMTS = (not read)
        //   0-UT case: NAME = fieldName, LBLS = (not read), FMTS = (not read)
        //
        // The Iso8211FieldReader reads variable-length subfields sequentially, consuming
        // up to the next UT or end of data. When the original data has only one UT, NAME
        // captures everything before it (including any '!' delimited subfield names), and
        // LBLS captures the format controls after it.
        reader.TryGetSubfield<string>("NAME", out var namePart);
        reader.TryGetSubfield<string>("LBLS", out var lblsPart);
        reader.TryGetSubfield<string>("FMTS", out var fmtsPart);

        namePart ??= string.Empty;
        lblsPart ??= string.Empty;
        fmtsPart ??= string.Empty;

        string fieldName;
        string? arrayDescriptor = null;
        string formatControls;
        var subfieldNames = ImmutableArray<string>.Empty;
        int repeatingGroupStartIndex = -1;

        if (!string.IsNullOrEmpty(fmtsPart))
        {
            // 2-UT case: NAME = field name, LBLS = labels/descriptor, FMTS = format controls.
            fieldName = namePart;
            formatControls = fmtsPart;

            if (lblsPart.Contains('!') || lblsPart.StartsWith('*'))
            {
                ParseSubfieldLabels(lblsPart, out subfieldNames, out repeatingGroupStartIndex);
            }
            else if ((dataStructureCode == Iso8211DataStructureCode.Array
                || dataStructureCode == Iso8211DataStructureCode.ConcatenatedArray)
                && !string.IsNullOrEmpty(lblsPart))
            {
                // Middle section is a true array descriptor (no subfield labels).
                arrayDescriptor = lblsPart;
            }
            else if (!string.IsNullOrEmpty(lblsPart))
            {
                // Single subfield label with no '!' or '*' — still parse as subfield labels.
                ParseSubfieldLabels(lblsPart, out subfieldNames, out repeatingGroupStartIndex);
            }
        }
        else if (namePart.Contains('!') || namePart.StartsWith('*'))
        {
            // NAME contains descriptor markers ('!' or '*'). This covers both the 1-UT case
            // (where LBLS holds format controls) and the trailing-UT-with-empty-format case
            // (where LBLS was not read because no data followed the UT).
            formatControls = lblsPart;
            ParseDescriptors(namePart, out fieldName, out subfieldNames, out repeatingGroupStartIndex);
        }
        else
        {
            // Simple case: NAME is field name only, LBLS (if present) holds format controls.
            fieldName = namePart;
            formatControls = lblsPart;
        }

        // Parse format controls into subfield formats.
        var formats = !string.IsNullOrEmpty(formatControls)
            ? ParseFormatControls(formatControls)
            : ImmutableArray<Iso8211SubfieldFormat>.Empty;

        // For Vector fields (DataStructureCode=1), the ISO 8211 standard says the field is
        // a one-dimensional array of subfield groups — all subfields implicitly repeat.
        // Some DDRs include an explicit '*' marker for the repeating group, but others rely
        // solely on the DataStructureCode. When no '*' is present, infer repeating from index 0.
        if (dataStructureCode == Iso8211DataStructureCode.Vector
            && repeatingGroupStartIndex < 0
            && !subfieldNames.IsDefaultOrEmpty)
        {
            repeatingGroupStartIndex = 0;
        }

        // Build subfield definitions by pairing names with formats.
        var subfieldDefinitions = BuildSubfieldDefinitions(subfieldNames, formats, repeatingGroupStartIndex);

        return new Iso8211FieldDefinition
        {
            Tag = tag,
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
    /// Parses subfield labels from a string containing names separated by '!' delimiters.
    /// </summary>
    /// <remarks>
    /// This method handles the case where subfield labels appear in a separate section
    /// (after the first unit terminator) in the DDR field data. Unlike <see cref="ParseDescriptors"/>,
    /// all parts are treated as subfield names — there is no leading field name.
    /// The special <c>*</c> prefix indicates the start of a repeating group.
    /// </remarks>
    private static void ParseSubfieldLabels(
        string labelsStr,
        out ImmutableArray<string> subfieldNames,
        out int repeatingGroupStartIndex)
    {
        repeatingGroupStartIndex = -1;

        if (string.IsNullOrEmpty(labelsStr))
        {
            subfieldNames = ImmutableArray<string>.Empty;
            return;
        }

        var parts = labelsStr.Split('!');
        var names = ImmutableArray.CreateBuilder<string>(parts.Length);
        int subfieldIndex = 0;

        for (int i = 0; i < parts.Length; i++)
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
    /// Parses the descriptor portion of a DDR field into a field name and subfield names.
    /// </summary>
    /// <remarks>
    /// The descriptor has the form: <c>FieldName</c> or <c>FieldName!SF1!SF2!SF3</c>.
    /// Subfield names are separated by <c>!</c> delimiters. The special
    /// <c>*</c> prefix indicates a repeating group of subfields.
    /// </remarks>
    private static void ParseDescriptors(
        string descriptorStr,
        out string fieldName,
        out ImmutableArray<string> subfieldNames,
        out int repeatingGroupStartIndex)
    {
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
                return ParseBitStringFormat(format, pos);

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
    /// Parses a bit string format like <c>B(40)</c>.
    /// </summary>
    /// <remarks>
    /// The ISO 8211 <c>B(n)</c> format represents a bit string of <c>n</c> bits.
    /// The width is stored in bytes (n / 8).
    /// </remarks>
    private static Iso8211SubfieldFormat ParseBitStringFormat(string format, int pos)
    {
        // Parse B(n) using the same parenthesized-width logic as character formats,
        // but the width is in bits — convert to bytes.
        pos++; // Skip 'B'

        int widthInBits = 0;

        if (pos < format.Length && format[pos] == '(')
        {
            pos++; // Skip '('
            while (pos < format.Length && char.IsDigit(format[pos]))
            {
                widthInBits = widthInBits * 10 + (format[pos] - '0');
                pos++;
            }
            // Skip closing ')'
        }

        return new Iso8211SubfieldFormat
        {
            FormatType = Iso8211SubfieldFormatType.BitString,
            Width = (widthInBits + 7) / 8  // Round up to whole bytes
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

}
