using System.Collections.Immutable;
using System.Text;
using EncDotNet.S57;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="S57DocumentReader.ReadFromDirectory"/> and
/// <see cref="S57DocumentReader.ReadFromDirectoryAsync"/>.
/// </summary>
public class S57DocumentReaderDirectoryTests : IDisposable
{
    private readonly string _tempDir;

    public S57DocumentReaderDirectoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"EncDotNet_Tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    #region Test Data Helpers

    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    private static byte[] CreateS57FileData(params byte[][] dataRecords)
    {
        var ddr = CreateS57Ddr();
        var totalSize = ddr.Length + dataRecords.Sum(r => r.Length);
        var result = new byte[totalSize];
        var offset = 0;

        Array.Copy(ddr, 0, result, offset, ddr.Length);
        offset += ddr.Length;

        foreach (var record in dataRecords)
        {
            Array.Copy(record, 0, result, offset, record.Length);
            offset += record.Length;
        }

        return result;
    }

    private static byte[] CreateS57Ddr()
    {
        var fields = new List<(string tag, byte[] data)>
        {
            ("0001", CreateDdrFieldData("", "", "()")),
            ("DSID", CreateDdrFieldData("DSID",
                "RCNM!RCID!EXPP!INTU!DSNM!EDTN!UPDN!UADT!ISDT!STED!PRSP!PSDN!PRED!PROF!AGEN!COMT",
                "(b11,b14,b11,b11,A,A,A,A,A,A,b11,A,A,b11,b12,A)")),
            ("DSSI", CreateDdrFieldData("DSSI",
                "DSTR!AALL!NALL!NOMR!NOCR!NOGR!NOLR!NOIN!NOCN!NOED!NOFA",
                "(b11,b11,b11,b14,b14,b14,b14,b14,b14,b14,b14)")),
            ("DSPM", CreateDdrFieldData("DSPM",
                "RCNM!RCID!HDAT!VDAT!SDAT!CSCL!DUNI!HUNI!PUNI!COUN!COMF!SOMF!COMT",
                "(b11,b14,b11,b11,b11,b14,b11,b11,b11,b11,b14,b14,A)")),
            ("FRID", CreateDdrFieldData("FRID",
                "RCNM!RCID!PRIM!GRUP!OBJL!RVER!RUIN",
                "(b11,b14,b11,b11,b12,b12,b11)")),
            ("FOID", CreateDdrFieldData("FOID", "AGEN!FIDN!FIDS", "(b12,b14,b12)")),
            ("ATTF", CreateDdrFieldData("ATTF", "*ATTL!ATVL", "(b12,A)", dataStructure: 1)),
            ("NATF", CreateDdrFieldData("NATF", "*ATTL!ATVL", "(b12,A)", dataStructure: 1)),
            ("FSPT", CreateDdrFieldData("FSPT", "*NAME!ORNT!USAG!MASK", "(b15,b11,b11,b11)", dataStructure: 1)),
            ("FFPT", CreateDdrFieldData("FFPT", "*LNAM!RIND!COMT", "(b18,b11,A)", dataStructure: 1)),
            ("VRID", CreateDdrFieldData("VRID", "RCNM!RCID!RVER!RUIN", "(b11,b14,b12,b11)")),
            ("ATTV", CreateDdrFieldData("ATTV", "*ATTL!ATVL", "(b12,A)", dataStructure: 1)),
            ("VRPT", CreateDdrFieldData("VRPT", "*NAME!ORNT!USAG!TOPI!MASK", "(b15,b11,b11,b11,b11)", dataStructure: 1)),
            ("SG2D", CreateDdrFieldData("SG2D", "*YCOO!XCOO", "(b24,b24)", dataStructure: 1)),
            ("SG3D", CreateDdrFieldData("SG3D", "*YCOO!XCOO!VE3D", "(b24,b24,b24)", dataStructure: 1)),
            ("FSPC", CreateDdrFieldData("FSPC", "FSUI!FSIX!NSPT", "(b11,b12,b12)")),
            ("FFPC", CreateDdrFieldData("FFPC", "FFUI!FFIX!NFPT", "(b11,b12,b12)")),
            ("VRPC", CreateDdrFieldData("VRPC", "VPUI!VPIX!NVPT", "(b11,b12,b12)")),
            ("SGCC", CreateDdrFieldData("SGCC", "CCUI!CCIX!CCNC", "(b11,b12,b12)")),
        };

        return CreateDdrRecord([.. fields]);
    }

