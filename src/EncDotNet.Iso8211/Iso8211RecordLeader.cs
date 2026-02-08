namespace EncDotNet.Iso8211;

/// <summary>
/// Represents the leader information for an ISO 8211 record.
/// </summary>
public readonly struct Iso8211RecordLeader
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
    /// Gets the extended character set indicators.
    /// </summary>
    public string ExtendedCharacterSetIndicator { get; init; }

    /// <summary>
    /// Gets the size of the field length field in directory entries.
    /// </summary>
    public int SizeOfFieldLengthField { get; init; }

    /// <summary>
    /// Gets the size of the field position field in directory entries.
    /// </summary>
    public int SizeOfFieldPositionField { get; init; }

    /// <summary>
    /// Gets the size of the field tag field in directory entries.
    /// </summary>
    public int SizeOfFieldTagField { get; init; }

    /// <summary>
    /// Creates an <see cref="Iso8211RecordLeader"/> from an <see cref="Iso8211Leader"/>.
    /// </summary>
    internal static Iso8211RecordLeader FromLeader(Iso8211Leader leader) => new()
    {
        RecordLength = leader.RecordLength,
        InterchangeLevel = leader.InterchangeLevel,
        LeaderIdentifier = leader.LeaderIdentifier,
        InlineCodeExtensionIndicator = leader.InlineCodeExtensionIndicator,
        VersionNumber = leader.VersionNumber,
        ApplicationIndicator = leader.ApplicationIndicator,
        FieldControlLength = leader.FieldControlLength,
        BaseAddressOfFieldArea = leader.BaseAddressOfFieldArea,
        ExtendedCharacterSetIndicator = $"{leader.ExtendedCharacterSetIndicator0}{leader.ExtendedCharacterSetIndicator1}{leader.ExtendedCharacterSetIndicator2}",
        SizeOfFieldLengthField = leader.SizeOfFieldLengthField,
        SizeOfFieldPositionField = leader.SizeOfFieldPositionField,
        SizeOfFieldTagField = leader.SizeOfFieldTagField
    };
}
