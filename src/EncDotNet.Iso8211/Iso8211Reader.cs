using System.Collections.Immutable;
using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a complete ISO 8211 document containing multiple records.
/// </summary>
public sealed class Iso8211Document
{
    /// <summary>
    /// Gets the records contained in this document.
    /// </summary>
    public ImmutableArray<Iso8211Record> Records { get; init; }

    /// <summary>
    /// Gets the Data Descriptive Record (DDR) if present.
    /// </summary>
    public Iso8211Record? DataDescriptiveRecord => Records.Length > 0 && Records[0].IsDataDescriptiveRecord ? Records[0] : null;

    /// <summary>
    /// Gets all data records (non-DDR records).
    /// </summary>
    public IEnumerable<Iso8211Record> DataRecords => Records.Where(r => !r.IsDataDescriptiveRecord);
}

/// <summary>
/// Represents a single ISO 8211 record with its leader, directory, and fields.
/// </summary>
public sealed class Iso8211Record
{
    /// <summary>
    /// Gets the leader information for this record.
    /// </summary>
    public Iso8211RecordLeader Leader { get; init; } = default!;

    /// <summary>
    /// Gets the directory entries for this record.
    /// </summary>
    public ImmutableArray<Iso8211DirectoryEntry> Directory { get; init; }

    /// <summary>
    /// Gets the fields contained in this record.
    /// </summary>
    public ImmutableArray<Iso8211Field> Fields { get; init; }

    /// <summary>
    /// Gets whether this record is a Data Descriptive Record (DDR).
    /// </summary>
    public bool IsDataDescriptiveRecord => Leader.LeaderIdentifier == 'L';

    /// <summary>
    /// Gets a field by its tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>The field with the specified tag, or null if not found.</returns>
    public Iso8211Field? GetFieldByTag(string tag) => Fields.FirstOrDefault(f => f.Tag == tag);

    /// <summary>
    /// Gets all fields with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>All fields with the specified tag.</returns>
    public IEnumerable<Iso8211Field> GetFieldsByTag(string tag) => Fields.Where(f => f.Tag == tag);
}

/// <summary>
/// Represents the leader information for an ISO 8211 record.
/// </summary>
public readonly struct Iso8211RecordLeader
{
    /// <summary>
    /// Gets the total length of the record in bytes.
    /// </summary>
    public int RecordLength { get; init; }

    /// <summary>
    /// Gets the interchange level character.
    /// </summary>
    public char InterchangeLevel { get; init; }

    /// <summary>
    /// Gets the leader identifier ('L' for DDR, 'D' for data record).
    /// </summary>
    public char LeaderIdentifier { get; init; }

    /// <summary>
    /// Gets the inline code extension indicator.
    /// </summary>
    public char InlineCodeExtensionIndicator { get; init; }

    /// <summary>
    /// Gets the version number character.
    /// </summary>
    public char VersionNumber { get; init; }

    /// <summary>
    /// Gets the application indicator character.
    /// </summary>
    public char ApplicationIndicator { get; init; }

    /// <summary>
    /// Gets the field control length.
    /// </summary>
    public int FieldControlLength { get; init; }

    /// <summary>
    /// Gets the base address of the field area.
    /// </summary>
    public int BaseAddressOfFieldArea { get; init; }

    /// <summary>
    /// Gets the extended character set indicators.
    /// </summary>
    public string ExtendedCharacterSetIndicator { get; init; }

    /// <summary>
    /// Gets the size of the field length field in directory entries.
    /// </summary>
    public int SizeOfFieldLengthField { get; init; }

    /// <summary>
    /// Gets the size of the field position field in directory entries.
    /// </summary>
    public int SizeOfFieldPositionField { get; init; }

    /// <summary>
    /// Gets the size of the field tag field in directory entries.
    /// </summary>
    public int SizeOfFieldTagField { get; init; }

    /// <summary>
    /// Creates an <see cref="Iso8211RecordLeader"/> from an <see cref="Iso8211Leader"/>.
    /// </summary>
    internal static Iso8211RecordLeader FromLeader(Iso8211Leader leader) => new()
    {
        RecordLength = leader.RecordLength,
        InterchangeLevel = leader.InterchangeLevel,
        LeaderIdentifier = leader.LeaderIdentifier,
        InlineCodeExtensionIndicator = leader.InlineCodeExtensionIndicator,
        VersionNumber = leader.VersionNumber,
        ApplicationIndicator = leader.ApplicationIndicator,
        FieldControlLength = leader.FieldControlLength,
        BaseAddressOfFieldArea = leader.BaseAddressOfFieldArea,
        ExtendedCharacterSetIndicator = $"{leader.ExtendedCharacterSetIndicator0}{leader.ExtendedCharacterSetIndicator1}{leader.ExtendedCharacterSetIndicator2}",
        SizeOfFieldLengthField = leader.SizeOfFieldLengthField,
        SizeOfFieldPositionField = leader.SizeOfFieldPositionField,
        SizeOfFieldTagField = leader.SizeOfFieldTagField
    };
}

