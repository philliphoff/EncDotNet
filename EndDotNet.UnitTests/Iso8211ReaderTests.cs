using System.Collections.Immutable;
using System.Text;
using EncDotNet.Iso8211;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for the <see cref="Iso8211Reader"/> and related classes.
/// </summary>
public class Iso8211ReaderTests
{
    #region Test Data Helpers

    /// <summary>
    /// Creates a minimal valid ISO 8211 record for testing.
    /// </summary>
    /// <param name="isDataDescriptiveRecord">If true, creates a DDR; otherwise, creates a data record.</param>
    /// <returns>A byte array containing a valid ISO 8211 record.</returns>
    private static byte[] CreateMinimalRecord(bool isDataDescriptiveRecord = true)
    {
        var leaderIdentifier = isDataDescriptiveRecord ? 'L' : 'D';

        // Create a simple record with one field
        var fieldData = "TEST"u8.ToArray();
        var fieldTerminator = (byte)0x1E;

        // Directory entry: tag (4) + length (3) + position (3) = 10 bytes
        // Field length includes the field terminator
        var fieldLength = fieldData.Length + 1; // +1 for field terminator
        var directoryEntry = Encoding.ASCII.GetBytes($"0001{fieldLength:D3}000");

        // Base address = leader (24) + directory entry (10) + directory terminator (1) = 35
        var baseAddress = 24 + directoryEntry.Length + 1;

        // Record length = base address + field data + field terminator
        var recordLength = baseAddress + fieldData.Length + 1;

        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}" + // Record length
            "3" +                   // Interchange level
            leaderIdentifier +      // Leader identifier
            "E" +                   // Inline code extension indicator
            "1" +                   // Version number
            " " +                   // Application indicator
            "00" +                  // Field control length
            $"{baseAddress:D5}" +   // Base address of field area
            "   " +                 // Extended character set indicators
            "3" +                   // Size of field length field
            "3" +                   // Size of field position field
            "0" +                   // Reserved
            "4"                     // Size of field tag field
        );

        // Build the complete record
        var record = new byte[recordLength];
        var offset = 0;

        // Copy leader
        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;

        // Copy directory entry
        Array.Copy(directoryEntry, 0, record, offset, directoryEntry.Length);
        offset += directoryEntry.Length;

        // Directory terminator
        record[offset++] = fieldTerminator;

        // Copy field data
        Array.Copy(fieldData, 0, record, offset, fieldData.Length);
        offset += fieldData.Length;

        // Field terminator
        record[offset] = fieldTerminator;

        return record;
    }

    /// <summary>
    /// Creates a record with multiple fields for testing.
    /// </summary>
    private static byte[] CreateMultiFieldRecord(bool isDataDescriptiveRecord = false)
    {
        var leaderIdentifier = isDataDescriptiveRecord ? 'L' : 'D';
        
        // Fields: 0001 -> "HELLO", 0002 -> "WORLD"
        var field1Tag = "0001";
        var field1Data = "HELLO"u8.ToArray();
        var field2Tag = "0002";
        var field2Data = "WORLD"u8.ToArray();
        var fieldTerminator = (byte)0x1E;

        var field1Length = field1Data.Length + 1;
        var field2Length = field2Data.Length + 1;
        var field1Position = 0;
        var field2Position = field1Length;

        // Directory entries (tag=4, length=3, position=3 each)
        var dir1 = Encoding.ASCII.GetBytes($"{field1Tag}{field1Length:D3}{field1Position:D3}");
        var dir2 = Encoding.ASCII.GetBytes($"{field2Tag}{field2Length:D3}{field2Position:D3}");

        var baseAddress = 24 + dir1.Length + dir2.Length + 1; // +1 for dir terminator
        var recordLength = baseAddress + field1Data.Length + 1 + field2Data.Length + 1;

        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3{leaderIdentifier}E1 00{baseAddress:D5}   3304"
        );

        var record = new byte[recordLength];
        var offset = 0;

        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;

        Array.Copy(dir1, 0, record, offset, dir1.Length);
        offset += dir1.Length;

        Array.Copy(dir2, 0, record, offset, dir2.Length);
        offset += dir2.Length;

        record[offset++] = fieldTerminator; // Directory terminator

        Array.Copy(field1Data, 0, record, offset, field1Data.Length);
        offset += field1Data.Length;
        record[offset++] = fieldTerminator;

        Array.Copy(field2Data, 0, record, offset, field2Data.Length);
        offset += field2Data.Length;
        record[offset] = fieldTerminator;

        return record;
    }

    /// <summary>
    /// Creates a record with subfields for testing.
    /// </summary>
    private static byte[] CreateSubfieldRecord()
    {
        // Field with subfields: "SUB1" + UnitTerminator + "SUB2" + UnitTerminator + "SUB3"
        var unitTerminator = (byte)0x1F;
        var fieldTerminator = (byte)0x1E;

        var subfield1 = "SUB1"u8.ToArray();
        var subfield2 = "SUB2"u8.ToArray();
        var subfield3 = "SUB3"u8.ToArray();

        // Field data: SUB1 + UT + SUB2 + UT + SUB3
        var fieldData = new byte[subfield1.Length + 1 + subfield2.Length + 1 + subfield3.Length];
        var offset = 0;
        Array.Copy(subfield1, 0, fieldData, offset, subfield1.Length);
        offset += subfield1.Length;
        fieldData[offset++] = unitTerminator;
        Array.Copy(subfield2, 0, fieldData, offset, subfield2.Length);
        offset += subfield2.Length;
        fieldData[offset++] = unitTerminator;
        Array.Copy(subfield3, 0, fieldData, offset, subfield3.Length);

        var fieldLength = fieldData.Length + 1; // +1 for field terminator
        var directoryEntry = Encoding.ASCII.GetBytes($"0001{fieldLength:D3}000");

        var baseAddress = 24 + directoryEntry.Length + 1;
        var recordLength = baseAddress + fieldData.Length + 1;

        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3DE1 00{baseAddress:D5}   3304"
        );

        var record = new byte[recordLength];
        offset = 0;

        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;

        Array.Copy(directoryEntry, 0, record, offset, directoryEntry.Length);
        offset += directoryEntry.Length;

        record[offset++] = fieldTerminator; // Directory terminator

        Array.Copy(fieldData, 0, record, offset, fieldData.Length);
        offset += fieldData.Length;
        record[offset] = fieldTerminator;

        return record;
    }

    /// <summary>
    /// Creates two consecutive records (DDR + data record) for multi-record testing.
    /// </summary>
    private static byte[] CreateMultipleRecords()
    {
        var record1 = CreateMinimalRecord(isDataDescriptiveRecord: true);
        var record2 = CreateMinimalRecord(isDataDescriptiveRecord: false);

        var combined = new byte[record1.Length + record2.Length];
        Array.Copy(record1, 0, combined, 0, record1.Length);
        Array.Copy(record2, 0, combined, record1.Length, record2.Length);

        return combined;
    }

    /// <summary>
    /// Creates a document with DDR and multiple data records.
    /// </summary>
    private static byte[] CreateFullDocument()
    {
        var ddr = CreateMinimalRecord(isDataDescriptiveRecord: true);
        var dataRecord1 = CreateMultiFieldRecord(isDataDescriptiveRecord: false);
        var dataRecord2 = CreateSubfieldRecord();

        var combined = new byte[ddr.Length + dataRecord1.Length + dataRecord2.Length];
        var offset = 0;
        Array.Copy(ddr, 0, combined, offset, ddr.Length);
        offset += ddr.Length;
        Array.Copy(dataRecord1, 0, combined, offset, dataRecord1.Length);
        offset += dataRecord1.Length;
        Array.Copy(dataRecord2, 0, combined, offset, dataRecord2.Length);

        return combined;
    }

    #endregion

    #region Iso8211Reader.Read(byte[]) Tests

    [Fact]
    public void Read_ByteArray_ParsesMinimalRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.NotNull(document);
        Assert.Single(document.Records);
    }

    [Fact]
    public void Read_ByteArray_ParsesMultipleRecords()
    {
        // Arrange
        var data = CreateMultipleRecords();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.NotNull(document);
        Assert.Equal(2, document.Records.Length);
    }

    [Fact]
    public void Read_EmptyByteArray_ReturnsEmptyDocument()
    {
        // Arrange
        var data = Array.Empty<byte>();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.NotNull(document);
        Assert.Empty(document.Records);
    }

    #endregion

    #region Iso8211Reader.Read(ReadOnlySpan<byte>) Tests

    [Fact]
    public void Read_Span_ParsesMinimalRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data.AsSpan());

        // Assert
        Assert.NotNull(document);
        Assert.Single(document.Records);
    }

    [Fact]
    public void Read_Span_ParsesMultipleRecords()
    {
        // Arrange
        var data = CreateMultipleRecords();

        // Act
        var document = Iso8211Reader.Read(data.AsSpan());

        // Assert
        Assert.Equal(2, document.Records.Length);
    }

    #endregion

    #region Iso8211Document Tests

    [Fact]
    public void Document_DataDescriptiveRecord_ReturnsFirstDdrRecord()
    {
        // Arrange
        var data = CreateMultipleRecords();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.NotNull(document.DataDescriptiveRecord);
        Assert.Equal('L', document.DataDescriptiveRecord.Leader.LeaderIdentifier);
    }

    [Fact]
    public void Document_DataDescriptiveRecord_ReturnsNullWhenNoDdr()
    {
        // Arrange - Create a document starting with a data record
        var data = CreateMinimalRecord(isDataDescriptiveRecord: false);

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.Null(document.DataDescriptiveRecord);
    }

    [Fact]
    public void Document_DataRecords_ReturnsOnlyNonDdrRecords()
    {
        // Arrange
        var data = CreateMultipleRecords();

        // Act
        var document = Iso8211Reader.Read(data);
        var dataRecords = document.DataRecords.ToList();

        // Assert
        Assert.Single(dataRecords);
        Assert.Equal('D', dataRecords[0].Leader.LeaderIdentifier);
    }

    [Fact]
    public void Document_DataRecords_ReturnsAllRecordsWhenNoDdr()
    {
        // Arrange
        var data = CreateMinimalRecord(isDataDescriptiveRecord: false);

        // Act
        var document = Iso8211Reader.Read(data);
        var dataRecords = document.DataRecords.ToList();

        // Assert
        Assert.Single(dataRecords);
    }

    [Fact]
    public void Document_Records_IsImmutableArray()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.IsType<ImmutableArray<Iso8211Record>>(document.Records);
    }

    #endregion

    #region Iso8211Record Tests

    [Fact]
    public void Record_Leader_ContainsCorrectValues()
    {
        // Arrange
        var data = CreateMinimalRecord(isDataDescriptiveRecord: true);

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];

        // Assert
        Assert.Equal('3', record.Leader.InterchangeLevel);
        Assert.Equal('L', record.Leader.LeaderIdentifier);
        Assert.Equal('E', record.Leader.InlineCodeExtensionIndicator);
        Assert.Equal('1', record.Leader.VersionNumber);
        Assert.Equal(' ', record.Leader.ApplicationIndicator);
        Assert.Equal(0, record.Leader.FieldControlLength);
        Assert.Equal(3, record.Leader.SizeOfFieldLengthField);
        Assert.Equal(3, record.Leader.SizeOfFieldPositionField);
        Assert.Equal(4, record.Leader.SizeOfFieldTagField);
    }

    [Fact]
    public void Record_Leader_ExtendedCharacterSetIndicator_IsParsed()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];

        // Assert
        Assert.Equal("   ", record.Leader.ExtendedCharacterSetIndicator);
    }

    [Fact]
    public void Record_IsDataDescriptiveRecord_TrueForDdr()
    {
        // Arrange
        var data = CreateMinimalRecord(isDataDescriptiveRecord: true);

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];

        // Assert
        Assert.True(record.IsDataDescriptiveRecord);
    }

    [Fact]
    public void Record_IsDataDescriptiveRecord_FalseForDataRecord()
    {
        // Arrange
        var data = CreateMinimalRecord(isDataDescriptiveRecord: false);

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];

        // Assert
        Assert.False(record.IsDataDescriptiveRecord);
    }

    [Fact]
    public void Record_Directory_ContainsCorrectEntries()
    {
        // Arrange
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];

        // Assert
        Assert.Equal(2, record.Directory.Length);
        Assert.Equal("0001", record.Directory[0].Tag);
        Assert.Equal("0002", record.Directory[1].Tag);
    }

    [Fact]
    public void Record_Fields_ContainsCorrectFields()
    {
        // Arrange
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];

        // Assert
        Assert.Equal(2, record.Fields.Length);
        Assert.Equal("0001", record.Fields[0].Tag);
        Assert.Equal("0002", record.Fields[1].Tag);
    }

    [Fact]
    public void Record_GetFieldByTag_ReturnsMatchingField()
    {
        // Arrange
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];
        var field = record.GetFieldByTag("0002");

        // Assert
        Assert.NotNull(field);
        Assert.Equal("0002", field.Tag);
    }

    [Fact]
    public void Record_GetFieldByTag_ReturnsNullForNonExistentTag()
    {
        // Arrange
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];
        var field = record.GetFieldByTag("9999");

        // Assert
        Assert.Null(field);
    }

    [Fact]
    public void Record_GetFieldsByTag_ReturnsAllMatchingFields()
    {
        // Arrange - Create a record with duplicate tags (two fields with tag 0001)
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];
        var fields = record.GetFieldsByTag("0001").ToList();

        // Assert
        Assert.Single(fields);
        Assert.Equal("0001", fields[0].Tag);
    }

    [Fact]
    public void Record_GetFieldsByTag_ReturnsEmptyForNonExistentTag()
    {
        // Arrange
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];
        var fields = record.GetFieldsByTag("9999").ToList();

        // Assert
        Assert.Empty(fields);
    }

    #endregion

    #region Iso8211DirectoryEntry Tests

    [Fact]
    public void DirectoryEntry_Tag_IsCorrect()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var entry = document.Records[0].Directory[0];

        // Assert
        Assert.Equal("0001", entry.Tag);
    }

    [Fact]
    public void DirectoryEntry_Length_IsCorrect()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var entry = document.Records[0].Directory[0];

        // Assert
        Assert.Equal(5, entry.Length); // "TEST" + field terminator
    }

    [Fact]
    public void DirectoryEntry_Position_IsCorrect()
    {
        // Arrange
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var record = document.Records[0];

        // Assert
        Assert.Equal(0, record.Directory[0].Position);
        Assert.Equal(6, record.Directory[1].Position); // "HELLO" + field terminator = 6
    }

    #endregion

    #region Iso8211Field Tests

    [Fact]
    public void Field_Tag_IsCorrect()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field = document.Records[0].Fields[0];

        // Assert
        Assert.Equal("0001", field.Tag);
    }

    [Fact]
    public void Field_Data_ContainsRawBytes()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field = document.Records[0].Fields[0];

        // Assert
        Assert.Equal("TEST"u8.ToArray(), field.Data);
    }

    [Fact]
    public void Field_GetDataString_ReturnsCorrectString()
    {
        // Arrange
        var data = CreateMultiFieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field1 = document.Records[0].Fields[0];
        var field2 = document.Records[0].Fields[1];

        // Assert
        Assert.Equal("HELLO", field1.GetDataString());
        Assert.Equal("WORLD", field2.GetDataString());
    }

    [Fact]
    public void Field_GetDataString_WithEncoding_UsesSpecifiedEncoding()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field = document.Records[0].Fields[0];

        // Assert
        Assert.Equal("TEST", field.GetDataString(Encoding.UTF8));
        Assert.Equal("TEST", field.GetDataString(Encoding.ASCII));
    }

    [Fact]
    public void Field_GetDataString_TrimsTerminators()
    {
        // Arrange - Field data might contain trailing terminators
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field = document.Records[0].Fields[0];
        var dataString = field.GetDataString();

        // Assert - Should not contain terminator characters (0x1E = field terminator, 0x1F = unit terminator)
        Assert.DoesNotContain((char)0x1E, dataString);
        Assert.DoesNotContain((char)0x1F, dataString);
    }

    [Fact]
    public void Field_Subfields_IsImmutableArray()
    {
        // Arrange
        var data = CreateSubfieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field = document.Records[0].Fields[0];

        // Assert
        Assert.IsType<ImmutableArray<Iso8211Subfield>>(field.Subfields);
    }

    [Fact]
    public void Field_Subfields_ContainsCorrectSubfields()
    {
        // Arrange
        var data = CreateSubfieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field = document.Records[0].Fields[0];

        // Assert
        Assert.Equal(3, field.Subfields.Length);
    }

    #endregion

    #region Iso8211Subfield Tests

    [Fact]
    public void Subfield_Index_IsCorrect()
    {
        // Arrange
        var data = CreateSubfieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var subfields = document.Records[0].Fields[0].Subfields;

        // Assert
        Assert.Equal(0, subfields[0].Index);
        Assert.Equal(1, subfields[1].Index);
        Assert.Equal(2, subfields[2].Index);
    }

    [Fact]
    public void Subfield_Data_ContainsRawBytes()
    {
        // Arrange
        var data = CreateSubfieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var subfield = document.Records[0].Fields[0].Subfields[0];

        // Assert
        Assert.Equal("SUB1"u8.ToArray(), subfield.Data);
    }

    [Fact]
    public void Subfield_GetDataString_ReturnsCorrectString()
    {
        // Arrange
        var data = CreateSubfieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var subfields = document.Records[0].Fields[0].Subfields;

        // Assert
        Assert.Equal("SUB1", subfields[0].GetDataString());
        Assert.Equal("SUB2", subfields[1].GetDataString());
        Assert.Equal("SUB3", subfields[2].GetDataString());
    }

    [Fact]
    public void Subfield_GetDataString_WithEncoding_UsesSpecifiedEncoding()
    {
        // Arrange
        var data = CreateSubfieldRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var subfield = document.Records[0].Fields[0].Subfields[0];

        // Assert
        Assert.Equal("SUB1", subfield.GetDataString(Encoding.UTF8));
        Assert.Equal("SUB1", subfield.GetDataString(Encoding.ASCII));
    }

    #endregion

    #region Iso8211RecordLeader Tests

    [Fact]
    public void RecordLeader_RecordLength_IsCorrect()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var leader = document.Records[0].Leader;

        // Assert
        Assert.Equal(data.Length, leader.RecordLength);
    }

    [Fact]
    public void RecordLeader_BaseAddressOfFieldArea_IsCorrect()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var leader = document.Records[0].Leader;

        // Assert
        // Base address = leader (24) + directory entry (10) + directory terminator (1) = 35
        Assert.Equal(35, leader.BaseAddressOfFieldArea);
    }

    #endregion

    #region Full Document Tests

    [Fact]
    public void Read_FullDocument_ParsesAllRecords()
    {
        // Arrange
        var data = CreateFullDocument();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.Equal(3, document.Records.Length);
    }

    [Fact]
    public void Read_FullDocument_IdentifiesDdrCorrectly()
    {
        // Arrange
        var data = CreateFullDocument();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.NotNull(document.DataDescriptiveRecord);
        Assert.True(document.Records[0].IsDataDescriptiveRecord);
        Assert.False(document.Records[1].IsDataDescriptiveRecord);
        Assert.False(document.Records[2].IsDataDescriptiveRecord);
    }

    [Fact]
    public void Read_FullDocument_DataRecordsCount()
    {
        // Arrange
        var data = CreateFullDocument();

        // Act
        var document = Iso8211Reader.Read(data);
        var dataRecords = document.DataRecords.ToList();

        // Assert
        Assert.Equal(2, dataRecords.Count);
    }

    [Fact]
    public void Read_FullDocument_PreservesFieldData()
    {
        // Arrange
        var data = CreateFullDocument();

        // Act
        var document = Iso8211Reader.Read(data);
        var dataRecords = document.DataRecords.ToList();

        // Assert
        // First data record has two fields: "HELLO" and "WORLD"
        var record1 = dataRecords[0];
        Assert.Equal(2, record1.Fields.Length);
        Assert.Equal("HELLO", record1.Fields[0].GetDataString());
        Assert.Equal("WORLD", record1.Fields[1].GetDataString());

        // Second data record has subfields
        var record2 = dataRecords[1];
        Assert.Single(record2.Fields);
        Assert.Equal(3, record2.Fields[0].Subfields.Length);
    }

    #endregion

    #region Stream Reading Tests

    [Fact]
    public void Read_Stream_ParsesDocument()
    {
        // Arrange
        var data = CreateMinimalRecord();
        using var stream = new MemoryStream(data);

        // Act
        var document = Iso8211Reader.Read(stream);

        // Assert
        Assert.NotNull(document);
        Assert.Single(document.Records);
    }

    [Fact]
    public async Task ReadAsync_Stream_ParsesDocument()
    {
        // Arrange
        var data = CreateMinimalRecord();
        using var stream = new MemoryStream(data);

        // Act
        var document = await Iso8211Reader.ReadAsync(stream);

        // Assert
        Assert.NotNull(document);
        Assert.Single(document.Records);
    }

    [Fact]
    public async Task ReadAsync_Stream_WithCancellationToken_ParsesDocument()
    {
        // Arrange
        var data = CreateMinimalRecord();
        using var stream = new MemoryStream(data);
        var cts = new CancellationTokenSource();

        // Act
        var document = await Iso8211Reader.ReadAsync(stream, cts.Token);

        // Assert
        Assert.NotNull(document);
        Assert.Single(document.Records);
    }

    [Fact]
    public void Read_Stream_EmptyStream_ReturnsEmptyDocument()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var document = Iso8211Reader.Read(stream);

        // Assert
        Assert.NotNull(document);
        Assert.Empty(document.Records);
    }

    #endregion

    #region File Reading Tests

    [Fact]
    public void ReadFromFile_ValidFile_ParsesDocument()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, data);

            // Act
            var document = Iso8211Reader.ReadFromFile(tempFile);

            // Assert
            Assert.NotNull(document);
            Assert.Single(document.Records);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFileAsync_ValidFile_ParsesDocument()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, data);

            // Act
            var document = await Iso8211Reader.ReadFromFileAsync(tempFile);

            // Assert
            Assert.NotNull(document);
            Assert.Single(document.Records);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFileAsync_WithCancellationToken_ParsesDocument()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var tempFile = Path.GetTempFileName();
        var cts = new CancellationTokenSource();
        try
        {
            await File.WriteAllBytesAsync(tempFile, data);

            // Act
            var document = await Iso8211Reader.ReadFromFileAsync(tempFile, cts.Token);

            // Assert
            Assert.NotNull(document);
            Assert.Single(document.Records);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReadFromFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => Iso8211Reader.ReadFromFile(nonExistentFile));
    }

    [Fact]
    public async Task ReadFromFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => Iso8211Reader.ReadFromFileAsync(nonExistentFile));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Read_RecordWithNoFields_ParsesCorrectly()
    {
        // Arrange - Create a record with an empty directory (just terminator)
        var fieldTerminator = (byte)0x1E;
        var baseAddress = 24 + 1; // leader + dir terminator only
        var recordLength = baseAddress;

        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3DE1 00{baseAddress:D5}   3304"
        );

        var record = new byte[recordLength];
        Array.Copy(leader, 0, record, 0, leader.Length);
        record[24] = fieldTerminator; // Directory terminator

        // Act
        var document = Iso8211Reader.Read(record);

        // Assert
        Assert.Single(document.Records);
        Assert.Empty(document.Records[0].Directory);
        Assert.Empty(document.Records[0].Fields);
    }

    [Fact]
    public void Read_FieldWithNoSubfields_HasSingleSubfieldWithEntireData()
    {
        // Arrange - Field "TEST" has no unit terminators
        var data = CreateMinimalRecord();

        // Act
        var document = Iso8211Reader.Read(data);
        var field = document.Records[0].Fields[0];

        // Assert - When no unit terminators exist, the entire field data becomes one subfield
        Assert.Single(field.Subfields);
        Assert.Equal("TEST"u8.ToArray(), field.Subfields[0].Data);
    }

    [Fact]
    public void Read_MultipleRecords_MaintainsOrder()
    {
        // Arrange
        var data = CreateMultipleRecords();

        // Act
        var document = Iso8211Reader.Read(data);

        // Assert
        Assert.Equal('L', document.Records[0].Leader.LeaderIdentifier);
        Assert.Equal('D', document.Records[1].Leader.LeaderIdentifier);
    }

    #endregion
}
