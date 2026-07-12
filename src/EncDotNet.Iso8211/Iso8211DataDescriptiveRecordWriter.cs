using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Encodes Data Descriptive Record (DDR) field definitions into ISO 8211 field data.
/// </summary>
/// <remarks>
/// <para>
/// This type is the symmetric inverse of <see cref="Iso8211DataDescriptiveRecordReader"/>. It
/// emits each field definition using the canonical two-unit-terminator layout that the reader
/// parses back into an equivalent <see cref="Iso8211FieldDefinition"/>:
/// </para>
/// <list type="number">
/// <item><description><b>Field controls</b> — the data structure code and data type code as
/// single digits, followed by filler bytes up to the DDR's field control length.</description></item>
/// <item><description><b>Field name</b> (<c>NAME</c>) — variable-length, terminated by a unit terminator.</description></item>
/// <item><description><b>Subfield labels</b> (<c>LBLS</c>) — subfield names joined by <c>!</c>, with a
/// <c>*</c> prefix marking the repeating group, terminated by a unit terminator.</description></item>
/// <item><description><b>Format controls</b> (<c>FMTS</c>) — the format controls string (e.g. <c>(b11,b14)</c>).</description></item>
/// </list>
/// <para>
/// The resulting fields are assembled into a DDR record (leader identifier <c>'L'</c>) that can be
/// serialized with <see cref="Iso8211DocumentWriter"/>.
/// </para>
/// </remarks>
public static class Iso8211DataDescriptiveRecordWriter
{
    /// <summary>
    /// The default field control length used for DDR records when none is specified.
    /// </summary>
    public const int DefaultFieldControlLength = 6;

    /// <summary>
    /// Encodes a single field definition into a DDR <see cref="Iso8211Field"/>.
    /// </summary>
    /// <param name="definition">The field definition to encode.</param>
    /// <param name="fieldControlLength">The DDR field control length (number of field-control bytes).</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <returns>The encoded DDR field. Its data excludes the trailing field terminator.</returns>
    public static Iso8211Field EncodeFieldDefinition(
        Iso8211FieldDefinition definition,
        int fieldControlLength = DefaultFieldControlLength,
        Iso8211WriterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        options ??= Iso8211WriterOptions.Default;

        var unitTerminator = options.TerminatorWidth >= 2 ? new byte[] { 0x1F, 0x00 } : new byte[] { 0x1F };
        var data = new List<byte>();

        AppendFieldControls(data, definition, fieldControlLength);

        // NAME section.
        data.AddRange(Encoding.ASCII.GetBytes(definition.FieldName ?? string.Empty));
        data.AddRange(unitTerminator);

        // LBLS (subfield descriptors) section.
        data.AddRange(Encoding.ASCII.GetBytes(BuildDescriptorString(definition)));
        data.AddRange(unitTerminator);

        // FMTS (format controls) section.
        data.AddRange(Encoding.ASCII.GetBytes(definition.FormatControls ?? string.Empty));

        return new Iso8211Field
        {
            Tag = definition.Tag,
            Data = data.ToArray()
        };
    }

    /// <summary>
    /// Builds a complete DDR <see cref="Iso8211Record"/> from a set of field definitions.
    /// </summary>
    /// <param name="fieldDefinitions">The field definitions to encode, in order.</param>
    /// <param name="fieldControlLength">The DDR field control length (number of field-control bytes).</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <returns>A DDR record (leader identifier <c>'L'</c>).</returns>
    public static Iso8211Record BuildDdr(
        IEnumerable<Iso8211FieldDefinition> fieldDefinitions,
        int fieldControlLength = DefaultFieldControlLength,
        Iso8211WriterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(fieldDefinitions);
        options ??= Iso8211WriterOptions.Default;

        var builder = new Iso8211RecordBuilder(options)
        {
            LeaderIdentifier = 'L',
            FieldControlLength = fieldControlLength
        };

        foreach (var definition in fieldDefinitions)
        {
            builder.AddField(EncodeFieldDefinition(definition, fieldControlLength, options));
        }

        return builder.Build();
    }

    private static void AppendFieldControls(List<byte> data, Iso8211FieldDefinition definition, int fieldControlLength)
    {
        if (fieldControlLength >= 1)
        {
            data.Add((byte)('0' + (int)definition.DataStructureCode));
        }
        if (fieldControlLength >= 2)
        {
            data.Add((byte)('0' + (int)definition.DataTypeCode));
        }

        var fillerLength = fieldControlLength - 2;
        if (fillerLength > 0)
        {
            // Filler bytes are ignored by the reader; use the conventional S-57 trailer
            // (auxiliary controls '0' padding followed by the printable graphic ';' and escape '&').
            var filler = new char[fillerLength];
            for (int i = 0; i < fillerLength; i++)
            {
                filler[i] = '0';
            }
            if (fillerLength >= 2)
            {
                filler[fillerLength - 2] = ';';
                filler[fillerLength - 1] = '&';
            }
            data.AddRange(Encoding.ASCII.GetBytes(filler));
        }
    }

    private static string BuildDescriptorString(Iso8211FieldDefinition definition)
    {
        var subfields = definition.SubfieldDefinitions;
        if (subfields is null || subfields.Count == 0)
        {
            // No subfield labels: fall back to any array descriptor, otherwise empty.
            return definition.ArrayDescriptor ?? string.Empty;
        }

        var repeatingStart = definition.RepeatingSubfieldStartIndex;
        var sb = new StringBuilder();
        for (int i = 0; i < subfields.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('!');
            }
            if (i == repeatingStart)
            {
                sb.Append('*');
            }
            sb.Append(subfields[i].Name);
        }
        return sb.ToString();
    }
}
