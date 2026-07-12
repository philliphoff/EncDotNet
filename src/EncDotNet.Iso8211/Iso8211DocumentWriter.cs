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
    /// <summary>
    /// Serializes an ISO 8211 document to a byte array.
    /// </summary>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <returns>The serialized ISO 8211 bytes.</returns>
    public static byte[] Write(Iso8211Document document, Iso8211WriterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        options ??= Iso8211WriterOptions.Default;

        var output = new List<byte>(EstimateCapacity(document));
        foreach (var record in document.Records)
        {
            Iso8211RecordWriter.Write(record, options, output);
        }

        return output.ToArray();
    }

    /// <summary>
    /// Serializes an ISO 8211 document to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    public static void Write(Stream stream, Iso8211Document document, Iso8211WriterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var bytes = Write(document, options);
        stream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Asynchronously serializes an ISO 8211 document to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteAsync(
        Stream stream,
        Iso8211Document document,
        Iso8211WriterOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var bytes = Write(document, options);
        await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Serializes an ISO 8211 document to a file.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    public static void WriteToFile(string path, Iso8211Document document, Iso8211WriterOptions? options = null)
    {
        var bytes = Write(document, options);
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// Asynchronously serializes an ISO 8211 document to a file.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="document">The document to serialize.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteToFileAsync(
        string path,
        Iso8211Document document,
        Iso8211WriterOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = Write(document, options);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    private static int EstimateCapacity(Iso8211Document document)
    {
        var total = 0;
        foreach (var record in document.Records)
        {
            total += 32 + (record.Directory.Count * 12);
            foreach (var field in record.Fields)
            {
                total += field.Data.Length + 1;
            }
        }
        return total > 0 ? total : 64;
    }
}
