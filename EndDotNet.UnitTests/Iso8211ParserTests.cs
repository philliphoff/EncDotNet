using System.Text;
using EncDotNet.Iso8211;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for the <see cref="Iso8211Parser"/> class.
/// </summary>
public class Iso8211ParserTests
{
    #region Test Data Helpers

    /// <summary>
    /// Creates a minimal valid ISO 8211 record for testing.
    /// </summary>
    /// <param name="isDataDescriptiveRecord">If true, creates a DDR; otherwise, creates a data record.</param>
    /// <returns>A byte array containing a valid ISO 8211 record.</returns>
    private static byte[] CreateMinimalRecord(bool isDataDescriptiveRecord = true)
    {
        // Leader format (24 bytes):
        // 0-4: Record length (5 chars)
        // 5: Interchange level
        // 6: Leader identifier ('L' for DDR, 'D' for data record)
        // 7: Inline code extension indicator
        // 8: Version number
        // 9: Application indicator
        // 10-11: Field control length (2 chars)
        // 12-16: Base address of field area (5 chars)
        // 17-19: Extended character set indicators (3 chars)
        // 20: Size of field length field
        // 21: Size of field position field
        // 22: Reserved
        // 23: Size of field tag field

        var leaderIdentifier = isDataDescriptiveRecord ? 'L' : 'D';

        // Create a simple record with one field
        var fieldTag = "0001"u8.ToArray();
        var fieldData = "TEST"u8.ToArray();
        var fieldTerminator = (byte)0x1E;

        // Directory entry: tag (4) + length (3) + position (3) = 10 bytes
        // Field length includes the field terminator
        var fieldLength = fieldData.Length + 1; // +1 for field terminator
        var directoryEntry = Encoding.ASCII.GetBytes($"0001{fieldLength:D3}000");
        var directoryTerminator = fieldTerminator;

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
        record[offset++] = directoryTerminator;

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
    private static byte[] CreateMultiFieldRecord()
    {
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
            $"{recordLength:D5}3LE1 00{baseAddress:D5}   3304"
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
            $"{recordLength:D5}3LE1 00{baseAddress:D5}   3304"
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
    /// Creates two consecutive records for multi-record testing.
    /// </summary>
    private static byte[] CreateMultipleRecords()
    {
        var record1 = CreateMinimalRecord(true);
        var record2 = CreateMinimalRecord(false);

        var combined = new byte[record1.Length + record2.Length];
        Array.Copy(record1, 0, combined, 0, record1.Length);
        Array.Copy(record2, 0, combined, record1.Length, record2.Length);

        return combined;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithSpan_InitializesCorrectly()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var parser = new Iso8211Parser(data);

        // Assert
        Assert.Equal(Iso8211TokenType.None, parser.TokenType);
        Assert.Equal(Iso8211ReaderState.None, parser.CurrentState);
        Assert.Equal(0, parser.BytesConsumed);
        Assert.True(parser.IsFinalBlock);
    }

    [Fact]
    public void Constructor_WithFinalBlockFalse_SetsIsFinalBlockCorrectly()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var parser = new Iso8211Parser(data, isFinalBlock: false);

        // Assert
        Assert.False(parser.IsFinalBlock);
    }

    [Fact]
    public void Constructor_WithFinalBlockTrue_SetsIsFinalBlockCorrectly()
    {
        // Arrange
        var data = CreateMinimalRecord();

        // Act
        var parser = new Iso8211Parser(data, isFinalBlock: true);

        // Assert
        Assert.True(parser.IsFinalBlock);
    }

    [Fact]
    public void Constructor_WithStreamingState_RestoresState()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser1 = new Iso8211Parser(data);
        parser1.Read(); // Read StartRecord

        var state = parser1.GetCurrentState();

        // Act - Create new parser with saved state (simulating continuation)
        var parser2 = new Iso8211Parser(data, isFinalBlock: true, state);

        // Assert
        Assert.Equal(Iso8211ReaderState.InLeader, parser2.CurrentState);
    }

    [Fact]
    public void Constructor_WithEmptySpan_InitializesCorrectly()
    {
        // Arrange
        var data = ReadOnlySpan<byte>.Empty;

        // Act
        var parser = new Iso8211Parser(data);

        // Assert
        Assert.Equal(Iso8211TokenType.None, parser.TokenType);
        Assert.Equal(Iso8211ReaderState.None, parser.CurrentState);
    }

