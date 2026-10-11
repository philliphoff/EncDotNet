using System.Runtime.InteropServices;
using EncDotNet.Iso8211;
using EncDotNet.Testing;

namespace EncDotNet.UnitTests;

/// <summary>
/// Tests for <see cref="Iso8211DocumentReader.EnumerateRecords(ReadOnlyMemory{byte})"/> and for how
/// each read method shares, or doesn't share, the caller's memory.
/// </summary>
public class Iso8211DocumentReaderStreamingTests
{
    [Fact]
    public void EnumerateRecords_ReturnsTheSameRecordsAsRead()
    {
        // The Large profile includes a record longer than 99 999 bytes, whose leader length is 00000.
        var bytes = SyntheticS57Cell.Create(SyntheticCellProfile.Large);

        var document = Iso8211DocumentReader.Read(bytes);
        var streamed = Iso8211DocumentReader.EnumerateRecords(bytes).ToList();

        Assert.Equal(document.Records.Count, streamed.Count);
        for (var r = 0; r < streamed.Count; r++)
        {
            var expected = document.Records[r];
            var actual = streamed[r];
            Assert.Equal(expected.Leader, actual.Leader);
            Assert.Equal(expected.Directory, actual.Directory);
            Assert.Equal(expected.Fields.Count, actual.Fields.Count);
            for (var f = 0; f < actual.Fields.Count; f++)
            {
                Assert.Equal(expected.Fields[f].Tag, actual.Fields[f].Tag);
                Assert.True(expected.Fields[f].Data.Span.SequenceEqual(actual.Fields[f].Data.Span), $"record {r} field {f}");
            }
        }
    }

    [Fact]
    public void EnumerateRecords_IsLazy()
    {
        var bytes = SyntheticS57Cell.Create(SyntheticCellProfile.Harbour);

        // Taking the first two records reads only those two.
        var first = Iso8211DocumentReader.EnumerateRecords(bytes).Take(2).ToList();

        Assert.True(first[0].IsDataDescriptiveRecord);
        Assert.Equal("0001", first[1].Fields[0].Tag);
    }

    [Fact]
    public void EnumerateRecords_StopsAtATruncatedRecord()
    {
        var bytes = SyntheticS57Cell.Create(SyntheticCellProfile.Harbour);
        var all = Iso8211DocumentReader.Read(bytes).Records.Count;

        var truncated = Iso8211DocumentReader.EnumerateRecords(bytes.AsMemory(0, bytes.Length - 10)).Count();

        Assert.Equal(all - 1, truncated);
    }

    [Fact]
    public void Read_ByteArray_ReadsFromACopy()
    {
        var bytes = SyntheticS57Cell.Create(SyntheticCellProfile.Harbour);
        var document = Iso8211DocumentReader.Read(bytes);
        var field = document.Records[1].Fields[1];
        var before = field.Data.ToArray();

        // Reusing the caller's buffer must not change the document.
        Array.Clear(bytes);

        Assert.Equal(before, field.Data.ToArray());
    }

    [Fact]
    public void Read_Memory_SlicesTheCallersMemory()
    {
        var bytes = SyntheticS57Cell.Create(SyntheticCellProfile.Harbour);
        var document = Iso8211DocumentReader.Read(bytes.AsMemory());

        Assert.True(MemoryMarshal.TryGetArray(document.Records[1].Fields[1].Data, out var segment));
        Assert.Same(bytes, segment.Array);
    }

    [Fact]
    public void Read_Stream_ReadsTheSameRecords()
    {
        var bytes = SyntheticS57Cell.Create(SyntheticCellProfile.Harbour);

        var document = Iso8211DocumentReader.Read(new MemoryStream(bytes));

        Assert.Equal(Iso8211DocumentReader.Read(bytes).Records.Count, document.Records.Count);
        Assert.True(document.Records[^1].Fields[^1].Data.Span.SequenceEqual(
            Iso8211DocumentReader.Read(bytes).Records[^1].Fields[^1].Data.Span));
    }

    [Fact]
    public void Read_SharesOneStringPerTag()
    {
        var document = Iso8211DocumentReader.Read(SyntheticS57Cell.Create(SyntheticCellProfile.Harbour));

        var vrids = document.DataRecords.SelectMany(r => r.Fields).Where(f => f.Tag == "VRID").Select(f => f.Tag).ToList();

        Assert.True(vrids.Count > 1);
        Assert.All(vrids, tag => Assert.Same(vrids[0], tag));
    }
}
