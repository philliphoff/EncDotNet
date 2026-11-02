using System.Text;
using EncDotNet.Iso8211;
using EncDotNet.S57;
using EncDotNet.S57.ExchangeSets;

namespace EncDotNet.UnitTests;

public class S57CatalogWriterTests
{
    private const string GoldenCatalog = "S57DocumentWriter/CATALOG.031";

    [Fact]
    public void SmallCatalog_MatchesGolden()
    {
        var cellCrc = S57CatalogWriter.ComputeCrc(S57DocumentWriter.Write(S57WriterTestCells.SmallCell()));

        S57WriterTestCells.AssertMatchesGolden(S57CatalogWriter.Write(S57WriterTestCells.SmallCatalog(cellCrc)), GoldenCatalog);
    }

    [Fact]
    public void SmallCatalog_ReadsBackTheSame()
    {
        var catalog = S57WriterTestCells.SmallCatalog("01234567");

        Assert.Equal(catalog.Entries, S57CatalogReader.Read(S57CatalogWriter.Write(catalog)).Entries);
    }

    [Fact]
    public void Coordinates_AreWrittenWithSixDecimalsWhenExact()
    {
        var bytes = S57CatalogWriter.Write(S57WriterTestCells.SmallCatalog("01234567"));

        Assert.Contains("BIN50.100000\u001F-1.250000\u001F50.150000\u001F-1.200000\u001F01234567\u001F", Encoding.ASCII.GetString(bytes), StringComparison.Ordinal);
    }

    [Fact]
    public void PreciseCoordinates_ReadBackTheSame()
    {
        var catalog = new S57Catalog
        {
            Entries = [new S57CatalogEntry { RecordId = 1, FileName = "A.000", SouthernmostLatitude = 50.123456789, NorthernmostLatitude = -0.1 }]
        };

        var entry = Assert.Single(S57CatalogReader.Read(S57CatalogWriter.Write(catalog)).Entries);

        Assert.Equal(50.123456789, entry.SouthernmostLatitude);
        Assert.Equal(-0.1, entry.NorthernmostLatitude);
        Assert.Null(entry.WesternmostLongitude);
        Assert.Equal("CD", entry.RecordName);
    }

    [Fact]
    public void Records_UseTheS57Leaders()
    {
        var bytes = S57CatalogWriter.Write(S57WriterTestCells.SmallCatalog("01234567"));
        var records = Iso8211DocumentReader.Read(bytes).Records;

        Assert.Equal("3LE1 09", Encoding.ASCII.GetString(bytes, 5, 7));
        var offset = records[0].Leader.RecordLength;
        Assert.Equal(" D     ", Encoding.ASCII.GetString(bytes, offset + 5, 7));
        Assert.Equal("00000", Encoding.ASCII.GetString(records[1].Fields[0].Data.Span));
        Assert.Equal("00001", Encoding.ASCII.GetString(records[2].Fields[0].Data.Span));
    }

    [Fact]
    public void NonAsciiText_Throws()
    {
        var catalog = new S57Catalog { Entries = [new S57CatalogEntry { RecordId = 1, FileName = "A.000", LongFileName = "Kīlauea" }] };

        var exception = Assert.Throws<ArgumentException>(() => S57CatalogWriter.Write(catalog));
        Assert.Contains("LFIL", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ComputeCrc_IsTheExchangeSetCrc32()
    {
        // The CRC-32 check value (of the ASCII digits 1 to 9).
        Assert.Equal("CBF43926", S57CatalogWriter.ComputeCrc("123456789"u8));
    }

    [Fact]
    public async Task ComputeCrcAsync_MatchesComputeCrc()
    {
        var data = S57DocumentWriter.Write(S57WriterTestCells.SmallCell());
        using var stream = new MemoryStream(data);

        Assert.Equal(S57CatalogWriter.ComputeCrc(data), await S57CatalogWriter.ComputeCrcAsync(stream, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WrittenExchangeSet_Verifies()
    {
        var root = Directory.CreateTempSubdirectory("encdotnet-writer-").FullName;
        try
        {
            var cell = S57DocumentWriter.Write(S57WriterTestCells.SmallCell());
            Directory.CreateDirectory(Path.Combine(root, "XX5WRITE"));
            await File.WriteAllBytesAsync(Path.Combine(root, "XX5WRITE", S57WriterTestCells.CellFileName), cell, TestContext.Current.CancellationToken);
            await S57CatalogWriter.WriteToFileAsync(
                Path.Combine(root, "CATALOG.031"),
                S57WriterTestCells.SmallCatalog(S57CatalogWriter.ComputeCrc(cell)),
                TestContext.Current.CancellationToken);

            var catalog = await S57CatalogReader.ReadFromFileAsync(Path.Combine(root, "CATALOG.031"), cancellationToken: TestContext.Current.CancellationToken);
            var result = await new S57ExchangeSetVerifier().VerifyAsync(root, catalog, cancellationToken: TestContext.Current.CancellationToken);

            Assert.False(result.HasChecksumMismatches);
            Assert.False(result.HasMissingFiles);
            Assert.Contains(result.FileResults, r => r.ChecksumOutcome == S57VerificationOutcome.Ok);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task StreamAndFileOverloads_WriteTheSameBytes()
    {
        var catalog = S57WriterTestCells.SmallCatalog("01234567");
        var expected = S57CatalogWriter.Write(catalog);

        using var stream = new MemoryStream();
        S57CatalogWriter.Write(stream, catalog);
        Assert.Equal(expected, stream.ToArray());

        using var asyncStream = new MemoryStream();
        await S57CatalogWriter.WriteAsync(asyncStream, catalog, TestContext.Current.CancellationToken);
        Assert.Equal(expected, asyncStream.ToArray());

        var path = Path.GetTempFileName();
        try
        {
            S57CatalogWriter.WriteToFile(path, catalog);
            Assert.Equal(expected, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
