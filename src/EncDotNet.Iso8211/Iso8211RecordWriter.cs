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
/// <para>
/// The leader holds the record length and the base address of the field area in five numeric
/// characters each (S-57 Part 3 Annex A.3), so neither can exceed 99 999. A record longer than
/// that is written with a record length of <c>00000</c>, as producers of such records do; the
/// reader re-derives the true length from the directory. A base address above 99 999 cannot be
/// written that way, because the reader needs it to locate the directory at all, so encoding
/// such a record throws <see cref="InvalidOperationException"/>.
/// </para>
/// </remarks>
internal static class Iso8211RecordWriter
{
    private const int LeaderLength = 24;
    private const byte FieldTerminator = 0x1E;
    private const int MaxLeaderNumericValue = 99_999;
    private const int UnspecifiedRecordLength = 0;

    /// <summary>
    /// Computes a record's layout and checks that its leader can be encoded, without emitting
    /// any bytes, so a document's records can all be checked before any of them is written.
    /// </summary>
    /// <param name="record">The record to encode.</param>
    /// <param name="options">The writer options.</param>
    /// <returns>The layout to pass to <see cref="Write(Iso8211Record, Iso8211RecordLayout, Span{byte})"/>.</returns>
    /// <exception cref="InvalidOperationException">The record cannot be encoded.</exception>
    public static Iso8211RecordLayout Plan(Iso8211Record record, Iso8211WriterOptions options)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(options);

        var layout = ComputeLayout(record.Leader, record.Fields, options);

        // The field control length has two characters in the leader.
        var fieldControlLength = record.Leader.FieldControlLength;
        if (fieldControlLength is < 0 or > 99)
        {
            var encodedLength = LeaderLength - 2 + fieldControlLength.ToString("D2", CultureInfo.InvariantCulture).Length;
            throw new InvalidOperationException($"Encoded leader length was {encodedLength}; expected {LeaderLength}.");
        }

