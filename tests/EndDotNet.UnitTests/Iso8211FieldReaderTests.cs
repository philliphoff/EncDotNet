using System.Collections.Immutable;
using System.Text;
using EncDotNet.Iso8211;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="Iso8211FieldReader"/> and <see cref="Iso8211SubfieldGroup"/>.
/// </summary>
public class Iso8211FieldReaderTests
{
    #region Test Helpers

    /// <summary>
    /// Creates a field definition with the specified subfields.
    /// </summary>
    private static Iso8211FieldDefinition CreateFieldDefinition(
        string tag,
        params (string name, Iso8211SubfieldFormatType formatType, int width, bool isRepeating)[] subfields)
    {
        var definitions = ImmutableArray.CreateBuilder<Iso8211SubfieldDefinition>();
        int repeatStartIndex = -1;

        for (int i = 0; i < subfields.Length; i++)
        {
            var (name, formatType, width, isRepeating) = subfields[i];

            if (isRepeating && repeatStartIndex < 0)
            {
                repeatStartIndex = i;
            }

            definitions.Add(new Iso8211SubfieldDefinition
            {
                Name = name,
                Format = new Iso8211SubfieldFormat
                {
                    FormatType = formatType,
                    Width = width
                },
                Index = i,
                IsRepeating = isRepeating
            });
        }

        return new Iso8211FieldDefinition
        {
            Tag = tag,
            DataStructureCode = Iso8211DataStructureCode.Vector,
            DataTypeCode = Iso8211DataTypeCode.MixedDataTypes,
            FieldName = tag,
            FormatControls = string.Empty,
            SubfieldDefinitions = definitions.ToImmutable(),
            RepeatingSubfieldStartIndex = repeatStartIndex
        };
    }

    /// <summary>
    /// Creates field data with fixed-width binary values.
    /// </summary>
    private static byte[] CreateBinaryFieldData(params byte[] values)
    {
        return values;
    }

    /// <summary>
    /// Creates field data by concatenating multiple sub-arrays.
    /// </summary>
    private static byte[] ConcatFieldData(params byte[][] arrays)
    {
        var totalLength = arrays.Sum(a => a.Length);
        var result = new byte[totalLength];
        var offset = 0;
        foreach (var array in arrays)
        {
            array.CopyTo(result, offset);
            offset += array.Length;
        }
        return result;
    }

    /// <summary>
    /// Creates a UInt16 little-endian byte array.
    /// </summary>
    private static byte[] UInt16LE(ushort value) => BitConverter.IsLittleEndian
        ? BitConverter.GetBytes(value)
        : BitConverter.GetBytes(value).Reverse().ToArray();

    /// <summary>
    /// Creates a UInt32 little-endian byte array.
    /// </summary>
    private static byte[] UInt32LE(uint value) => BitConverter.IsLittleEndian
        ? BitConverter.GetBytes(value)
        : BitConverter.GetBytes(value).Reverse().ToArray();

