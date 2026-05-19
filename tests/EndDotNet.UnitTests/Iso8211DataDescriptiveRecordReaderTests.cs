using System.Collections.Immutable;
using System.Text;
using EncDotNet.Iso8211;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="Iso8211DataDescriptiveRecord"/>, <see cref="Iso8211DataDescriptiveRecordReader"/>,
/// and related DDR types.
/// </summary>
public class Iso8211DataDescriptiveRecordReaderTests
{
    #region Test Data Helpers

    /// <summary>
    /// Creates a DDR record with a single field definition.
    /// </summary>
    /// <param name="tag">The field tag (4 characters).</param>
    /// <param name="fieldControlLength">The field control length to set in the leader.</param>
    /// <param name="dataStructureCode">The data structure code character.</param>
    /// <param name="dataTypeCode">The data type code character.</param>
    /// <param name="fieldName">The field name.</param>
    /// <param name="subfieldNames">Subfield names separated by '!'.</param>
    /// <param name="formatControls">The format controls string (e.g., "(A,I(10),b11)").</param>
    private static Iso8211Record CreateDdrRecord(
        string tag = "DSID",
        int fieldControlLength = 2,
        char dataStructureCode = '1',
        char dataTypeCode = '6',
        string fieldName = "Data Set Identification",
        string? subfieldNames = "RCNM!RCID!EXPP",
        string formatControls = "(b11,b14,b11)")
    {
        // Build the field data: [controls][descriptors]\x1F[formats]
        var sb = new StringBuilder();

        if (fieldControlLength >= 2)
        {
            sb.Append(dataStructureCode);
            sb.Append(dataTypeCode);
        }

        // Descriptors: FieldName!SF1!SF2!...
        sb.Append(fieldName);
        if (!string.IsNullOrEmpty(subfieldNames))
        {
            sb.Append('!');
            sb.Append(subfieldNames);
        }

        sb.Append('\x1F'); // Unit terminator
        sb.Append(formatControls);

        var fieldData = Encoding.ASCII.GetBytes(sb.ToString());

        var field = new Iso8211Field
        {
            Tag = tag,
            Data = fieldData
        };

        return new Iso8211Record
        {
            Leader = new Iso8211RecordLeader
            {
                LeaderIdentifier = 'L',
                FieldControlLength = fieldControlLength,
                SizeOfFieldTagField = 4,
                SizeOfFieldLengthField = 3,
                SizeOfFieldPositionField = 3,
                RecordLength = 100,
                BaseAddressOfFieldArea = 35,
                InterchangeLevel = '3',
                VersionNumber = '1',
                ExtendedCharacterSetIndicator = "   "
            },
            Directory = ImmutableArray.Create(new Iso8211DirectoryEntry
            {
                Tag = tag,
                Length = fieldData.Length + 1,
                Position = 0
            }),
            Fields = ImmutableArray.Create(field)
        };
    }

    /// <summary>
    /// Creates a DDR record with multiple field definitions.
    /// </summary>
    private static Iso8211Record CreateMultiFieldDdrRecord()
    {
        var fields = new[]
        {
            CreateFieldData("0000", 2, '0', '0', "Record Directory Entry", null, "(A)"),
            CreateFieldData("DSID", 2, '1', '6', "Data Set Identification", "RCNM!RCID!EXPP!INTU!DSNM!EDTN", "(b11,b14,b11,b11,A,A)"),
            CreateFieldData("DSPM", 2, '1', '6', "Data Set Parameters", "RCNM!RCID!HDAT!VDAT!SDAT!CSCL!COMF!SOMF", "(b11,b14,b11,b11,b11,b14,b14,b14)"),
            CreateFieldData("SG2D", 2, '1', '6', "2D Coordinate", "*YCOO!XCOO", "(2b24)")
        };

        var isoFields = ImmutableArray.CreateBuilder<Iso8211Field>();
        var dirEntries = ImmutableArray.CreateBuilder<Iso8211DirectoryEntry>();

        foreach (var (tag, data) in fields)
        {
            isoFields.Add(new Iso8211Field
            {
                Tag = tag,
                Data = data
            });
            dirEntries.Add(new Iso8211DirectoryEntry
            {
                Tag = tag,
                Length = data.Length + 1,
                Position = 0
            });
        }

        return new Iso8211Record
        {
            Leader = new Iso8211RecordLeader
            {
                LeaderIdentifier = 'L',
                FieldControlLength = 2,
                SizeOfFieldTagField = 4,
                SizeOfFieldLengthField = 3,
                SizeOfFieldPositionField = 3,
                RecordLength = 500,
                BaseAddressOfFieldArea = 35,
                InterchangeLevel = '3',
                VersionNumber = '1',
                ExtendedCharacterSetIndicator = "   "
            },
            Directory = dirEntries.ToImmutable(),
            Fields = isoFields.ToImmutable()
        };
    }

    private static (string Tag, byte[] Data) CreateFieldData(
        string tag, int fieldControlLength, char dataStructureCode, char dataTypeCode,
        string fieldName, string? subfieldNames, string formatControls)
    {
        var sb = new StringBuilder();

        if (fieldControlLength >= 2)
        {
            sb.Append(dataStructureCode);
            sb.Append(dataTypeCode);
        }

        sb.Append(fieldName);
        if (!string.IsNullOrEmpty(subfieldNames))
        {
            sb.Append('!');
            sb.Append(subfieldNames);
        }

        sb.Append('\x1F');
        sb.Append(formatControls);

        return (tag, Encoding.ASCII.GetBytes(sb.ToString()));
    }

    /// <summary>
    /// Creates a non-DDR record for negative testing.
    /// </summary>
    private static Iso8211Record CreateDataRecord()
    {
        return new Iso8211Record
        {
            Leader = new Iso8211RecordLeader
            {
                LeaderIdentifier = 'D',
                FieldControlLength = 0
            },
            Directory = ImmutableArray<Iso8211DirectoryEntry>.Empty,
            Fields = ImmutableArray<Iso8211Field>.Empty
        };
    }

    #endregion

    #region Parse Tests

    [Fact]
    public void Parse_ValidDdr_ReturnsDataDescriptiveRecord()
    {
        // Arrange
        var record = CreateDdrRecord();

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.NotNull(ddr);
        Assert.Single(ddr.FieldDefinitions);
    }

