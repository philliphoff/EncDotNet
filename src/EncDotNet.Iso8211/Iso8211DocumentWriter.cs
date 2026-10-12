using System.Buffers;

namespace EncDotNet.Iso8211;

/// <summary>
/// Provides methods to write (serialize) an <see cref="Iso8211Document"/> to ISO 8211 bytes.
/// </summary>
/// <remarks>
/// <para>
/// This writer is the symmetric inverse of <see cref="Iso8211DocumentReader"/>. For a document
/// produced by the reader from a canonically-encoded source, <c>Read → Write</c> yields
/// byte-identical output, and <c>Read → Write → Read</c> yields an equivalent object model.
/// </para>
/// <para>
/// The leader flag characters and directory entry-map sizes are preserved from each record's
/// <see cref="Iso8211Record.Leader"/>, while the record length and base address of the field
/// area are recomputed from the record's fields.
/// </para>
/// </remarks>
public static class Iso8211DocumentWriter
{
    // Stream and file output is gathered into chunks of about this size before each write.
    private const int ChunkSize = 64 * 1024;

    /// <summary>
    /// Serializes an ISO 8211 document to a byte array.
    /// </summary>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <returns>The serialized ISO 8211 bytes.</returns>
    public static byte[] Write(Iso8211Document document, Iso8211WriterOptions? options = null)
    {
        var plan = Plan(document, options);

        var output = new byte[plan.TotalLength];
        var offset = 0;
        for (int i = 0; i < plan.Layouts.Length; i++)
        {
            var length = plan.Layouts[i].RecordLength;
            Iso8211RecordWriter.Write(document.Records[i], plan.Layouts[i], output.AsSpan(offset, length));
            offset += length;
        }

        return output;
    }

    /// <summary>
    /// Serializes an ISO 8211 document to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <remarks>
    /// Every record is checked before anything is written, so a document that can't be encoded
    /// leaves the stream untouched. The output is then written a chunk of records at a time
    /// rather than built in memory first.
    /// </remarks>
    public static void Write(Stream stream, Iso8211Document document, Iso8211WriterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        WriteChunks(stream, document, Plan(document, options));
    }

    /// <summary>
    /// Asynchronously serializes an ISO 8211 document to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <remarks>
    /// Every record is checked before anything is written, so a document that can't be encoded
    /// leaves the stream untouched. The output is then written a chunk of records at a time
    /// rather than built in memory first.
    /// </remarks>
    public static async Task WriteAsync(
        Stream stream,
        Iso8211Document document,
        Iso8211WriterOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await WriteChunksAsync(stream, document, Plan(document, options), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Serializes an ISO 8211 document to a file.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <remarks>
    /// Every record is checked before the file is opened, so a document that can't be encoded
    /// doesn't create or change the file.
    /// </remarks>
    public static void WriteToFile(string path, Iso8211Document document, Iso8211WriterOptions? options = null)
    {
        var plan = Plan(document, options);

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1);
        WriteChunks(stream, document, plan);
    }

    /// <summary>
    /// Asynchronously serializes an ISO 8211 document to a file.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <remarks>
    /// Every record is checked before the file is opened, so a document that can't be encoded
    /// doesn't create or change the file.
    /// </remarks>
    public static async Task WriteToFileAsync(
        string path,
        Iso8211Document document,
        Iso8211WriterOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var plan = Plan(document, options);

        var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1, useAsync: true);
        await using (stream.ConfigureAwait(false))
        {
            await WriteChunksAsync(stream, document, plan, cancellationToken).ConfigureAwait(false);
        }
    }

    // Works out every record's layout, which also checks that each can be encoded.
    private static WritePlan Plan(Iso8211Document document, Iso8211WriterOptions? options)
    {
        ArgumentNullException.ThrowIfNull(document);
        options ??= Iso8211WriterOptions.Default;

        var records = document.Records;
        var layouts = new Iso8211RecordLayout[records.Count];
        long total = 0;
        var max = 0;
        for (int i = 0; i < layouts.Length; i++)
        {
            layouts[i] = Iso8211RecordWriter.Plan(records[i], options);
            total += layouts[i].RecordLength;
            max = Math.Max(max, layouts[i].RecordLength);
        }

        if (total > Array.MaxLength)
        {
            throw new InvalidOperationException($"The document encodes to {total:N0} bytes, more than an array can hold.");
        }

        return new WritePlan(layouts, (int)total, max);
    }

    private static void WriteChunks(Stream stream, Iso8211Document document, WritePlan plan)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(Math.Max(ChunkSize, plan.MaxRecordLength));
        try
        {
            var record = 0;
            while (record < plan.Layouts.Length)
            {
                var length = FillChunk(document, plan, buffer, ref record);
                stream.Write(buffer, 0, length);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static async Task WriteChunksAsync(Stream stream, Iso8211Document document, WritePlan plan, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(Math.Max(ChunkSize, plan.MaxRecordLength));
        try
        {
            var record = 0;
            while (record < plan.Layouts.Length)
            {
                var length = FillChunk(document, plan, buffer, ref record);
                await stream.WriteAsync(buffer.AsMemory(0, length), cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // Encodes as many whole records as fit in the buffer, starting at the given record, and
    // returns the number of bytes written. The buffer always holds at least the longest record.
    private static int FillChunk(Iso8211Document document, WritePlan plan, byte[] buffer, ref int record)
    {
        var offset = 0;
        while (record < plan.Layouts.Length)
        {
            var length = plan.Layouts[record].RecordLength;
            if (offset + length > buffer.Length)
            {
                break;
            }

            Iso8211RecordWriter.Write(document.Records[record], plan.Layouts[record], buffer.AsSpan(offset, length));
            offset += length;
            record++;
        }

        return offset;
    }

    private readonly record struct WritePlan(Iso8211RecordLayout[] Layouts, int TotalLength, int MaxRecordLength);
}
