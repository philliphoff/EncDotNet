using System.Text;
using EncDotNet.Iso8211;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="Iso8211DocumentWriter"/>, <see cref="Iso8211RecordBuilder"/>,
/// and <see cref="Iso8211DocumentBuilder"/>, focusing on round-trip fidelity with
/// <see cref="Iso8211DocumentReader"/>.
/// </summary>
public class Iso8211DocumentWriterTests
{
    private const byte UT = 0x1F;
    private const byte FT = 0x1E;

    #region Byte-identical round-trip (Read -> Write)

    [Fact]
    public void Write_SingleFieldRecord_IsByteIdentical()
    {
        var original = BuildRecordBytes('D', new[] { ("0001", "TEST"u8.ToArray()) });

        var document = Iso8211DocumentReader.Read(original);
        var written = Iso8211DocumentWriter.Write(document);

        Assert.Equal(original, written);
    }

    [Fact]
    public void Write_MultiFieldRecord_IsByteIdentical()
    {
        var original = BuildRecordBytes('D', new[]
        {
            ("0001", "HELLO"u8.ToArray()),
            ("0002", "WORLD"u8.ToArray())
        });

        var document = Iso8211DocumentReader.Read(original);
        var written = Iso8211DocumentWriter.Write(document);

        Assert.Equal(original, written);
    }

    [Fact]
    public void Write_SubfieldRecordWithUnitTerminators_IsByteIdentical()
    {
        var fieldData = Concat("SUB1"u8.ToArray(), new[] { UT }, "SUB2"u8.ToArray(), new[] { UT }, "SUB3"u8.ToArray());
        var original = BuildRecordBytes('D', new[] { ("0001", fieldData) });

        var document = Iso8211DocumentReader.Read(original);
        var written = Iso8211DocumentWriter.Write(document);

        Assert.Equal(original, written);
    }

    [Fact]
    public void Write_BinaryFieldDataContainingTerminatorBytes_IsByteIdentical()
    {
        // Field data that includes 0x1E / 0x1F as ordinary binary payload bytes.
        var fieldData = new byte[] { 0x01, 0x1E, 0x02, 0x1F, 0x03, 0xFF, 0x00 };
        var original = BuildRecordBytes('D', new[] { ("VRID", fieldData) });

        var document = Iso8211DocumentReader.Read(original);
        var written = Iso8211DocumentWriter.Write(document);

        Assert.Equal(original, written);
    }

    [Fact]
    public void Write_EmptyField_IsByteIdentical()
    {
        var original = BuildRecordBytes('D', new[] { ("0001", Array.Empty<byte>()) });

        var document = Iso8211DocumentReader.Read(original);
        var written = Iso8211DocumentWriter.Write(document);

        Assert.Equal(original, written);
    }

    [Fact]
    public void Write_RepeatedFieldTags_IsByteIdentical()
    {
        var original = BuildRecordBytes('D', new[]
        {
            ("ATTF", "A"u8.ToArray()),
            ("ATTF", "BB"u8.ToArray()),
            ("ATTF", "CCC"u8.ToArray())
        });

        var document = Iso8211DocumentReader.Read(original);
        var written = Iso8211DocumentWriter.Write(document);

        Assert.Equal(original, written);
    }

    [Fact]
    public void Write_MultipleRecords_IsByteIdentical()
    {
        var ddr = BuildRecordBytes('L', new[] { ("0001", "DDRDATA"u8.ToArray()) });
        var dr = BuildRecordBytes('D', new[] { ("FRID", "FEATURE"u8.ToArray()) });
        var original = Concat(ddr, dr);

        var document = Iso8211DocumentReader.Read(original);
        var written = Iso8211DocumentWriter.Write(document);

        Assert.Equal(original, written);
        Assert.Equal(2, document.Records.Count);
    }

    #endregion

    #region Round-trip via builders (Write -> Read)

    [Fact]
    public void Build_And_Write_DataRecord_RoundTripsThroughReader()
    {
        var record = new Iso8211RecordBuilder()
            .AddField("0001", "HELLO"u8.ToArray())
            .AddField("0002", "WORLD"u8.ToArray())
            .Build();

        var document = new Iso8211DocumentBuilder().AddRecord(record).Build();
        var bytes = Iso8211DocumentWriter.Write(document);

        var reparsed = Iso8211DocumentReader.Read(bytes);

        var reparsedRecord = Assert.Single(reparsed.Records);
        Assert.Equal('D', reparsedRecord.Leader.LeaderIdentifier);
        Assert.Equal(2, reparsedRecord.Fields.Count);
        Assert.Equal("0001", reparsedRecord.Fields[0].Tag);
        Assert.Equal("HELLO", reparsedRecord.Fields[0].GetDataString());
        Assert.Equal("0002", reparsedRecord.Fields[1].Tag);
        Assert.Equal("WORLD", reparsedRecord.Fields[1].GetDataString());
    }

    [Fact]
    public void RecordBuilder_ComputesLeaderAndDirectory()
    {
        var record = new Iso8211RecordBuilder()
            .AddField("0001", "HELLO"u8.ToArray())
            .AddField("0002", "WORLD!"u8.ToArray())
            .Build();

        Assert.Equal(2, record.Directory.Count);
        Assert.Equal("0001", record.Directory[0].Tag);
        Assert.Equal(6, record.Directory[0].Length); // "HELLO" + FT
        Assert.Equal(0, record.Directory[0].Position);
        Assert.Equal(7, record.Directory[1].Length); // "WORLD!" + FT
        Assert.Equal(6, record.Directory[1].Position);

        // Length/position fields auto-size to a single digit here; tag size defaults to 4.
        // Base address = 24 + directory(2 * (4+1+1)) + 1 FT.
        Assert.Equal(24 + (2 * 6) + 1, record.Leader.BaseAddressOfFieldArea);
        Assert.Equal(record.Leader.BaseAddressOfFieldArea + 6 + 7, record.Leader.RecordLength);
    }

