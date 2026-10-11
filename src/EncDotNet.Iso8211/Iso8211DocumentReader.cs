using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Provides methods to read ISO 8211 formatted data and return structured objects.
/// </summary>
/// <remarks>
/// <para>
/// This reader uses <see cref="Iso8211Reader"/> internally for parsing. The <c>Read</c> methods
/// build a complete <see cref="Iso8211Document"/>; <see cref="EnumerateRecords(ReadOnlyMemory{byte})"/>
/// returns one record at a time, so memory use doesn't grow with the size of the file.
/// </para>
/// <para>
/// Each field's <see cref="Iso8211Field.Data"/> is a slice of the data read rather than a copy
/// of its own. The methods that take a <see cref="byte"/> array, a span, a file or a stream read
/// from a private copy, so the document never shares memory with the caller. The methods that
/// take a <see cref="ReadOnlyMemory{T}"/> slice the caller's memory directly.
/// </para>
/// </remarks>
public static class Iso8211DocumentReader
{
    /// <summary>
    /// Reads an ISO 8211 document from a byte array.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read. The document is read from a copy, so the
    /// array can be reused afterwards.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document Read(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Read(data.AsMemory().ToArray().AsMemory());
    }

    /// <summary>
    /// Reads an ISO 8211 document from a span of bytes.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read. The document is read from a copy.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document Read(ReadOnlySpan<byte> data)
    {
        return Read(data.ToArray().AsMemory());
    }

    /// <summary>
    /// Reads an ISO 8211 document from memory, without copying it.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    /// <remarks>
    /// Every field's <see cref="Iso8211Field.Data"/> is a slice of <paramref name="data"/>, so
    /// the document holds a reference to it. Don't modify or reuse the memory while the document
    /// is in use. To read from a buffer you will reuse, call <see cref="Read(byte[])"/>, which
    /// reads from a copy.
    /// </remarks>
    public static Iso8211Document Read(ReadOnlyMemory<byte> data)
    {
        return new Iso8211Document
        {
            Records = [.. EnumerateRecords(data)]
        };
    }

    /// <summary>
    /// Reads an ISO 8211 document from a file.
    /// </summary>
    /// <param name="path">The path to the ISO 8211 file.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document ReadFromFile(string path)
    {
        return Read(File.ReadAllBytes(path).AsMemory());
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
        return Read(data.AsMemory());
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
        return Read(WrittenBytes(memoryStream));
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
        return Read(WrittenBytes(memoryStream));
    }

    /// <summary>
    /// Reads an ISO 8211 document using an <see cref="Iso8211Reader"/>.
    /// </summary>
    /// <param name="parser">The forward-only reader to use for parsing.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    /// <remarks>
    /// The reader only exposes spans, so each field's data is copied. When you have the data as
    /// memory, <see cref="Read(ReadOnlyMemory{byte})"/> avoids the copies.
    /// </remarks>
    public static Iso8211Document Read(ref Iso8211Reader parser)
    {
        var records = new List<Iso8211Record>();
        var tags = new TagTable();

        while (parser.Read())
        {
            if (parser.TokenType == Iso8211TokenType.StartRecord)
            {
                records.Add(ReadRecord(ref parser, tags, ReadOnlyMemory<byte>.Empty, default));
            }
            else if (parser.TokenType == Iso8211TokenType.EndOfData)
            {
                break;
            }
        }

        return new Iso8211Document
        {
            Records = records
        };
    }

    /// <summary>
    /// Reads ISO 8211 records one at a time, without building a whole document.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read.</param>
    /// <returns>
    /// The records in file order, the Data Descriptive Record (DDR) first. Records are read as
    /// the sequence is enumerated, so a record that has been processed can be released.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Every field's <see cref="Iso8211Field.Data"/> is a slice of <paramref name="data"/>, so
    /// don't modify the memory while the records are in use.
    /// </para>
    /// <para>
    /// Enumeration stops at the end of the data, or at a record that can't be read, such as one
    /// cut short by the end of the data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// byte[] data = File.ReadAllBytes("path/to/file.000");
    /// Iso8211DataDescriptiveRecord? ddr = null;
    /// foreach (Iso8211Record record in Iso8211DocumentReader.EnumerateRecords(data))
    /// {
    ///     if (record.IsDataDescriptiveRecord)
    ///     {
    ///         ddr = Iso8211DataDescriptiveRecordReader.Read(record);
    ///         continue;
    ///     }
    ///
    ///     // Decode the record's fields with ddr, then let it go.
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<Iso8211Record> EnumerateRecords(ReadOnlyMemory<byte> data)
    {
        var tags = new TagTable();
        var offset = 0;

        while (TryReadRecordAt(data, offset, tags, out var record, out var length))
        {
            yield return record;
            offset += length;
        }
    }

    /// <summary>
    /// Reads the record that starts at <paramref name="offset"/>.
    /// </summary>
    /// <remarks>
    /// A separate method because <see cref="Iso8211Reader"/> is a <c>ref struct</c>, which an
    /// iterator can't hold across a <c>yield</c>.
    /// </remarks>
    private static bool TryReadRecordAt(ReadOnlyMemory<byte> data, int offset, TagTable tags, out Iso8211Record record, out int length)
    {
        var remaining = data.Span[offset..];
        var parser = new Iso8211Reader(remaining);
        if (!parser.Read() || parser.TokenType != Iso8211TokenType.StartRecord)
        {
            record = null!;
            length = 0;
            return false;
        }

        // The leader's length, or the length derived from the directory for a record too long
        // for the leader's five digits.
        length = parser.CurrentLeader.RecordLength;
        record = ReadRecord(ref parser, tags, data[offset..], remaining);
        return true;
    }

    /// <summary>
    /// Reads a single ISO 8211 record from the reader.
    /// </summary>
    /// <param name="parser">The forward-only reader positioned at a StartRecord token.</param>
    /// <param name="tags">Tag strings already seen, so each distinct tag is allocated once.</param>
    /// <param name="source">
    /// The memory <paramref name="parser"/> reads, which fields are sliced from; empty to copy
    /// each field's data instead.
    /// </param>
    /// <param name="sourceSpan">The span <paramref name="parser"/> was created over.</param>
    /// <returns>The parsed record.</returns>
    private static Iso8211Record ReadRecord(ref Iso8211Reader parser, TagTable tags, ReadOnlyMemory<byte> source, ReadOnlySpan<byte> sourceSpan)
    {
        if (parser.TokenType != Iso8211TokenType.StartRecord)
        {
            throw new InvalidOperationException("Reader must be positioned at a StartRecord token.");
        }

        var leader = Iso8211RecordLeader.FromLeader(parser.CurrentLeader);
        var directoryEntries = new List<Iso8211DirectoryEntry>(parser.DirectoryEntryCount);
        var fields = new List<Iso8211Field>(parser.DirectoryEntryCount);

        // Read directory entries
        while (parser.Read())
        {
            if (parser.TokenType == Iso8211TokenType.DirectoryEntry)
            {
                var entry = new Iso8211DirectoryEntry
                {
                    Tag = tags.Get(parser.CurrentTag),
                    Length = parser.CurrentLength,
                    Position = parser.CurrentPosition
                };
                directoryEntries.Add(entry);
            }
            else if (parser.TokenType == Iso8211TokenType.Field)
            {
                // First field read via auto-transition from directory to field area
                fields.Add(new Iso8211Field { Tag = directoryEntries[fields.Count].Tag, Data = FieldData(ref parser, source, sourceSpan) });
                break;
            }
            else if (parser.TokenType == Iso8211TokenType.EndRecord)
            {
                // Record with no fields
                break;
            }
        }

        // Read remaining fields using pre-parsed directory entry info,
        // bypassing redundant directory re-parsing in TryReadField
        for (int i = fields.Count; i < directoryEntries.Count; i++)
        {
            var entry = directoryEntries[i];
            parser.TryReadFieldDirect(entry.Position, entry.Length);
            fields.Add(new Iso8211Field { Tag = entry.Tag, Data = FieldData(ref parser, source, sourceSpan) });
        }

        // Advance past end of record
        if (parser.TokenType != Iso8211TokenType.EndRecord)
        {
            parser.Read();
        }

        return new Iso8211Record
        {
            Leader = leader,
            Directory = directoryEntries,
            Fields = fields
        };
    }

    /// <summary>
    /// Gets the current field's data: a slice of <paramref name="source"/>, or a copy when it's empty.
    /// </summary>
    private static ReadOnlyMemory<byte> FieldData(ref Iso8211Reader parser, ReadOnlyMemory<byte> source, ReadOnlySpan<byte> sourceSpan)
    {
        var value = parser.ValueSpan;
        if (source.IsEmpty)
        {
            return value.ToArray();
        }

        // The reader's value is a slice of the span it was given; find where it starts.
        var start = (int)Unsafe.ByteOffset(ref MemoryMarshal.GetReference(sourceSpan), ref MemoryMarshal.GetReference(value));
        return source.Slice(start, value.Length);
    }

    /// <summary>
    /// Gets the bytes written to a memory stream, without copying them.
    /// </summary>
    private static ReadOnlyMemory<byte> WrittenBytes(MemoryStream stream) =>
        stream.TryGetBuffer(out var buffer) ? buffer.AsMemory() : stream.ToArray();

    /// <summary>
    /// Shares one string per distinct field tag, so a document with tens of thousands of fields
    /// holds a handful of tag strings rather than one per directory entry.
    /// </summary>
    private sealed class TagTable
    {
        private readonly Dictionary<long, string> _shortTags = [];

        public string Get(ReadOnlySpan<byte> tag)
        {
            if (tag.IsEmpty)
            {
                // As Iso8211Reader.GetTagString reports it.
                throw new InvalidOperationException("No tag available for the current token.");
            }

            // Tags are at most a few ASCII characters, usually four. Pack those bytes and the
            // length into one key; the length keeps "ABC" and "ABC\0" apart.
            if (tag.Length <= 4)
            {
                Span<byte> packed = stackalloc byte[4];
                packed.Clear();
                tag.CopyTo(packed);
                var key = BinaryPrimitives.ReadUInt32LittleEndian(packed) | ((long)tag.Length << 32);
                if (!_shortTags.TryGetValue(key, out var value))
                {
                    value = Encoding.ASCII.GetString(tag);
                    _shortTags[key] = value;
                }

                return value;
            }

            return Encoding.ASCII.GetString(tag);
        }
    }
}
