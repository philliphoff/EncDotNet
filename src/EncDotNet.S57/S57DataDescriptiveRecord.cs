using System.Text;
using EncDotNet.Iso8211;

namespace EncDotNet.S57;

/// <summary>
/// Builds the data descriptive record (DDR) that the S-57 writers put at the start of a file.
/// </summary>
/// <remarks>
/// The DDR follows S-57 Part 3, Annex A.2: a leader of <c>3LE1 09</c> with the extended
/// character set indicator <c>" ! "</c>, nine-character field controls, and the field names,
/// subfield labels and formats of the field tree structures in Part 3, clause 7. The field
/// controls of ATTF and NATF carry the truncated escape sequence of their lexical level
/// (Annex B.2, table B.1).
/// </remarks>
internal static class S57DataDescriptiveRecord
{
    private const byte UnitTerminator = 0x1F;

    private const string ElementaryField = "0";
    private const string VectorField = "1";
    private const string ArrayField = "2";
    private const string ImplicitPoint = "1";
    private const string BinaryForm = "5";
    private const string MixedDataTypes = "6";

    // The DDR of a base cell, in the order the ENC product specification lists its fields.
    private static readonly FieldDefinition[] CellFields =
    [
        new(S57FieldTags.DSID, VectorField, MixedDataTypes, "Data set identification field",
            "RCNM!RCID!EXPP!INTU!DSNM!EDTN!UPDN!UADT!ISDT!STED!PRSP!PSDN!PRED!PROF!AGEN!COMT",
            "(b11,b14,2b11,3A,2A(8),R(4),b11,2A,b11,b12,A)"),
        new(S57FieldTags.DSSI, VectorField, MixedDataTypes, "Data set structure information field",
            "DSTR!AALL!NALL!NOMR!NOCR!NOGR!NOLR!NOIN!NOCN!NOED!NOFA",
            "(3b11,8b14)"),
        new(S57FieldTags.DSPM, VectorField, MixedDataTypes, "Data set parameter field",
            "RCNM!RCID!HDAT!VDAT!SDAT!CSCL!DUNI!HUNI!PUNI!COUN!COMF!SOMF!COMT",
            "(b11,b14,3b11,b14,4b11,2b14,A)"),
        new(S57FieldTags.VRID, VectorField, MixedDataTypes, "Vector record identifier field",
            "RCNM!RCID!RVER!RUIN",
            "(b11,b14,b12,b11)"),
        new(S57FieldTags.ATTV, ArrayField, MixedDataTypes, "Vector record attribute field",
            "*ATTL!ATVL",
            "(b12,A)"),
        new(S57FieldTags.VRPT, ArrayField, MixedDataTypes, "Vector record pointer field",
            "*NAME!ORNT!USAG!TOPI!MASK",
            "(B(40),4b11)"),
        new(S57FieldTags.SG2D, ArrayField, BinaryForm, "2-D coordinate field",
            "*YCOO!XCOO",
            "(2b24)"),
        new(S57FieldTags.SG3D, ArrayField, BinaryForm, "3-D coordinate (sounding array) field",
            "*YCOO!XCOO!VE3D",
            "(3b24)"),
        new(S57FieldTags.FRID, VectorField, MixedDataTypes, "Feature record identifier field",
            "RCNM!RCID!PRIM!GRUP!OBJL!RVER!RUIN",
            "(b11,b14,2b11,2b12,b11)"),
        new(S57FieldTags.FOID, VectorField, MixedDataTypes, "Feature object identifier field",
            "AGEN!FIDN!FIDS",
            "(b12,b14,b12)"),
        new(S57FieldTags.ATTF, ArrayField, MixedDataTypes, "Feature record attribute field",
            "*ATTL!ATVL",
            "(b12,A)"),
        new(S57FieldTags.NATF, ArrayField, MixedDataTypes, "Feature record national attribute field",
            "*ATTL!ATVL",
            "(b12,A)"),
        new(S57FieldTags.FFPT, ArrayField, MixedDataTypes, "Feature record to feature object pointer field",
            "*LNAM!RIND!COMT",
            "(B(64),b11,A)"),
        new(S57FieldTags.FSPT, ArrayField, MixedDataTypes, "Feature record to spatial record pointer field",
            "*NAME!ORNT!USAG!MASK",
            "(B(40),3b11)"),
    ];

    // The parent and child of each field in the tree structures of S-57 Part 3, clause 7.
    private const string CellFieldTree =
        "0001DSIDDSIDDSSI0001DSPM0001VRIDVRIDATTVVRIDVRPTVRIDSG2DVRIDSG3D" +
        "0001FRIDFRIDFOIDFRIDATTFFRIDNATFFRIDFFPTFRIDFSPT";

    // The arc/curve fields of vector records (S-57 Part 3, clauses 7.7.1.8 to 7.7.1.11), which a
    // DDR declares only when the cell has arc/curve geometry. The AR2D and EL2D descriptors are
    // ISO/IEC 8211 Cartesian labels: one row per point, each a YCOO and XCOO pair.
    private static readonly FieldDefinition[] ArcCurveFields =
    [
        new(S57FieldTags.ARCC, VectorField, MixedDataTypes, "Arc/Curve definition field",
            "ATYP!SURF!ORDR!RESO!FPMF",
            "(3b11,2b14)"),
        new(S57FieldTags.AR2D, ArrayField, BinaryForm, "Arc coordinate field",
            "STPT!CTPT!ENPT*YCOO!XCOO",
            "(2b24)"),
        new(S57FieldTags.EL2D, ArrayField, BinaryForm, "Ellipse coordinate field",
            "STPT!CTPT!ENPT!CDPM!CDPR*YCOO!XCOO",
            "(2b24)"),
        new(S57FieldTags.CT2D, ArrayField, BinaryForm, "Curve coordinate field",
            "*YCOO!XCOO",
            "(2b24)"),
    ];

