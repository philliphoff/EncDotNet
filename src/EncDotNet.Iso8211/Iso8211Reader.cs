using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Represents metadata about an ISO 8211 field within a record.
/// </summary>
public sealed record Iso8211FieldMetadata(
    string Tag,
    int Length,
    int Position,
    byte[] Data)
{
    /// <summary>
    /// Gets the field data as a string using the specified encoding.
    /// </summary>
    public string GetDataAsString(Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        return encoding.GetString(Data).TrimEnd('\x1f', '\x1e');
    }
}

/// <summary>
/// Represents a directory entry in an ISO 8211 record.
/// </summary>
public sealed record Iso8211DirectoryEntry(
    string Tag,
    int Length,
    int Position);

/// <summary>
/// Represents metadata about an ISO 8211 record.
/// </summary>
public sealed record Iso8211RecordMetadata(
    int RecordLength,
    char InterchangeLevel,
    char LeaderIdentifier,
    char InlineCodeExtensionIndicator,
    char VersionNumber,
    char ApplicationIndicator,
    int FieldControlLength,
    int BaseAddressOfFieldArea,
    char ExtendedCharacterSetIndicator0,
    char ExtendedCharacterSetIndicator1,
    char ExtendedCharacterSetIndicator2,
    int SizeOfFieldLengthField,
    int SizeOfFieldPositionField,
    int Reserved,
    int SizeOfFieldTagField,
    IReadOnlyList<Iso8211DirectoryEntry> DirectoryEntries,
    IReadOnlyList<Iso8211FieldMetadata> Fields);

/// <summary>
/// Represents the Data Descriptive Record (DDR) which describes the structure of data records.
/// </summary>
public sealed record Iso8211DataDescriptiveRecord(
    Iso8211RecordMetadata Metadata,
    IReadOnlyDictionary<string, Iso8211FieldDescriptor> FieldDescriptors);

/// <summary>
/// Describes a field's structure and format.
/// </summary>
public sealed record Iso8211FieldDescriptor(
    string Tag,
    string Name,
    string ArrayDescriptor,
    string FormatControls);

/// <summary>
/// Represents a complete ISO 8211 file with its DDR and data records.
/// </summary>
public sealed record Iso8211File(
    Iso8211DataDescriptiveRecord DataDescriptiveRecord,
    IReadOnlyList<Iso8211RecordMetadata> DataRecords);

