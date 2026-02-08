using System.Text;

namespace EncDotNet.Iso8211;

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