        return layout;
    }

    /// <summary>
    /// Encodes the specified record into <paramref name="destination"/>, which must hold
    /// exactly <see cref="Iso8211RecordLayout.RecordLength"/> bytes.
    /// </summary>
    /// <param name="record">The record to encode.</param>
    /// <param name="layout">The record's layout, from <see cref="Plan"/>.</param>
    /// <param name="destination">The bytes to write the record to.</param>
    public static void Write(Iso8211Record record, Iso8211RecordLayout layout, Span<byte> destination)
    {
        var fields = record.Fields;

        WriteLeader(destination, record.Leader, layout.RecordLength, layout.BaseAddress, layout.LengthSize, layout.PositionSize, layout.TagSize);
        var offset = LeaderLength;

        // Directory. Each field's length includes its terminator, and its position is the sum
        // of the lengths before it.
        var position = 0;
        for (int i = 0; i < fields.Count; i++)
        {
            var length = fields[i].Data.Length + 1;
            WriteTag(destination.Slice(offset, layout.TagSize), fields[i].Tag);
            offset += layout.TagSize;
            WriteNumeric(destination.Slice(offset, layout.LengthSize), length);
            offset += layout.LengthSize;
            WriteNumeric(destination.Slice(offset, layout.PositionSize), position);
            offset += layout.PositionSize;
            position += length;
        }
        destination[offset++] = FieldTerminator;

        // Field area.
        for (int i = 0; i < fields.Count; i++)
        {
            var data = fields[i].Data.Span;
            data.CopyTo(destination[offset..]);
            offset += data.Length;
            destination[offset++] = FieldTerminator;
        }

        if (offset != layout.RecordLength)
        {
            throw new InvalidOperationException($"Encoded record length was {offset}; expected {layout.RecordLength}.");
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
    /// <exception cref="InvalidOperationException">
    /// Thrown when the base address of the field area exceeds the leader's five-digit maximum.
    /// </exception>
    public static Iso8211RecordLayout ComputeLayout(
        Iso8211RecordLeader leader,
        IReadOnlyList<Iso8211Field> fields,
        Iso8211WriterOptions options)
    {
        var fieldCount = fields.Count;
        var fieldAreaLength = 0;
        var maxLength = 0;
        var maxPosition = 0;

        for (int i = 0; i < fieldCount; i++)
        {
            var length = fields[i].Data.Length + 1; // +1 for the field terminator
            maxLength = Math.Max(maxLength, length);
            maxPosition = Math.Max(maxPosition, fieldAreaLength);
            fieldAreaLength += length;
        }

        var tagSize = DetermineTagSize(leader, fields, options);
        var lengthSize = DetermineNumericSize(leader.SizeOfFieldLengthField, maxLength, options);
        var positionSize = DetermineNumericSize(leader.SizeOfFieldPositionField, maxPosition, options);

        var entrySize = tagSize + lengthSize + positionSize;
        var directoryBytes = entrySize * fieldCount;
        var baseAddress = LeaderLength + directoryBytes + 1; // +1 for the directory field terminator
        if (baseAddress > MaxLeaderNumericValue)
        {
            throw new InvalidOperationException(
                $"The record's directory places its field area at offset {baseAddress}, which exceeds the leader's five-digit maximum of {MaxLeaderNumericValue}.");
        }

        var recordLength = baseAddress + fieldAreaLength;

        return new Iso8211RecordLayout(tagSize, lengthSize, positionSize, baseAddress, recordLength);
    }

    private static void WriteLeader(
        Span<byte> output,
        Iso8211RecordLeader leader,
        int recordLength,
        int baseAddress,
        int lengthSize,
        int positionSize,
        int tagSize)
    {
        Span<char> text = stackalloc char[LeaderLength];

        // A record length beyond five digits is written as "00000"; the reader recovers it
        // from the directory.
        FormatDigits(text[..5], recordLength > MaxLeaderNumericValue ? UnspecifiedRecordLength : recordLength);
        text[5] = OrDefault(leader.InterchangeLevel, '3');
        text[6] = OrDefault(leader.LeaderIdentifier, 'D');
        text[7] = OrDefault(leader.InlineCodeExtensionIndicator, 'E');
        text[8] = OrDefault(leader.VersionNumber, '1');
        text[9] = OrDefault(leader.ApplicationIndicator, ' ');
        FormatDigits(text.Slice(10, 2), leader.FieldControlLength);
        FormatDigits(text.Slice(12, 5), baseAddress);
        FitExtendedCharset(text.Slice(17, 3), leader.ExtendedCharacterSetIndicator);
        text[20] = (char)('0' + lengthSize);
        text[21] = (char)('0' + positionSize);
        text[22] = '0'; // Reserved.
        text[23] = (char)('0' + tagSize);

        // ASCII, with '?' for anything outside it, as the leader has always been encoded.
        var written = Encoding.ASCII.GetBytes(text, output[..LeaderLength]);
        if (written != LeaderLength)
        {
            throw new InvalidOperationException($"Encoded leader length was {written}; expected {LeaderLength}.");
        }
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

    // The largest value has the most digits, so it alone decides the size.
    private static int DetermineNumericSize(int declaredSize, int maxValue, Iso8211WriterOptions options)
    {
        var required = maxValue == 0 ? 1 : (int)Math.Floor(Math.Log10(maxValue)) + 1;

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

    private static void WriteTag(Span<byte> output, string tag)
    {
        tag ??= string.Empty;
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = i < tag.Length ? (byte)tag[i] : (byte)' ';
        }
    }

    // Writes a non-negative value as decimal digits, zero-padded to fill the output.
    private static void WriteNumeric(Span<byte> output, int value)
    {
        var remaining = value;
        for (int i = output.Length - 1; i >= 0; i--)
        {
            output[i] = (byte)('0' + (remaining % 10));
            remaining /= 10;
        }

        if (remaining != 0)
        {
            throw new InvalidOperationException($"Value {value} does not fit in {output.Length} digit(s).");
        }
    }

    // Writes a value from 0 to the width's maximum as zero-padded decimal digits.
    private static void FormatDigits(Span<char> output, int value)
    {
        for (int i = output.Length - 1; i >= 0; i--)
        {
            output[i] = (char)('0' + (value % 10));
            value /= 10;
        }
    }

    private static void FitExtendedCharset(Span<char> output, string? value)
    {
        value ??= "   ";
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = i < value.Length ? value[i] : ' ';
        }
    }

    private static char OrDefault(char value, char fallback) => value == '\0' ? fallback : value;
}

/// <summary>
/// Describes the computed directory layout of an ISO 8211 record.
/// </summary>
internal readonly struct Iso8211RecordLayout
{
    public Iso8211RecordLayout(
        int tagSize,
        int lengthSize,
        int positionSize,
        int baseAddress,
        int recordLength)
    {
        TagSize = tagSize;
        LengthSize = lengthSize;
        PositionSize = positionSize;
        BaseAddress = baseAddress;
        RecordLength = recordLength;
    }

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
