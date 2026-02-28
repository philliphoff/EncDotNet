using System.Text;
using EncDotNet.S57.Catalogs;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for the <see cref="S57CatalogReader"/> and related catalog types.
/// </summary>
public class S57CatalogReaderTests
{
    #region Test Data Helpers

    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    /// <summary>
    /// Creates a minimal S-57 catalog ISO 8211 document with a DDR and optional CATD data records.
    /// </summary>
    private static byte[] CreateCatalogDocument(params byte[][] dataRecords)
    {
        var ddr = CreateCatalogDdr();

        var totalSize = ddr.Length;
        foreach (var record in dataRecords)
        {
            totalSize += record.Length;
        }

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

    /// <summary>
    /// Creates a DDR (Data Descriptive Record) with CATD field definition.
    /// </summary>
    private static byte[] CreateCatalogDdr()
    {
        var fields = new List<(string tag, byte[] data)>
        {
            ("0001", CreateDdrFieldData("", "", "()")),
            ("CATD", CreateDdrFieldData(
                "CATD",
                "RCNM!RCID!FILE!LFIL!VOLM!IMPL!SLAT!WLON!NLAT!ELON!CRCS!COMT",
                "(A,I,A,A,A,A,A,A,A,A,A,A)"))
        };

        return CreateDdrRecord(fields.ToArray());
    }

    /// <summary>
    /// Creates DDR field data with field controls, descriptors, and format controls.
    /// </summary>
    private static byte[] CreateDdrFieldData(string fieldName, string subfieldDescriptors, string formatControls)
    {
        using var ms = new MemoryStream();

        // Field controls: data structure code (Elementary=0) + data type code (implicit point=6)
        ms.WriteByte((byte)'0');
        ms.WriteByte((byte)'6');

        var descriptors = string.IsNullOrEmpty(fieldName)
            ? subfieldDescriptors
            : (string.IsNullOrEmpty(subfieldDescriptors) ? fieldName : $"{fieldName}!{subfieldDescriptors}");

        if (!string.IsNullOrEmpty(descriptors))
        {
            ms.Write(Encoding.ASCII.GetBytes(descriptors));
        }

        ms.WriteByte(UnitTerminator);
        ms.Write(Encoding.ASCII.GetBytes(formatControls));
        ms.WriteByte(FieldTerminator);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates a DDR record from field definitions.
    /// </summary>
    private static byte[] CreateDdrRecord((string tag, byte[] data)[] fields)
    {
        var directoryEntries = new List<byte[]>();
        var currentPosition = 0;

        foreach (var (tag, data) in fields)
        {
            var entry = Encoding.ASCII.GetBytes($"{tag}{data.Length:D3}{currentPosition:D3}");
            directoryEntries.Add(entry);
            currentPosition += data.Length;
        }

        var directorySize = directoryEntries.Sum(e => e.Length);
        var baseAddress = 24 + directorySize + 1;
        var totalFieldSize = fields.Sum(f => f.data.Length);
        var recordLength = baseAddress + totalFieldSize;

        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3LE1 02{baseAddress:D5}   3304"
        );

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

    /// <summary>
    /// Creates a CATD data record with the specified catalog entry values.
    /// </summary>
    private static byte[] CreateCatdRecord(
        string rcnm = "CD",
        uint rcid = 1,
        string file = "US5WA51M.000",
        string lfil = "US5WA51M.000",
        string volm = "V01X01",
        string impl = "BIN",
        string? slat = null,
        string? wlon = null,
        string? nlat = null,
        string? elon = null,
        string crcs = "",
        string comt = "")
    {
        using var ms = new MemoryStream();

        WriteString(ms, rcnm);
        WriteString(ms, rcid.ToString());
        WriteString(ms, file);
        WriteString(ms, lfil);
        WriteString(ms, volm);
        WriteString(ms, impl);
        WriteString(ms, slat ?? "");
        WriteString(ms, wlon ?? "");
        WriteString(ms, nlat ?? "");
        WriteString(ms, elon ?? "");
        WriteString(ms, crcs);
        WriteString(ms, comt);
        ms.WriteByte(FieldTerminator);

        return CreateDataRecord("CATD", ms.ToArray());
    }

    /// <summary>
    /// Creates a data record with a single field.
    /// </summary>
    private static byte[] CreateDataRecord(string tag, byte[] fieldData)
    {
        var directoryEntry = Encoding.ASCII.GetBytes($"{tag}{fieldData.Length:D3}000");
        var baseAddress = 24 + directoryEntry.Length + 1;
        var recordLength = baseAddress + fieldData.Length;

        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3DE1 00{baseAddress:D5}   3304"
        );

        var record = new byte[recordLength];
        var offset = 0;

        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;

        Array.Copy(directoryEntry, 0, record, offset, directoryEntry.Length);
        offset += directoryEntry.Length;

        record[offset++] = FieldTerminator;

        Array.Copy(fieldData, 0, record, offset, fieldData.Length);

        return record;
    }

    /// <summary>
    /// Writes a string followed by a unit terminator.
    /// </summary>
    private static void WriteString(MemoryStream ms, string value)
    {
        ms.Write(Encoding.ASCII.GetBytes(value));
        ms.WriteByte(UnitTerminator);
    }

    #endregion

    #region S57Catalog Parsing Tests

    [Fact]
    public void Read_EmptyDocument_ReturnsEmptyEntries()
    {
        // Arrange
        var data = CreateCatalogDocument();

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        Assert.NotNull(catalog);
        Assert.Empty(catalog.Entries);
    }

    [Fact]
    public void Read_SingleCatalogEntry_ParsesAllFields()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(
            rcnm: "CD",
            rcid: 42,
            file: "US5WA51M.000",
            lfil: "ENC_ROOT/US5WA51M/US5WA51M.000",
            volm: "V01X01",
            impl: "BIN",
            slat: "47.500000",
            wlon: "-122.500000",
            nlat: "48.000000",
            elon: "-122.000000",
            crcs: "ABCD1234",
            comt: "Test chart"
        );
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        Assert.Single(catalog.Entries);
        var entry = catalog.Entries[0];
        Assert.Equal("CD", entry.RecordName);
        Assert.Equal(42u, entry.RecordId);
        Assert.Equal("US5WA51M.000", entry.FileName);
        Assert.Equal("ENC_ROOT/US5WA51M/US5WA51M.000", entry.LongFileName);
        Assert.Equal("V01X01", entry.Volume);
        Assert.Equal("BIN", entry.Implementation);
        Assert.Equal(47.5, entry.SouthernmostLatitude);
        Assert.Equal(-122.5, entry.WesternmostLongitude);
        Assert.Equal(48.0, entry.NorthernmostLatitude);
        Assert.Equal(-122.0, entry.EasternmostLongitude);
        Assert.Equal("ABCD1234", entry.CrcChecksum);
        Assert.Equal("Test chart", entry.Comment);
    }

