namespace EncDotNet.Iso8211;

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
