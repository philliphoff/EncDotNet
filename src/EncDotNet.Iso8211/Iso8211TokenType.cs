namespace EncDotNet.Iso8211;

/// <summary>
/// Specifies the type of ISO 8211 token.
/// </summary>
public enum Iso8211TokenType : byte
{
    /// <summary>
    /// No token has been read yet.
    /// </summary>
    None = 0,

    /// <summary>
    /// The start of an ISO 8211 record (leader parsed).
    /// </summary>
    StartRecord,

    /// <summary>
    /// The end of an ISO 8211 record.
    /// </summary>
    EndRecord,

    /// <summary>
    /// A directory entry within a record.
    /// </summary>
    DirectoryEntry,

    /// <summary>
    /// A field within a record.
    /// </summary>
    Field,

    /// <summary>
    /// End of data.
    /// </summary>
    EndOfData
}
