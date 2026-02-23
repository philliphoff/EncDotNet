using System.Collections.Immutable;
using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Provides methods to read ISO 8211 formatted data and return structured objects.
/// </summary>
/// <remarks>
/// This reader uses <see cref="Iso8211Reader"/> internally for parsing
/// and builds a complete object model of the ISO 8211 data.
/// </remarks>
public static class Iso8211DocumentReader
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
        var reader = new Iso8211Reader(data);
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
    /// <param name="parser">The forward-only reader to use for parsing.</param>
    /// <returns>The parsed ISO 8211 document.</returns>
    public static Iso8211Document Read(ref Iso8211Reader parser)
    {
        var records = ImmutableArray.CreateBuilder<Iso8211Record>();

        while (parser.Read())
        {
            if (parser.TokenType == Iso8211TokenType.StartRecord)
            {
                var record = ReadRecord(ref parser);
                records.Add(record);
            }
            else if (parser.TokenType == Iso8211TokenType.EndOfData)
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
    /// <param name="parser">The forward-only reader positioned at a StartRecord token.</param>
    /// <returns>The parsed record.</returns>
    private static Iso8211Record ReadRecord(ref Iso8211Reader parser)
    {
        if (parser.TokenType != Iso8211TokenType.StartRecord)
        {
            throw new InvalidOperationException("Reader must be positioned at a StartRecord token.");
        }

        var leader = Iso8211RecordLeader.FromLeader(parser.CurrentLeader);
        var directoryEntries = ImmutableArray.CreateBuilder<Iso8211DirectoryEntry>();
        var fields = ImmutableArray.CreateBuilder<Iso8211Field>();

        // Read directory entries
        while (parser.Read())
        {
            if (parser.TokenType == Iso8211TokenType.DirectoryEntry)
            {
                var entry = new Iso8211DirectoryEntry
                {
                    Tag = parser.GetTagString(),
                    Length = parser.CurrentLength,
                    Position = parser.CurrentPosition
                };
                directoryEntries.Add(entry);
            }
            else if (parser.TokenType == Iso8211TokenType.Field)
            {
                // First field read via auto-transition from directory to field area
                fields.Add(new Iso8211Field { Tag = directoryEntries[fields.Count].Tag, Data = parser.ValueSpan.ToArray() });
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
            fields.Add(new Iso8211Field { Tag = entry.Tag, Data = parser.ValueSpan.ToArray() });
        }

        // Advance past end of record
        if (parser.TokenType != Iso8211TokenType.EndRecord)
        {
            parser.Read();
        }

        return new Iso8211Record
        {
            Leader = leader,
            Directory = directoryEntries.ToImmutable(),
            Fields = fields.ToImmutable()
        };
    }
}