    [Fact]
    public void Read_MultipleCatalogEntries_ParsesAllEntries()
    {
        // Arrange
        var record1 = CreateCatdRecord(rcid: 1, file: "CATALOG.031", impl: "ASC");
        var record2 = CreateCatdRecord(rcid: 2, file: "US5WA51M.000", impl: "BIN",
            slat: "47.0", wlon: "-123.0", nlat: "48.0", elon: "-122.0");
        var record3 = CreateCatdRecord(rcid: 3, file: "US5WA52M.000", impl: "BIN",
            slat: "48.0", wlon: "-123.0", nlat: "49.0", elon: "-122.0");
        var data = CreateCatalogDocument(record1, record2, record3);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        Assert.Equal(3, catalog.Entries.Length);
        Assert.Equal(1u, catalog.Entries[0].RecordId);
        Assert.Equal(2u, catalog.Entries[1].RecordId);
        Assert.Equal(3u, catalog.Entries[2].RecordId);
    }

    [Fact]
    public void Read_EntryWithGeographicBounds_ParsesCoordinates()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(
            slat: "47.123456",
            wlon: "-122.987654",
            nlat: "48.654321",
            elon: "-121.012345"
        );
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        var entry = catalog.Entries[0];
        Assert.NotNull(entry.SouthernmostLatitude);
        Assert.NotNull(entry.WesternmostLongitude);
        Assert.NotNull(entry.NorthernmostLatitude);
        Assert.NotNull(entry.EasternmostLongitude);
        Assert.Equal(47.123456, entry.SouthernmostLatitude!.Value, 6);
        Assert.Equal(-122.987654, entry.WesternmostLongitude!.Value, 6);
        Assert.Equal(48.654321, entry.NorthernmostLatitude!.Value, 6);
        Assert.Equal(-121.012345, entry.EasternmostLongitude!.Value, 6);
    }

    [Fact]
    public void Read_EntryWithoutGeographicBounds_ReturnsNullCoordinates()
    {
        // Arrange - No lat/lon values provided (empty strings)
        var catdRecord = CreateCatdRecord(
            file: "CATALOG.031",
            impl: "ASC"
        );
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        var entry = catalog.Entries[0];
        Assert.Null(entry.SouthernmostLatitude);
        Assert.Null(entry.WesternmostLongitude);
        Assert.Null(entry.NorthernmostLatitude);
        Assert.Null(entry.EasternmostLongitude);
    }

    [Fact]
    public void Read_EntryRecordName_IsString()
    {
        // Arrange - RCNM in catalog is a 2-character string, not a numeric byte
        var catdRecord = CreateCatdRecord(rcnm: "CD");
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        Assert.Equal("CD", catalog.Entries[0].RecordName);
    }

    [Fact]
    public void Read_EntryWithEmptyOptionalFields_ReturnsEmptyStrings()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(crcs: "", comt: "");
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        var entry = catalog.Entries[0];
        Assert.Equal("", entry.CrcChecksum);
        Assert.Equal("", entry.Comment);
    }

    [Fact]
    public void Read_MultipleCatalogEntries_PreservesOrder()
    {
        // Arrange
        var record1 = CreateCatdRecord(rcid: 10, file: "FIRST.000");
        var record2 = CreateCatdRecord(rcid: 20, file: "SECOND.000");
        var record3 = CreateCatdRecord(rcid: 30, file: "THIRD.000");
        var data = CreateCatalogDocument(record1, record2, record3);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        Assert.Equal(3, catalog.Entries.Length);
        Assert.Equal("FIRST.000", catalog.Entries[0].FileName);
        Assert.Equal("SECOND.000", catalog.Entries[1].FileName);
        Assert.Equal("THIRD.000", catalog.Entries[2].FileName);
    }

    [Fact]
    public void Read_AsciiImplementation_ParsesCorrectly()
    {
        // Arrange - Catalog self-reference entry (CATALOG.031 itself) uses ASC implementation
        var catdRecord = CreateCatdRecord(
            rcid: 1,
            file: "CATALOG.031",
            lfil: "CATALOG.031",
            impl: "ASC"
        );
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        Assert.Equal("ASC", catalog.Entries[0].Implementation);
    }

    [Fact]
    public void Read_NegativeCoordinates_ParsesCorrectly()
    {
        // Arrange - Southern hemisphere and western longitude
        var catdRecord = CreateCatdRecord(
            slat: "-33.900000",
            wlon: "-151.300000",
            nlat: "-33.800000",
            elon: "-151.200000"
        );
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        var entry = catalog.Entries[0];
        Assert.Equal(-33.9, entry.SouthernmostLatitude!.Value, 6);
        Assert.Equal(-151.3, entry.WesternmostLongitude!.Value, 6);
        Assert.Equal(-33.8, entry.NorthernmostLatitude!.Value, 6);
        Assert.Equal(-151.2, entry.EasternmostLongitude!.Value, 6);
    }

    #endregion

    #region Read Method Variant Tests

    [Fact]
    public void Read_ByteArray_ReturnsCatalog()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(file: "BYTEARRAY.000");
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data);

        // Assert
        Assert.Single(catalog.Entries);
        Assert.Equal("BYTEARRAY.000", catalog.Entries[0].FileName);
    }

    [Fact]
    public void Read_ReadOnlySpan_ReturnsCatalog()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(file: "SPANTEST.000");
        var data = CreateCatalogDocument(catdRecord);

        // Act
        var catalog = S57CatalogReader.Read(data.AsSpan());

        // Assert
        Assert.Single(catalog.Entries);
        Assert.Equal("SPANTEST.000", catalog.Entries[0].FileName);
    }

    [Fact]
    public void Read_Stream_ReturnsCatalog()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(file: "STREAMTEST.000");
        var data = CreateCatalogDocument(catdRecord);

        // Act
        S57Catalog catalog;
        using (var stream = new MemoryStream(data))
        {
            catalog = S57CatalogReader.Read(stream);
        }

        // Assert
        Assert.Single(catalog.Entries);
        Assert.Equal("STREAMTEST.000", catalog.Entries[0].FileName);
    }

    [Fact]
    public async Task ReadAsync_Stream_ReturnsCatalog()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(file: "ASYNCTEST.000");
        var data = CreateCatalogDocument(catdRecord);

        // Act
        S57Catalog catalog;
        using (var stream = new MemoryStream(data))
        {
            catalog = await S57CatalogReader.ReadAsync(stream);
        }

        // Assert
        Assert.Single(catalog.Entries);
        Assert.Equal("ASYNCTEST.000", catalog.Entries[0].FileName);
    }

    [Fact]
    public void ReadFromFile_ValidFile_ReturnsCatalog()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(file: "FILETEST.000");
        var data = CreateCatalogDocument(catdRecord);
        var tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllBytes(tempFile, data);

            // Act
            var catalog = S57CatalogReader.ReadFromFile(tempFile);

            // Assert
            Assert.Single(catalog.Entries);
            Assert.Equal("FILETEST.000", catalog.Entries[0].FileName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFileAsync_ValidFile_ReturnsCatalog()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(file: "ASYNCFILE.000");
        var data = CreateCatalogDocument(catdRecord);
        var tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllBytesAsync(tempFile, data);

            // Act
            var catalog = await S57CatalogReader.ReadFromFileAsync(tempFile);

            // Assert
            Assert.Single(catalog.Entries);
            Assert.Equal("ASYNCFILE.000", catalog.Entries[0].FileName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFileAsync_WithCancellationToken_ReturnsCatalog()
    {
        // Arrange
        var catdRecord = CreateCatdRecord(file: "CANCELTEST.000");
        var data = CreateCatalogDocument(catdRecord);
        var tempFile = Path.GetTempFileName();
        var cts = new CancellationTokenSource();

        try
        {
            await File.WriteAllBytesAsync(tempFile, data);

            // Act
            var catalog = await S57CatalogReader.ReadFromFileAsync(tempFile, cancellationToken: cts.Token);

            // Assert
            Assert.Single(catalog.Entries);
            Assert.Equal("CANCELTEST.000", catalog.Entries[0].FileName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region S57CatalogEntry Tests

    [Fact]
    public void CatalogEntry_DefaultValues_AreEmpty()
    {
        // Arrange & Act
        var entry = new S57CatalogEntry();

        // Assert
        Assert.Equal("", entry.RecordName);
        Assert.Equal(0u, entry.RecordId);
        Assert.Equal("", entry.FileName);
        Assert.Equal("", entry.LongFileName);
        Assert.Equal("", entry.Volume);
        Assert.Equal("", entry.Implementation);
        Assert.Null(entry.SouthernmostLatitude);
        Assert.Null(entry.WesternmostLongitude);
        Assert.Null(entry.NorthernmostLatitude);
        Assert.Null(entry.EasternmostLongitude);
        Assert.Equal("", entry.CrcChecksum);
        Assert.Equal("", entry.Comment);
    }

    #endregion
}