    #endregion

    #region Read Tests - Basic Flow

    [Fact]
    public void Read_FirstCall_ReturnsStartRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Act
        var result = parser.Read();

        // Assert
        Assert.True(result);
        Assert.Equal(Iso8211TokenType.StartRecord, parser.TokenType);
        Assert.Equal(Iso8211ReaderState.InLeader, parser.CurrentState);
    }

    [Fact]
    public void Read_AfterStartRecord_ReturnsDirectoryEntry()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord

        // Act
        var result = parser.Read();

        // Assert
        Assert.True(result);
        Assert.Equal(Iso8211TokenType.DirectoryEntry, parser.TokenType);
        Assert.Equal(Iso8211ReaderState.InDirectory, parser.CurrentState);
    }

    [Fact]
    public void Read_AfterLastDirectoryEntry_ReturnsField()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry

        // Act
        var result = parser.Read();

        // Assert
        Assert.True(result);
        Assert.Equal(Iso8211TokenType.Field, parser.TokenType);
        Assert.Equal(Iso8211ReaderState.InFieldArea, parser.CurrentState);
    }

    [Fact]
    public void Read_AfterLastField_ReturnsEndRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Act
        var result = parser.Read();

        // Assert
        Assert.True(result);
        Assert.Equal(Iso8211TokenType.EndRecord, parser.TokenType);
        Assert.Equal(Iso8211ReaderState.None, parser.CurrentState);
    }

    [Fact]
    public void Read_AfterEndRecord_WithNoMoreData_ReturnsFalse()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field
        parser.Read(); // EndRecord

        // Act
        var result = parser.Read();

        // Assert
        Assert.False(result);
        Assert.Equal(Iso8211TokenType.EndOfData, parser.TokenType);
    }

    [Fact]
    public void Read_EmptyData_ReturnsFalse()
    {
        // Arrange
        var data = ReadOnlySpan<byte>.Empty;
        var parser = new Iso8211Parser(data);

        // Act
        var result = parser.Read();

        // Assert
        Assert.False(result);
        Assert.Equal(Iso8211TokenType.EndOfData, parser.TokenType);
    }

    #endregion

    #region Leader Tests

    [Fact]
    public void Read_ParsesLeaderCorrectly()
    {
        // Arrange
        var data = CreateMinimalRecord(isDataDescriptiveRecord: true);
        var parser = new Iso8211Parser(data);

        // Act
        parser.Read();

        // Assert
        var leader = parser.CurrentLeader;
        Assert.Equal('L', leader.LeaderIdentifier);
        Assert.Equal('3', leader.InterchangeLevel);
        Assert.Equal('E', leader.InlineCodeExtensionIndicator);
        Assert.Equal('1', leader.VersionNumber);
        Assert.Equal(' ', leader.ApplicationIndicator);
        Assert.True(leader.IsDataDescriptiveRecord);
    }

    [Fact]
    public void Read_DataRecord_HasCorrectLeaderIdentifier()
    {
        // Arrange
        var data = CreateMinimalRecord(isDataDescriptiveRecord: false);
        var parser = new Iso8211Parser(data);

        // Act
        parser.Read();

        // Assert
        var leader = parser.CurrentLeader;
        Assert.Equal('D', leader.LeaderIdentifier);
        Assert.False(leader.IsDataDescriptiveRecord);
    }

    [Fact]
    public void Read_LeaderDimensions_AreCorrect()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Act
        parser.Read();

        // Assert
        var leader = parser.CurrentLeader;
        Assert.Equal(3, leader.SizeOfFieldLengthField);
        Assert.Equal(3, leader.SizeOfFieldPositionField);
        Assert.Equal(4, leader.SizeOfFieldTagField);
    }

    #endregion

    #region Directory Entry Tests

    [Fact]
    public void DirectoryEntry_HasCorrectTag()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord

        // Act
        parser.Read(); // DirectoryEntry

        // Assert
        Assert.True(parser.TryGetTagString(out var tag));
        Assert.Equal("0001", tag);
    }

    [Fact]
    public void DirectoryEntry_HasCorrectLength()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry

        // Assert
        Assert.Equal(5, parser.CurrentLength); // "TEST" + field terminator
    }

    [Fact]
    public void DirectoryEntry_HasCorrectPosition()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry

        // Assert
        Assert.Equal(0, parser.CurrentPosition);
    }

    [Fact]
    public void DirectoryEntryCount_IsCorrect()
    {
        // Arrange
        var data = CreateMultiFieldRecord();
        var parser = new Iso8211Parser(data);

        // Act
        parser.Read(); // StartRecord

        // Assert
        Assert.Equal(2, parser.DirectoryEntryCount);
    }

    [Fact]
    public void CurrentDirectoryIndex_IncrementsCorrectly()
    {
        // Arrange
        var data = CreateMultiFieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord

        // Act & Assert
        Assert.Equal(0, parser.CurrentDirectoryIndex);
        Assert.True(parser.HasMoreDirectoryEntries);

        parser.Read(); // First directory entry
        Assert.Equal(1, parser.CurrentDirectoryIndex);

        parser.Read(); // Second directory entry
        Assert.Equal(2, parser.CurrentDirectoryIndex);
        Assert.False(parser.HasMoreDirectoryEntries);
    }

    #endregion

    #region Field Tests

    [Fact]
    public void Field_HasCorrectData()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Act
        var valueString = parser.GetValueString();

        // Assert
        Assert.Equal("TEST", valueString);
    }

    [Fact]
    public void Field_HasCorrectTag()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Assert
        Assert.Equal("0001", parser.GetTagString());
    }

    [Fact]
    public void MultipleFields_AreReadCorrectly()
    {
        // Arrange
        var data = CreateMultiFieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // Dir 1
        parser.Read(); // Dir 2
        parser.Read(); // Field 1

        // Assert first field
        Assert.Equal("0001", parser.GetTagString());
        Assert.Equal("HELLO", parser.GetValueString());

        // Act - read second field
        parser.Read(); // Field 2

        // Assert second field
        Assert.Equal("0002", parser.GetTagString());
        Assert.Equal("WORLD", parser.GetValueString());
    }

    [Fact]
    public void CurrentFieldIndex_IncrementsCorrectly()
    {
        // Arrange
        var data = CreateMultiFieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // Dir 1
        parser.Read(); // Dir 2

        // Act & Assert
        Assert.True(parser.HasMoreFields);
        Assert.Equal(0, parser.CurrentFieldIndex);

        parser.Read(); // Field 1
        Assert.Equal(1, parser.CurrentFieldIndex);

        parser.Read(); // Field 2
        Assert.Equal(2, parser.CurrentFieldIndex);
        Assert.False(parser.HasMoreFields);
    }

    [Fact]
    public void TryGetValueString_ReturnsFalseForEmptySpan()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        // Don't read any fields

        // Act
        var result = parser.TryGetValueString(out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void CopyValueTo_CopiesDataCorrectly()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        var destination = new byte[10];

        // Act
        var bytesCopied = parser.CopyValueTo(destination);

        // Assert
        Assert.Equal(4, bytesCopied); // "TEST"
        Assert.Equal("TEST"u8.ToArray(), destination[..4]);
    }

    [Fact]
    public void CopyValueTo_WithSmallDestination_CopiesPartialData()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        var destination = new byte[2];

        // Act
        var bytesCopied = parser.CopyValueTo(destination);

        // Assert
        Assert.Equal(2, bytesCopied);
        Assert.Equal("TE"u8.ToArray(), destination);
    }

    #endregion

    #region Subfield Tests

    [Fact]
    public void ReadSubfield_ReadsFirstSubfield()
    {
        // Arrange
        var data = CreateSubfieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Act
        var result = parser.ReadSubfield();

        // Assert
        Assert.True(result);
        Assert.Equal(Iso8211TokenType.Subfield, parser.TokenType);
        Assert.Equal("SUB1", parser.GetSubfieldString());
    }

    [Fact]
    public void ReadSubfield_ReadsAllSubfields()
    {
        // Arrange
        var data = CreateSubfieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Act & Assert
        Assert.True(parser.ReadSubfield());
        Assert.Equal("SUB1", parser.GetSubfieldString());

        Assert.True(parser.ReadSubfield());
        Assert.Equal("SUB2", parser.GetSubfieldString());

        Assert.True(parser.ReadSubfield());
        Assert.Equal("SUB3", parser.GetSubfieldString());

        Assert.False(parser.ReadSubfield()); // No more subfields
    }

    [Fact]
    public void ReadSubfield_ReturnsFalseWhenNotInField()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord

        // Act
        var result = parser.ReadSubfield();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetSubfieldString_ReturnsEmptyForNoData()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Act
        var result = parser.GetSubfieldString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Depth Tests

    [Fact]
    public void CurrentDepth_IsZeroInitially()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Assert
        Assert.Equal(0, parser.CurrentDepth);
    }

    [Fact]
    public void CurrentDepth_IsOneInRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord

        // Assert
        Assert.Equal(1, parser.CurrentDepth);
    }

    [Fact]
    public void CurrentDepth_IsOneForDirectoryEntry()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry

        // Assert
        Assert.Equal(1, parser.CurrentDepth);
    }

    [Fact]
    public void CurrentDepth_IsTwoForField()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Assert
        Assert.Equal(2, parser.CurrentDepth);
    }

    [Fact]
    public void CurrentDepth_IsTwoForSubfield()
    {
        // Arrange
        var data = CreateSubfieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field
        parser.ReadSubfield();

        // Assert
        Assert.Equal(2, parser.CurrentDepth);
    }

    [Fact]
    public void CurrentDepth_IsZeroAtEndRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field
        parser.Read(); // EndRecord

        // Assert
        Assert.Equal(0, parser.CurrentDepth);
    }

    #endregion

    #region Skip Tests

    [Fact]
    public void SkipDirectory_SkipsAllDirectoryEntries()
    {
        // Arrange
        var data = CreateMultiFieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // First DirectoryEntry - transitions to InDirectory state

        // Act
        parser.SkipDirectory();

        // Assert
        Assert.Equal(Iso8211ReaderState.InFieldArea, parser.CurrentState);
        Assert.Equal(0, parser.CurrentFieldIndex);
    }

    [Fact]
    public void SkipRecord_SkipsEntireRecord()
    {
        // Arrange
        var data = CreateMultipleRecords();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord of first record

        // Act
        parser.SkipRecord();

        // Assert
        Assert.Equal(Iso8211TokenType.EndRecord, parser.TokenType);
        Assert.Equal(Iso8211ReaderState.None, parser.CurrentState);

        // Should be able to read next record
        var result = parser.Read();
        Assert.True(result);
        Assert.Equal(Iso8211TokenType.StartRecord, parser.TokenType);
    }

    [Fact]
    public void TrySkip_FromStartRecord_SkipsEntireRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord

        // Act
        var result = parser.TrySkip();

        // Assert
        Assert.True(result);
        Assert.Equal(Iso8211TokenType.EndRecord, parser.TokenType);
    }

    [Fact]
    public void TrySkip_FromSubfield_SkipsToEndOfField()
    {
        // Arrange
        var data = CreateSubfieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field
        parser.ReadSubfield(); // First subfield

        // Act
        var result = parser.TrySkip();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Skip_DoesNotThrow()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord

        // Act & Assert - Should not throw
        parser.Skip();
    }

    #endregion

    #region Multiple Records Tests

    [Fact]
    public void Read_MultipleRecords_ReadsAllRecords()
    {
        // Arrange
        var data = CreateMultipleRecords();
        var parser = new Iso8211Parser(data);

        // Act & Assert - First record
        Assert.True(parser.Read()); // StartRecord
        Assert.Equal(Iso8211TokenType.StartRecord, parser.TokenType);
        Assert.Equal('L', parser.CurrentLeader.LeaderIdentifier);

        Assert.True(parser.Read()); // DirectoryEntry
        Assert.True(parser.Read()); // Field
        Assert.True(parser.Read()); // EndRecord

        // Second record
        Assert.True(parser.Read()); // StartRecord
        Assert.Equal(Iso8211TokenType.StartRecord, parser.TokenType);
        Assert.Equal('D', parser.CurrentLeader.LeaderIdentifier);

        Assert.True(parser.Read()); // DirectoryEntry
        Assert.True(parser.Read()); // Field
        Assert.True(parser.Read()); // EndRecord

        // End of data
        Assert.False(parser.Read());
        Assert.Equal(Iso8211TokenType.EndOfData, parser.TokenType);
    }

    #endregion

    #region BytesConsumed Tests

    [Fact]
    public void BytesConsumed_IncreasesAsDataIsRead()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Act & Assert
        Assert.Equal(0, parser.BytesConsumed);

        parser.Read(); // StartRecord - consumes leader (24 bytes)
        Assert.Equal(24, parser.BytesConsumed);
    }

    [Fact]
    public void BytesConsumed_MatchesRecordLengthAfterEndRecord()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        var expectedLength = data.Length;

        // Act
        while (parser.Read() && parser.TokenType != Iso8211TokenType.EndRecord)
        {
            // Continue reading
        }

        // Assert
        Assert.Equal(expectedLength, parser.BytesConsumed);
    }

    #endregion

    #region Streaming State Tests

    [Fact]
    public void GetCurrentState_ReturnsValidState()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry

        // Act
        var state = parser.GetCurrentState();

        // Assert - verify we can create a new parser with the state
        var parser2 = new Iso8211Parser(data, isFinalBlock: true, state);
        Assert.Equal(Iso8211ReaderState.InDirectory, parser2.CurrentState);
        Assert.Equal(1, parser2.CurrentDirectoryIndex);
    }

    #endregion

    #region Tag Methods Tests

    [Fact]
    public void TryGetTagString_ReturnsFalseWhenNoTag()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Act
        var result = parser.TryGetTagString(out var tag);

        // Assert
        Assert.False(result);
        Assert.Equal(string.Empty, tag);
    }

    [Fact]
    public void GetTagString_ThrowsWhenNoTag()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Act & Assert
        InvalidOperationException? exception = null;
        try
        {
            _ = parser.GetTagString();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
    }

    [Fact]
    public void CurrentTag_ReturnsRawBytes()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry

        // Act
        var tag = parser.CurrentTag;

        // Assert
        Assert.Equal(4, tag.Length);
        Assert.Equal("0001"u8.ToArray(), tag.ToArray());
    }

    #endregion

    #region Encoding Tests

    [Fact]
    public void GetValueString_UsesSpecifiedEncoding()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Act
        var valueUtf8 = parser.GetValueString(Encoding.UTF8);
        var valueAscii = parser.GetValueString(Encoding.ASCII);

        // Assert
        Assert.Equal("TEST", valueUtf8);
        Assert.Equal("TEST", valueAscii);
    }

    [Fact]
    public void TryGetValueString_UsesSpecifiedEncoding()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field

        // Act
        var result = parser.TryGetValueString(out var value, Encoding.UTF8);

        // Assert
        Assert.True(result);
        Assert.Equal("TEST", value);
    }

    [Fact]
    public void GetSubfieldString_UsesSpecifiedEncoding()
    {
        // Arrange
        var data = CreateSubfieldRecord();
        var parser = new Iso8211Parser(data);
        parser.Read(); // StartRecord
        parser.Read(); // DirectoryEntry
        parser.Read(); // Field
        parser.ReadSubfield();

        // Act
        var value = parser.GetSubfieldString(Encoding.UTF8);

        // Assert
        Assert.Equal("SUB1", value);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Read_InsufficientDataForLeader_NotFinalBlock_ReturnsFalse()
    {
        // Arrange
        var data = new byte[10]; // Less than 24 bytes needed for leader
        var parser = new Iso8211Parser(data, isFinalBlock: false);

        // Act
        var result = parser.Read();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Read_InsufficientDataForLeader_FinalBlock_ReturnsEndOfData()
    {
        // Arrange
        var data = new byte[10]; // Less than 24 bytes needed for leader
        var parser = new Iso8211Parser(data, isFinalBlock: true);

        // Act
        var result = parser.Read();

        // Assert
        Assert.False(result);
        Assert.Equal(Iso8211TokenType.EndOfData, parser.TokenType);
    }

    [Fact]
    public void Read_AfterEndOfData_ContinuesReturningFalse()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);

        // Read to end
        while (parser.Read()) { }

        // Act & Assert
        Assert.False(parser.Read());
        Assert.False(parser.Read());
        Assert.False(parser.Read());
        Assert.Equal(Iso8211TokenType.EndOfData, parser.TokenType);
    }

    [Fact]
    public void CopyValueTo_WhenEmpty_ReturnsZero()
    {
        // Arrange
        var data = CreateMinimalRecord();
        var parser = new Iso8211Parser(data);
        var destination = new byte[10];

        // Act
        var result = parser.CopyValueTo(destination);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion
}
