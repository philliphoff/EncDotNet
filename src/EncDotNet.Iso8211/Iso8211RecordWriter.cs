using System.Globalization;
using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Encodes a single <see cref="Iso8211Record"/> into its ISO 8211 byte representation.
/// </summary>
/// <remarks>
/// <para>
/// This type is the symmetric inverse of the record parsing performed by
/// <see cref="Iso8211DocumentReader"/>. It emits the 24-byte record leader, the directory
/// (one entry per field, terminated by a field terminator), and the field area (each field's
/// data followed by a field terminator).
/// </para>
/// <para>
/// The record length and base address of the field area are always recomputed from the
/// record's fields, while the leader flag characters and — unless
/// <see cref="Iso8211WriterOptions.AutoSizeDirectoryEntries"/> is enabled — the directory
/// entry-map sizes are taken from <see cref="Iso8211Record.Leader"/>. This yields byte-identical
/// output for canonically-encoded sources read by <see cref="Iso8211DocumentReader"/>.
/// </para>
/// </remarks>
internal static class Iso8211RecordWriter
{
    private const int LeaderLength = 24;
    private const byte FieldTerminator = 0x1E;

    /// <summary>
    /// Encodes the specified record and appends the resulting bytes to <paramref name="output"/>.
    /// </summary>
    /// <param name="record">The record to encode.</param>
    /// <param name="options">The writer options.</param>
    /// <param name="output">The destination buffer to append the encoded record to.</param>
    public static void Write(Iso8211Record record, Iso8211WriterOptions options, List<byte> output)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(output);

        var fields = record.Fields;
        var layout = ComputeLayout(record.Leader, fields, options);

        WriteLeader(output, record.Leader, layout.RecordLength, layout.BaseAddress, layout.LengthSize, layout.PositionSize, layout.TagSize);

        // Directory.
        for (int i = 0; i < fields.Count; i++)
        {
            WriteTag(output, fields[i].Tag, layout.TagSize);
            WriteNumeric(output, layout.Lengths[i], layout.LengthSize);
            WriteNumeric(output, layout.Positions[i], layout.PositionSize);
        }
        output.Add(FieldTerminator);

