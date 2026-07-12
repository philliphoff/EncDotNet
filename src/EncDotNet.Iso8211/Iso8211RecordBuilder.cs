namespace EncDotNet.Iso8211;

/// <summary>
/// Builds an <see cref="Iso8211Record"/> from a set of fields, computing its directory and leader.
/// </summary>
/// <remarks>
/// <para>
/// The builder recomputes the record length, base address, directory entries, and (when
/// <see cref="Iso8211WriterOptions.AutoSizeDirectoryEntries"/> is enabled) the entry-map sizes
/// from the added fields, while preserving the leader flag characters configured on the builder.
/// </para>
/// </remarks>
public sealed class Iso8211RecordBuilder
{
    private readonly Iso8211WriterOptions _options;
    private readonly List<Iso8211Field> _fields = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211RecordBuilder"/> class.
    /// </summary>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    public Iso8211RecordBuilder(Iso8211WriterOptions? options = null)
    {
        _options = options ?? Iso8211WriterOptions.Default;
    }

    /// <summary>
    /// Gets or sets the interchange level character. Defaults to <c>'3'</c>.
    /// </summary>
    public char InterchangeLevel { get; set; } = '3';

    /// <summary>
    /// Gets or sets the leader identifier (<c>'L'</c> for a DDR, <c>'D'</c> for a data record).
    /// Defaults to <c>'D'</c>.
    /// </summary>
    public char LeaderIdentifier { get; set; } = 'D';

    /// <summary>
    /// Gets or sets the inline code extension indicator. Defaults to <c>'E'</c>.
    /// </summary>
    public char InlineCodeExtensionIndicator { get; set; } = 'E';

    /// <summary>
    /// Gets or sets the version number character. Defaults to <c>'1'</c>.
    /// </summary>
    public char VersionNumber { get; set; } = '1';

    /// <summary>
    /// Gets or sets the application indicator character. Defaults to a space.
    /// </summary>
    public char ApplicationIndicator { get; set; } = ' ';

    /// <summary>
    /// Gets or sets the field control length recorded in the leader. Defaults to <c>0</c>.
    /// </summary>
    public int FieldControlLength { get; set; }

    /// <summary>
    /// Gets or sets the three-character extended character set indicator. Defaults to three spaces.
    /// </summary>
    public string ExtendedCharacterSetIndicator { get; set; } = "   ";

    /// <summary>
    /// Gets or sets the declared size of the field tag field. Defaults to <c>4</c>.
    /// </summary>
    /// <remarks>Ignored when <see cref="Iso8211WriterOptions.AutoSizeDirectoryEntries"/> is enabled.</remarks>
    public int SizeOfFieldTagField { get; set; } = 4;

    /// <summary>
    /// Gets or sets the declared size of the field length field, or <c>0</c> to auto-size. Defaults to <c>0</c>.
    /// </summary>
    public int SizeOfFieldLengthField { get; set; }

    /// <summary>
    /// Gets or sets the declared size of the field position field, or <c>0</c> to auto-size. Defaults to <c>0</c>.
    /// </summary>
    public int SizeOfFieldPositionField { get; set; }

    /// <summary>
    /// Adds a field to the record.
    /// </summary>
    /// <param name="field">The field to add.</param>
    /// <returns>This builder, for chaining.</returns>
    public Iso8211RecordBuilder AddField(Iso8211Field field)
    {
        ArgumentNullException.ThrowIfNull(field);
        _fields.Add(field);
        return this;
    }

    /// <summary>
    /// Builds a field with <see cref="Iso8211FieldBuilder"/> and adds it to the record.
    /// </summary>
    /// <param name="fieldBuilder">The field builder to build and add.</param>
    /// <returns>This builder, for chaining.</returns>
    public Iso8211RecordBuilder AddField(Iso8211FieldBuilder fieldBuilder)
    {
        ArgumentNullException.ThrowIfNull(fieldBuilder);
        _fields.Add(fieldBuilder.Build());
        return this;
    }

    /// <summary>
    /// Adds a field from a tag and pre-encoded field data.
    /// </summary>
    /// <param name="tag">The field tag.</param>
    /// <param name="data">The field data, excluding the trailing field terminator.</param>
    /// <returns>This builder, for chaining.</returns>
    public Iso8211RecordBuilder AddField(string tag, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(data);
        _fields.Add(new Iso8211Field { Tag = tag, Data = data });
        return this;
    }

    /// <summary>
    /// Builds the <see cref="Iso8211Record"/> with a computed directory and leader.
    /// </summary>
    /// <returns>The built record.</returns>
    public Iso8211Record Build()
    {
        var seedLeader = new Iso8211RecordLeader
        {
            InterchangeLevel = InterchangeLevel,
            LeaderIdentifier = LeaderIdentifier,
            InlineCodeExtensionIndicator = InlineCodeExtensionIndicator,
            VersionNumber = VersionNumber,
            ApplicationIndicator = ApplicationIndicator,
            FieldControlLength = FieldControlLength,
            ExtendedCharacterSetIndicator = ExtendedCharacterSetIndicator,
            SizeOfFieldTagField = SizeOfFieldTagField,
            SizeOfFieldLengthField = SizeOfFieldLengthField,
            SizeOfFieldPositionField = SizeOfFieldPositionField
        };

        var layout = Iso8211RecordWriter.ComputeLayout(seedLeader, _fields, _options);

        var directory = new List<Iso8211DirectoryEntry>(_fields.Count);
        for (int i = 0; i < _fields.Count; i++)
        {
            directory.Add(new Iso8211DirectoryEntry
            {
                Tag = _fields[i].Tag,
                Length = layout.Lengths[i],
                Position = layout.Positions[i]
            });
        }

        var leader = seedLeader with
        {
            RecordLength = layout.RecordLength,
            BaseAddressOfFieldArea = layout.BaseAddress,
            SizeOfFieldTagField = layout.TagSize,
            SizeOfFieldLengthField = layout.LengthSize,
            SizeOfFieldPositionField = layout.PositionSize
        };

        return new Iso8211Record
        {
            Leader = leader,
            Directory = directory,
            Fields = new List<Iso8211Field>(_fields)
        };
    }
}
