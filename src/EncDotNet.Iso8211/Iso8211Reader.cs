using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Provides a high-performance, low-allocation, forward-only reader for ISO 8211 (ISO/IEC 8211) formatted data.
/// </summary>
/// <remarks>
/// <para>
/// This reader is modeled after <see cref="System.Text.Json.Utf8JsonReader"/> and is designed for
/// scenarios requiring minimal memory allocations and maximum throughput.
/// </para>
/// <para>
/// The reader exposes ISO 8211 data as a series of tokens that can be read or skipped.
/// Call <see cref="Read"/> to advance to the next token, then inspect <see cref="TokenType"/>
/// and use the various value accessors to retrieve data.
/// </para>
/// </remarks>
public ref struct Iso8211Reader
{
    private const int LeaderLength = 24;

    private ReadOnlySpan<byte> _buffer;
    private int _consumed;
    private long _totalConsumed;
    private bool _isFinalBlock;

    // Current token state
    private Iso8211TokenType _tokenType;
    private Iso8211ReaderState _state;

    // Leader info (parsed at StartRecord)
    private Iso8211Leader _leader;

    // Directory state
    private int _directoryEntrySize;
    private int _directoryEntryCount;
    private int _currentDirectoryIndex;
    private int _directoryOffset;

    // Field state
    private int _currentFieldIndex;
    private int _fieldAreaOffset;
    private ReadOnlySpan<byte> _currentFieldTag;
    private int _currentFieldLength;
    private int _currentFieldPosition;
    private ReadOnlySpan<byte> _currentFieldData;

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211Reader"/> struct with a span of bytes.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read.</param>
    public Iso8211Reader(ReadOnlySpan<byte> data) : this(data, isFinalBlock: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211Reader"/> struct with a span of bytes
    /// and an indication of whether this is the final block of data.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read.</param>
    /// <param name="isFinalBlock"><c>true</c> if this is the final block of data; otherwise, <c>false</c>.</param>
    public Iso8211Reader(ReadOnlySpan<byte> data, bool isFinalBlock)
    {
        _buffer = data;
        _consumed = 0;
        _totalConsumed = 0;
        _isFinalBlock = isFinalBlock;

        _tokenType = Iso8211TokenType.None;
        _state = Iso8211ReaderState.None;
        _leader = default;

        _directoryEntrySize = 0;
        _directoryEntryCount = 0;
        _currentDirectoryIndex = 0;
        _directoryOffset = 0;

        _currentFieldIndex = 0;
        _fieldAreaOffset = 0;
        _currentFieldTag = default;
        _currentFieldLength = 0;
        _currentFieldPosition = 0;
        _currentFieldData = default;

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211Reader"/> struct with a span of bytes,
    /// an indication of whether this is the final block of data, and a previous reader state.
    /// </summary>
    /// <param name="data">The ISO 8211 data to read.</param>
    /// <param name="isFinalBlock"><c>true</c> if this is the final block of data; otherwise, <c>false</c>.</param>
    /// <param name="state">The previous reader state to restore.</param>
    /// <remarks>
    /// This constructor is used in streaming scenarios where data arrives incrementally.
    /// The <paramref name="data"/> should contain only the unprocessed portion of the stream.
    /// </remarks>
    public Iso8211Reader(ReadOnlySpan<byte> data, bool isFinalBlock, Iso8211StreamingReaderState state)
    {
        _buffer = data;
        _consumed = 0;
        _totalConsumed = state.BytesConsumed;
        _isFinalBlock = isFinalBlock;

        _tokenType = Iso8211TokenType.None;
        _state = state.State;
        _leader = state.Leader;

        _directoryEntrySize = state.DirectoryEntrySize;
        _directoryEntryCount = state.DirectoryEntryCount;
        _currentDirectoryIndex = state.CurrentDirectoryIndex;
        _directoryOffset = 0;

        _currentFieldIndex = state.CurrentFieldIndex;
        _fieldAreaOffset = 0;
        _currentFieldTag = default;
        _currentFieldLength = 0;
        _currentFieldPosition = 0;
        _currentFieldData = default;

    }

    /// <summary>
    /// Gets a value indicating whether this is the final block of data.
    /// </summary>
    public readonly bool IsFinalBlock => _isFinalBlock;

    /// <summary>
    /// Gets the type of the current token.
    /// </summary>
    public readonly Iso8211TokenType TokenType => _tokenType;

    /// <summary>
    /// Gets the current reader state.
    /// </summary>
    public readonly Iso8211ReaderState CurrentState => _state;

    /// <summary>
    /// Gets the number of bytes consumed so far.
    /// </summary>
    public readonly long BytesConsumed => _totalConsumed + _consumed;

    /// <summary>
    /// Gets the leader information for the current record.
    /// </summary>
    /// <remarks>
    /// Only valid when <see cref="TokenType"/> is <see cref="Iso8211TokenType.StartRecord"/> or
    /// when inside a record (DirectoryEntry, Field).
    /// </remarks>
    public readonly Iso8211Leader CurrentLeader => _leader;

    /// <summary>
    /// Gets the current depth within the ISO 8211 structure.
    /// </summary>
    /// <remarks>
    /// 0 = At root level (between records or at EndOfData)
    /// 1 = Inside a record
    /// 2 = Inside a field
    /// </remarks>
    public readonly int CurrentDepth
    {
        get
        {
            return _tokenType switch
            {
                Iso8211TokenType.None => 0,
                Iso8211TokenType.StartRecord => 1,
                Iso8211TokenType.DirectoryEntry => 1,
                Iso8211TokenType.Field => 2,
                Iso8211TokenType.EndRecord => 0,
                Iso8211TokenType.EndOfData => 0,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Gets the tag of the current directory entry or field.
    /// </summary>
    /// <remarks>
    /// Only valid when <see cref="TokenType"/> is <see cref="Iso8211TokenType.DirectoryEntry"/> or
    /// <see cref="Iso8211TokenType.Field"/>.
    /// </remarks>
    public readonly ReadOnlySpan<byte> CurrentTag => _currentFieldTag;

    /// <summary>
    /// Gets the length of the current directory entry or field.
    /// </summary>
    public readonly int CurrentLength => _currentFieldLength;

    /// <summary>
    /// Gets the position of the current directory entry or field within the field area.
    /// </summary>
    public readonly int CurrentPosition => _currentFieldPosition;

    /// <summary>
    /// Gets the data of the current field.
    /// </summary>
    /// <remarks>
    /// Only valid when <see cref="TokenType"/> is <see cref="Iso8211TokenType.Field"/>.
    /// </remarks>
    public readonly ReadOnlySpan<byte> ValueSpan => _currentFieldData;

    /// <summary>
    /// Gets the index of the current directory entry within the record.
    /// </summary>
    public readonly int CurrentDirectoryIndex => _currentDirectoryIndex;

    /// <summary>
    /// Gets the index of the current field within the record.
    /// </summary>
    public readonly int CurrentFieldIndex => _currentFieldIndex;

    /// <summary>
    /// Gets the total number of directory entries (and fields) in the current record.
    /// </summary>
    /// <remarks>
    /// Only valid after <see cref="TokenType"/> becomes <see cref="Iso8211TokenType.StartRecord"/>.
    /// </remarks>
    public readonly int DirectoryEntryCount => _directoryEntryCount;

    /// <summary>
    /// Gets whether there are more directory entries to read in the current record.
    /// </summary>
    public readonly bool HasMoreDirectoryEntries => _currentDirectoryIndex < _directoryEntryCount;

    /// <summary>
    /// Gets whether there are more fields to read in the current record.
    /// </summary>
    public readonly bool HasMoreFields => _currentFieldIndex < _directoryEntryCount;

    /// <summary>
    /// Reads the next ISO 8211 token from the input data.
    /// </summary>
    /// <returns><c>true</c> if a token was successfully read; <c>false</c> if the end of data was reached.</returns>
    public bool Read()
    {
        // If we've already hit end of data, return false
        if (_state == Iso8211ReaderState.EndOfData)
        {
            _tokenType = Iso8211TokenType.EndOfData;
            return false;
        }

        switch (_state)
        {
            case Iso8211ReaderState.None:
                return TryReadRecordStart();

            case Iso8211ReaderState.InLeader:
                // After reading leader, move to directory
                _state = Iso8211ReaderState.InDirectory;
                _currentDirectoryIndex = 0;
                return TryReadDirectoryEntry();

            case Iso8211ReaderState.InDirectory:
                if (_currentDirectoryIndex < _directoryEntryCount)
                {
                    return TryReadDirectoryEntry();
                }
                else
                {
                    // Move to field area
                    _state = Iso8211ReaderState.InFieldArea;
                    _currentFieldIndex = 0;
                    return TryReadField();
                }

            case Iso8211ReaderState.InFieldArea:
                if (_currentFieldIndex < _directoryEntryCount)
                {
                    return TryReadField();
                }
                else
                {
                    // End of record
                    return EndRecord();
                }

            default:
                return false;
        }
    }

    /// <summary>
    /// Skips the current token and any children.
    /// </summary>
    /// <remarks>
    /// If the current token is StartRecord, skips the entire record.
    /// If the current token is Field, skips to the next field or end of record.
    /// </remarks>
    public void Skip()
    {
        TrySkip();
    }

    /// <summary>
    /// Attempts to skip the current token and any children.
    /// </summary>
    /// <returns><c>true</c> if skipping succeeded; <c>false</c> if more data is needed.</returns>
    /// <remarks>
    /// If the current token is StartRecord, skips the entire record.
    /// If the current token is Field, skips to the next field or end of record.
    /// </remarks>
    public bool TrySkip()
    {
        switch (_tokenType)
        {
            case Iso8211TokenType.StartRecord:
                // Skip entire record
                return TrySkipRecord();

            case Iso8211TokenType.DirectoryEntry:
                // Just advance to next directory entry or field
                return true;

            case Iso8211TokenType.Field:
                // Skip to next field (already consumed)
                return true;

            default:
                return true;
        }
    }

    /// <summary>
    /// Skips all remaining directory entries and positions at the first field.
    /// </summary>
    public void SkipDirectory()
    {
        if (_state == Iso8211ReaderState.InDirectory)
        {
            _currentDirectoryIndex = _directoryEntryCount;
            _state = Iso8211ReaderState.InFieldArea;
            _currentFieldIndex = 0;
        }
    }

    /// <summary>
    /// Skips the current record and positions at the start of the next record.
    /// </summary>
    public void SkipRecord()
    {
        TrySkipRecord();
    }

    /// <summary>
    /// Attempts to skip the current record and position at the start of the next record.
    /// </summary>
    /// <returns><c>true</c> if skipping succeeded; <c>false</c> if more data is needed.</returns>
    public bool TrySkipRecord()
    {
        if (_state == Iso8211ReaderState.None || _state == Iso8211ReaderState.EndOfData)
        {
            return true;
        }

        // Move past the current record
        var recordEnd = _fieldAreaOffset + (_leader.RecordLength - _leader.BaseAddressOfFieldArea);
        
        // Check if we have enough data
        if (recordEnd > _buffer.Length)
        {
            if (_isFinalBlock)
            {
                _state = Iso8211ReaderState.Error;
                return false;
            }
            // Need more data
            return false;
        }
        
        var bytesToSkip = recordEnd - _consumed;
        if (bytesToSkip > 0)
        {
            _consumed += bytesToSkip;
        }

        // Reset state for next record
        _state = Iso8211ReaderState.None;
        _tokenType = Iso8211TokenType.EndRecord;
        _currentDirectoryIndex = 0;
        _currentFieldIndex = 0;
        return true;
    }

    /// <summary>
    /// Tries to get the current field tag as a string.
    /// </summary>
    /// <param name="tag">When this method returns, contains the tag string if successful.</param>
    /// <returns><c>true</c> if the tag was successfully retrieved; otherwise, <c>false</c>.</returns>
    public readonly bool TryGetTagString(out string tag)
    {
        if (_currentFieldTag.IsEmpty)
        {
            tag = string.Empty;
            return false;
        }

        tag = Encoding.ASCII.GetString(_currentFieldTag);
        return true;
    }

    /// <summary>
    /// Gets the current field tag as a string.
    /// </summary>
    /// <returns>The tag string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no tag is available.</exception>
    public readonly string GetTagString()
    {
        if (!TryGetTagString(out var tag))
        {
            throw new InvalidOperationException("No tag available for the current token.");
        }
        return tag;
    }

    /// <summary>
    /// Gets the current field data as a string using the specified encoding.
    /// </summary>
    /// <param name="encoding">The encoding to use. Defaults to ASCII.</param>
    /// <returns>The field data as a string.</returns>
    public readonly string GetValueString(Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        return encoding.GetString(ValueSpan).TrimEnd('\x1f', '\x1e');
    }

    /// <summary>
    /// Tries to get the current field data as a string.
    /// </summary>
    /// <param name="value">When this method returns, contains the value string if successful.</param>
    /// <param name="encoding">The encoding to use. Defaults to ASCII.</param>
    /// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
    public readonly bool TryGetValueString(out string value, Encoding? encoding = null)
    {
        if (ValueSpan.IsEmpty)
        {
            value = string.Empty;
            return false;
        }

        encoding ??= Encoding.ASCII;
        value = encoding.GetString(ValueSpan).TrimEnd('\x1f', '\x1e');
        return true;
    }

    /// <summary>
    /// Copies the current field data to the specified destination.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    /// <returns>The number of bytes copied.</returns>
    public readonly int CopyValueTo(Span<byte> destination)
    {
        if (ValueSpan.IsEmpty)
        {
            return 0;
        }

        var bytesToCopy = Math.Min(ValueSpan.Length, destination.Length);
        ValueSpan.Slice(0, bytesToCopy).CopyTo(destination);
        return bytesToCopy;
    }

    /// <summary>
    /// Gets the current state of the reader for use in streaming scenarios.
    /// </summary>
    /// <returns>The current reader state that can be used to resume reading.</returns>
    public readonly Iso8211StreamingReaderState GetCurrentState()
    {
        return new Iso8211StreamingReaderState(
            _state,
            _totalConsumed + _consumed,
            _leader,
            _directoryEntrySize,
            _directoryEntryCount,
            _currentDirectoryIndex,
            _currentFieldIndex);
    }

    private bool TryReadRecordStart()
    {
        // Check if we have enough data for the leader
        if (_buffer.Length - _consumed < LeaderLength)
        {
            if (_isFinalBlock)
            {
                _state = Iso8211ReaderState.EndOfData;
                _tokenType = Iso8211TokenType.EndOfData;
                return false;
            }
            // Need more data
            return false;
        }

        var leaderSpan = _buffer.Slice(_consumed, LeaderLength);

        // Parse leader
        var recordLength = ParseNumeric(leaderSpan, 0, 5);
        
        // Check if we have the entire record
        if (_buffer.Length - _consumed < recordLength)
        {
            if (_isFinalBlock)
            {
                _state = Iso8211ReaderState.Error;
                return false;
            }
            // Need more data
            return false;
        }

        _leader = new Iso8211Leader
        {
            RecordLength = recordLength,
            InterchangeLevel = (char)leaderSpan[5],
            LeaderIdentifier = (char)leaderSpan[6],
            InlineCodeExtensionIndicator = (char)leaderSpan[7],
            VersionNumber = (char)leaderSpan[8],
            ApplicationIndicator = (char)leaderSpan[9],
            FieldControlLength = ParseNumeric(leaderSpan, 10, 2),
            BaseAddressOfFieldArea = ParseNumeric(leaderSpan, 12, 5),
            ExtendedCharacterSetIndicator0 = (char)leaderSpan[17],
            ExtendedCharacterSetIndicator1 = (char)leaderSpan[18],
            ExtendedCharacterSetIndicator2 = (char)leaderSpan[19],
            SizeOfFieldLengthField = leaderSpan[20] - '0',
            SizeOfFieldPositionField = leaderSpan[21] - '0',
            Reserved = leaderSpan[22] - '0',
            SizeOfFieldTagField = leaderSpan[23] - '0'
        };

        // Calculate directory info
        var directoryLength = _leader.BaseAddressOfFieldArea - LeaderLength - 1; // -1 for field terminator
        _directoryEntrySize = _leader.SizeOfFieldTagField + _leader.SizeOfFieldLengthField + _leader.SizeOfFieldPositionField;
        _directoryEntryCount = directoryLength / _directoryEntrySize;

        _directoryOffset = _consumed + LeaderLength;
        _fieldAreaOffset = _consumed + _leader.BaseAddressOfFieldArea;

        _consumed += LeaderLength;
        _state = Iso8211ReaderState.InLeader;
        _tokenType = Iso8211TokenType.StartRecord;
        _currentDirectoryIndex = 0;
        _currentFieldIndex = 0;

        return true;
    }

    private bool TryReadDirectoryEntry()
    {
        if (_currentDirectoryIndex >= _directoryEntryCount)
        {
            return false;
        }

        var entryOffset = _directoryOffset + (_currentDirectoryIndex * _directoryEntrySize);
        var entrySpan = _buffer.Slice(entryOffset, _directoryEntrySize);

        _currentFieldTag = entrySpan.Slice(0, _leader.SizeOfFieldTagField);
        _currentFieldLength = ParseNumeric(entrySpan, _leader.SizeOfFieldTagField, _leader.SizeOfFieldLengthField);
        _currentFieldPosition = ParseNumeric(entrySpan, _leader.SizeOfFieldTagField + _leader.SizeOfFieldLengthField, _leader.SizeOfFieldPositionField);

        _currentDirectoryIndex++;
        _tokenType = Iso8211TokenType.DirectoryEntry;

        // Advance consumed past directory if this is the last entry
        if (_currentDirectoryIndex >= _directoryEntryCount)
        {
            // Move consumed to the field area (past directory + terminator)
            _consumed = _fieldAreaOffset;
        }

        return true;
    }

    private bool TryReadField()
    {
        if (_currentFieldIndex >= _directoryEntryCount)
        {
            return false;
        }

        // Re-read directory entry to get field info
        var entryOffset = _directoryOffset + (_currentFieldIndex * _directoryEntrySize);
        var entrySpan = _buffer.Slice(entryOffset, _directoryEntrySize);

        _currentFieldTag = entrySpan.Slice(0, _leader.SizeOfFieldTagField);
        _currentFieldLength = ParseNumeric(entrySpan, _leader.SizeOfFieldTagField, _leader.SizeOfFieldLengthField);
        _currentFieldPosition = ParseNumeric(entrySpan, _leader.SizeOfFieldTagField + _leader.SizeOfFieldLengthField, _leader.SizeOfFieldPositionField);

        // Field data (excluding terminator)
        var dataLength = _currentFieldLength - 1;
        var fieldDataOffset = _fieldAreaOffset + _currentFieldPosition;
        _currentFieldData = _buffer.Slice(fieldDataOffset, dataLength);

        _currentFieldIndex++;
        _tokenType = Iso8211TokenType.Field;

        return true;
    }

    private bool EndRecord()
    {
        // Move consumed to end of record
        var recordEnd = _fieldAreaOffset + (_leader.RecordLength - _leader.BaseAddressOfFieldArea);
        _consumed = recordEnd;

        _state = Iso8211ReaderState.None;
        _tokenType = Iso8211TokenType.EndRecord;
        _currentDirectoryIndex = 0;
        _currentFieldIndex = 0;

        return true;
    }

    private static int ParseNumeric(ReadOnlySpan<byte> data, int offset, int length)
    {
        var value = 0;
        for (int i = 0; i < length; i++)
        {
            var c = (char)data[offset + i];
            if (c == ' ')
            {
                continue;
            }
            if (c < '0' || c > '9')
            {
                throw new InvalidDataException($"Invalid numeric character '{c}' at position {offset + i}.");
            }
            value = value * 10 + (c - '0');
        }
        return value;
    }
}
