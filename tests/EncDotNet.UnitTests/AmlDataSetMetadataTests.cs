using EncDotNet.S57;
using EncDotNet.S57.Aml;

namespace EncDotNet.UnitTests;

public class AmlDataSetMetadataTests
{
    [Fact]
    public void WellFormedComment_Parses()
    {
        const string comment = "title=Wrecks 50°N; IDO Status = Final; Protective Marking = UNCLASSIFIED; Owner Authority = Test Authority; Caveat = None";

        Assert.True(AmlDataSetMetadata.TryParse(comment, out var metadata));

        Assert.Equal("Wrecks 50°N", metadata.Title);
        Assert.Equal("Final", metadata.IdoStatus);
        Assert.Equal("UNCLASSIFIED", metadata.ProtectiveMarking);
        Assert.Equal("Test Authority", metadata.OwnerAuthority);
        Assert.Equal("None", metadata.Caveat);
        Assert.Equal(5, metadata.Entries.Count);
        Assert.Equal("Final", metadata.Entries["IDO Status"]);
    }

    [Fact]
    public void KeysIgnoreCaseAndSpacing()
    {
        const string comment = "  TITLE =  Wrecks ;IDOStatus=Draft;protective   marking=RESTRICTED ;  OWNER AUTHORITY=X;  ";

        Assert.True(AmlDataSetMetadata.TryParse(comment, out var metadata));

        Assert.Equal("Wrecks", metadata.Title);
        Assert.Equal("Draft", metadata.IdoStatus);
        Assert.Equal("RESTRICTED", metadata.ProtectiveMarking);
        Assert.Equal("X", metadata.OwnerAuthority);
        Assert.Equal("Wrecks", metadata.Entries["title"]);
    }

    [Fact]
    public void UnknownKeys_AreKeptInEntries()
    {
        Assert.True(AmlDataSetMetadata.TryParse("title=Routes; Release Date = 2026-01-01; Note = a=b", out var metadata));

        Assert.Equal("Routes", metadata.Title);
        Assert.Equal("2026-01-01", metadata.Entries["Release Date"]);
        Assert.Equal("a=b", metadata.Entries["note"]);
        Assert.Equal(3, metadata.Entries.Count);
    }

    [Fact]
    public void MissingKeys_AreNull()
    {
        Assert.True(AmlDataSetMetadata.TryParse("Protective Marking = UNCLASSIFIED", out var metadata));

        Assert.Equal("UNCLASSIFIED", metadata.ProtectiveMarking);
        Assert.Null(metadata.Title);
        Assert.Null(metadata.IdoStatus);
        Assert.Null(metadata.OwnerAuthority);
        Assert.Null(metadata.Caveat);
    }

    [Fact]
    public void DuplicateKey_KeepsTheFirstValue()
    {
        Assert.True(AmlDataSetMetadata.TryParse("title=First; Title = Second", out var metadata));

        Assert.Equal("First", metadata.Title);
        Assert.Single(metadata.Entries);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Produced by the UK Hydrographic Office")]
    [InlineData("title=Wrecks; free text without a separator")]
    [InlineData("=Wrecks; IDO Status = Final")]
    [InlineData("Producer = Someone; Version = 2")]
    public void NonAmlComment_DoesNotParse(string? comment)
    {
        Assert.False(AmlDataSetMetadata.TryParse(comment, out var metadata));
        Assert.Null(metadata);
    }

    [Fact]
    public void Parse_ThrowsForNonAmlComment()
    {
        Assert.Throws<FormatException>(() => AmlDataSetMetadata.Parse("EncDotNet S-57 writer golden cell"));
    }

    [Fact]
    public void Parse_ReadsTheCommentOfAWrittenCell()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with
        {
            DataSetIdentification = cell.DataSetIdentification! with { Comment = "title=Wrecks 50°N; IDO Status = Final" }
        };

        var document = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        var metadata = AmlDataSetMetadata.Parse(document.DataSetIdentification?.Comment ?? "");
        Assert.Equal("Wrecks 50°N", metadata.Title);
        Assert.Equal("Final", metadata.IdoStatus);
    }
}