/// <summary>
/// Represents a directory entry within an ISO 8211 record.
/// </summary>
public sealed class Iso8211DirectoryEntry
{
    /// <summary>
    /// Gets the field tag.
    /// </summary>
    public string Tag { get; init; } = string.Empty;

    /// <summary>
    /// Gets the field length in bytes.
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// Gets the field position within the field area.
    /// </summary>
    public int Position { get; init; }
}

/// <summary>
/// Represents a field within an ISO 8211 record.
/// </summary>
public sealed class Iso8211Field
{
    /// <summary>
    /// Gets the field tag.
    /// </summary>
    public string Tag { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw field data.
    /// </summary>
    public byte[] Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Gets the subfields contained in this field.
    /// </summary>
    public ImmutableArray<Iso8211Subfield> Subfields { get; init; }

    /// <summary>
    /// Gets the field data as a string using the specified encoding.
    /// </summary>
    /// <param name="encoding">The encoding to use. Defaults to ASCII.</param>
    /// <returns>The field data as a string.</returns>
    public string GetDataString(Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        return encoding.GetString(Data).TrimEnd('\x1f', '\x1e');
    }
}

/// <summary>
/// Represents a subfield within an ISO 8211 field.
/// </summary>
public sealed class Iso8211Subfield
{
    /// <summary>
    /// Gets the subfield index within the parent field.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets the raw subfield data.
    /// </summary>
    public byte[] Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Gets the subfield data as a string using the specified encoding.
    /// </summary>
    /// <param name="encoding">The encoding to use. Defaults to ASCII.</param>
    /// <returns>The subfield data as a string.</returns>
    public string GetDataString(Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        return encoding.GetString(Data);
    }
}

/// <summary>
/// Provides methods to read ISO 8211 formatted data and return structured objects.
/// </summary>
/// <remarks>
/// This reader uses <see cref="ForwardOnlyIso8211Reader"/> internally for parsing
/// and builds a complete object model of the ISO 8211 data.
/// </remarks>
public static class Iso8211Reader
{
    /// <summary>
    /// Reads an ISO 8211 document from a byte array.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document Read(byte[] data)
    {
        return Read(data.AsSpan());
    }

    /// <summary>
    /// Reads an ISO 8211 document from a span of bytes.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document Read(ReadOnlySpan<byte> data)
    {
        var reader = new ForwardOnlyIso8211Reader(data);
        return Read(ref reader);
    }

    /// <summary>
    /// Reads an ISO 8211 document from a file.
    /// </summary>
    /// <param name="path">The path to the ISO 8211 file.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document ReadFromFile(string path)
    {
        var data = File.ReadAllBytes(path);
        return Read(data);
    }

    /// <summary>
    /// Asynchronously reads an ISO 8211 document from a file.
    /// </summary>
    /// <param name="path">The path to the ISO 8211 file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<Iso8211Document> ReadFromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(data);
    }