    /// <summary>
    /// Creates an Int32 little-endian byte array.
    /// </summary>
    private static byte[] Int32LE(int value) => BitConverter.IsLittleEndian
        ? BitConverter.GetBytes(value)
        : BitConverter.GetBytes(value).Reverse().ToArray();

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesReader()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("SF1", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));
        var data = new byte[] { 0x42 };

        // Act
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.NotNull(reader);
        Assert.Same(fieldDef, reader.FieldDefinition);
    }

    [Fact]
    public void Constructor_WithNullFieldDefinition_ThrowsArgumentNullException()
    {
        // Arrange
        var data = new byte[] { 0x42 };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Iso8211FieldReader(null!, data));
    }

    [Fact]
    public void Constructor_WithNullDataArray_ThrowsArgumentNullException()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("SF1", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Iso8211FieldReader(fieldDef, (byte[])null!));
    }

    [Fact]
    public void Constructor_WithSpan_WorksCorrectly()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("SF1", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));
        var data = new byte[] { 0x42 };

        // Act
        var reader = new Iso8211FieldReader(fieldDef, data.AsSpan());

        // Assert
        Assert.Equal(1, reader.SubfieldCount);
    }

    #endregion

    #region GetSubfield Tests - Unsigned Binary

    [Fact]
    public void GetSubfield_UnsignedByte_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));
        var data = new byte[] { 0x64 }; // 100
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<byte>("RCNM");

        // Assert
        Assert.Equal(100, value);
    }

    [Fact]
    public void GetSubfield_UnsignedShort_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("AGEN", Iso8211SubfieldFormatType.UnsignedInteger, 2, false));
        var data = UInt16LE(12345);
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<ushort>("AGEN");

        // Assert
        Assert.Equal(12345, value);
    }

    [Fact]
    public void GetSubfield_UnsignedInt_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("RCID", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = UInt32LE(1234567890);
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<uint>("RCID");

        // Assert
        Assert.Equal(1234567890u, value);
    }

    [Fact]
    public void GetSubfield_UnsignedToInt_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("RCID", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = UInt32LE(1234567890);
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<int>("RCID");

        // Assert
        Assert.Equal(1234567890, value);
    }

    #endregion

    #region GetSubfield Tests - Signed Binary

    [Fact]
    public void GetSubfield_SignedByte_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("VAL", Iso8211SubfieldFormatType.SignedInteger, 1, false));
        var data = new byte[] { 0x9C }; // -100 as signed byte
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<sbyte>("VAL");

        // Assert
        Assert.Equal(-100, value);
    }

    [Fact]
    public void GetSubfield_SignedInt_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, false));
        var data = Int32LE(-123456);
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<int>("YCOO");

        // Assert
        Assert.Equal(-123456, value);
    }

    [Fact]
    public void GetSubfield_SignedPositiveInt_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, false));
        var data = Int32LE(987654321);
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<int>("YCOO");

        // Assert
        Assert.Equal(987654321, value);
    }

    #endregion

    #region GetSubfield Tests - Character Data

    [Fact]
    public void GetSubfield_FixedWidthString_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("DSNM", Iso8211SubfieldFormatType.CharacterData, 8, false));
        var data = Encoding.ASCII.GetBytes("TESTFILE");
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<string>("DSNM");

        // Assert
        Assert.Equal("TESTFILE", value);
    }

    [Fact]
    public void GetSubfield_VariableLengthString_StopsAtUnitTerminator()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("DSNM", Iso8211SubfieldFormatType.CharacterData, 0, false),
            ("EDTN", Iso8211SubfieldFormatType.CharacterData, 0, false));
        
        // Use \u001F instead of \x1F to avoid hex digit consumption (\x1FE would be wrong)
        var data = Encoding.ASCII.GetBytes("FILE1\u001FEDITION1\u001E");
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Verify subfield count matches expectation (should be 2)
        Assert.Equal(2, reader.SubfieldCount);

        // Act
        var dsnm = reader.GetSubfield<string>("DSNM");
        var edtn = reader.GetSubfield<string>("EDTN");

        // Assert
        Assert.Equal("FILE1", dsnm);
        Assert.Equal("EDITION1", edtn);
    }

    [Fact]
    public void GetSubfield_StringWithTrailingSpaces_IsTrimmed()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("NAME", Iso8211SubfieldFormatType.CharacterData, 10, false));
        var data = Encoding.ASCII.GetBytes("CHART     ");
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<string>("NAME");

        // Assert
        Assert.Equal("CHART", value);
    }

    #endregion

    #region GetSubfield Tests - ASCII Integer

    [Fact]
    public void GetSubfield_AsciiInteger_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("INTU", Iso8211SubfieldFormatType.Integer, 5, false));
        var data = Encoding.ASCII.GetBytes("12345");
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<int>("INTU");

        // Assert
        Assert.Equal(12345, value);
    }

    [Fact]
    public void GetSubfield_AsciiIntegerWithLeadingSpaces_ParsesCorrectly()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("NUM", Iso8211SubfieldFormatType.Integer, 10, false));
        var data = Encoding.ASCII.GetBytes("       123");
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<int>("NUM");

        // Assert
        Assert.Equal(123, value);
    }

    #endregion

    #region GetSubfield Tests - ASCII Real

    [Fact]
    public void GetSubfield_AsciiReal_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("LAT", Iso8211SubfieldFormatType.Real, 10, false));
        var data = Encoding.ASCII.GetBytes("   12.3456");
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<double>("LAT");

        // Assert
        Assert.Equal(12.3456, value, precision: 6);
    }

    #endregion

    #region GetSubfield Error Cases

    [Fact]
    public void GetSubfield_NonExistentSubfield_ThrowsKeyNotFoundException()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));
        var data = new byte[] { 0x42 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => reader.GetSubfield<byte>("XXXX"));
    }

    [Fact]
    public void TryGetSubfield_NonExistentSubfield_ReturnsFalse()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));
        var data = new byte[] { 0x42 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var result = reader.TryGetSubfield<byte>("XXXX", out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetSubfield_ExistingSubfield_ReturnsTrue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));
        var data = new byte[] { 0x64 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var result = reader.TryGetSubfield<byte>("RCNM", out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(100, value);
    }

    #endregion

    #region Multiple Subfield Tests

    [Fact]
    public void GetSubfield_MultipleFixedWidthSubfields_ReturnsCorrectValues()
    {
        // Arrange - simulates S-57 DSID field header
        var fieldDef = CreateFieldDefinition("DSID",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("RCID", Iso8211SubfieldFormatType.UnsignedInteger, 4, false),
            ("EXPP", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("INTU", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));

        var data = ConcatFieldData(
            new byte[] { 10 },           // RCNM = 10
            UInt32LE(12345),             // RCID = 12345
            new byte[] { 1 },            // EXPP = 1
            new byte[] { 5 }             // INTU = 5
        );
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var rcnm = reader.GetSubfield<byte>("RCNM");
        var rcid = reader.GetSubfield<uint>("RCID");
        var expp = reader.GetSubfield<byte>("EXPP");
        var intu = reader.GetSubfield<byte>("INTU");

        // Assert
        Assert.Equal(10, rcnm);
        Assert.Equal(12345u, rcid);
        Assert.Equal(1, expp);
        Assert.Equal(5, intu);
    }

    [Fact]
    public void GetSubfield_MixedFixedAndVariableWidth_ReturnsCorrectValues()
    {
        // Arrange - binary + variable-length string
        var fieldDef = CreateFieldDefinition("TEST",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("RCID", Iso8211SubfieldFormatType.UnsignedInteger, 4, false),
            ("DSNM", Iso8211SubfieldFormatType.CharacterData, 0, false)); // variable-length

        var binaryData = ConcatFieldData(
            new byte[] { 20 },           // RCNM = 20
            UInt32LE(999)                // RCID = 999
        );
        var stringData = Encoding.ASCII.GetBytes("US5NY12M\x1E");
        var data = ConcatFieldData(binaryData, stringData);

        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var rcnm = reader.GetSubfield<byte>("RCNM");
        var rcid = reader.GetSubfield<uint>("RCID");
        var dsnm = reader.GetSubfield<string>("DSNM");

        // Assert
        Assert.Equal(20, rcnm);
        Assert.Equal(999u, rcid);
        Assert.Equal("US5NY12M", dsnm);
    }

    #endregion

    #region Repeating Group Tests

    [Fact]
    public void GetSubfieldValues_RepeatingGroup_ReturnsAllValues()
    {
        // Arrange - SG2D style: repeating YCOO/XCOO pairs
        var fieldDef = CreateFieldDefinition("SG2D",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("XCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true));

        var data = ConcatFieldData(
            Int32LE(1000000),    // YCOO[0]
            Int32LE(2000000),    // XCOO[0]
            Int32LE(1000001),    // YCOO[1]
            Int32LE(2000001),    // XCOO[1]
            Int32LE(1000002),    // YCOO[2]
            Int32LE(2000002)     // XCOO[2]
        );

        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var ycoords = reader.GetSubfieldValues<int>("YCOO");
        var xcoords = reader.GetSubfieldValues<int>("XCOO");

        // Assert
        Assert.Equal(3, ycoords.Length);
        Assert.Equal(3, xcoords.Length);

        Assert.Equal(1000000, ycoords[0]);
        Assert.Equal(1000001, ycoords[1]);
        Assert.Equal(1000002, ycoords[2]);

        Assert.Equal(2000000, xcoords[0]);
        Assert.Equal(2000001, xcoords[1]);
        Assert.Equal(2000002, xcoords[2]);
    }

    [Fact]
    public void GroupCount_RepeatingGroup_ReturnsCorrectCount()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("SG2D",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("XCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true));

        var data = ConcatFieldData(
            Int32LE(100), Int32LE(200),
            Int32LE(101), Int32LE(201),
            Int32LE(102), Int32LE(202)
        );

        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.Equal(3, reader.GroupCount);
        Assert.True(reader.HasRepeatingGroups);
    }

    [Fact]
    public void GroupCount_NoRepeatingGroup_ReturnsOne()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("B", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));

        var data = new byte[] { 1, 2 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.Equal(1, reader.GroupCount);
        Assert.False(reader.HasRepeatingGroups);
    }

    [Fact]
    public void GetSubfieldGroups_ReturnsCorrectGroups()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("SG2D",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("XCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true));

        var data = ConcatFieldData(
            Int32LE(100), Int32LE(200),
            Int32LE(101), Int32LE(201)
        );

        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var groups = reader.GetSubfieldGroups().ToArray();

        // Assert
        Assert.Equal(2, groups.Length);

        Assert.Equal(0, groups[0].Index);
        Assert.Equal(100, groups[0].GetSubfield<int>("YCOO"));
        Assert.Equal(200, groups[0].GetSubfield<int>("XCOO"));

        Assert.Equal(1, groups[1].Index);
        Assert.Equal(101, groups[1].GetSubfield<int>("YCOO"));
        Assert.Equal(201, groups[1].GetSubfield<int>("XCOO"));
    }

    [Fact]
    public void GetSubfieldGroups_SingleGroup_ReturnsOneGroup()
    {
        // Arrange - non-repeating field
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("B", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));

        var data = ConcatFieldData(new byte[] { 42 }, UInt32LE(12345));
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var groups = reader.GetSubfieldGroups().ToArray();

        // Assert
        Assert.Single(groups);
        Assert.Equal(0, groups[0].Index);
        Assert.Equal(2, groups[0].Count);
    }

    #endregion

    #region GetSubfieldAt Tests

    [Fact]
    public void GetSubfieldAt_ValidIndex_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("B", Iso8211SubfieldFormatType.UnsignedInteger, 2, false),
            ("C", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));

        var data = ConcatFieldData(
            new byte[] { 1 },
            UInt16LE(2),
            UInt32LE(3)
        );
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var a = reader.GetSubfieldAt<byte>(0);
        var b = reader.GetSubfieldAt<ushort>(1);
        var c = reader.GetSubfieldAt<uint>(2);

        // Assert
        Assert.Equal(1, a);
        Assert.Equal(2, b);
        Assert.Equal(3u, c);
    }

    [Fact]
    public void GetSubfieldAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));
        var data = new byte[] { 1 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetSubfieldAt<byte>(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetSubfieldAt<byte>(1));
    }

    #endregion

    #region GetSubfieldBytes Tests

    [Fact]
    public void GetSubfieldBytes_ReturnsRawBytes()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("DATA", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var bytes = reader.GetSubfieldBytes("DATA");

        // Assert
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0x01, bytes[0]);
        Assert.Equal(0x02, bytes[1]);
        Assert.Equal(0x03, bytes[2]);
        Assert.Equal(0x04, bytes[3]);
    }

    [Fact]
    public void GetSubfieldBytes_NonExistentSubfield_ThrowsKeyNotFoundException()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("DATA", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => reader.GetSubfieldBytes("XXXX"));
    }

    #endregion

    #region Type Conversion Tests

    [Fact]
    public void GetSubfield_BinaryToDouble_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("VAL", Iso8211SubfieldFormatType.SignedInteger, 4, false));
        var data = Int32LE(12345678);
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<double>("VAL");

        // Assert
        Assert.Equal(12345678.0, value);
    }

    [Fact]
    public void GetSubfield_BinaryToString_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("VAL", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = UInt32LE(12345);
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<string>("VAL");

        // Assert
        Assert.Equal("12345", value);
    }

    [Fact]
    public void GetSubfield_CharacterDataToInt_ParsesCorrectly()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("NUM", Iso8211SubfieldFormatType.CharacterData, 5, false));
        var data = Encoding.ASCII.GetBytes("12345");
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var value = reader.GetSubfield<int>("NUM");

        // Assert
        Assert.Equal(12345, value);
    }

    #endregion

    #region SubfieldCount and Data Properties Tests

    [Fact]
    public void SubfieldCount_ReturnsCorrectCount()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("B", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("C", Iso8211SubfieldFormatType.UnsignedInteger, 1, false));

        var data = new byte[] { 1, 2, 3 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.Equal(3, reader.SubfieldCount);
    }

    [Fact]
    public void Data_ReturnsOriginalData()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act
        var returnedData = reader.Data;

        // Assert
        Assert.Equal(data.Length, returnedData.Length);
        Assert.True(data.SequenceEqual(returnedData.ToArray()));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_EmptyData_CreatesReaderWithNoSubfields()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = Array.Empty<byte>();

        // Act
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.Equal(0, reader.SubfieldCount);
    }

    [Fact]
    public void Constructor_EmptyFieldDefinition_CreatesReaderWithNoSubfields()
    {
        // Arrange
        var fieldDef = new Iso8211FieldDefinition
        {
            Tag = "TEST",
            SubfieldDefinitions = ImmutableArray<Iso8211SubfieldDefinition>.Empty
        };
        var data = new byte[] { 1, 2, 3 };

        // Act
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.Equal(0, reader.SubfieldCount);
    }

    [Fact]
    public void GetSubfield_DataShorterThanExpected_ReturnsPartialValue()
    {
        // Arrange - field expects 4 bytes but only 2 are provided
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 4, false));
        var data = new byte[] { 0x01, 0x02 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act - should read what's available
        var value = reader.GetSubfieldAt<ushort>(0);

        // Assert - reads only available bytes
        Assert.Equal(0x0201, value); // Little-endian
    }

    [Fact]
    public void GetSubfieldValues_NonExistentSubfield_ThrowsKeyNotFoundException()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.UnsignedInteger, 1, true));
        var data = new byte[] { 1, 2, 3 };
        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => reader.GetSubfieldValues<byte>("XXXX"));
    }

    #endregion

    #region S-57 Realistic Tests

    [Fact]
    public void ReadRealisticDsidField()
    {
        // Arrange - realistic S-57 DSID field structure
        var fieldDef = CreateFieldDefinition("DSID",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("RCID", Iso8211SubfieldFormatType.UnsignedInteger, 4, false),
            ("EXPP", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("INTU", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),
            ("DSNM", Iso8211SubfieldFormatType.CharacterData, 0, false),
            ("EDTN", Iso8211SubfieldFormatType.CharacterData, 0, false));

        var binaryPart = ConcatFieldData(
            new byte[] { 10 },       // RCNM = Data Set General Information (10)
            UInt32LE(1),             // RCID = 1
            new byte[] { 2 },        // EXPP = 2 (New Edition)
            new byte[] { 5 }         // INTU = 5 (Intended Usage)
        );
        // Use \u001F and \u001E instead of \x1F and \x1E to avoid hex digit consumption
        var stringPart = Encoding.ASCII.GetBytes("US5NY12M\u001F001\u001E");
        var data = ConcatFieldData(binaryPart, stringPart);

        var reader = new Iso8211FieldReader(fieldDef, data);

        // Act & Assert
        Assert.Equal(10, reader.GetSubfield<byte>("RCNM"));
        Assert.Equal(1u, reader.GetSubfield<uint>("RCID"));
        Assert.Equal(2, reader.GetSubfield<byte>("EXPP"));
        Assert.Equal(5, reader.GetSubfield<byte>("INTU"));
        Assert.Equal("US5NY12M", reader.GetSubfield<string>("DSNM"));
        Assert.Equal("001", reader.GetSubfield<string>("EDTN"));
    }

    [Fact]
    public void ReadRealisticSg2dField()
    {
        // Arrange - S-57 SG2D (2D coordinate) field with 3 coordinate pairs
        var fieldDef = CreateFieldDefinition("SG2D",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("XCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true));

        // NYC coordinates scaled (approximately 40.7128°N, 74.0060°W)
        var data = ConcatFieldData(
            Int32LE(407128000),   // YCOO[0] - latitude * 10^7
            Int32LE(-740060000),  // XCOO[0] - longitude * 10^7
            Int32LE(407130000),   // YCOO[1]
            Int32LE(-740062000),  // XCOO[1]
            Int32LE(407125000),   // YCOO[2]
            Int32LE(-740058000)   // XCOO[2]
        );

        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.Equal(6, reader.SubfieldCount); // 3 pairs
        Assert.Equal(3, reader.GroupCount);

        var ycoords = reader.GetSubfieldValues<int>("YCOO");
        var xcoords = reader.GetSubfieldValues<int>("XCOO");

        Assert.Equal(3, ycoords.Length);
        Assert.Equal(3, xcoords.Length);

        Assert.Equal(407128000, ycoords[0]);
        Assert.Equal(-740060000, xcoords[0]);
    }

    [Fact]
    public void ReadRealisticAttfField()
    {
        // Arrange - S-57 ATTF (Feature Attribute) field with variable-length strings
        var fieldDef = CreateFieldDefinition("ATTF",
            ("ATTL", Iso8211SubfieldFormatType.UnsignedInteger, 2, true),
            ("ATVL", Iso8211SubfieldFormatType.CharacterData, 0, true));

        // Two attribute pairs
        var data = ConcatFieldData(
            UInt16LE(100),  // ATTL[0] = attribute label 100
            Encoding.ASCII.GetBytes("PORT\x1F"),
            UInt16LE(101),  // ATTL[1] = attribute label 101
            Encoding.ASCII.GetBytes("NEW YORK\x1E")
        );

        var reader = new Iso8211FieldReader(fieldDef, data);

        // Assert
        Assert.Equal(4, reader.SubfieldCount); // 2 pairs
        Assert.Equal(2, reader.GroupCount);

        var attlValues = reader.GetSubfieldValues<ushort>("ATTL");
        var atvlValues = reader.GetSubfieldValues<string>("ATVL");

        Assert.Equal(2, attlValues.Length);
        Assert.Equal(2, atvlValues.Length);

        Assert.Equal(100, attlValues[0]);
        Assert.Equal("PORT", atvlValues[0]);

        Assert.Equal(101, attlValues[1]);
        Assert.Equal("NEW YORK", atvlValues[1]);
    }

    #endregion

    #region Iso8211SubfieldGroup Tests

    [Fact]
    public void SubfieldGroup_GetSubfieldAt_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("SG2D",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("XCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true));

        var data = ConcatFieldData(
            Int32LE(100), Int32LE(200),
            Int32LE(101), Int32LE(201)
        );

        var reader = new Iso8211FieldReader(fieldDef, data);
        var groups = reader.GetSubfieldGroups().ToArray();

        // Act
        var y0 = groups[0].GetSubfieldAt<int>(0);
        var x0 = groups[0].GetSubfieldAt<int>(1);
        var y1 = groups[1].GetSubfieldAt<int>(0);
        var x1 = groups[1].GetSubfieldAt<int>(1);

        // Assert
        Assert.Equal(100, y0);
        Assert.Equal(200, x0);
        Assert.Equal(101, y1);
        Assert.Equal(201, x1);
    }

    [Fact]
    public void SubfieldGroup_GetSubfieldAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("SG2D",
            ("YCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("XCOO", Iso8211SubfieldFormatType.SignedInteger, 4, true));

        var data = ConcatFieldData(Int32LE(100), Int32LE(200));
        var reader = new Iso8211FieldReader(fieldDef, data);
        var group = reader.GetSubfieldGroups().First();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => group.GetSubfieldAt<int>(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => group.GetSubfieldAt<int>(2));
    }

    [Fact]
    public void SubfieldGroup_Count_ReturnsCorrectValue()
    {
        // Arrange
        var fieldDef = CreateFieldDefinition("TEST",
            ("A", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("B", Iso8211SubfieldFormatType.SignedInteger, 4, true),
            ("C", Iso8211SubfieldFormatType.SignedInteger, 4, true));

        var data = ConcatFieldData(Int32LE(1), Int32LE(2), Int32LE(3));
        var reader = new Iso8211FieldReader(fieldDef, data);
        var group = reader.GetSubfieldGroups().First();

        // Assert
        Assert.Equal(3, group.Count);
    }

    [Fact]
    public void SubfieldGroup_GetSubfield_WithNonRepeatingPrefix_ReturnsCorrectValues()
    {
        // Arrange - field with non-repeating prefix subfields followed by a repeating group
        // This simulates patterns like FSPT: RCNM(fixed) + NAME(repeating) + ORNT(repeating) + USAG(repeating) + MASK(repeating)
        var fieldDef = CreateFieldDefinition("FSPT",
            ("RCNM", Iso8211SubfieldFormatType.UnsignedInteger, 1, false),  // non-repeating prefix
            ("RCID", Iso8211SubfieldFormatType.UnsignedInteger, 4, false),  // non-repeating prefix
            ("ORNT", Iso8211SubfieldFormatType.UnsignedInteger, 1, true),   // repeating group start
            ("USAG", Iso8211SubfieldFormatType.UnsignedInteger, 1, true),
            ("MASK", Iso8211SubfieldFormatType.UnsignedInteger, 1, true));

        var data = ConcatFieldData(
            new byte[] { 130 },      // RCNM = 130 (Edge)
            UInt32LE(42),            // RCID = 42
            new byte[] { 1 },        // ORNT[0] = 1 (Forward)
            new byte[] { 1 },        // USAG[0] = 1 (Exterior)
            new byte[] { 2 },        // MASK[0] = 2 (Show)
            new byte[] { 2 },        // ORNT[1] = 2 (Reverse)
            new byte[] { 2 },        // USAG[1] = 2 (Interior)
            new byte[] { 1 }         // MASK[1] = 1 (Mask)
        );

        var reader = new Iso8211FieldReader(fieldDef, data);
        var groups = reader.GetSubfieldGroups().ToArray();

        // Assert - should have the non-repeating prefix as group 0, then 2 repeating groups
        // The prefix subfields should be accessible and repeating groups should correctly resolve subfield names
        Assert.True(groups.Length >= 2);

        // Verify repeating group subfields are correctly resolved by name
        // Groups after the prefix should map ORNT/USAG/MASK correctly
        var repeatingGroups = groups.Where(g => g.Count == 3).ToArray();
        Assert.Equal(2, repeatingGroups.Length);

        Assert.Equal(1, repeatingGroups[0].GetSubfield<byte>("ORNT"));
        Assert.Equal(1, repeatingGroups[0].GetSubfield<byte>("USAG"));
        Assert.Equal(2, repeatingGroups[0].GetSubfield<byte>("MASK"));

        Assert.Equal(2, repeatingGroups[1].GetSubfield<byte>("ORNT"));
        Assert.Equal(2, repeatingGroups[1].GetSubfield<byte>("USAG"));
        Assert.Equal(1, repeatingGroups[1].GetSubfield<byte>("MASK"));
    }

    [Fact]
    public void SubfieldGroup_GetSubfieldBytes_WithNonRepeatingPrefix_ReturnsCorrectBytes()
    {
        // Arrange - same pattern: non-repeating prefix + repeating group
        var fieldDef = CreateFieldDefinition("TEST",
            ("HDR", Iso8211SubfieldFormatType.UnsignedInteger, 2, false),   // non-repeating prefix
            ("VAL", Iso8211SubfieldFormatType.UnsignedInteger, 4, true),    // repeating group start
            ("FLG", Iso8211SubfieldFormatType.UnsignedInteger, 1, true));

        var data = ConcatFieldData(
            UInt16LE(0xBEEF),        // HDR
            UInt32LE(100),           // VAL[0]
            new byte[] { 0xAA },     // FLG[0]
            UInt32LE(200),           // VAL[1]
            new byte[] { 0xBB }      // FLG[1]
        );

        var reader = new Iso8211FieldReader(fieldDef, data);
        var groups = reader.GetSubfieldGroups().ToArray();

        var repeatingGroups = groups.Where(g => g.Count == 2).ToArray();
        Assert.Equal(2, repeatingGroups.Length);

        // Act & Assert - verify GetSubfieldBytes resolves correctly in repeating groups
        var val0Bytes = repeatingGroups[0].GetSubfieldBytes("VAL");
        Assert.Equal(4, val0Bytes.Length);
        Assert.Equal(UInt32LE(100), val0Bytes.ToArray());

        var flg0Bytes = repeatingGroups[0].GetSubfieldBytes("FLG");
        Assert.Equal(1, flg0Bytes.Length);
        Assert.Equal(0xAA, flg0Bytes[0]);

        var val1Bytes = repeatingGroups[1].GetSubfieldBytes("VAL");
        Assert.Equal(UInt32LE(200), val1Bytes.ToArray());

        var flg1Bytes = repeatingGroups[1].GetSubfieldBytes("FLG");
        Assert.Equal(0xBB, flg1Bytes[0]);
    }

    [Fact]
    public void GetSubfieldGroups_Ucs2NatfField_ParsesCorrectly()
    {
        // Arrange - simulates a NATF field with lexical level 2 (UCS-2/UTF-16LE)
        // ATTL(b12) + ATVL(A) repeating, with UCS-2 encoded string and 2-byte terminators
        var fieldDef = CreateFieldDefinition("NATF",
            ("ATTL", Iso8211SubfieldFormatType.UnsignedInteger, 2, true),
            ("ATVL", Iso8211SubfieldFormatType.CharacterData, 0, true));

        // Real data from US5KFMCE chart: ATTL=0x012C (300), ATVL="SAM" in UTF-16LE
        // 2C 01  53 00 41 00 4D 00  1F 00  1E
        var data = new byte[]
        {
            0x2C, 0x01,                         // ATTL = 300
            0x53, 0x00, 0x41, 0x00, 0x4D, 0x00, // "SAM" in UTF-16LE
            0x1F, 0x00,                         // UCS-2 unit terminator
            0x1E                                // field terminator
        };

        var reader = new Iso8211FieldReader(fieldDef, data, lexicalLevel: 2);

        // Act
        var groups = reader.GetSubfieldGroups().ToArray();

        // Assert
        Assert.Single(groups);
        Assert.Equal(300, groups[0].GetSubfield<ushort>("ATTL"));
        Assert.Equal("SAM", groups[0].GetSubfield<string>("ATVL"));
    }

    [Fact]
    public void GetSubfieldGroups_Ucs2NatfFieldMultipleGroups_ParsesCorrectly()
    {
        // Arrange - two NATF attribute groups in UCS-2
        var fieldDef = CreateFieldDefinition("NATF",
            ("ATTL", Iso8211SubfieldFormatType.UnsignedInteger, 2, true),
            ("ATVL", Iso8211SubfieldFormatType.CharacterData, 0, true));

        var data = new byte[]
        {
            0x2C, 0x01,                                     // ATTL[0] = 300
            0x53, 0x00, 0x41, 0x00, 0x4D, 0x00,             // "SAM" in UTF-16LE
            0x1F, 0x00,                                     // UCS-2 unit terminator
            0x64, 0x00,                                     // ATTL[1] = 100
            0x42, 0x00, 0x4F, 0x00, 0x42, 0x00,             // "BOB" in UTF-16LE
            0x1F, 0x00,                                     // UCS-2 unit terminator
            0x1E                                            // field terminator
        };

        var reader = new Iso8211FieldReader(fieldDef, data, lexicalLevel: 2);

        // Act
        var groups = reader.GetSubfieldGroups().ToArray();

        // Assert
        Assert.Equal(2, groups.Length);

        Assert.Equal(300, groups[0].GetSubfield<ushort>("ATTL"));
        Assert.Equal("SAM", groups[0].GetSubfield<string>("ATVL"));

        Assert.Equal(100, groups[1].GetSubfield<ushort>("ATTL"));
        Assert.Equal("BOB", groups[1].GetSubfield<string>("ATVL"));
    }

    #endregion
}