    private static byte[] CreateDdrFieldData(string fieldName, string subfieldDescriptors, string formatControls, int dataStructure = 0, int dataType = 6)
    {
        using var ms = new MemoryStream();
        ms.WriteByte((byte)('0' + dataStructure));
        ms.WriteByte((byte)('0' + dataType));
        var descriptors = string.IsNullOrEmpty(fieldName)
            ? subfieldDescriptors
            : (string.IsNullOrEmpty(subfieldDescriptors) ? fieldName : $"{fieldName}!{subfieldDescriptors}");
        if (!string.IsNullOrEmpty(descriptors))
            ms.Write(Encoding.ASCII.GetBytes(descriptors));
        ms.WriteByte(UnitTerminator);
        ms.Write(Encoding.ASCII.GetBytes(formatControls));
        ms.WriteByte(FieldTerminator);
        return ms.ToArray();
    }

    private static byte[] CreateDdrRecord((string tag, byte[] data)[] fields)
    {
        var directoryEntries = new List<byte[]>();
        var currentPosition = 0;
        foreach (var (tag, data) in fields)
        {
            directoryEntries.Add(Encoding.ASCII.GetBytes($"{tag}{data.Length:D3}{currentPosition:D3}"));
            currentPosition += data.Length;
        }
        var directorySize = directoryEntries.Sum(e => e.Length);
        var baseAddress = 24 + directorySize + 1;
        var totalFieldSize = fields.Sum(f => f.data.Length);
        var recordLength = baseAddress + totalFieldSize;
        var leader = Encoding.ASCII.GetBytes($"{recordLength:D5}3LE1 02{baseAddress:D5}   3304");
        var record = new byte[recordLength];
        var offset = 0;
        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;
        foreach (var entry in directoryEntries)
        {
            Array.Copy(entry, 0, record, offset, entry.Length);
            offset += entry.Length;
        }
        record[offset++] = FieldTerminator;
        foreach (var (_, data) in fields)
        {
            Array.Copy(data, 0, record, offset, data.Length);
            offset += data.Length;
        }
        return record;
    }

    private static byte[] CreateFeatureRecord(
        uint rcid,
        ushort objl,
        byte ruin = 1,
        S57AttributeValue[]? attributes = null)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write((byte)100);       // RCNM = Feature
        writer.Write(rcid);            // RCID
        writer.Write((byte)1);         // PRIM = Point
        writer.Write((byte)2);         // GRUP
        writer.Write(objl);            // OBJL
        writer.Write((ushort)1);       // RVER
        writer.Write(ruin);            // RUIN
        writer.Write(FieldTerminator);
        var fridData = ms.ToArray();

        var fields = new List<(string tag, byte[] data)> { ("FRID", fridData) };

        if (attributes is { Length: > 0 })
        {
            using var attfMs = new MemoryStream();
            using var attfWriter = new BinaryWriter(attfMs);
            foreach (var attr in attributes)
            {
                attfWriter.Write((ushort)attr.AttributeCode);
                WriteString(attfWriter, attr.Value);
            }
            attfWriter.Write(FieldTerminator);
            fields.Add(("ATTF", attfMs.ToArray()));
        }