    /// <summary>
    /// Reads an ISO 8211 document from a stream.
    /// </summary>
    /// <param name="stream">The stream containing ISO 8211 data.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document Read(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return Read(memoryStream.ToArray());
    }

    /// <summary>
    /// Asynchronously reads an ISO 8211 document from a stream.
    /// </summary>
    /// <param name="stream">The stream containing ISO 8211 data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<Iso8211Document> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        return Read(memoryStream.ToArray());
    }

    /// <summary>
    /// Reads an ISO 8211 document using a ForwardOnlyIso8211Reader.
    /// </summary>
    /// <param name="reader">The forward-only reader to use for parsing.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document Read(ref ForwardOnlyIso8211Reader reader)
    {
        var records = ImmutableArray.CreateBuilder<Iso8211Record>();

        while (reader.Read())
        {
            if (reader.TokenType == Iso8211TokenType.StartRecord)
            {
                var record = ReadRecord(ref reader);
                records.Add(record);
            }
            else if (reader.TokenType == Iso8211TokenType.EndOfData)
            {
                break;
            }
        }

        return new Iso8211Document
        {
            Records = records.ToImmutable()
        };
    }

    /// <summary>
    /// Reads a single ISO 8211 record from the reader.
    /// </summary>
    /// <param name="reader">The forward-only reader positioned at a StartRecord token.</param>
    /// <returns>The parsed record.</returns>
    private static Iso8211Record ReadRecord(ref ForwardOnlyIso8211Reader reader)
    {
        if (reader.TokenType != Iso8211TokenType.StartRecord)
        {
            throw new InvalidOperationException("Reader must be positioned at a StartRecord token.");
        }

        var leader = Iso8211RecordLeader.FromLeader(reader.CurrentLeader);
        var directoryEntries = ImmutableArray.CreateBuilder<Iso8211DirectoryEntry>();
        var fields = ImmutableArray.CreateBuilder<Iso8211Field>();

        // Read directory entries
        while (reader.Read())
        {
            if (reader.TokenType == Iso8211TokenType.DirectoryEntry)
            {
                var entry = new Iso8211DirectoryEntry
                {
                    Tag = reader.GetTagString(),
                    Length = reader.CurrentLength,
                    Position = reader.CurrentPosition
                };
                directoryEntries.Add(entry);
            }
            else if (reader.TokenType == Iso8211TokenType.Field)
            {
                // Read first field and break to field reading loop
                var field = ReadField(ref reader);
                fields.Add(field);
                break;
            }
            else if (reader.TokenType == Iso8211TokenType.EndRecord)
            {
                // Record with no fields
                break;
            }
        }

        // Read remaining fields
        while (reader.Read())
        {
            if (reader.TokenType == Iso8211TokenType.Field)
            {
                var field = ReadField(ref reader);
                fields.Add(field);
            }
            else if (reader.TokenType == Iso8211TokenType.EndRecord)
            {
                break;
            }
        }

        return new Iso8211Record
        {
            Leader = leader,
            Directory = directoryEntries.ToImmutable(),
            Fields = fields.ToImmutable()
        };
    }

    /// <summary>
    /// Reads a single field from the reader.
    /// </summary>
    /// <param name="reader">The forward-only reader positioned at a Field token.</param>
    /// <returns>The parsed field.</returns>
    private static Iso8211Field ReadField(ref ForwardOnlyIso8211Reader reader)
    {
        if (reader.TokenType != Iso8211TokenType.Field)
        {
            throw new InvalidOperationException("Reader must be positioned at a Field token.");
        }

        var tag = reader.GetTagString();
        var data = reader.ValueSpan.ToArray();
        var subfields = ReadSubfields(ref reader);

        return new Iso8211Field
        {
            Tag = tag,
            Data = data,
            Subfields = subfields
        };
    }

    /// <summary>
    /// Reads all subfields from the current field.
    /// </summary>
    /// <param name="reader">The forward-only reader positioned at a Field token.</param>
    /// <returns>The parsed subfields.</returns>
    private static ImmutableArray<Iso8211Subfield> ReadSubfields(ref ForwardOnlyIso8211Reader reader)
    {
        var subfields = ImmutableArray.CreateBuilder<Iso8211Subfield>();
        int index = 0;

        while (reader.ReadSubfield())
        {
            var subfield = new Iso8211Subfield
            {
                Index = index++,
                Data = reader.CurrentSubfieldData.ToArray()
            };
            subfields.Add(subfield);
        }

        return subfields.ToImmutable();
    }
}
