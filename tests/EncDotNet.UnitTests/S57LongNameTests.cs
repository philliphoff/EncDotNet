using EncDotNet.S57;

namespace EncDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="S57LongName"/> and its conversions to and from <c>LNAM</c> and
/// <see cref="S57RecordName"/>.
/// </summary>
public class S57LongNameTests
{
    public static TheoryData<ushort, uint, ushort> LongNames => new()
    {
        { 540, 12345, 1 },
        { 0, 0, 0 },
        { ushort.MaxValue, uint.MaxValue, ushort.MaxValue },
        { 1, (uint)int.MaxValue, 1 },
        { 1, (uint)int.MaxValue + 1, 1 },
        { ushort.MaxValue, 1, 0 },
        { 0, 1, ushort.MaxValue },
    };

    [Fact]
    public void ToLnam_WritesAgenFidnFidsLittleEndian()
    {
        var name = new S57LongName(0x0102, 0x03040506, 0x0708);

        Assert.Equal([0x02, 0x01, 0x06, 0x05, 0x04, 0x03, 0x08, 0x07], name.ToLnam());
    }

    [Fact]
    public void FromLnam_ReadsAgenFidnFidsLittleEndian()
    {
        // AGEN 550 (0x0226), FIDN 4294967294 (0xFFFFFFFE), FIDS 2 (0x0002).
        byte[] lnam = [0x26, 0x02, 0xFE, 0xFF, 0xFF, 0xFF, 0x02, 0x00];

        var name = S57LongName.FromLnam(lnam);

        Assert.Equal(new S57LongName(550, 4294967294, 2), name);
    }

    [Theory]
    [MemberData(nameof(LongNames))]
    public void Lnam_RoundTrips(ushort agencyCode, uint featureId, ushort featureSubdivision)
    {
        var name = new S57LongName(agencyCode, featureId, featureSubdivision);

        var lnam = name.ToLnam();

        Assert.Equal(S57LongName.LnamLength, lnam.Length);
        Assert.Equal(name, S57LongName.FromLnam(lnam));
    }

    [Fact]
    public void TryWriteLnam_WritesFirstEightBytes()
    {
        var name = new S57LongName(ushort.MaxValue, (uint)int.MaxValue + 1, 1);
        var buffer = new byte[10];
        Array.Fill(buffer, (byte)0xAA);

        Assert.True(name.TryWriteLnam(buffer));

        Assert.Equal([0xFF, 0xFF, 0x00, 0x00, 0x00, 0x80, 0x01, 0x00, 0xAA, 0xAA], buffer);
    }

    [Fact]
    public void TryWriteLnam_ShortDestination_ReturnsFalseAndWritesNothing()
    {
        var buffer = new byte[S57LongName.LnamLength - 1];

        Assert.False(new S57LongName(1, 2, 3).TryWriteLnam(buffer));
        Assert.All(buffer, b => Assert.Equal(0, b));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(9)]
    public void FromLnam_WrongLength_Throws(int length)
    {
        Assert.Throws<ArgumentException>(() => S57LongName.FromLnam(new byte[length]));
    }

    [Theory]
    [MemberData(nameof(LongNames))]
    public void RecordName_FromLongName_RoundTrips(ushort agencyCode, uint featureId, ushort featureSubdivision)
    {
        var name = new S57LongName(agencyCode, featureId, featureSubdivision);

        var recordName = S57RecordName.FromLongName(name);

        Assert.Equal(agencyCode, recordName.AgencyCode);
        Assert.Equal(unchecked((int)featureId), recordName.FeatureId);
        Assert.Equal(featureSubdivision, recordName.FeatureSubdivision);
        Assert.Equal(0, recordName.RecordNameCode);
        Assert.Equal(0, recordName.RecordId);

        // An all-zero long name reads back as no long name.
        Assert.Equal(name == default ? null : name, recordName.LongName);
    }

    [Fact]
    public void RecordName_FromLongName_FidnAboveIntMaxValue_KeepsBits()
    {
        var recordName = S57RecordName.FromLongName(new S57LongName(1, uint.MaxValue, 1));

        Assert.Equal(-1, recordName.FeatureId);
        Assert.Equal(uint.MaxValue, recordName.LongName?.FeatureId);
    }

    [Fact]
    public void FeaturePointer_LongNameAtLimits_RoundTripsThroughWriter()
    {
        var cell = S57WriterTestCells.SmallCell();
        var index = cell.FeatureRecords.ToList().FindIndex(f => f.FeaturePointers.Count > 0);
        var feature = cell.FeatureRecords[index];
        var target = new S57LongName(ushort.MaxValue, uint.MaxValue, ushort.MaxValue);
        feature = feature with
        {
            FeaturePointers = [feature.FeaturePointers[0] with { Name = target }, .. feature.FeaturePointers.Skip(1)]
        };
        cell = cell with
        {
            FeatureRecords = [.. cell.FeatureRecords.Take(index), feature, .. cell.FeatureRecords.Skip(index + 1)]
        };

        var written = S57DocumentWriter.Write(cell);
        var reread = S57DocumentReader.Read(written);

        Assert.True(written.AsSpan().IndexOf(target.ToLnam()) >= 0);
        Assert.Equal(target, reread.FeatureRecords[index].FeaturePointers[0].Name);
    }
}