        return CreateDataRecordMultiField([.. fields]);
    }

    private static byte[] CreateVectorRecord(
        byte rcnm,
        uint rcid,
        byte ruin = 1,
        S57Coordinate2D[]? coordinates = null)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(rcnm);
        writer.Write(rcid);
        writer.Write((ushort)1);       // RVER
        writer.Write(ruin);            // RUIN
        writer.Write(FieldTerminator);
        var vridData = ms.ToArray();

        var fields = new List<(string tag, byte[] data)> { ("VRID", vridData) };

        if (coordinates is { Length: > 0 })
        {
            using var sg2dMs = new MemoryStream();
            using var sg2dWriter = new BinaryWriter(sg2dMs);
            foreach (var c in coordinates)
            {
                sg2dWriter.Write(c.Y);
                sg2dWriter.Write(c.X);
            }
            sg2dWriter.Write(FieldTerminator);
            fields.Add(("SG2D", sg2dMs.ToArray()));
        }

        return CreateDataRecordMultiField([.. fields]);
    }

    private static byte[] CreateDataRecordMultiField(params (string tag, byte[] data)[] fields)
    {
        var directoryEntries = new List<byte[]>();
        var currentPosition = 0;
        foreach (var (tag, data) in fields)
        {
            directoryEntries.Add(Encoding.ASCII.GetBytes($"{tag}{data.Length:D3}{currentPosition:D3}"));
            currentPosition += data.Length;
        }
        var directorySize = directoryEntries.Sum(e => e.Length);
        var baseAddress = 24 + directorySize + 1;
        var totalFieldSize = fields.Sum(f => f.data.Length);
        var recordLength = baseAddress + totalFieldSize;
        var leader = Encoding.ASCII.GetBytes($"{recordLength:D5}3DE1 00{baseAddress:D5}   3304");
        var record = new byte[recordLength];
        var offset = 0;
        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;
        foreach (var entry in directoryEntries)
        {
            Array.Copy(entry, 0, record, offset, entry.Length);
            offset += entry.Length;
        }
        record[offset++] = FieldTerminator;
        foreach (var (_, data) in fields)
        {
            Array.Copy(data, 0, record, offset, data.Length);
            offset += data.Length;
        }
        return record;
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        writer.Write(Encoding.ASCII.GetBytes(value));
        writer.Write(UnitTerminator);
    }

    private void WriteChartFile(string stem, string extension, byte[] data)
    {
        File.WriteAllBytes(Path.Combine(_tempDir, $"{stem}{extension}"), data);
    }

    #endregion

    #region ReadFromDirectory Tests

    [Fact]
    public void ReadFromDirectory_BaseOnly_ReturnsBaseDocument()
    {
        // Arrange — base file with two features
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE,
                attributes: [new S57AttributeValue(100, "10.0")]),
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.LIGHTS));
        WriteChartFile("US5WA51M", ".000", baseData);

        // Act
        var doc = S57DocumentReader.ReadFromDirectory(_tempDir);

        // Assert
        Assert.Equal(2, doc.FeatureRecords.Length);
        Assert.Equal(1, doc.FeatureRecords[0].RecordName.RecordId);
        Assert.Equal(2, doc.FeatureRecords[1].RecordName.RecordId);
    }

    [Fact]
    public void ReadFromDirectory_BaseAndOneUpdate_AppliesUpdate()
    {
        // Arrange — base with feature RCID=1, update inserts RCID=2
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.LIGHTS,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var doc = S57DocumentReader.ReadFromDirectory(_tempDir);

        // Assert
        Assert.Equal(2, doc.FeatureRecords.Length);
        Assert.Equal(S57ObjectCode.DEPARE, doc.FeatureRecords[0].ObjectCode);
        Assert.Equal(S57ObjectCode.LIGHTS, doc.FeatureRecords[1].ObjectCode);
    }

    [Fact]
    public void ReadFromDirectory_MultipleUpdatesAppliedInOrder()
    {
        // Arrange — base with RCID=1, update 1 inserts RCID=2, update 2 deletes RCID=1
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE));
        WriteChartFile("US5WA51M", ".000", baseData);

        var update1 = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.BOYLAT,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", update1);

        var update2 = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE,
                ruin: (byte)S57UpdateInstruction.Delete));
        WriteChartFile("US5WA51M", ".002", update2);

        // Act
        var doc = S57DocumentReader.ReadFromDirectory(_tempDir);

        // Assert — only RCID=2 should remain
        Assert.Single(doc.FeatureRecords);
        Assert.Equal(2, doc.FeatureRecords[0].RecordName.RecordId);
        Assert.Equal(S57ObjectCode.BOYLAT, doc.FeatureRecords[0].ObjectCode);
    }

    [Fact]
    public void ReadFromDirectory_UpdateModifiesAttributes()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE,
                attributes: [new S57AttributeValue(100, "10.0"), new S57AttributeValue(200, "20.0")]));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE,
                ruin: (byte)S57UpdateInstruction.Modify,
                attributes: [new S57AttributeValue(200, "25.0")]));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var doc = S57DocumentReader.ReadFromDirectory(_tempDir);

        // Assert
        Assert.Single(doc.FeatureRecords);
        var attrs = doc.FeatureRecords[0].Attributes;
        Assert.Equal(2, attrs.Length);
        Assert.Equal("10.0", attrs[0].Value);
        Assert.Equal("25.0", attrs[1].Value);
    }

    [Fact]
    public void ReadFromDirectory_IgnoresUnrelatedFiles()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE));
        WriteChartFile("US5WA51M", ".000", baseData);

        // Write an unrelated .txt file and a different-stem .001
        File.WriteAllText(Path.Combine(_tempDir, "README.txt"), "not a chart");
        File.WriteAllText(Path.Combine(_tempDir, "OTHER.001"), "not a chart");

        // Act
        var doc = S57DocumentReader.ReadFromDirectory(_tempDir);

        // Assert — only the base record, no crash from unrelated files
        Assert.Single(doc.FeatureRecords);
    }

    [Fact]
    public void ReadFromDirectory_UpdatesAppliedInNumericOrder()
    {
        // Arrange — write updates out of filesystem order to verify numeric sorting
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE));
        WriteChartFile("US5WA51M", ".000", baseData);

        // Update 10 deletes RCID=1
        var update10 = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, ruin: (byte)S57UpdateInstruction.Delete,
                objl: (ushort)S57ObjectCode.DEPARE));
        WriteChartFile("US5WA51M", ".010", update10);

        // Update 2 inserts RCID=2
        var update2 = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.LIGHTS,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".002", update2);

        // Act
        var doc = S57DocumentReader.ReadFromDirectory(_tempDir);

        // Assert — update 2 applied first (insert RCID=2), then update 10 (delete RCID=1)
        Assert.Single(doc.FeatureRecords);
        Assert.Equal(2, doc.FeatureRecords[0].RecordName.RecordId);
    }

    [Fact]
    public void ReadFromDirectory_NoBaseFile_ThrowsFileNotFoundException()
    {
        // Arrange — empty directory (no .000 file)

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => S57DocumentReader.ReadFromDirectory(_tempDir));
    }

    [Fact]
    public void ReadFromDirectory_BaseWithVectors_ReturnsVectors()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateVectorRecord(rcnm: S57RecordNameCodes.Edge, rcid: 10,
                coordinates: [new S57Coordinate2D { X = 100, Y = 200 }]));
        WriteChartFile("US5WA51M", ".000", baseData);

        // Act
        var doc = S57DocumentReader.ReadFromDirectory(_tempDir);

        // Assert
        Assert.Single(doc.VectorRecords);
        Assert.Equal(10, doc.VectorRecords[0].RecordName.RecordId);
        Assert.Single(doc.VectorRecords[0].Coordinates2D);
    }

    #endregion

    #region ReadFromDirectoryAsync Tests

    [Fact]
    public async Task ReadFromDirectoryAsync_BaseAndUpdate_AppliesUpdate()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.SOUNDG,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var doc = await S57DocumentReader.ReadFromDirectoryAsync(_tempDir);

        // Assert
        Assert.Equal(2, doc.FeatureRecords.Length);
        Assert.Equal(S57ObjectCode.DEPARE, doc.FeatureRecords[0].ObjectCode);
        Assert.Equal(S57ObjectCode.SOUNDG, doc.FeatureRecords[1].ObjectCode);
    }

    [Fact]
    public async Task ReadFromDirectoryAsync_NoBaseFile_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => S57DocumentReader.ReadFromDirectoryAsync(_tempDir));
    }

    [Fact]
    public async Task ReadFromDirectoryAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.LIGHTS,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", updateData);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => S57DocumentReader.ReadFromDirectoryAsync(_tempDir, cancellationToken: cts.Token));
    }

    #endregion
}
