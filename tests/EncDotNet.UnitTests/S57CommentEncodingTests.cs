using System.Text;
using EncDotNet.S57;

namespace EncDotNet.UnitTests;

public class S57CommentEncodingTests
{
    // UKHO AML 1.0 cells put the degree sign (0xB0 in ISO 8859-1) in the DSID comment.
    private const string DsidComment = "title=Wrecks 50°N; IDO Status = Final; Protective Marking = UNCLASSIFIED; Owner Authority = Test";
    private const string DspmComment = "Bezugssystem für Höhen";
    private const string FfptComment = "Ausrüstung";

    [Fact]
    public void Latin1Comments_AreWrittenAsSingleBytes()
    {
        var written = S57DocumentWriter.Write(CellWithLatin1Comments());

        Assert.True(written.AsSpan().IndexOf(Encoding.Latin1.GetBytes(DsidComment)) >= 0);
        Assert.True(written.AsSpan().IndexOf(Encoding.Latin1.GetBytes(DspmComment)) >= 0);
        Assert.True(written.AsSpan().IndexOf(Encoding.Latin1.GetBytes(FfptComment)) >= 0);
    }

    [Fact]
    public void Latin1Comments_ReadAsLatin1()
    {
        // The reader used to decode COMT as UTF-8, turning a lone 0xB0 into U+FFFD.
        var document = S57DocumentReader.Read(S57DocumentWriter.Write(CellWithLatin1Comments()));

        Assert.Equal(DsidComment, document.DataSetIdentification?.Comment);
        Assert.Equal(DspmComment, document.DataSetParameters?.Comment);
        Assert.Equal(FfptComment, FeatureWithPointerComment(document).FeaturePointers[0].Comment);
    }

    [Fact]
    public void Latin1Comments_RoundTrip()
    {
        var cell = CellWithLatin1Comments();

        var reread = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        S57DocumentWriterTests.AssertSameDocument(cell, reread);
        Assert.Equal(S57DocumentWriter.Write(cell), S57DocumentWriter.Write(reread));
    }

    [Fact]
    public void CommentOutsideLatin1_Throws()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with { DataSetIdentification = cell.DataSetIdentification! with { Comment = "Ωμέγα" } };

        var exception = Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(cell));
        Assert.Contains("COMT", exception.Message, StringComparison.Ordinal);
    }

    private static S57Document CellWithLatin1Comments()
    {
        var cell = S57WriterTestCells.SmallCell();
        var index = cell.FeatureRecords.ToList().FindIndex(f => f.FeaturePointers.Any(p => p.Comment == "equipment"));
        var feature = cell.FeatureRecords[index];
        feature = feature with
        {
            FeaturePointers = [feature.FeaturePointers[0] with { Comment = FfptComment }, .. feature.FeaturePointers.Skip(1)]
        };

        return cell with
        {
            DataSetIdentification = cell.DataSetIdentification! with { Comment = DsidComment },
            DataSetParameters = cell.DataSetParameters! with { Comment = DspmComment },
            FeatureRecords = [.. cell.FeatureRecords.Take(index), feature, .. cell.FeatureRecords.Skip(index + 1)]
        };
    }

    private static S57FeatureRecord FeatureWithPointerComment(S57Document document) =>
        document.FeatureRecords.Single(f => f.FeaturePointers.Any(p => p.Comment == FfptComment));
}
