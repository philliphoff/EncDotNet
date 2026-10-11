using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a field within an ISO 8211 record.
/// </summary>
public sealed record Iso8211Field
{
    /// <summary>
    /// Gets the field tag.
    /// </summary>
    public string Tag { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw field data, without the field terminator.
    /// </summary>
    /// <remarks>
    /// Fields read by <see cref="Iso8211DocumentReader"/> are slices of the data it read, not
    /// copies. Whether that data is shared with the caller depends on the read method: see
    /// <see cref="Iso8211DocumentReader.Read(ReadOnlyMemory{byte})"/>. A <see cref="byte"/> array
    /// converts implicitly when you build a field.
    /// </remarks>
    public ReadOnlyMemory<byte> Data { get; init; }

    /// <summary>
    /// Gets the field data as a string using the specified encoding.
    /// </summary>
    /// <param name="encoding">The encoding to use. Defaults to ASCII.</param>
    /// <returns>The field data as a string.</returns>
    public string GetDataString(Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        return encoding.GetString(Data.Span).TrimEnd('\x1f', '\x1e');
    }
}
