using System.Text;
using EncDotNet.Iso8211;

namespace EncDotNet.UnitTests;

/// <summary>
/// Tests for writing fields that end with a two-byte terminator, and for writing a data
/// record's field control length blank.
/// </summary>
public class Iso8211WriterLeaderAndTerminatorTests
{
    private static readonly Iso8211WriterOptions TwoByteNatf = new()
    {
        AutoSizeDirectoryEntries = true,
        TwoByteTerminatorFieldTags = new HashSet<string> { "NATF" }
    };

    [Fact]
    public void TwoByteTerminatorField_ReadsAndWritesBackUnchanged()
    {
        // A UCS-2 field as S-57 encodes NATF at lexical level 2: ATTL, "A", then the unit and
        // field terminators, each two bytes. The data holds the field terminator's first byte.
        byte[] natf = [0x2D, 0x01, 0x41, 0x00, 0x1F, 0x00, 0x1E];
        var record = new Iso8211RecordBuilder(TwoByteNatf)
            .AddField("0001", [0x01, 0x00])
            .AddField("NATF", natf)
            .Build();

        var bytes = Iso8211DocumentWriter.Write(new Iso8211Document { Records = [record] }, TwoByteNatf);

        Assert.Equal(new byte[] { 0x41, 0x00, 0x1F, 0x00, 0x1E, 0x00 }, bytes[^6..]);
        Assert.Equal(0x1E, bytes[^9]); // The 0001 field keeps its one-byte terminator.

        var reread = Iso8211DocumentReader.Read(bytes);
        Assert.Equal(natf, reread.Records[0].Fields[1].Data.ToArray());
        Assert.Equal(bytes, Iso8211DocumentWriter.Write(reread, TwoByteNatf));
    }

    [Fact]
    public void TwoByteTerminatorFieldWithoutItsFirstByte_Throws()
    {
        var record = new Iso8211RecordBuilder(TwoByteNatf).AddField("NATF", [0x41, 0x00, 0x1F, 0x00]).Build();

        Assert.Throws<InvalidOperationException>(() => Iso8211DocumentWriter.Write(new Iso8211Document { Records = [record] }, TwoByteNatf));
    }

    [Theory]
    [InlineData(false, "00")]
    [InlineData(true, "  ")]
    public void DataRecordFieldControlLength_IsBlankWhenAsked(bool blank, string expected)
    {
        var options = new Iso8211WriterOptions { AutoSizeDirectoryEntries = true, BlankDataRecordFieldControlLength = blank };
        var ddr = new Iso8211RecordBuilder(options) { LeaderIdentifier = 'L', FieldControlLength = 9 }
            .AddField("0000", "0000;&   "u8.ToArray())
            .Build();
        var data = new Iso8211RecordBuilder(options).AddField("0001", [0x01, 0x00]).Build();

        var bytes = Iso8211DocumentWriter.Write(new Iso8211Document { Records = [ddr, data] }, options);

        Assert.Equal("09", Encoding.ASCII.GetString(bytes, 10, 2));
        Assert.Equal(expected, Encoding.ASCII.GetString(bytes, ddr.Leader.RecordLength + 10, 2));
        Assert.Equal(0, Iso8211DocumentReader.Read(bytes).Records[1].Leader.FieldControlLength);
    }
}