        // Field area.
        for (int i = 0; i < fields.Count; i++)
        {
            output.AddRange(fields[i].Data);
            output.Add(FieldTerminator);
        }
    }

    /// <summary>
    /// Computes the directory layout (entry-map sizes, per-field lengths and positions, base
    /// address, and record length) for a record without emitting any bytes.
    /// </summary>
    /// <param name="leader">The record leader supplying declared entry-map sizes.</param>
    /// <param name="fields">The record's fields.</param>
    /// <param name="options">The writer options.</param>
    /// <returns>The computed record layout.</returns>
    public static Iso8211RecordLayout ComputeLayout(
        Iso8211RecordLeader leader,
        IReadOnlyList<Iso8211Field> fields,
        Iso8211WriterOptions options)
    {
        var fieldCount = fields.Count;
        var fieldLengths = new int[fieldCount];
        var fieldPositions = new int[fieldCount];
        var fieldAreaLength = 0;

        for (int i = 0; i < fieldCount; i++)
        {
            var length = fields[i].Data.Length + 1; // +1 for the field terminator
            fieldLengths[i] = length;
            fieldPositions[i] = fieldAreaLength;
            fieldAreaLength += length;
        }

        var tagSize = DetermineTagSize(leader, fields, options);
        var lengthSize = DetermineNumericSize(leader.SizeOfFieldLengthField, fieldLengths, options);
        var positionSize = DetermineNumericSize(leader.SizeOfFieldPositionField, fieldPositions, options);

        var entrySize = tagSize + lengthSize + positionSize;
        var directoryBytes = entrySize * fieldCount;
        var baseAddress = LeaderLength + directoryBytes + 1; // +1 for the directory field terminator
        var recordLength = baseAddress + fieldAreaLength;

        return new Iso8211RecordLayout(fieldLengths, fieldPositions, tagSize, lengthSize, positionSize, baseAddress, recordLength);
    }

    private static void WriteLeader(
        List<byte> output,
        Iso8211RecordLeader leader,
        int recordLength,
        int baseAddress,
        int lengthSize,
        int positionSize,
        int tagSize)
    {
        var sb = new StringBuilder(LeaderLength);
        sb.Append(recordLength.ToString("D5", CultureInfo.InvariantCulture));
        sb.Append(OrDefault(leader.InterchangeLevel, '3'));
        sb.Append(OrDefault(leader.LeaderIdentifier, 'D'));
        sb.Append(OrDefault(leader.InlineCodeExtensionIndicator, 'E'));
        sb.Append(OrDefault(leader.VersionNumber, '1'));
        sb.Append(OrDefault(leader.ApplicationIndicator, ' '));
        sb.Append(leader.FieldControlLength.ToString("D2", CultureInfo.InvariantCulture));
        sb.Append(baseAddress.ToString("D5", CultureInfo.InvariantCulture));
        sb.Append(FitExtendedCharset(leader.ExtendedCharacterSetIndicator));
        sb.Append((char)('0' + lengthSize));
        sb.Append((char)('0' + positionSize));
        sb.Append('0'); // Reserved.
        sb.Append((char)('0' + tagSize));

        var leaderText = sb.ToString();
        if (leaderText.Length != LeaderLength)
        {
            throw new InvalidOperationException($"Encoded leader length was {leaderText.Length}; expected {LeaderLength}.");
        }

        output.AddRange(Encoding.ASCII.GetBytes(leaderText));
    }

    private static int DetermineTagSize(Iso8211RecordLeader leader, IReadOnlyList<Iso8211Field> fields, Iso8211WriterOptions options)
    {
        var maxTag = 0;
        foreach (var field in fields)
        {
            if (field.Tag.Length > maxTag)
            {
                maxTag = field.Tag.Length;
            }
        }

        if (!options.AutoSizeDirectoryEntries && leader.SizeOfFieldTagField > 0)
        {
            if (maxTag > leader.SizeOfFieldTagField)
            {
                throw new InvalidOperationException(
                    $"A field tag of length {maxTag} does not fit the leader-declared tag size of {leader.SizeOfFieldTagField}.");
            }
            return leader.SizeOfFieldTagField;
        }

        return Math.Max(maxTag, 1);
    }

    private static int DetermineNumericSize(int declaredSize, int[] values, Iso8211WriterOptions options)
    {
        var required = 1;
        foreach (var value in values)
        {
            var digits = value == 0 ? 1 : (int)Math.Floor(Math.Log10(value)) + 1;
            if (digits > required)
            {
                required = digits;
            }
        }

        if (!options.AutoSizeDirectoryEntries && declaredSize > 0)
        {
            if (required > declaredSize)
            {
                throw new InvalidOperationException(
                    $"A directory value requires {required} digits, which exceeds the leader-declared size of {declaredSize}.");
            }
            return declaredSize;
        }

        return required;
    }

    private static void WriteTag(List<byte> output, string tag, int tagSize)
    {
        tag ??= string.Empty;
        for (int i = 0; i < tagSize; i++)
        {
            output.Add(i < tag.Length ? (byte)tag[i] : (byte)' ');
        }
    }

    private static void WriteNumeric(List<byte> output, int value, int size)
    {
        var text = value.ToString(CultureInfo.InvariantCulture);
        if (text.Length > size)
        {
            throw new InvalidOperationException($"Value {value} does not fit in {size} digit(s).");
        }
        for (int i = 0; i < size - text.Length; i++)
        {
            output.Add((byte)'0');
        }
        for (int i = 0; i < text.Length; i++)
        {
            output.Add((byte)text[i]);
        }
    }

    private static string FitExtendedCharset(string? value)
    {
        value ??= "   ";
        if (value.Length == 3)
        {
            return value;
        }
        if (value.Length > 3)
        {
            return value.Substring(0, 3);
        }
        return value.PadRight(3, ' ');
    }

    private static char OrDefault(char value, char fallback) => value == '\0' ? fallback : value;
}

/// <summary>
/// Describes the computed directory layout of an ISO 8211 record.
/// </summary>
internal readonly struct Iso8211RecordLayout
{
    public Iso8211RecordLayout(
        int[] lengths,
        int[] positions,
        int tagSize,
        int lengthSize,
        int positionSize,
        int baseAddress,
        int recordLength)
    {
        Lengths = lengths;
        Positions = positions;
        TagSize = tagSize;
        LengthSize = lengthSize;
        PositionSize = positionSize;
        BaseAddress = baseAddress;
        RecordLength = recordLength;
    }

    /// <summary>Gets the per-field lengths, including each field's trailing terminator.</summary>
    public int[] Lengths { get; }

    /// <summary>Gets the per-field positions within the field area.</summary>
    public int[] Positions { get; }

    /// <summary>Gets the size of the field tag field in directory entries.</summary>
    public int TagSize { get; }

    /// <summary>Gets the size of the field length field in directory entries.</summary>
    public int LengthSize { get; }

    /// <summary>Gets the size of the field position field in directory entries.</summary>
    public int PositionSize { get; }

    /// <summary>Gets the base address of the field area.</summary>
    public int BaseAddress { get; }

    /// <summary>Gets the total record length in bytes.</summary>
    public int RecordLength { get; }
}
