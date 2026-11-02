using System.Buffers.Binary;
using System.Text;
using EncDotNet.Iso8211;
using EncDotNet.S57;
using EncDotNet.Testing;

namespace EncDotNet.UnitTests;

public class S57DocumentWriterTests
{
    private const string GoldenCell = "S57DocumentWriter/" + S57WriterTestCells.CellFileName;

    [Fact]
    public void SmallCell_MatchesGolden()
    {
        var written = S57DocumentWriter.Write(S57WriterTestCells.SmallCell());

        S57WriterTestCells.AssertMatchesGolden(written, GoldenCell);
    }

    [Fact]
    public void GoldenCell_ReadsAsTheSmallCell()
    {
        var golden = S57DocumentReader.Read(File.ReadAllBytes(S57WriterTestCells.GoldenPath(GoldenCell)));

        AssertSameDocument(S57WriterTestCells.SmallCell(), golden);
    }

    [Fact]
    public void SmallCell_ReadsBackTheSame()
    {
        var cell = S57WriterTestCells.SmallCell();

        AssertSameDocument(cell, S57DocumentReader.Read(S57DocumentWriter.Write(cell)));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 2)]
    public void LexicalLevels_ReadBackTheSame(int aall, int nall)
    {
        var cell = WithText(S57WriterTestCells.SmallCell(aall, nall), aall, nall);

        AssertSameDocument(cell, S57DocumentReader.Read(S57DocumentWriter.Write(cell)));
    }

    [Fact]
    public void Latin1Attributes_AreWrittenAsSingleBytes()
    {
        var written = S57DocumentWriter.Write(S57WriterTestCells.SmallCell());

        // OBJNAM at lexical level 1 is ISO 8859-1, so '°' is the single byte 0xB0.
        var objnam = Encoding.Latin1.GetBytes("Bouée du Chêne 45°\u001F");
        Assert.True(written.AsSpan().IndexOf(objnam) >= 0);
    }

    [Fact]
    public void Ucs2NationalAttributes_EndWithTwoByteTerminators()
    {
        var bytes = S57DocumentWriter.Write(S57WriterTestCells.SmallCell());
        var natf = Iso8211DocumentReader.Read(bytes).DataRecords.SelectMany(r => r.Fields).Single(f => f.Tag == S57FieldTags.NATF);
        var text = Encoding.Unicode.GetBytes("Noordzee 1 \u03a9\u03bc\u03ad\u03b3\u03b1");

        // ATTL, the UCS-2 text, then the UCS-2 unit and field terminators (S-57 Part 3,
        // clause 2.5). The reader leaves the field terminator's first byte in the data.
        Assert.Equal(text, natf.Data.Span[2..^3].ToArray());
        Assert.Equal(new byte[] { 0x1F, 0x00, 0x1E }, natf.Data.Span[^3..].ToArray());
        Assert.True(bytes.AsSpan().IndexOf((byte[])[.. text, 0x1F, 0x00, 0x1E, 0x00]) >= 0);
    }

    [Fact]
    public void Ddr_DeclaresTheLexicalLevelsInTheFieldControls()
    {
        var ddr = Iso8211DocumentReader.Read(S57DocumentWriter.Write(S57WriterTestCells.SmallCell())).Records[0];

        Assert.Equal("3LE1 09", LeaderText(ddr)[5..12]);
        Assert.Equal(" ! ", LeaderText(ddr)[17..20]);
        Assert.Equal("2600;&-A ", FieldControls(ddr, S57FieldTags.ATTF));
        Assert.Equal("2600;&%/A", FieldControls(ddr, S57FieldTags.NATF));
        Assert.Equal("1600;&   ", FieldControls(ddr, S57FieldTags.DSID));
    }

    [Fact]
    public void DataRecords_HaveTheS57Leader()
    {
        var bytes = S57DocumentWriter.Write(S57WriterTestCells.SmallCell());
        var records = Iso8211DocumentReader.Read(bytes).Records;

        // S-57 Part 3, Annex A, table A.3: " D     " then a blank field control length.
        var offset = records[0].Leader.RecordLength;
        Assert.Equal(" D     ", Encoding.ASCII.GetString(bytes, offset + 5, 7));
        Assert.Equal("   ", Encoding.ASCII.GetString(bytes, offset + 17, 3));
    }

    [Fact]
    public void DataRecords_AreNumberedFromOne()
    {
        var records = Iso8211DocumentReader.Read(S57DocumentWriter.Write(S57WriterTestCells.SmallCell())).DataRecords.ToList();

        for (var i = 0; i < records.Count; i++)
        {
            Assert.Equal("0001", records[i].Fields[0].Tag);
            Assert.Equal(i + 1, BinaryPrimitives.ReadUInt16LittleEndian(records[i].Fields[0].Data.Span));
        }
    }

    [Fact]
    public void Dssi_CountsTheRecords()
    {
        var bytes = S57DocumentWriter.Write(S57WriterTestCells.SmallCell());
        var document = Iso8211DocumentReader.Read(bytes);
        var definitions = Iso8211DataDescriptiveRecordReader.Read(document.Records[0]);
        var dssi = document.DataRecords.SelectMany(r => r.Fields).Single(f => f.Tag == S57FieldTags.DSSI);
        var reader = new Iso8211FieldReader(definitions.GetFieldDefinition(S57FieldTags.DSSI) ?? throw new InvalidOperationException(), dssi.Data);

        // One meta (M_COVR), three geo, one collection (C_AGGR); two isolated nodes, two
        // connected nodes, one edge, no faces.
        uint[] expected = [1, 0, 3, 1, 2, 2, 1, 0];
        string[] names = ["NOMR", "NOCR", "NOGR", "NOLR", "NOIN", "NOCN", "NOED", "NOFA"];
        Assert.Equal(expected, names.Select(n => reader.GetSubfield<uint>(n)).ToArray());
    }

    [Fact]
    public void Fields_AreWrittenInTreeOrder()
    {
        var records = Iso8211DocumentReader.Read(S57DocumentWriter.Write(S57WriterTestCells.SmallCell())).DataRecords
            .Select(r => string.Join(',', r.Fields.Select(f => f.Tag)))
            .ToList();

        Assert.Equal("0001,DSID,DSSI", records[0]);
        Assert.Equal("0001,DSPM", records[1]);
        Assert.Equal("0001,VRID,ATTV,SG2D", records[2]);
        Assert.Equal("0001,VRID,SG3D", records[3]);
        Assert.Equal("0001,VRID,VRPT,SG2D", records[6]);
        Assert.Equal("0001,FRID,FOID,ATTF,NATF,FFPT,FSPT", records[8]);
        Assert.Equal("0001,FRID,FOID,FFPT", records[11]);
    }

    [Fact]
    public void Write_IsDeterministic()
    {
        Assert.Equal(S57DocumentWriter.Write(S57WriterTestCells.SmallCell()), S57DocumentWriter.Write(S57WriterTestCells.SmallCell()));
    }

    [Fact]
    public async Task StreamAndFileOverloads_WriteTheSameBytes()
    {
        var cell = S57WriterTestCells.SmallCell();
        var expected = S57DocumentWriter.Write(cell);

        using var stream = new MemoryStream();
        S57DocumentWriter.Write(stream, cell);
        Assert.Equal(expected, stream.ToArray());

        using var asyncStream = new MemoryStream();
        await S57DocumentWriter.WriteAsync(asyncStream, cell, TestContext.Current.CancellationToken);
        Assert.Equal(expected, asyncStream.ToArray());

        var path = Path.GetTempFileName();
        try
        {
            S57DocumentWriter.WriteToFile(path, cell);
            Assert.Equal(expected, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));

            await S57DocumentWriter.WriteToFileAsync(path, cell, TestContext.Current.CancellationToken);
            Assert.Equal(expected, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(path);
        }
    }

    public static TheoryData<string> Profiles => new() { "Harbour", "Approach", "Large" };

    [Theory]
    [MemberData(nameof(Profiles))]
    public void SyntheticCells_ReadBackTheSame(string profileName)
    {
        var source = S57DocumentReader.Read(SyntheticS57Cell.Create(Profile(profileName)));
        var written = S57DocumentWriter.Write(source);

        AssertSameDocument(source, S57DocumentReader.Read(written));

        // Writing what was read back gives the same bytes again.
        Assert.Equal(written, S57DocumentWriter.Write(S57DocumentReader.Read(written)));
    }

    [Fact]
    public void FeatureWithoutLongName_ReadsBackWithoutOne()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with
        {
            FeatureRecords = [cell.FeatureRecords[0] with { RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, 1) }]
        };

        var reread = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        Assert.Null(Assert.Single(reread.FeatureRecords).LongName);
    }

    [Fact]
    public void MissingExchangePurpose_IsWrittenAsNew()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with { DataSetIdentification = cell.DataSetIdentification! with { ExchangePurpose = 0 } };

        var reread = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        Assert.Equal(1, reread.DataSetIdentification?.ExchangePurpose);
    }

    [Fact]
    public void MissingDataSetIdentification_Throws()
    {
        var cell = S57WriterTestCells.SmallCell() with { DataSetIdentification = null };

        Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
    }

    [Fact]
    public void MissingDataSetParameters_Throws()
    {
        var cell = S57WriterTestCells.SmallCell() with { DataSetParameters = null };

        Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
    }

    [Fact]
    public void UpdateRecord_Throws()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with { FeatureRecords = [cell.FeatureRecords[0] with { UpdateInstruction = S57UpdateInstruction.Modify }] };

        var exception = Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
        Assert.Contains("RUIN", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UpdateControlField_Throws()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with
        {
            VectorRecords = [cell.VectorRecords[0] with { CoordinateControl = new S57FieldUpdateControl { UpdateInstruction = S57UpdateInstruction.Insert, Index = 1, Count = 1 } }]
        };

        Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
    }

    [Fact]
    public void ValueOutsideItsFormat_ThrowsNamingTheSubfield()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with { FeatureRecords = [cell.FeatureRecords[0] with { Group = 256 }] };

        var exception = Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
        Assert.Contains("GRUP", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 2, "Bouée")]          // ATTF at level 0 is ASCII.
    [InlineData(1, 2, "Ωμέγα")] // ATTF at level 1 is ISO 8859-1.
    public void AttributeTheLevelCantEncode_Throws(int aall, int nall, string objnam)
    {
        var cell = S57WriterTestCells.SmallCell(aall, nall);
        cell = cell with { FeatureRecords = [cell.FeatureRecords[1] with { Attributes = [new S57AttributeValue(116, objnam)] }] };

        var exception = Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
        Assert.Contains("ATVL", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NationalAttributeOutsideTheBasicMultilingualPlane_Throws()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with { FeatureRecords = [cell.FeatureRecords[1] with { NationalAttributes = [new S57AttributeValue(301, "\U0001F6A2")] }] };

        Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(0, 3)]
    public void UnsupportedLexicalLevel_Throws(int aall, int nall)
    {
        Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(S57WriterTestCells.SmallCell(aall, nall)));
    }

    [Fact]
    public void Latin1AttributeOfARealCellLayout_ReadsAsLatin1()
    {
        // The reader decodes ATTF at the DSSI lexical level: ISO 8859-1 at level 1, where it
        // used to decode UTF-8 and turn '°' (0xB0) into U+FFFD.
        var document = S57DocumentReader.Read(S57DocumentWriter.Write(S57WriterTestCells.SmallCell()));

        Assert.Equal("Bouée du Chêne 45°", document.FeatureRecords[1].Attributes[2].Value);
    }

    internal static void AssertSameDocument(S57Document expected, S57Document actual)
    {
        Assert.Equal(S57DocumentFingerprint.Dump(expected), S57DocumentFingerprint.Dump(actual));
        Assert.Equal(expected.DataSetIdentification?.ExchangePurpose, actual.DataSetIdentification?.ExchangePurpose);
    }

    private static S57Document WithText(S57Document cell, int aall, int nall)
    {
        // Text each lexical level can hold: ASCII, then Latin-1, then Greek.
        var attf = aall == 0 ? "North sea 1" : "Mer du Nord °";
        var natf = nall switch { 0 => "Noordzee 1", 1 => "Nordsee ü", _ => "Βόρεια Θάλασσα" };
        var buoy = cell.FeatureRecords[1] with
        {
            Attributes = [new S57AttributeValue(116, attf)],
            NationalAttributes = [new S57AttributeValue(301, natf), new S57AttributeValue(304, "")]
        };
        return cell with { FeatureRecords = [cell.FeatureRecords[0], buoy, .. cell.FeatureRecords.Skip(2)] };
    }

    private static SyntheticCellProfile Profile(string name) => name switch
    {
        "Large" => SyntheticCellProfile.Large,
        "Harbour" => SyntheticCellProfile.Harbour,
        "Approach" => SyntheticCellProfile.Approach,
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private static string LeaderText(Iso8211Record record)
    {
        var leader = record.Leader;
        return $"{leader.RecordLength:D5}{leader.InterchangeLevel}{leader.LeaderIdentifier}{leader.InlineCodeExtensionIndicator}{leader.VersionNumber}{leader.ApplicationIndicator}{leader.FieldControlLength:D2}{leader.BaseAddressOfFieldArea:D5}{leader.ExtendedCharacterSetIndicator}";
    }

    private static string FieldControls(Iso8211Record ddr, string tag) =>
        Encoding.ASCII.GetString(ddr.Fields.Single(f => f.Tag == tag).Data.Span[..9]);
}
