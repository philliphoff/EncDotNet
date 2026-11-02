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
    /// Gets or sets a value indicating whether a data record whose leader has a field control
    /// length of zero is written with two spaces there instead of <c>00</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only the DDR uses field controls, so ISO/IEC 8211 leaves leader positions 10–11 of a data
    /// record blank, and S-57 requires them to be two spaces (S-57 Part 3, Annex A, table A.3).
    /// The reader reads both forms as zero. The DDR (leader identifier <c>'L'</c>) is unaffected.
    /// </para>
    /// <para>The default is <c>false</c>, which writes <c>00</c>.</para>
    /// </remarks>
    public bool BlankDataRecordFieldControlLength { get; set; }

    /// <summary>
    /// Gets or sets the tags of data record fields that end with the two-byte field terminator
    /// <c>0x1E 0x00</c>, or <c>null</c> for none.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A field encoded in a two-byte character set has its terminators in that character set:
    /// S-57 ends a field at lexical level 2 (UCS-2), such as a national attribute (NATF) field,
    /// with <c>0x1E 0x00</c> (S-57 Part 3, clause 2.5 and Annex B.2).
    /// </para>
    /// <para>
    /// <see cref="Iso8211DocumentReader"/> removes only the final byte of every field as its
    /// terminator, so the data of such a field keeps the terminator's first byte, <c>0x1E</c>.
    /// The writer ends a field whose tag is in this set with <c>0x00</c> instead of
    /// <c>0x1E</c>, completing the terminator, so these fields read and write back unchanged.
    /// Their data must end with <c>0x1E</c>; writing one that doesn't throws
    /// <see cref="InvalidOperationException"/>. The fields of the DDR (leader identifier
    /// <c>'L'</c>), which describe these fields, keep one-byte terminators.
    /// </para>
    /// </remarks>
    public IReadOnlySet<string>? TwoByteTerminatorFieldTags { get; set; }

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
