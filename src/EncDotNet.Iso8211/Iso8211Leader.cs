namespace EncDotNet.Iso8211;

/// <summary>
/// Contains the parsed leader information for the current record.
/// </summary>
public readonly struct Iso8211Leader
{
    /// <summary>
    /// Gets the total length of the record in bytes.
    /// </summary>
    public int RecordLength { get; init; }

    /// <summary>
    /// Gets the interchange level character.
    /// </summary>
    public char InterchangeLevel { get; init; }

    /// <summary>
    /// Gets the leader identifier ('L' for DDR, 'D' for data record).
    /// </summary>
    public char LeaderIdentifier { get; init; }

    /// <summary>
    /// Gets the inline code extension indicator.
    /// </summary>
    public char InlineCodeExtensionIndicator { get; init; }

    /// <summary>
    /// Gets the version number character.
    /// </summary>
    public char VersionNumber { get; init; }

    /// <summary>
    /// Gets the application indicator character.
    /// </summary>
    public char ApplicationIndicator { get; init; }

    /// <summary>
    /// Gets the field control length.
    /// </summary>
    public int FieldControlLength { get; init; }

    /// <summary>
    /// Gets the base address of the field area.
    /// </summary>
    public int BaseAddressOfFieldArea { get; init; }

    /// <summary>
    /// Gets the first extended character set indicator character.
    /// </summary>
    public char ExtendedCharacterSetIndicator0 { get; init; }

    /// <summary>
    /// Gets the second extended character set indicator character.
    /// </summary>
    public char ExtendedCharacterSetIndicator1 { get; init; }

    /// <summary>
    /// Gets the third extended character set indicator character.
    /// </summary>
    public char ExtendedCharacterSetIndicator2 { get; init; }

    /// <summary>
    /// Gets the size of the field length field in directory entries.
    /// </summary>
    public int SizeOfFieldLengthField { get; init; }

    /// <summary>
    /// Gets the size of the field position field in directory entries.
    /// </summary>
    public int SizeOfFieldPositionField { get; init; }

    /// <summary>
    /// Gets the reserved field value.
    /// </summary>
    public int Reserved { get; init; }

    /// <summary>
    /// Gets the size of the field tag field in directory entries.
    /// </summary>
    public int SizeOfFieldTagField { get; init; }

    /// <summary>
    /// Gets whether this is a Data Descriptive Record (DDR).
    /// </summary>
    public bool IsDataDescriptiveRecord => LeaderIdentifier == 'L';
}
