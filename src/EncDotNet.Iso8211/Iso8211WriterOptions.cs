using System.Text;

namespace EncDotNet.Iso8211;

/// <summary>
/// Options for configuring the ISO 8211 writer and builders.
/// </summary>
/// <remarks>
/// <para>
/// These options are the write-side counterpart to <see cref="Iso8211ReaderOptions"/> and
/// control how records, fields, and subfields are encoded to ISO 8211 bytes.
/// </para>
/// </remarks>
public sealed class Iso8211WriterOptions
{
    /// <summary>
    /// Gets the default writer options (ASCII, lexical level 1, auto-sized directory entries).
    /// </summary>
    public static Iso8211WriterOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the lexical level, which determines terminator width and character encoding.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lexical level 0 and 1 use single-byte terminators (UT <c>0x1F</c>, FT <c>0x1E</c>) and
    /// ASCII/UTF-8 character data. Lexical level 2 uses two-byte terminators (e.g. <c>0x1F 0x00</c>)
    /// and UCS-2 character data, consistent with <see cref="Iso8211FieldReader"/>.
    /// </para>
    /// <para>The default is <c>1</c>.</para>
    /// </remarks>
    public int LexicalLevel { get; set; } = 1;

    /// <summary>
    /// Gets or sets the character encoding used for character-based subfield data.
    /// </summary>
    /// <remarks>
    /// When <c>null</c> (the default), the encoding is derived from <see cref="LexicalLevel"/>:
    /// ASCII for levels 0/1 and Unicode (UCS-2) for level 2.
    /// </remarks>
    public Encoding? Encoding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether directory entry field-length and field-position
    /// widths are automatically sized to fit the record, overriding the sizes declared on the
    /// record leader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <c>false</c> (the default), the writer preserves the entry-map sizes declared on the
    /// leader (<see cref="Iso8211RecordLeader.SizeOfFieldLengthField"/>,
    /// <see cref="Iso8211RecordLeader.SizeOfFieldPositionField"/>,
    /// <see cref="Iso8211RecordLeader.SizeOfFieldTagField"/>), which yields byte-identical output
    /// for canonically-encoded sources.
    /// </para>
    /// <para>
    /// When <c>true</c>, the writer computes the minimum widths required to encode the record's
    /// largest field length and position. This is useful when constructing records from scratch.
    /// </para>
    /// </remarks>
    public bool AutoSizeDirectoryEntries { get; set; }

    /// <summary>
    /// Gets the terminator width in bytes implied by <see cref="LexicalLevel"/>.
    /// </summary>
    internal int TerminatorWidth => LexicalLevel >= 2 ? 2 : 1;

    /// <summary>
    /// Gets the effective character encoding, deriving it from <see cref="LexicalLevel"/> when
    /// <see cref="Encoding"/> is not explicitly set.
    /// </summary>
    internal Encoding EffectiveEncoding => Encoding ?? (LexicalLevel >= 2 ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII);
}