    [Fact]
    public void Parse_NonDdrRecord_ThrowsArgumentException()
    {
        // Arrange
        var record = CreateDataRecord();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Iso8211DataDescriptiveRecordReader.Read(record));
    }

    [Fact]
    public void Parse_MultiFieldDdr_ParsesAllFields()
    {
        // Arrange
        var record = CreateMultiFieldDdrRecord();

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal(4, ddr.FieldDefinitions.Count);
    }

    #endregion

    #region Field Definition Tests

    [Fact]
    public void FieldDefinition_Tag_IsCorrect()
    {
        // Arrange
        var record = CreateDdrRecord(tag: "DSID");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal("DSID", ddr.FieldDefinitions[0].Tag);
    }

    [Fact]
    public void FieldDefinition_DataStructureCode_IsParsed()
    {
        // Arrange
        var record = CreateDdrRecord(dataStructureCode: '1');

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal(Iso8211DataStructureCode.Vector, ddr.FieldDefinitions[0].DataStructureCode);
    }

    [Theory]
    [InlineData('0', Iso8211DataStructureCode.Elementary)]
    [InlineData('1', Iso8211DataStructureCode.Vector)]
    [InlineData('2', Iso8211DataStructureCode.Array)]
    [InlineData('3', Iso8211DataStructureCode.ConcatenatedArray)]
    public void FieldDefinition_DataStructureCode_AllValues(char code, Iso8211DataStructureCode expected)
    {
        // Arrange
        var record = CreateDdrRecord(dataStructureCode: code);

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal(expected, ddr.FieldDefinitions[0].DataStructureCode);
    }

    [Fact]
    public void FieldDefinition_DataTypeCode_IsParsed()
    {
        // Arrange
        var record = CreateDdrRecord(dataTypeCode: '6');

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal(Iso8211DataTypeCode.MixedDataTypes, ddr.FieldDefinitions[0].DataTypeCode);
    }

    [Theory]
    [InlineData('0', Iso8211DataTypeCode.CharacterString)]
    [InlineData('1', Iso8211DataTypeCode.ImplicitPoint)]
    [InlineData('2', Iso8211DataTypeCode.ExplicitPoint)]
    [InlineData('5', Iso8211DataTypeCode.Binary)]
    [InlineData('6', Iso8211DataTypeCode.MixedDataTypes)]
    public void FieldDefinition_DataTypeCode_AllValues(char code, Iso8211DataTypeCode expected)
    {
        // Arrange
        var record = CreateDdrRecord(dataTypeCode: code);

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal(expected, ddr.FieldDefinitions[0].DataTypeCode);
    }

    [Fact]
    public void FieldDefinition_FieldName_IsParsed()
    {
        // Arrange
        var record = CreateDdrRecord(fieldName: "Data Set Identification");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal("Data Set Identification", ddr.FieldDefinitions[0].FieldName);
    }

    [Fact]
    public void FieldDefinition_FormatControls_IsParsed()
    {
        // Arrange
        var record = CreateDdrRecord(formatControls: "(b11,b14,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal("(b11,b14,b11)", ddr.FieldDefinitions[0].FormatControls);
    }

    [Fact]
    public void FieldDefinition_SubfieldDefinitions_AreCreated()
    {
        // Arrange
        //var record = CreateDdrRecord(subfieldNames: "RCNM!RCID!EXPP", formatControls: "(b11,b14,b11)");
        var record = CreateDdrRecord(subfieldNames: "RCNM!RCID!EXPP!INTU!DSNM!EDTN!UPDN!UADT!ISDT!STED!PRSP!PSDN!PRED!PROF!AGEN!COMT", formatControls: "(b11,b14,2b11,3A,2A(8),R(4),b11,2A,b11,b12,A)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Equal(16, ddr.FieldDefinitions[0].SubfieldDefinitions.Count);
    }

    [Fact]
    public void FieldDefinition_GetFieldDefinition_ByTag()
    {
        // Arrange
        var record = CreateMultiFieldDdrRecord();
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Act
        var dsid = ddr.GetFieldDefinition("DSID");
        var dspm = ddr.GetFieldDefinition("DSPM");
        var missing = ddr.GetFieldDefinition("XXXX");

        // Assert
        Assert.NotNull(dsid);
        Assert.Equal("DSID", dsid.Tag);
        Assert.NotNull(dspm);
        Assert.Equal("DSPM", dspm.Tag);
        Assert.Null(missing);
    }

    #endregion

    #region Subfield Definition Tests

    [Fact]
    public void SubfieldDefinition_Name_IsCorrect()
    {
        // Arrange
        var record = CreateDdrRecord(subfieldNames: "RCNM!RCID!EXPP", formatControls: "(b11,b14,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var subfields = ddr.FieldDefinitions[0].SubfieldDefinitions;

        // Assert
        Assert.Equal("RCNM", subfields[0].Name);
        Assert.Equal("RCID", subfields[1].Name);
        Assert.Equal("EXPP", subfields[2].Name);
    }

    [Fact]
    public void SubfieldDefinition_Index_IsCorrect()
    {
        // Arrange
        var record = CreateDdrRecord(subfieldNames: "RCNM!RCID!EXPP", formatControls: "(b11,b14,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var subfields = ddr.FieldDefinitions[0].SubfieldDefinitions;

        // Assert
        Assert.Equal(0, subfields[0].Index);
        Assert.Equal(1, subfields[1].Index);
        Assert.Equal(2, subfields[2].Index);
    }

    [Fact]
    public void SubfieldDefinition_Format_IsPairedCorrectly()
    {
        // Arrange
        var record = CreateDdrRecord(subfieldNames: "RCNM!RCID!EXPP", formatControls: "(b11,b14,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var subfields = ddr.FieldDefinitions[0].SubfieldDefinitions;

        // Assert
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[0].Format.FormatType);
        Assert.Equal(1, subfields[0].Format.Width);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[1].Format.FormatType);
        Assert.Equal(4, subfields[1].Format.Width);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[2].Format.FormatType);
        Assert.Equal(1, subfields[2].Format.Width);
    }

    [Fact]
    public void SubfieldDefinition_GetByName_ReturnsCorrectDefinition()
    {
        // Arrange
        var record = CreateDdrRecord(subfieldNames: "RCNM!RCID!EXPP", formatControls: "(b11,b14,b11)");
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];

        // Act
        var rcid = fieldDef.GetSubfieldDefinition("RCID");
        var missing = fieldDef.GetSubfieldDefinition("XXXX");

        // Assert
        Assert.NotNull(rcid);
        Assert.Equal("RCID", rcid.Name);
        Assert.Equal(1, rcid.Index);
        Assert.Null(missing);
    }

    [Fact]
    public void SubfieldDefinition_WithRepeatingGroupMarker_ParsesCorrectly()
    {
        // Arrange — "*YCOO!XCOO" means YCOO and XCOO repeat as a group
        var record = CreateDdrRecord(
            tag: "SG2D",
            fieldName: "2D Coordinate",
            subfieldNames: "*YCOO!XCOO",
            formatControls: "(2b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];
        var subfields = fieldDef.SubfieldDefinitions;

        // Assert — the '*' should be stripped from the name
        Assert.Equal(2, subfields.Count);
        Assert.Equal("YCOO", subfields[0].Name);
        Assert.Equal("XCOO", subfields[1].Name);

        // Assert — repeating group is detected
        Assert.True(fieldDef.HasRepeatingGroup);
        Assert.Equal(0, fieldDef.RepeatingSubfieldStartIndex);
        Assert.True(subfields[0].IsRepeating);
        Assert.True(subfields[1].IsRepeating);

        // Assert — format repeat count expanded correctly
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, subfields[0].Format.FormatType);
        Assert.Equal(4, subfields[0].Format.Width);
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, subfields[1].Format.FormatType);
        Assert.Equal(4, subfields[1].Format.Width);
    }

    #endregion

    #region Format Controls Parsing Tests

    [Fact]
    public void ParseFormatControls_SingleCharacterFormat()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(A)");

        // Assert
        Assert.Single(formats);
        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, formats[0].FormatType);
        Assert.Equal(0, formats[0].Width);
        Assert.True(formats[0].IsVariableLength);
    }

    [Fact]
    public void ParseFormatControls_CharacterFormatWithWidth()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(A(20))");

        // Assert
        Assert.Single(formats);
        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, formats[0].FormatType);
        Assert.Equal(20, formats[0].Width);
        Assert.True(formats[0].IsFixedWidth);
    }

    [Fact]
    public void ParseFormatControls_IntegerFormat()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(I(10))");

        // Assert
        Assert.Single(formats);
        Assert.Equal(Iso8211SubfieldFormatType.Integer, formats[0].FormatType);
        Assert.Equal(10, formats[0].Width);
    }

    [Fact]
    public void ParseFormatControls_RealFormat()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(R)");

        // Assert
        Assert.Single(formats);
        Assert.Equal(Iso8211SubfieldFormatType.Real, formats[0].FormatType);
        Assert.Equal(0, formats[0].Width);
    }

    [Fact]
    public void ParseFormatControls_UnsignedBinaryFormats()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(b11,b12,b14)");

        // Assert
        Assert.Equal(3, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);
        Assert.Equal(1, formats[0].ByteSize);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[1].FormatType);
        Assert.Equal(2, formats[1].Width);
        Assert.Equal(2, formats[1].ByteSize);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[2].FormatType);
        Assert.Equal(4, formats[2].Width);
        Assert.Equal(4, formats[2].ByteSize);
    }

    [Fact]
    public void ParseFormatControls_SignedBinaryFormats()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(b21,b22,b24)");

        // Assert
        Assert.Equal(3, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);

        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[1].FormatType);
        Assert.Equal(2, formats[1].Width);

        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[2].FormatType);
        Assert.Equal(4, formats[2].Width);
    }

    [Fact]
    public void ParseFormatControls_MixedFormats()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(b11,b14,b11,b11,A,A)");

        // Assert
        Assert.Equal(6, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[1].FormatType);
        Assert.Equal(4, formats[1].Width);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[2].FormatType);
        Assert.Equal(1, formats[2].Width);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[3].FormatType);
        Assert.Equal(1, formats[3].Width);

        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, formats[4].FormatType);
        Assert.Equal(0, formats[4].Width);

        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, formats[5].FormatType);
        Assert.Equal(0, formats[5].Width);
    }

    [Fact]
    public void ParseFormatControls_EmptyString_ReturnsEmpty()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("");

        // Assert
        Assert.Empty(formats);
    }

    [Fact]
    public void ParseFormatControls_EmptyParentheses_ReturnsEmpty()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("()");

        // Assert
        Assert.Empty(formats);
    }

    [Fact]
    public void ParseFormatControls_WithoutParentheses_StillParses()
    {
        // Act
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("b11,b14");

        // Assert
        Assert.Equal(2, formats.Length);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[1].FormatType);
        Assert.Equal(4, formats[1].Width);
    }

    #endregion

    #region SubfieldFormat Tests

    [Fact]
    public void SubfieldFormat_IsFixedWidth_TrueForNonZeroWidth()
    {
        var format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 10 };
        Assert.True(format.IsFixedWidth);
        Assert.False(format.IsVariableLength);
    }

    [Fact]
    public void SubfieldFormat_IsVariableLength_TrueForZeroWidth()
    {
        var format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 0 };
        Assert.True(format.IsVariableLength);
        Assert.False(format.IsFixedWidth);
    }

    [Fact]
    public void SubfieldFormat_ByteSize_CorrectForBinaryTypes()
    {
        var b11 = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 1 };
        var b12 = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 2 };
        var b14 = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 4 };
        var b24 = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.SignedInteger, Width = 4 };

        Assert.Equal(1, b11.ByteSize);
        Assert.Equal(2, b12.ByteSize);
        Assert.Equal(4, b14.ByteSize);
        Assert.Equal(4, b24.ByteSize);
    }

    [Fact]
    public void SubfieldFormat_ToString_FormatsCorrectly()
    {
        Assert.Equal("A", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 0 }.ToString());
        Assert.Equal("A(20)", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 20 }.ToString());
        Assert.Equal("I(10)", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.Integer, Width = 10 }.ToString());
        Assert.Equal("R", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.Real, Width = 0 }.ToString());
        Assert.Equal("b11", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 1 }.ToString());
        Assert.Equal("b12", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 2 }.ToString());
        Assert.Equal("b14", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 4 }.ToString());
        Assert.Equal("b21", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.SignedInteger, Width = 1 }.ToString());
        Assert.Equal("b24", new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.SignedInteger, Width = 4 }.ToString());
    }

    [Fact]
    public void SubfieldFormat_Equality_WorksCorrectly()
    {
        var a = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 4 };
        var b = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 4 };
        var c = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.SignedInteger, Width = 4 };
        var d = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 2 };

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.NotEqual(a, c);
        Assert.True(a != c);
        Assert.NotEqual(a, d);
        Assert.True(a != d);
    }

    [Fact]
    public void SubfieldFormat_GetHashCode_EqualForEqualValues()
    {
        var a = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 4 };
        var b = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 4 };

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    #endregion

    #region Realistic S-57 DDR Field Tests

    [Fact]
    public void Parse_S57DsidField_ParsesCorrectly()
    {
        // Arrange — simulate a real S-57 DSID DDR field
        var record = CreateDdrRecord(
            tag: "DSID",
            dataStructureCode: '1',
            dataTypeCode: '6',
            fieldName: "Data Set Identification",
            subfieldNames: "RCNM!RCID!EXPP!INTU!DSNM!EDTN!UPDN!UADT!ISDT!STED!PRSP!PSDN!PRED!PROF!AGEN!COMT",
            formatControls: "(b11,b14,b11,b11,A,A,A,A,A,A,b11,A,A,b11,b12,A)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var dsid = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal("DSID", dsid.Tag);
        Assert.Equal(Iso8211DataStructureCode.Vector, dsid.DataStructureCode);
        Assert.Equal(Iso8211DataTypeCode.MixedDataTypes, dsid.DataTypeCode);
        Assert.Equal("Data Set Identification", dsid.FieldName);
        Assert.Equal(16, dsid.SubfieldDefinitions.Count);

        // Assert — Vector field implicitly repeats all subfields (no explicit '*' needed)
        Assert.True(dsid.HasRepeatingGroup);
        Assert.Equal(0, dsid.RepeatingSubfieldStartIndex);
        Assert.All(dsid.SubfieldDefinitions, sf => Assert.True(sf.IsRepeating));

        // Verify specific subfields
        var rcnm = dsid.GetSubfieldDefinition("RCNM")!;
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, rcnm.Format.FormatType);
        Assert.Equal(1, rcnm.Format.Width);

        var rcid = dsid.GetSubfieldDefinition("RCID")!;
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, rcid.Format.FormatType);
        Assert.Equal(4, rcid.Format.Width);

        var dsnm = dsid.GetSubfieldDefinition("DSNM")!;
        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, dsnm.Format.FormatType);
        Assert.True(dsnm.Format.IsVariableLength);

        var agen = dsid.GetSubfieldDefinition("AGEN")!;
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, agen.Format.FormatType);
        Assert.Equal(2, agen.Format.Width);
    }

    [Fact]
    public void Parse_S57SG2DField_ParsesRepeatingGroup()
    {
        // Arrange — SG2D has repeating YCOO/XCOO pairs with repeat count format
        var record = CreateDdrRecord(
            tag: "SG2D",
            dataStructureCode: '1',
            dataTypeCode: '5',
            fieldName: "2-D Coordinate Field",
            subfieldNames: "*YCOO!XCOO",
            formatControls: "(2b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var sg2d = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal("SG2D", sg2d.Tag);
        Assert.Equal(Iso8211DataStructureCode.Vector, sg2d.DataStructureCode);
        Assert.Equal(Iso8211DataTypeCode.Binary, sg2d.DataTypeCode);
        Assert.Equal(2, sg2d.SubfieldDefinitions.Count);

        Assert.Equal("YCOO", sg2d.SubfieldDefinitions[0].Name);
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, sg2d.SubfieldDefinitions[0].Format.FormatType);
        Assert.Equal(4, sg2d.SubfieldDefinitions[0].Format.Width);

        Assert.Equal("XCOO", sg2d.SubfieldDefinitions[1].Name);
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, sg2d.SubfieldDefinitions[1].Format.FormatType);
        Assert.Equal(4, sg2d.SubfieldDefinitions[1].Format.Width);

        // Assert repeating group
        Assert.True(sg2d.HasRepeatingGroup);
        Assert.Equal(0, sg2d.RepeatingSubfieldStartIndex);
        Assert.True(sg2d.SubfieldDefinitions[0].IsRepeating);
        Assert.True(sg2d.SubfieldDefinitions[1].IsRepeating);
    }

    [Fact]
    public void Parse_S57AttfField_ParsesMixedTypes()
    {
        // Arrange — ATTF has ATTL(b12) + ATVL(A)
        var record = CreateDdrRecord(
            tag: "ATTF",
            dataStructureCode: '1',
            dataTypeCode: '6',
            fieldName: "Feature Record Attribute",
            subfieldNames: "*ATTL!ATVL",
            formatControls: "(b12,A)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var attf = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal(2, attf.SubfieldDefinitions.Count);

        Assert.Equal("ATTL", attf.SubfieldDefinitions[0].Name);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, attf.SubfieldDefinitions[0].Format.FormatType);
        Assert.Equal(2, attf.SubfieldDefinitions[0].Format.Width);

        Assert.Equal("ATVL", attf.SubfieldDefinitions[1].Name);
        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, attf.SubfieldDefinitions[1].Format.FormatType);
        Assert.True(attf.SubfieldDefinitions[1].Format.IsVariableLength);

        // Assert repeating group
        Assert.True(attf.HasRepeatingGroup);
        Assert.Equal(0, attf.RepeatingSubfieldStartIndex);
        Assert.True(attf.SubfieldDefinitions[0].IsRepeating);
        Assert.True(attf.SubfieldDefinitions[1].IsRepeating);
    }

    #endregion

    #region Format Repeat Count Expansion Tests

    [Fact]
    public void ParseFormatControls_RepeatCount_ExpandsToMultipleFormats()
    {
        // Act — "2b24" should expand to two b24 entries
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(2b24)");

        // Assert
        Assert.Equal(2, formats.Length);
        Assert.All(formats, f =>
        {
            Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, f.FormatType);
            Assert.Equal(4, f.Width);
        });
    }

    [Fact]
    public void ParseFormatControls_RepeatCountThree_ExpandsToThreeFormats()
    {
        // Act — "3A" should expand to three A entries
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(3A)");

        // Assert
        Assert.Equal(3, formats.Length);
        Assert.All(formats, f =>
        {
            Assert.Equal(Iso8211SubfieldFormatType.CharacterData, f.FormatType);
            Assert.Equal(0, f.Width);
        });
    }

    [Fact]
    public void ParseFormatControls_RepeatCountWithWidth_ExpandsCorrectly()
    {
        // Act — "3I(5)" should expand to three I(5) entries
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(3I(5))");

        // Assert
        Assert.Equal(3, formats.Length);
        Assert.All(formats, f =>
        {
            Assert.Equal(Iso8211SubfieldFormatType.Integer, f.FormatType);
            Assert.Equal(5, f.Width);
        });
    }

    [Fact]
    public void ParseFormatControls_MixedRepeatAndSingle_ExpandsCorrectly()
    {
        // Act — "b11,2b24,A" should produce: b11, b24, b24, A
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(b11,2b24,A)");

        // Assert
        Assert.Equal(4, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);

        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[1].FormatType);
        Assert.Equal(4, formats[1].Width);

        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[2].FormatType);
        Assert.Equal(4, formats[2].Width);

        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, formats[3].FormatType);
        Assert.Equal(0, formats[3].Width);
    }

    [Fact]
    public void ParseFormatControls_RepeatCountOne_ProducesSingleFormat()
    {
        // Act — "1b24" should produce exactly one b24
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(1b24)");

        // Assert
        Assert.Single(formats);
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[0].FormatType);
        Assert.Equal(4, formats[0].Width);
    }

    [Fact]
    public void ParseFormatControls_MultipleRepeatCounts_ExpandAll()
    {
        // Act — "2b11,3b24" should produce: b11, b11, b24, b24, b24
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(2b11,3b24)");

        // Assert
        Assert.Equal(5, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[1].FormatType);
        Assert.Equal(1, formats[1].Width);

        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[2].FormatType);
        Assert.Equal(4, formats[2].Width);
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[3].FormatType);
        Assert.Equal(4, formats[3].Width);
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[4].FormatType);
        Assert.Equal(4, formats[4].Width);
    }

    [Fact]
    public void ParseFormatControls_NestedParentheses_InnerGroupExpanded()
    {
        // Act — "(b11,(3b24))" should produce: b11, b24, b24, b24
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(b11,(3b24))");

        // Assert
        Assert.Equal(4, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);

        for (int i = 1; i < 4; i++)
        {
            Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[i].FormatType);
            Assert.Equal(4, formats[i].Width);
        }
    }

    [Fact]
    public void ParseFormatControls_NestedParentheses_RepeatBeforeParen()
    {
        // Act — "(b11,3(b24))" should produce: b11, b24, b24, b24
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(b11,3(b24))");

        // Assert
        Assert.Equal(4, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[0].FormatType);
        Assert.Equal(1, formats[0].Width);

        for (int i = 1; i < 4; i++)
        {
            Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, formats[i].FormatType);
            Assert.Equal(4, formats[i].Width);
        }
    }

    [Fact]
    public void ParseFormatControls_NestedParentheses_MixedGroup()
    {
        // Act — "(A,2(I(10),b14))" should produce: A, I(10), b14, I(10), b14
        var formats = Iso8211DataDescriptiveRecordReader.ParseFormatControls("(A,2(I(10),b14))");

        // Assert
        Assert.Equal(5, formats.Length);

        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, formats[0].FormatType);

        Assert.Equal(Iso8211SubfieldFormatType.Integer, formats[1].FormatType);
        Assert.Equal(10, formats[1].Width);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[2].FormatType);
        Assert.Equal(4, formats[2].Width);

        Assert.Equal(Iso8211SubfieldFormatType.Integer, formats[3].FormatType);
        Assert.Equal(10, formats[3].Width);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, formats[4].FormatType);
        Assert.Equal(4, formats[4].Width);
    }

    #endregion

    #region Repeating Group Tests

    [Fact]
    public void RepeatingGroupStartIndex_NoRepeatingGroup_IsNegativeOne()
    {
        // Arrange — Elementary field (DataStructureCode=0) with no '*' marker
        var record = CreateDdrRecord(
            dataStructureCode: '0',
            subfieldNames: "RCNM!RCID!EXPP",
            formatControls: "(b11,b14,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal(-1, fieldDef.RepeatingSubfieldStartIndex);
        Assert.False(fieldDef.HasRepeatingGroup);
        Assert.All(fieldDef.SubfieldDefinitions, sf => Assert.False(sf.IsRepeating));
    }

    [Fact]
    public void RepeatingGroupStartIndex_AllSubfieldsRepeat_IsZero()
    {
        // Arrange — "*YCOO!XCOO" means all subfields repeat
        var record = CreateDdrRecord(
            subfieldNames: "*YCOO!XCOO",
            formatControls: "(2b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal(0, fieldDef.RepeatingSubfieldStartIndex);
        Assert.True(fieldDef.HasRepeatingGroup);
        Assert.True(fieldDef.SubfieldDefinitions[0].IsRepeating);
        Assert.True(fieldDef.SubfieldDefinitions[1].IsRepeating);
    }

    [Fact]
    public void RepeatingGroupStartIndex_PartialRepeat_MarksOnlyRepeatingSubfields()
    {
        // Arrange — "RCNM!RCID!*ATTL!ATVL" means ATTL and ATVL repeat, but RCNM and RCID do not
        var record = CreateDdrRecord(
            subfieldNames: "RCNM!RCID!*ATTL!ATVL",
            formatControls: "(b11,b14,b12,A)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal(2, fieldDef.RepeatingSubfieldStartIndex);
        Assert.True(fieldDef.HasRepeatingGroup);

        Assert.False(fieldDef.SubfieldDefinitions[0].IsRepeating); // RCNM
        Assert.False(fieldDef.SubfieldDefinitions[1].IsRepeating); // RCID
        Assert.True(fieldDef.SubfieldDefinitions[2].IsRepeating);  // ATTL
        Assert.True(fieldDef.SubfieldDefinitions[3].IsRepeating);  // ATVL
    }

    [Fact]
    public void Parse_S57FsptField_ParsesRepeatingGroupWithFourSubfields()
    {
        // Arrange — FSPT: *NAME!ORNT!USAG!MASK all repeat
        var record = CreateDdrRecord(
            tag: "FSPT",
            dataStructureCode: '1',
            dataTypeCode: '6',
            fieldName: "Feature Record to Spatial Record Pointer",
            subfieldNames: "*NAME!ORNT!USAG!MASK",
            formatControls: "(b12,b11,b11,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fspt = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal(4, fspt.SubfieldDefinitions.Count);
        Assert.True(fspt.HasRepeatingGroup);
        Assert.Equal(0, fspt.RepeatingSubfieldStartIndex);
        Assert.All(fspt.SubfieldDefinitions, sf => Assert.True(sf.IsRepeating));

        Assert.Equal("NAME", fspt.SubfieldDefinitions[0].Name);
        Assert.Equal("ORNT", fspt.SubfieldDefinitions[1].Name);
        Assert.Equal("USAG", fspt.SubfieldDefinitions[2].Name);
        Assert.Equal("MASK", fspt.SubfieldDefinitions[3].Name);
    }

    [Fact]
    public void Parse_MultiFieldDdr_SG2DFieldHasRepeatingGroup()
    {
        // Arrange
        var record = CreateMultiFieldDdrRecord();

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert — DSID and DSPM are Vector fields, so they implicitly have repeating groups
        var dsid = ddr.GetFieldDefinition("DSID")!;
        Assert.True(dsid.HasRepeatingGroup);

        var dspm = ddr.GetFieldDefinition("DSPM")!;
        Assert.True(dspm.HasRepeatingGroup);

        // Assert — SG2D should have a repeating group
        var sg2d = ddr.GetFieldDefinition("SG2D")!;
        Assert.True(sg2d.HasRepeatingGroup);
        Assert.Equal(0, sg2d.RepeatingSubfieldStartIndex);
        Assert.Equal(2, sg2d.SubfieldDefinitions.Count);
        Assert.True(sg2d.SubfieldDefinitions[0].IsRepeating);
        Assert.True(sg2d.SubfieldDefinitions[1].IsRepeating);
    }

    #endregion

    #region No Field Controls Tests

    [Fact]
    public void Parse_WithZeroFieldControlLength_DefaultsToCodes()
    {
        // Arrange — field control length = 0 means no control bytes
        var record = CreateDdrRecord(fieldControlLength: 0);

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert — should default to Elementary/CharacterString
        Assert.Equal(Iso8211DataStructureCode.Elementary, ddr.FieldDefinitions[0].DataStructureCode);
        Assert.Equal(Iso8211DataTypeCode.CharacterString, ddr.FieldDefinitions[0].DataTypeCode);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Parse_FieldWithNoSubfields_HasEmptySubfieldDefinitions()
    {
        // Arrange
        var record = CreateDdrRecord(
            subfieldNames: null,
            formatControls: "(A)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);

        // Assert
        Assert.Empty(ddr.FieldDefinitions[0].SubfieldDefinitions);
    }

    [Fact]
    public void Parse_EmptyFormatControls_HasEmptyFormats()
    {
        // Arrange
        var record = CreateDdrRecord(
            subfieldNames: "SF1!SF2",
            formatControls: "");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var subfields = ddr.FieldDefinitions[0].SubfieldDefinitions;

        // Assert — subfields get default format (A, width 0)
        Assert.Equal(2, subfields.Count);
        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, subfields[0].Format.FormatType);
        Assert.Equal(0, subfields[0].Format.Width);
    }

    [Fact]
    public void Parse_MoreSubfieldsThanFormats_DefaultsExtraToCharacterData()
    {
        // Arrange — 3 subfields but only 2 formats
        var record = CreateDdrRecord(
            subfieldNames: "A!B!C",
            formatControls: "(b11,b14)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var subfields = ddr.FieldDefinitions[0].SubfieldDefinitions;

        // Assert
        Assert.Equal(3, subfields.Count);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[0].Format.FormatType);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[1].Format.FormatType);
        Assert.Equal(Iso8211SubfieldFormatType.CharacterData, subfields[2].Format.FormatType);
    }

    #endregion

    #region Two-UT Format Tests (Real S-57 File Format)

    /// <summary>
    /// Creates a DDR record using the two-UT format found in real S-57 files:
    /// [FieldControls][FieldName] UT [SubfieldLabels] UT [FormatControls]
    /// </summary>
    private static Iso8211Record CreateDdrRecordTwoUt(
        string tag = "DSID",
        int fieldControlLength = 2,
        char dataStructureCode = '1',
        char dataTypeCode = '6',
        string fieldName = "Data Set Identification",
        string? subfieldLabels = "RCNM!RCID!EXPP",
        string formatControls = "(b11,b14,b11)")
    {
        var sb = new StringBuilder();

        if (fieldControlLength >= 2)
        {
            sb.Append(dataStructureCode);
            sb.Append(dataTypeCode);
        }

        // Field name in first section
        sb.Append(fieldName);
        sb.Append('\x1F'); // First UT — separates field name from subfield labels

        // Subfield labels in second section
        if (!string.IsNullOrEmpty(subfieldLabels))
        {
            sb.Append(subfieldLabels);
        }

        sb.Append('\x1F'); // Second UT — separates subfield labels from format controls
        sb.Append(formatControls);

        var fieldData = Encoding.ASCII.GetBytes(sb.ToString());

        var field = new Iso8211Field
        {
            Tag = tag,
            Data = fieldData
        };

        return new Iso8211Record
        {
            Leader = new Iso8211RecordLeader
            {
                LeaderIdentifier = 'L',
                FieldControlLength = fieldControlLength,
                SizeOfFieldTagField = 4,
                SizeOfFieldLengthField = 3,
                SizeOfFieldPositionField = 3,
                RecordLength = 100,
                BaseAddressOfFieldArea = 35,
                InterchangeLevel = '3',
                VersionNumber = '1',
                ExtendedCharacterSetIndicator = "   "
            },
            Directory = ImmutableArray.Create(new Iso8211DirectoryEntry
            {
                Tag = tag,
                Length = fieldData.Length + 1,
                Position = 0
            }),
            Fields = ImmutableArray.Create(field)
        };
    }

    [Fact]
    public void Parse_TwoUtFormat_ParsesSubfieldDefinitions()
    {
        // Arrange — two-UT format: [FieldName] UT [SubfieldLabels] UT [FormatControls]
        var record = CreateDdrRecordTwoUt(
            subfieldLabels: "RCNM!RCID!EXPP",
            formatControls: "(b11,b14,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal("Data Set Identification", fieldDef.FieldName);
        Assert.Equal(3, fieldDef.SubfieldDefinitions.Count);
        Assert.Equal("RCNM", fieldDef.SubfieldDefinitions[0].Name);
        Assert.Equal("RCID", fieldDef.SubfieldDefinitions[1].Name);
        Assert.Equal("EXPP", fieldDef.SubfieldDefinitions[2].Name);
    }

    [Fact]
    public void Parse_TwoUtFormat_SubfieldFormatsArePairedCorrectly()
    {
        // Arrange
        var record = CreateDdrRecordTwoUt(
            subfieldLabels: "RCNM!RCID!EXPP",
            formatControls: "(b11,b14,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var subfields = ddr.FieldDefinitions[0].SubfieldDefinitions;

        // Assert
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[0].Format.FormatType);
        Assert.Equal(1, subfields[0].Format.Width);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[1].Format.FormatType);
        Assert.Equal(4, subfields[1].Format.Width);

        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, subfields[2].Format.FormatType);
        Assert.Equal(1, subfields[2].Format.Width);
    }

    [Fact]
    public void Parse_TwoUtFormat_S57DsidField_FullParse()
    {
        // Arrange — simulate a real S-57 DSID DDR field with two-UT format
        var record = CreateDdrRecordTwoUt(
            tag: "DSID",
            dataStructureCode: '1',
            dataTypeCode: '6',
            fieldName: "Data Set Identification",
            subfieldLabels: "RCNM!RCID!EXPP!INTU!DSNM!EDTN!UPDN!UADT!ISDT!STED!PRSP!PSDN!PRED!PROF!AGEN!COMT",
            formatControls: "(b11,b14,b11,b11,A,A,A,A,A,A,b11,A,A,b11,b12,A)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var dsid = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal("DSID", dsid.Tag);
        Assert.Equal(Iso8211DataStructureCode.Vector, dsid.DataStructureCode);
        Assert.Equal(Iso8211DataTypeCode.MixedDataTypes, dsid.DataTypeCode);
        Assert.Equal("Data Set Identification", dsid.FieldName);
        Assert.Equal(16, dsid.SubfieldDefinitions.Count);

        // Vector field implicitly repeats all subfields
        Assert.True(dsid.HasRepeatingGroup);
        Assert.Null(dsid.ArrayDescriptor);

        var rcnm = dsid.GetSubfieldDefinition("RCNM")!;
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, rcnm.Format.FormatType);
        Assert.Equal(1, rcnm.Format.Width);

        var rcid = dsid.GetSubfieldDefinition("RCID")!;
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, rcid.Format.FormatType);
        Assert.Equal(4, rcid.Format.Width);

        var agen = dsid.GetSubfieldDefinition("AGEN")!;
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, agen.Format.FormatType);
        Assert.Equal(2, agen.Format.Width);
    }

    [Fact]
    public void Parse_TwoUtFormat_RepeatingGroup_ParsesCorrectly()
    {
        // Arrange — two-UT format with repeating group marker
        var record = CreateDdrRecordTwoUt(
            tag: "SG2D",
            dataStructureCode: '1',
            dataTypeCode: '5',
            fieldName: "2-D Coordinate Field",
            subfieldLabels: "*YCOO!XCOO",
            formatControls: "(2b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var sg2d = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal(2, sg2d.SubfieldDefinitions.Count);
        Assert.Equal("YCOO", sg2d.SubfieldDefinitions[0].Name);
        Assert.Equal("XCOO", sg2d.SubfieldDefinitions[1].Name);

        Assert.True(sg2d.HasRepeatingGroup);
        Assert.Equal(0, sg2d.RepeatingSubfieldStartIndex);
        Assert.True(sg2d.SubfieldDefinitions[0].IsRepeating);
        Assert.True(sg2d.SubfieldDefinitions[1].IsRepeating);

        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, sg2d.SubfieldDefinitions[0].Format.FormatType);
        Assert.Equal(4, sg2d.SubfieldDefinitions[0].Format.Width);
    }

    [Fact]
    public void Parse_TwoUtFormat_NoArrayDescriptorForVectorFields()
    {
        // Arrange — vector field should not produce an array descriptor
        var record = CreateDdrRecordTwoUt(
            dataStructureCode: '1',
            subfieldLabels: "RCNM!RCID",
            formatControls: "(b11,b14)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];

        // Assert — subfield labels should NOT be stored as array descriptor
        Assert.Null(fieldDef.ArrayDescriptor);
        Assert.Equal(2, fieldDef.SubfieldDefinitions.Count);
    }

    #endregion

    #region Array-Coded Fields With Subfield Labels Tests

    [Fact]
    public void Parse_TwoUtFormat_ArrayStructureWithSubfieldLabels_ParsesAsSubfields()
    {
        // Arrange — some real S-57 DDR files encode SG2D with DataStructureCode=Array ('2')
        // but still use the middle section for subfield labels (e.g., "*YCOO!XCOO").
        // Previously this was stored as ArrayDescriptor and SubfieldDefinitions was empty.
        var record = CreateDdrRecordTwoUt(
            tag: "SG2D",
            dataStructureCode: '2',
            dataTypeCode: '5',
            fieldName: "2-D Coordinate Field",
            subfieldLabels: "*YCOO!XCOO",
            formatControls: "(2b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var sg2d = ddr.FieldDefinitions[0];

        // Assert — middle section parsed as subfield labels, not array descriptor
        Assert.Null(sg2d.ArrayDescriptor);
        Assert.Equal(2, sg2d.SubfieldDefinitions.Count);
        Assert.Equal("YCOO", sg2d.SubfieldDefinitions[0].Name);
        Assert.Equal("XCOO", sg2d.SubfieldDefinitions[1].Name);

        // Assert — repeating group from '*' marker
        Assert.True(sg2d.HasRepeatingGroup);
        Assert.Equal(0, sg2d.RepeatingSubfieldStartIndex);
        Assert.True(sg2d.SubfieldDefinitions[0].IsRepeating);
        Assert.True(sg2d.SubfieldDefinitions[1].IsRepeating);

        // Assert — formats paired correctly
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, sg2d.SubfieldDefinitions[0].Format.FormatType);
        Assert.Equal(4, sg2d.SubfieldDefinitions[0].Format.Width);
    }

    [Fact]
    public void Parse_TwoUtFormat_ArrayStructureWithSubfieldLabelsNoAsterisk_ParsesAsSubfields()
    {
        // Arrange — Array-coded field with subfield labels but no '*' marker
        var record = CreateDdrRecordTwoUt(
            tag: "VRID",
            dataStructureCode: '2',
            dataTypeCode: '6',
            fieldName: "Vector Record Identifier",
            subfieldLabels: "RCNM!RCID!RVER!RUIN",
            formatControls: "(b11,b14,b12,b11)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var vrid = ddr.FieldDefinitions[0];

        // Assert — parsed as subfield labels, not array descriptor
        Assert.Null(vrid.ArrayDescriptor);
        Assert.Equal(4, vrid.SubfieldDefinitions.Count);
        Assert.Equal("RCNM", vrid.SubfieldDefinitions[0].Name);
        Assert.Equal("RCID", vrid.SubfieldDefinitions[1].Name);
        Assert.Equal("RVER", vrid.SubfieldDefinitions[2].Name);
        Assert.Equal("RUIN", vrid.SubfieldDefinitions[3].Name);
    }

    [Fact]
    public void Parse_TwoUtFormat_ArrayStructureWithTrueArrayDescriptor_StoresAsArrayDescriptor()
    {
        // Arrange — a genuine array descriptor (no '!' or '*') should still be stored as ArrayDescriptor
        var record = CreateDdrRecordTwoUt(
            tag: "TEST",
            dataStructureCode: '2',
            dataTypeCode: '0',
            fieldName: "Test Array",
            subfieldLabels: "3x5",
            formatControls: "(A)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var fieldDef = ddr.FieldDefinitions[0];

        // Assert — no '!' or '*', so treated as array descriptor
        Assert.Equal("3x5", fieldDef.ArrayDescriptor);
        Assert.Empty(fieldDef.SubfieldDefinitions);
    }

    [Fact]
    public void Parse_TwoUtFormat_ConcatenatedArrayWithSubfieldLabels_ParsesAsSubfields()
    {
        // Arrange — ConcatenatedArray (DataStructureCode=3) with subfield labels
        var record = CreateDdrRecordTwoUt(
            tag: "SG3D",
            dataStructureCode: '3',
            dataTypeCode: '5',
            fieldName: "3-D Coordinate Field",
            subfieldLabels: "*YCOO!XCOO!VE3D",
            formatControls: "(3b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var sg3d = ddr.FieldDefinitions[0];

        // Assert
        Assert.Null(sg3d.ArrayDescriptor);
        Assert.Equal(3, sg3d.SubfieldDefinitions.Count);
        Assert.Equal("YCOO", sg3d.SubfieldDefinitions[0].Name);
        Assert.Equal("XCOO", sg3d.SubfieldDefinitions[1].Name);
        Assert.Equal("VE3D", sg3d.SubfieldDefinitions[2].Name);

        Assert.True(sg3d.HasRepeatingGroup);
        Assert.Equal(0, sg3d.RepeatingSubfieldStartIndex);
    }

    [Fact]
    public void Parse_TwoUtFormat_ConcatenatedArrayWithLeaderAndRepeating_ParsesCorrectly()
    {
        // Arrange — ConcatenatedArray with mid-string '*' separating leader from repeating subfields.
        // This is the S-101 C3IL pattern: VCID is a leader subfield, YCOO/XCOO/ZCOO repeat.
        var record = CreateDdrRecordTwoUt(
            tag: "C3IL",
            dataStructureCode: '3',
            dataTypeCode: '5',
            fieldName: "3-D Coordinate with Leader",
            subfieldLabels: "VCID*YCOO!XCOO!ZCOO",
            formatControls: "(b11,3b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var c3il = ddr.FieldDefinitions[0];

        // Assert — 4 subfield definitions
        Assert.Equal(4, c3il.SubfieldDefinitions.Count);

        Assert.Equal("VCID", c3il.SubfieldDefinitions[0].Name);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, c3il.SubfieldDefinitions[0].Format.FormatType);
        Assert.Equal(1, c3il.SubfieldDefinitions[0].Format.Width);

        Assert.Equal("YCOO", c3il.SubfieldDefinitions[1].Name);
        Assert.Equal(Iso8211SubfieldFormatType.SignedInteger, c3il.SubfieldDefinitions[1].Format.FormatType);
        Assert.Equal(4, c3il.SubfieldDefinitions[1].Format.Width);

        Assert.Equal("XCOO", c3il.SubfieldDefinitions[2].Name);
        Assert.Equal("ZCOO", c3il.SubfieldDefinitions[3].Name);

        // Repeating group starts at index 1 (after VCID leader)
        Assert.True(c3il.HasRepeatingGroup);
        Assert.Equal(1, c3il.RepeatingSubfieldStartIndex);

        // VCID is non-repeating leader, YCOO/XCOO/ZCOO are repeating
        Assert.False(c3il.SubfieldDefinitions[0].IsRepeating);
        Assert.True(c3il.SubfieldDefinitions[1].IsRepeating);
        Assert.True(c3il.SubfieldDefinitions[2].IsRepeating);
        Assert.True(c3il.SubfieldDefinitions[3].IsRepeating);
    }

    [Fact]
    public void Parse_TwoUtFormat_VectorStyleStarAtStart_StillWorks()
    {
        // Arrange — regression guard: '*' at the start of labels (Vector-style)
        var record = CreateDdrRecordTwoUt(
            tag: "SG2D",
            dataStructureCode: '1',
            dataTypeCode: '6',
            fieldName: "2-D Coordinate Field",
            subfieldLabels: "*YCOO!XCOO",
            formatControls: "(2b24)");

        // Act
        var ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        var sg2d = ddr.FieldDefinitions[0];

        // Assert
        Assert.Equal(2, sg2d.SubfieldDefinitions.Count);
        Assert.Equal("YCOO", sg2d.SubfieldDefinitions[0].Name);
        Assert.Equal("XCOO", sg2d.SubfieldDefinitions[1].Name);
        Assert.Equal(0, sg2d.RepeatingSubfieldStartIndex);
        Assert.True(sg2d.HasRepeatingGroup);
    }

    #endregion
}
