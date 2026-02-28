using System.Collections.Immutable;
using System.Text;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using EncDotNet.S57.ExchangeSets;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="S57ExchangeSet.ReadChart"/> and
/// <see cref="S57ExchangeSet.ReadChartAsync"/>.
/// </summary>
public class S57ExchangeSetChartTests : IDisposable
{
    private readonly string _tempDir;

    public S57ExchangeSetChartTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"EncDotNet_ChartDirTests_{Guid.NewGuid():N}");
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
        byte primitive = 1,
        byte ruin = 1,
        S57AttributeValue[]? attributes = null,
        (byte rcnm, uint rcid, byte ornt, byte usag, byte mask)[]? spatialPointers = null)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write((byte)100);       // RCNM = Feature
        writer.Write(rcid);            // RCID
        writer.Write(primitive);       // PRIM
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

        if (spatialPointers is { Length: > 0 })
        {
            using var fsptMs = new MemoryStream();
            using var fsptWriter = new BinaryWriter(fsptMs);
            foreach (var (rcnm, spRcid, ornt, usag, mask) in spatialPointers)
            {
                // NAME is 5 bytes: 1 byte RCNM + 4 bytes RCID
                fsptWriter.Write(rcnm);
                fsptWriter.Write(spRcid);
                fsptWriter.Write(ornt);
                fsptWriter.Write(usag);
                fsptWriter.Write(mask);
            }
            fsptWriter.Write(FieldTerminator);
            fields.Add(("FSPT", fsptMs.ToArray()));
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

    private S57ExchangeSet CreateExchangeSet(string stem = "US5WA51M")
    {
        var updateFileNames = Directory.EnumerateFiles(_tempDir, $"{stem}.*")
            .Where(f =>
            {
                var ext = Path.GetExtension(f);
                return ext.Length == 4
                    && int.TryParse(ext.AsSpan(1), out var n)
                    && n > 0;
            })
            .OrderBy(f => int.Parse(Path.GetExtension(f).AsSpan(1)))
            .Select(Path.GetFileName)
            .ToImmutableArray();

        return new S57ExchangeSet
        {
            CatalogFileName = "CATALOG.031",
            BaseCellFileName = $"{stem}.000",
            UpdateFileNames = updateFileNames!,
        };
    }

    #endregion

    #region ReadChart Tests

    [Fact]
    public void ReadChart_BaseOnly_ReturnsChartWithTypedFeatures()
    {
        // Arrange — base file with a point feature and an area feature
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1),
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.DEPARE, primitive: 3));
        WriteChartFile("US5WA51M", ".000", baseData);

        // Act
        var chart = CreateExchangeSet().ReadChart(_tempDir);

        // Assert
        Assert.Single(chart.PointFeatures);
        Assert.Single(chart.AreaFeatures);
        Assert.Equal(S57ObjectCode.LIGHTS, chart.PointFeatures[0].ObjectCode);
        Assert.Equal(S57ObjectCode.DEPARE, chart.AreaFeatures[0].ObjectCode);
    }

    [Fact]
    public void ReadChart_BaseAndUpdate_AppliesUpdateToChart()
    {
        // Arrange — base with RCID=1 point, update inserts RCID=2 line
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.COALNE, primitive: 2,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var chart = CreateExchangeSet().ReadChart(_tempDir);

        // Assert
        Assert.Single(chart.PointFeatures);
        Assert.Single(chart.LineFeatures);
        Assert.Equal(S57ObjectCode.LIGHTS, chart.PointFeatures[0].ObjectCode);
        Assert.Equal(S57ObjectCode.COALNE, chart.LineFeatures[0].ObjectCode);
    }

    [Fact]
    public void ReadChart_UpdateDeletesFeature_FeatureRemovedFromChart()
    {
        // Arrange — base with two features, update deletes one
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1),
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.DEPARE, primitive: 3));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1,
                ruin: (byte)S57UpdateInstruction.Delete));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var chart = CreateExchangeSet().ReadChart(_tempDir);

        // Assert
        Assert.Empty(chart.PointFeatures);
        Assert.Single(chart.AreaFeatures);
        Assert.Single(chart.AllFeatures);
    }

    [Fact]
    public void ReadChart_UpdateModifiesAttributes_ChartReflectsChange()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1,
                attributes: [new S57AttributeValue(116, "RED")]));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1,
                ruin: (byte)S57UpdateInstruction.Modify,
                attributes: [new S57AttributeValue(116, "GREEN")]));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var chart = CreateExchangeSet().ReadChart(_tempDir);

        // Assert
        Assert.Single(chart.PointFeatures);
        Assert.Equal("GREEN", chart.PointFeatures[0].GetAttributeValue(116));
    }

    [Fact]
    public void ReadChart_WithVectors_CategorizesSpatialRecords()
    {
        // Arrange — base with an isolated node and an edge
        var baseData = CreateS57FileData(
            CreateVectorRecord(rcnm: S57RecordNameCodes.IsolatedNode, rcid: 1,
                coordinates: [new S57Coordinate2D { X = -1225000000, Y = 475000000 }]),
            CreateVectorRecord(rcnm: S57RecordNameCodes.Edge, rcid: 2,
                coordinates: [new S57Coordinate2D { X = 100, Y = 200 }]));
        WriteChartFile("US5WA51M", ".000", baseData);

        // Act
        var chart = CreateExchangeSet().ReadChart(_tempDir);

        // Assert
        Assert.Single(chart.IsolatedNodes);
        Assert.Single(chart.Edges);
        Assert.Empty(chart.ConnectedNodes);
        Assert.Empty(chart.Faces);

        var node = chart.IsolatedNodes.Values.First();
        Assert.True(node.HasPosition);
        Assert.Equal(-1225000000, node.Position!.Value.X);
    }

    [Fact]
    public void ReadChart_MultipleUpdates_AllAppliedInOrder()
    {
        // Arrange — base with RCID=1, update 1 inserts RCID=2, update 2 inserts RCID=3
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE, primitive: 3));
        WriteChartFile("US5WA51M", ".000", baseData);

        var update1 = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", update1);

        var update2 = CreateS57FileData(
            CreateFeatureRecord(rcid: 3, objl: (ushort)S57ObjectCode.COALNE, primitive: 2,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".002", update2);

        // Act
        var chart = CreateExchangeSet().ReadChart(_tempDir);

        // Assert — chart has all three feature types
        Assert.Single(chart.AreaFeatures);
        Assert.Single(chart.PointFeatures);
        Assert.Single(chart.LineFeatures);
        Assert.Equal(3, chart.AllFeatures.Count);
    }

    [Fact]
    public void ReadChart_NoBaseFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => CreateExchangeSet().ReadChart(_tempDir));
    }

    [Fact]
    public void ReadChart_FeatureQueryMethods_WorkAfterUpdates()
    {
        // Arrange — base with LIGHTS, update adds a second LIGHTS
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.LIGHTS, primitive: 1,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var chart = CreateExchangeSet().ReadChart(_tempDir);

        // Assert — query methods work on updated chart
        var lights = chart.GetPointFeaturesByObjectCode(S57ObjectCode.LIGHTS).ToList();
        Assert.Collection(lights,
            l => Assert.Equal(S57ObjectCode.LIGHTS, l.ObjectCode),
            l => Assert.Equal(S57ObjectCode.LIGHTS, l.ObjectCode));

        var byName = chart.GetFeature(S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, 2));
        Assert.NotNull(byName);
        Assert.Equal(S57ObjectCode.LIGHTS, byName.ObjectCode);
    }

    #endregion

    #region ReadChartAsync Tests

    [Fact]
    public async Task ReadChartAsync_BaseAndUpdate_AppliesUpdateToChart()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE, primitive: 3));
        WriteChartFile("US5WA51M", ".000", baseData);

        var updateData = CreateS57FileData(
            CreateFeatureRecord(rcid: 2, objl: (ushort)S57ObjectCode.SOUNDG, primitive: 1,
                ruin: (byte)S57UpdateInstruction.Insert));
        WriteChartFile("US5WA51M", ".001", updateData);

        // Act
        var chart = await CreateExchangeSet().ReadChartAsync(_tempDir);

        // Assert
        Assert.Single(chart.AreaFeatures);
        Assert.Single(chart.PointFeatures);
        Assert.Equal(S57ObjectCode.DEPARE, chart.AreaFeatures[0].ObjectCode);
        Assert.Equal(S57ObjectCode.SOUNDG, chart.PointFeatures[0].ObjectCode);
    }

    [Fact]
    public async Task ReadChartAsync_NoBaseFile_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => CreateExchangeSet().ReadChartAsync(_tempDir));
    }

    [Fact]
    public async Task ReadChartAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        // Arrange
        var baseData = CreateS57FileData(
            CreateFeatureRecord(rcid: 1, objl: (ushort)S57ObjectCode.DEPARE, primitive: 3));
        WriteChartFile("US5WA51M", ".000", baseData);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateExchangeSet().ReadChartAsync(_tempDir, cancellationToken: cts.Token));
    }

    #endregion
}