/// <summary>
/// Reads ISO 8211 (ISO/IEC 8211) formatted files and returns metadata about records, directories, and fields.
/// </summary>
public sealed class Iso8211Reader : IDisposable
{
    private const int LeaderLength = 24;
    private const byte FieldTerminator = 0x1E;
    private const byte UnitTerminator = 0x1F;

    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211Reader"/> class.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="leaveOpen">Whether to leave the stream open when the reader is disposed.</param>
    public Iso8211Reader(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// Opens an ISO 8211 file for reading.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A new <see cref="Iso8211Reader"/> instance.</returns>
    public static Iso8211Reader Open(string path)
    {
        var stream = File.OpenRead(path);
        return new Iso8211Reader(stream, leaveOpen: false);
    }

    /// <summary>
    /// Reads the entire ISO 8211 file and returns its structure.
    /// </summary>
    /// <returns>An <see cref="Iso8211File"/> containing the DDR and all data records.</returns>
    public Iso8211File ReadFile()
    {
        // Reset stream position
        _stream.Position = 0;

        // Read the Data Descriptive Record (DDR) - always the first record
        var ddrMetadata = ReadRecord();
        if (ddrMetadata is null)
        {
            throw new InvalidDataException("Failed to read Data Descriptive Record.");
        }

        var ddr = ParseDataDescriptiveRecord(ddrMetadata);

        // Read all data records
        var dataRecords = new List<Iso8211RecordMetadata>();
        while (true)
        {
            var record = ReadRecord();
            if (record is null)
            {
                break;
            }
            dataRecords.Add(record);
        }

        return new Iso8211File(ddr, dataRecords);
    }

    /// <summary>
    /// Reads the next record from the stream.
    /// </summary>
    /// <returns>The record metadata, or null if end of stream.</returns>
    public Iso8211RecordMetadata? ReadRecord()
    {
        // Read the 24-byte leader
        var leader = new byte[LeaderLength];
        var bytesRead = _stream.Read(leader, 0, LeaderLength);
        
        if (bytesRead == 0)
        {
            return null; // End of file
        }

        if (bytesRead < LeaderLength)
        {
            throw new InvalidDataException($"Incomplete leader: expected {LeaderLength} bytes, got {bytesRead}.");
        }

        // Parse leader fields
        var recordLength = ParseNumeric(leader, 0, 5);
        var interchangeLevel = (char)leader[5];
        var leaderIdentifier = (char)leader[6];
        var inlineCodeExtensionIndicator = (char)leader[7];
        var versionNumber = (char)leader[8];
        var applicationIndicator = (char)leader[9];
        var fieldControlLength = ParseNumeric(leader, 10, 2);
        var baseAddressOfFieldArea = ParseNumeric(leader, 12, 5);
        var extendedCharacterSetIndicator0 = (char)leader[17];
        var extendedCharacterSetIndicator1 = (char)leader[18];
        var extendedCharacterSetIndicator2 = (char)leader[19];

        // Entry map (positions 20-23)
        var sizeOfFieldLengthField = leader[20] - '0';
        var sizeOfFieldPositionField = leader[21] - '0';
        var reserved = leader[22] - '0';
        var sizeOfFieldTagField = leader[23] - '0';

        // Calculate directory size
        var directoryLength = baseAddressOfFieldArea - LeaderLength - 1; // -1 for field terminator
        var entrySize = sizeOfFieldTagField + sizeOfFieldLengthField + sizeOfFieldPositionField;
        var entryCount = directoryLength / entrySize;

        // Read directory entries
        var directoryData = new byte[directoryLength + 1]; // +1 for field terminator
        bytesRead = _stream.Read(directoryData, 0, directoryData.Length);
        if (bytesRead < directoryData.Length)
        {
            throw new InvalidDataException("Incomplete directory data.");
        }

        var directoryEntries = new List<Iso8211DirectoryEntry>();
        for (int i = 0; i < entryCount; i++)
        {
            var offset = i * entrySize;
            var tag = Encoding.ASCII.GetString(directoryData, offset, sizeOfFieldTagField);
            var length = ParseNumeric(directoryData, offset + sizeOfFieldTagField, sizeOfFieldLengthField);
            var position = ParseNumeric(directoryData, offset + sizeOfFieldTagField + sizeOfFieldLengthField, sizeOfFieldPositionField);
            directoryEntries.Add(new Iso8211DirectoryEntry(tag, length, position));
        }

        // Read field area
        var fieldAreaLength = recordLength - baseAddressOfFieldArea;
        var fieldAreaData = new byte[fieldAreaLength];
        bytesRead = _stream.Read(fieldAreaData, 0, fieldAreaLength);
        if (bytesRead < fieldAreaLength)
        {
            throw new InvalidDataException("Incomplete field area data.");
        }

        // Extract fields based on directory entries
        var fields = new List<Iso8211FieldMetadata>();
        foreach (var entry in directoryEntries)
        {
            // Length includes the field terminator
            var dataLength = entry.Length - 1;
            var data = new byte[dataLength];
            Array.Copy(fieldAreaData, entry.Position, data, 0, dataLength);
            fields.Add(new Iso8211FieldMetadata(entry.Tag, entry.Length, entry.Position, data));
        }

        return new Iso8211RecordMetadata(
            recordLength,
            interchangeLevel,
            leaderIdentifier,
            inlineCodeExtensionIndicator,
            versionNumber,
            applicationIndicator,
            fieldControlLength,
            baseAddressOfFieldArea,
            extendedCharacterSetIndicator0,
            extendedCharacterSetIndicator1,
            extendedCharacterSetIndicator2,
            sizeOfFieldLengthField,
            sizeOfFieldPositionField,
            reserved,
            sizeOfFieldTagField,
            directoryEntries,
            fields);
    }

    /// <summary>
    /// Reads all records from the stream.
    /// </summary>
    /// <returns>An enumerable of record metadata.</returns>
    public IEnumerable<Iso8211RecordMetadata> ReadRecords()
    {
        while (true)
        {
            var record = ReadRecord();
            if (record is null)
            {
                yield break;
            }
            yield return record;
        }
    }

    /// <summary>
    /// Asynchronously reads the next record from the stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The record metadata, or null if end of stream.</returns>
    public async Task<Iso8211RecordMetadata?> ReadRecordAsync(CancellationToken cancellationToken = default)
    {
        // Read the 24-byte leader
        var leader = new byte[LeaderLength];
        var bytesRead = await _stream.ReadAsync(leader.AsMemory(0, LeaderLength), cancellationToken);
        
        if (bytesRead == 0)
        {
            return null; // End of file
        }

        if (bytesRead < LeaderLength)
        {
            throw new InvalidDataException($"Incomplete leader: expected {LeaderLength} bytes, got {bytesRead}.");
        }

        // Parse leader fields
        var recordLength = ParseNumeric(leader, 0, 5);
        var interchangeLevel = (char)leader[5];
        var leaderIdentifier = (char)leader[6];
        var inlineCodeExtensionIndicator = (char)leader[7];
        var versionNumber = (char)leader[8];
        var applicationIndicator = (char)leader[9];
        var fieldControlLength = ParseNumeric(leader, 10, 2);
        var baseAddressOfFieldArea = ParseNumeric(leader, 12, 5);
        var extendedCharacterSetIndicator0 = (char)leader[17];
        var extendedCharacterSetIndicator1 = (char)leader[18];
        var extendedCharacterSetIndicator2 = (char)leader[19];

        // Entry map (positions 20-23)
        var sizeOfFieldLengthField = leader[20] - '0';
        var sizeOfFieldPositionField = leader[21] - '0';
        var reserved = leader[22] - '0';
        var sizeOfFieldTagField = leader[23] - '0';

        // Calculate directory size
        var directoryLength = baseAddressOfFieldArea - LeaderLength - 1;
        var entrySize = sizeOfFieldTagField + sizeOfFieldLengthField + sizeOfFieldPositionField;
        var entryCount = directoryLength / entrySize;

        // Read directory entries
        var directoryData = new byte[directoryLength + 1];
        bytesRead = await _stream.ReadAsync(directoryData.AsMemory(0, directoryData.Length), cancellationToken);
        if (bytesRead < directoryData.Length)
        {
            throw new InvalidDataException("Incomplete directory data.");
        }

        var directoryEntries = new List<Iso8211DirectoryEntry>();
        for (int i = 0; i < entryCount; i++)
        {
            var offset = i * entrySize;
            var tag = Encoding.ASCII.GetString(directoryData, offset, sizeOfFieldTagField);
            var length = ParseNumeric(directoryData, offset + sizeOfFieldTagField, sizeOfFieldLengthField);
            var position = ParseNumeric(directoryData, offset + sizeOfFieldTagField + sizeOfFieldLengthField, sizeOfFieldPositionField);
            directoryEntries.Add(new Iso8211DirectoryEntry(tag, length, position));
        }

        // Read field area
        var fieldAreaLength = recordLength - baseAddressOfFieldArea;
        var fieldAreaData = new byte[fieldAreaLength];
        bytesRead = await _stream.ReadAsync(fieldAreaData.AsMemory(0, fieldAreaLength), cancellationToken);
        if (bytesRead < fieldAreaLength)
        {
            throw new InvalidDataException("Incomplete field area data.");
        }

        // Extract fields based on directory entries
        var fields = new List<Iso8211FieldMetadata>();
        foreach (var entry in directoryEntries)
        {
            var dataLength = entry.Length - 1;
            var data = new byte[dataLength];
            Array.Copy(fieldAreaData, entry.Position, data, 0, dataLength);
            fields.Add(new Iso8211FieldMetadata(entry.Tag, entry.Length, entry.Position, data));
        }

        return new Iso8211RecordMetadata(
            recordLength,
            interchangeLevel,
            leaderIdentifier,
            inlineCodeExtensionIndicator,
            versionNumber,
            applicationIndicator,
            fieldControlLength,
            baseAddressOfFieldArea,
            extendedCharacterSetIndicator0,
            extendedCharacterSetIndicator1,
            extendedCharacterSetIndicator2,
            sizeOfFieldLengthField,
            sizeOfFieldPositionField,
            reserved,
            sizeOfFieldTagField,
            directoryEntries,
            fields);
    }

    /// <summary>
    /// Asynchronously reads all records from the stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async enumerable of record metadata.</returns>
    public async IAsyncEnumerable<Iso8211RecordMetadata> ReadRecordsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var record = await ReadRecordAsync(cancellationToken);
            if (record is null)
            {
                yield break;
            }
            yield return record;
        }
    }

    private Iso8211DataDescriptiveRecord ParseDataDescriptiveRecord(Iso8211RecordMetadata metadata)
    {
        var fieldDescriptors = new Dictionary<string, Iso8211FieldDescriptor>();

        foreach (var field in metadata.Fields)
        {
            // Skip the file control field (tag "0000" or "0001")
            if (field.Tag == "0000")
            {
                continue;
            }

            // Parse field descriptor
            var dataString = field.GetDataAsString();
            var descriptor = ParseFieldDescriptor(field.Tag, dataString, metadata.FieldControlLength);
            fieldDescriptors[field.Tag] = descriptor;
        }

        return new Iso8211DataDescriptiveRecord(metadata, fieldDescriptors);
    }

    private Iso8211FieldDescriptor ParseFieldDescriptor(string tag, string data, int fieldControlLength)
    {
        // Field control (first fieldControlLength characters)
        // Followed by field name, array descriptor, and format controls separated by delimiters

        var fieldControl = data.Length >= fieldControlLength 
            ? data.Substring(0, fieldControlLength) 
            : string.Empty;
        
        var remainder = data.Length > fieldControlLength 
            ? data.Substring(fieldControlLength) 
            : string.Empty;

        // Split by unit terminator (0x1F) to get components
        var parts = remainder.Split('\x1f');
        
        var name = parts.Length > 0 ? parts[0] : string.Empty;
        var arrayDescriptor = parts.Length > 1 ? parts[1] : string.Empty;
        var formatControls = parts.Length > 2 ? parts[2] : string.Empty;

        return new Iso8211FieldDescriptor(tag, name, arrayDescriptor, formatControls);
    }

    private static int ParseNumeric(byte[] data, int offset, int length)
    {
        var value = 0;
        for (int i = 0; i < length; i++)
        {
            var c = (char)data[offset + i];
            if (c == ' ')
            {
                continue; // Skip spaces
            }
            if (c < '0' || c > '9')
            {
                throw new InvalidDataException($"Invalid numeric character '{c}' at position {offset + i}.");
            }
            value = value * 10 + (c - '0');
        }
        return value;
    }

    /// <summary>
    /// Disposes the reader and optionally the underlying stream.
    /// </summary>
    public void Dispose()
    {
        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }
}