    [Fact]
    public void RecordBuilder_AutoSizesDirectoryEntries_WhenRequested()
    {
        var options = new Iso8211WriterOptions { AutoSizeDirectoryEntries = true };
        var record = new Iso8211RecordBuilder(options)
            .AddField("0001", "AB"u8.ToArray())
            .Build();

        // "AB" + FT = length 3 (1 digit), position 0 (1 digit), tag "0001" (4).
        Assert.Equal(4, record.Leader.SizeOfFieldTagField);
        Assert.Equal(1, record.Leader.SizeOfFieldLengthField);
        Assert.Equal(1, record.Leader.SizeOfFieldPositionField);

        // Ensure it still round-trips.
        var document = new Iso8211DocumentBuilder().AddRecord(record).Build();
        var bytes = Iso8211DocumentWriter.Write(document, options);
        var reparsed = Iso8211DocumentReader.Read(bytes);
        Assert.Equal("AB", reparsed.Records[0].Fields[0].GetDataString());
    }

    #endregion

    #region Stream / file overloads

    [Fact]
    public async Task WriteAsync_And_WriteToFile_ProduceSameBytes()
    {
        var original = BuildRecordBytes('D', new[] { ("0001", "TEST"u8.ToArray()) });
        var document = Iso8211DocumentReader.Read(original);
        var expected = Iso8211DocumentWriter.Write(document);

        using var ms = new MemoryStream();
        await Iso8211DocumentWriter.WriteAsync(ms, document);
        Assert.Equal(expected, ms.ToArray());

        var path = Path.GetTempFileName();
        try
        {
            await Iso8211DocumentWriter.WriteToFileAsync(path, document);
            Assert.Equal(expected, await File.ReadAllBytesAsync(path));

            Iso8211DocumentWriter.WriteToFile(path, document);
            Assert.Equal(expected, File.ReadAllBytes(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Write_ThenReadFromFile_RoundTrips()
    {
        var original = BuildRecordBytes('D', new[] { ("0001", "ROUNDTRIP"u8.ToArray()) });
        var document = Iso8211DocumentReader.Read(original);

        var path = Path.GetTempFileName();
        try
        {
            Iso8211DocumentWriter.WriteToFile(path, document);
            var reparsed = Iso8211DocumentReader.ReadFromFile(path);
            Assert.Equal("ROUNDTRIP", reparsed.Records[0].Fields[0].GetDataString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    #endregion

    #region Optional real-file corpus (self-skipping)

    [Fact]
    public void Corpus_ReadWriteRead_IsEquivalent()
    {
        // Populated only when a fixtures directory is provided via environment variable,
        // so CI remains green without shipping large binary charts.
        var dir = Environment.GetEnvironmentVariable("ISO8211_CORPUS_DIR");
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
        {
            return;
        }

        var files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var ext = Path.GetExtension(f);
                return ext.Equals(".000", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".031", StringComparison.OrdinalIgnoreCase);
            });

        foreach (var path in files)
        {
            var original = File.ReadAllBytes(path);
            var document = Iso8211DocumentReader.Read(original);
            var written = Iso8211DocumentWriter.Write(document);

            // Read -> Write -> Read must yield an equivalent object model; canonical sources
            // additionally round-trip byte-for-byte.
            var reparsed = Iso8211DocumentReader.Read(written);
            Assert.Equal(document.Records.Count, reparsed.Records.Count);
            for (int i = 0; i < document.Records.Count; i++)
            {
                var a = document.Records[i];
                var b = reparsed.Records[i];
                Assert.Equal(a.Fields.Count, b.Fields.Count);
                for (int j = 0; j < a.Fields.Count; j++)
                {
                    Assert.Equal(a.Fields[j].Tag, b.Fields[j].Tag);
                    Assert.Equal(a.Fields[j].Data, b.Fields[j].Data);
                }
            }
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Builds a canonical ISO 8211 record (24-byte leader, directory, field area) using the
    /// same layout the reader expects: tag size 4, length size 3, position size 3.
    /// </summary>
    private static byte[] BuildRecordBytes(char leaderIdentifier, IReadOnlyList<(string Tag, byte[] Data)> fields)
    {
        var directory = new List<byte>();
        var fieldArea = new List<byte>();
        var position = 0;

        foreach (var (tag, data) in fields)
        {
            var length = data.Length + 1; // +1 for FT
            directory.AddRange(Encoding.ASCII.GetBytes($"{tag}{length:D3}{position:D3}"));
            fieldArea.AddRange(data);
            fieldArea.Add(FT);
            position += length;
        }
        directory.Add(FT); // directory terminator

        var baseAddress = 24 + directory.Count;
        var recordLength = baseAddress + fieldArea.Count;

        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3{leaderIdentifier}E1 00{baseAddress:D5}   3304");

        return Concat(leader, directory.ToArray(), fieldArea.ToArray());
    }

    private static byte[] Concat(params byte[][] arrays)
    {
        var result = new List<byte>();
        foreach (var array in arrays)
        {
            result.AddRange(array);
        }
        return result.ToArray();
    }

    #endregion
}