    private const string ArcCurveFieldTree = "VRIDARCCARCCAR2DARCCEL2DARCCCT2D";

    private const string CatalogFieldTree = "0001CATD";

    /// <summary>
    /// Builds the DDR of a base cell.
    /// </summary>
    /// <param name="attfLexicalLevel">The ATTF lexical level (AALL).</param>
    /// <param name="natfLexicalLevel">The NATF lexical level (NALL).</param>
    /// <param name="includeArcCurves">Whether to declare the arc/curve fields (ARCC, AR2D, EL2D
    /// and CT2D), after SG3D.</param>
    public static Iso8211Record Build(int attfLexicalLevel, int natfLexicalLevel, bool includeArcCurves = false)
    {
        var builder = NewDdr();
        var fieldTree = includeArcCurves
            ? CellFieldTree.Replace("VRIDSG3D", "VRIDSG3D" + ArcCurveFieldTree, StringComparison.Ordinal)
            : CellFieldTree;
        builder.AddField(FileControlField(fieldTree));
        builder.AddField(RecordIdentifierField(BinaryForm, "(b12)"));
        IEnumerable<FieldDefinition> fields = includeArcCurves
            ? [.. CellFields.TakeWhile(f => f.Tag != S57FieldTags.FRID), .. ArcCurveFields, .. CellFields.SkipWhile(f => f.Tag != S57FieldTags.FRID)]
            : CellFields;
        foreach (var field in fields)
        {
            var escape = field.Tag switch
            {
                S57FieldTags.ATTF => EscapeSequence(attfLexicalLevel),
                S57FieldTags.NATF => EscapeSequence(natfLexicalLevel),
                _ => EscapeSequence(0)
            };
            builder.AddField(DescriptiveField(field, escape));
        }

        return builder.Build();
    }

    /// <summary>
    /// Builds the DDR of a <c>CATALOG.031</c> file, in the ASCII implementation.
    /// </summary>
    /// <remarks>
    /// The catalogue uses only lexical level 0, so its leader has no extended character set
    /// indicator. Its directory entries have six-digit lengths and positions, as NOAA's do.
    /// </remarks>
    public static Iso8211Record BuildCatalog()
    {
        var builder = NewDdr(autoSizeDirectoryEntries: false);
        builder.ExtendedCharacterSetIndicator = "   ";
        builder.SizeOfFieldLengthField = 6;
        builder.SizeOfFieldPositionField = 6;
        builder.AddField(FileControlField(CatalogFieldTree));
        builder.AddField(RecordIdentifierField(ImplicitPoint, "(I(5))"));
        builder.AddField(DescriptiveField(
            new(S57FieldTags.CATD, VectorField, MixedDataTypes, "Catalogue Directory Field",
                "RCNM!RCID!FILE!LFIL!VOLM!IMPL!SLAT!WLON!NLAT!ELON!CRCS!COMT",
                "(A(2),I(10),3A,A(3),4R,2A)"),
            EscapeSequence(0)));
        return builder.Build();
    }

    private static Iso8211RecordBuilder NewDdr(bool autoSizeDirectoryEntries = true) => new(new Iso8211WriterOptions { AutoSizeDirectoryEntries = autoSizeDirectoryEntries })
    {
        InterchangeLevel = '3',
        LeaderIdentifier = 'L',
        InlineCodeExtensionIndicator = 'E',
        VersionNumber = '1',
        ApplicationIndicator = ' ',
        FieldControlLength = 9,
        ExtendedCharacterSetIndicator = " ! "
    };

    // The truncated escape sequence of a lexical level (S-57 Part 3, Annex B.2, table B.1).
    private static string EscapeSequence(int lexicalLevel) => lexicalLevel switch
    {
        1 => "-A ",
        2 => "%/A",
        _ => "   "
    };

    // The file control field: field controls, an empty file title, and the field tree.
    private static Iso8211Field FileControlField(string fieldTree)
    {
        var data = new List<byte>(Encoding.ASCII.GetBytes("0000;&   "));
        data.Add(UnitTerminator);
        data.AddRange(Encoding.ASCII.GetBytes(fieldTree));
        return new Iso8211Field { Tag = "0000", Data = data.ToArray() };
    }

    private static Iso8211Field RecordIdentifierField(string dataType, string format) =>
        DescriptiveField(new("0001", ElementaryField, dataType, "ISO/IEC 8211 Record Identifier", string.Empty, format), EscapeSequence(0));

    // Field controls (structure, type, "00", ";&", escape sequence), then the field name,
    // subfield labels and format controls, separated by unit terminators.
    private static Iso8211Field DescriptiveField(FieldDefinition field, string escapeSequence)
    {
        var data = new List<byte>(128);
        data.AddRange(Encoding.ASCII.GetBytes($"{field.Structure}{field.Type}00;&{escapeSequence}{field.Name}"));
        data.Add(UnitTerminator);
        data.AddRange(Encoding.ASCII.GetBytes(field.Labels));
        data.Add(UnitTerminator);
        data.AddRange(Encoding.ASCII.GetBytes(field.Formats));
        return new Iso8211Field { Tag = field.Tag, Data = data.ToArray() };
    }

    private readonly record struct FieldDefinition(string Tag, string Structure, string Type, string Name, string Labels, string Formats);
}
