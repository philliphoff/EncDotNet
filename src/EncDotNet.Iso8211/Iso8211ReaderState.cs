namespace EncDotNet.Iso8211;

/// <summary>
/// Represents the state of the forward-only reader.
/// </summary>
public enum Iso8211ReaderState : byte
{
    /// <summary>
    /// Initial state, no data read yet.
    /// </summary>
    None = 0,

    /// <summary>
    /// Reading the record leader.
    /// </summary>
    InLeader,

    /// <summary>
    /// Reading directory entries.
    /// </summary>
    InDirectory,

    /// <summary>
    /// Reading fields.
    /// </summary>
    InFieldArea,

    /// <summary>
    /// An error occurred.
    /// </summary>
    Error,

    /// <summary>
    /// End of data reached.
    /// </summary>
    EndOfData
}
