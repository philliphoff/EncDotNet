namespace EncDotNet.Iso8211;

/// <summary>
/// Captures the current state of a <see cref="Iso8211Parser"/> to support resuming
/// reading after getting more data.
/// </summary>
/// <remarks>
/// This is used in streaming scenarios where data arrives incrementally.
/// </remarks>
public readonly record struct Iso8211StreamingReaderState
{
    internal Iso8211StreamingReaderState(
        Iso8211ReaderState state,
        long bytesConsumed,
        Iso8211Leader leader,
        int directoryEntrySize,
        int directoryEntryCount,
        int currentDirectoryIndex,
        int currentFieldIndex)
    {
        State = state;
        BytesConsumed = bytesConsumed;
        Leader = leader;
        DirectoryEntrySize = directoryEntrySize;
        DirectoryEntryCount = directoryEntryCount;
        CurrentDirectoryIndex = currentDirectoryIndex;
        CurrentFieldIndex = currentFieldIndex;
    }

    internal Iso8211ReaderState State { get; }
    internal long BytesConsumed { get; }
    internal Iso8211Leader Leader { get; }
    internal int DirectoryEntrySize { get; }
    internal int DirectoryEntryCount { get; }
    internal int CurrentDirectoryIndex { get; }
    internal int CurrentFieldIndex { get; }
}
