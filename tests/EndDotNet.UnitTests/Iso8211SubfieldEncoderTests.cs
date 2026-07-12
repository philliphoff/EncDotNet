using System.Collections.Immutable;
using EncDotNet.Iso8211;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="Iso8211SubfieldEncoder"/>, <see cref="Iso8211FieldBuilder"/>, and
/// <see cref="Iso8211DataDescriptiveRecordWriter"/>. Each subfield format type is verified by
/// encoding a value and decoding it back with <see cref="Iso8211FieldReader"/>.
/// </summary>
public class Iso8211SubfieldEncoderTests
{
    private static readonly Iso8211WriterOptions Options = Iso8211WriterOptions.Default;

    #region Direct binary encoding

    [Fact]
    public void Encode_UnsignedBinary_IsLittleEndian()
    {
        var format = Format(Iso8211SubfieldFormatType.UnsignedInteger, 2);
        var bytes = Iso8211SubfieldEncoder.Encode((ushort)0x1234, format, Options);
        Assert.Equal(new byte[] { 0x34, 0x12 }, bytes);
    }

    [Fact]
    public void Encode_UnsignedBinary_SingleByte()
    {
        var format = Format(Iso8211SubfieldFormatType.UnsignedInteger, 1);
        var bytes = Iso8211SubfieldEncoder.Encode((byte)0xAB, format, Options);
        Assert.Equal(new byte[] { 0xAB }, bytes);
    }

    [Fact]
    public void Encode_SignedBinary_NegativeIsTwosComplementLittleEndian()
    {
        var format = Format(Iso8211SubfieldFormatType.SignedInteger, 4);
        var bytes = Iso8211SubfieldEncoder.Encode(-1, format, Options);
        Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, bytes);
    }

    [Fact]
    public void Encode_SignedBinary_KnownValue()
    {
        var format = Format(Iso8211SubfieldFormatType.SignedInteger, 4);
        var bytes = Iso8211SubfieldEncoder.Encode(-2, format, Options);
        Assert.Equal(new byte[] { 0xFE, 0xFF, 0xFF, 0xFF }, bytes);
    }

    [Fact]
    public void Encode_FloatingBinary_Double_RoundTrips()
    {
        var format = Format(Iso8211SubfieldFormatType.FloatingPoint, 8);
        var bytes = Iso8211SubfieldEncoder.Encode(3.14159, format, Options);
        Assert.Equal(8, bytes.Length);
        Assert.Equal(3.14159, System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(bytes), 10);
    }

    [Fact]
    public void Encode_FloatingBinary_Single_RoundTrips()
    {
        var format = Format(Iso8211SubfieldFormatType.FloatingPoint, 4);
        var bytes = Iso8211SubfieldEncoder.Encode(1.5f, format, Options);
        Assert.Equal(4, bytes.Length);
        Assert.Equal(1.5f, System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(bytes));
    }

    [Fact]
    public void Encode_FixedCharacter_PadsWithSpaces()
    {
        var format = Format(Iso8211SubfieldFormatType.CharacterData, 6);
        var bytes = Iso8211SubfieldEncoder.Encode("AB", format, Options);
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("AB    "), bytes);
    }

    [Fact]
    public void Encode_FixedInteger_ZeroPadsRightJustified()
    {
        var format = Format(Iso8211SubfieldFormatType.Integer, 5);
        var bytes = Iso8211SubfieldEncoder.Encode(42, format, Options);
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("00042"), bytes);
    }

    [Fact]
    public void Encode_FixedInteger_NegativeIncludesSign()
    {
        var format = Format(Iso8211SubfieldFormatType.Integer, 4);
        var bytes = Iso8211SubfieldEncoder.Encode(-5, format, Options);
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("-005"), bytes);
    }

    [Fact]
    public void Encode_BitString_RawBytes()
    {
        var format = Format(Iso8211SubfieldFormatType.BitString, 2);
        var bytes = Iso8211SubfieldEncoder.Encode(new byte[] { 0xDE, 0xAD }, format, Options);
        Assert.Equal(new byte[] { 0xDE, 0xAD }, bytes);
    }

    #endregion

    #region Encode -> decode round-trips through Iso8211FieldReader

    [Fact]
    public void RoundTrip_FixedBinarySubfields()
    {
        var definition = FieldDefinition(
            tag: "FRID",
            Iso8211DataStructureCode.Array,
            Iso8211DataTypeCode.Binary,
            "(b11,b14)",
            repeatingStart: -1,
            ("RCNM", Format(Iso8211SubfieldFormatType.UnsignedInteger, 1)),
            ("RCID", Format(Iso8211SubfieldFormatType.UnsignedInteger, 4)));

        var field = new Iso8211FieldBuilder(definition)
            .AddSubfield((byte)110)
            .AddSubfield((uint)123456)
            .Build();

        var reader = new Iso8211FieldReader(definition, field.Data);
        Assert.Equal(110, reader.GetSubfield<byte>("RCNM"));
        Assert.Equal(123456u, reader.GetSubfield<uint>("RCID"));
    }

    [Fact]
    public void RoundTrip_SignedBinarySubfields()
    {
        var definition = FieldDefinition(
            tag: "TEST",
            Iso8211DataStructureCode.Array,
            Iso8211DataTypeCode.Binary,
            "(b24,b24)",
            repeatingStart: -1,
            ("YCOO", Format(Iso8211SubfieldFormatType.SignedInteger, 4)),
            ("XCOO", Format(Iso8211SubfieldFormatType.SignedInteger, 4)));

        var field = new Iso8211FieldBuilder(definition)
            .AddSubfield(-100)
            .AddSubfield(200)
            .Build();

        var reader = new Iso8211FieldReader(definition, field.Data);
        Assert.Equal(-100, reader.GetSubfield<int>("YCOO"));
        Assert.Equal(200, reader.GetSubfield<int>("XCOO"));
    }

    [Fact]
    public void RoundTrip_VariableCharacterSubfields()
    {
        var definition = FieldDefinition(
            tag: "DSID",
            Iso8211DataStructureCode.Array,
            Iso8211DataTypeCode.MixedDataTypes,
            "(A,A)",
            repeatingStart: -1,
            ("DSNM", Format(Iso8211SubfieldFormatType.CharacterData, 0)),
            ("EDTN", Format(Iso8211SubfieldFormatType.CharacterData, 0)));

        var field = new Iso8211FieldBuilder(definition)
            .AddSubfield("US5WA22M.000")
            .AddSubfield("2")
            .Build();

        var reader = new Iso8211FieldReader(definition, field.Data);
        Assert.Equal("US5WA22M.000", reader.GetSubfield<string>("DSNM"));
        Assert.Equal("2", reader.GetSubfield<string>("EDTN"));
    }

    [Fact]
    public void RoundTrip_MixedFixedAndVariableSubfields()
    {
        var definition = FieldDefinition(
            tag: "MIXD",
            Iso8211DataStructureCode.Array,
            Iso8211DataTypeCode.MixedDataTypes,
            "(b11,A,I(4))",
            repeatingStart: -1,
            ("CODE", Format(Iso8211SubfieldFormatType.UnsignedInteger, 1)),
            ("NAME", Format(Iso8211SubfieldFormatType.CharacterData, 0)),
            ("NUMB", Format(Iso8211SubfieldFormatType.Integer, 4)));

        var field = new Iso8211FieldBuilder(definition)
            .AddSubfields((byte)7, "HELLO", 42)
            .Build();

        var reader = new Iso8211FieldReader(definition, field.Data);
        Assert.Equal(7, reader.GetSubfield<byte>("CODE"));
        Assert.Equal("HELLO", reader.GetSubfield<string>("NAME"));
        Assert.Equal(42, reader.GetSubfield<int>("NUMB"));
    }

    [Fact]
    public void RoundTrip_RepeatingGroup()
    {
        var definition = FieldDefinition(
            tag: "SG2D",
            Iso8211DataStructureCode.Vector,
            Iso8211DataTypeCode.Binary,
            "(2b24)",
            repeatingStart: 0,
            ("YCOO", Format(Iso8211SubfieldFormatType.SignedInteger, 4)),
            ("XCOO", Format(Iso8211SubfieldFormatType.SignedInteger, 4)));

        var field = new Iso8211FieldBuilder(definition)
            .AddSubfields(10, 20)   // group 0
            .AddSubfields(30, 40)   // group 1
            .AddSubfields(50, 60)   // group 2
            .Build();

        var reader = new Iso8211FieldReader(definition, field.Data);
        var ys = reader.GetSubfieldValues<int>("YCOO");
        var xs = reader.GetSubfieldValues<int>("XCOO");
        Assert.Equal(new[] { 10, 30, 50 }, ys);
        Assert.Equal(new[] { 20, 40, 60 }, xs);
    }

    #endregion

    #region DDR writer round-trips through the DDR reader

    [Fact]
    public void Ddr_EncodeFieldDefinition_RoundTripsNonRepeating()
    {
        var definition = FieldDefinition(
            tag: "FRID",
            Iso8211DataStructureCode.Array,
            Iso8211DataTypeCode.Binary,
            "(b11,b14)",
            repeatingStart: -1,
            fieldName: "Feature record identifier",
            ("RCNM", Format(Iso8211SubfieldFormatType.UnsignedInteger, 1)),
            ("RCID", Format(Iso8211SubfieldFormatType.UnsignedInteger, 4)));

        var ddr = Iso8211DataDescriptiveRecordWriter.BuildDdr(new[] { definition });
        var document = new Iso8211DocumentBuilder().AddRecord(ddr).Build();
        var bytes = Iso8211DocumentWriter.Write(document);

        var reparsed = Iso8211DocumentReader.Read(bytes);
        var parsedDdr = Iso8211DataDescriptiveRecordReader.Read(reparsed.Records[0]);

        var def = Assert.Single(parsedDdr.FieldDefinitions);
        Assert.Equal("FRID", def.Tag);
        Assert.Equal(Iso8211DataStructureCode.Array, def.DataStructureCode);
        Assert.Equal(Iso8211DataTypeCode.Binary, def.DataTypeCode);
        Assert.Equal("Feature record identifier", def.FieldName);
        Assert.Equal(-1, def.RepeatingSubfieldStartIndex);
        Assert.Equal(2, def.SubfieldDefinitions.Count);
        Assert.Equal("RCNM", def.SubfieldDefinitions[0].Name);
        Assert.Equal(Iso8211SubfieldFormatType.UnsignedInteger, def.SubfieldDefinitions[0].Format.FormatType);
        Assert.Equal(1, def.SubfieldDefinitions[0].Format.Width);
        Assert.Equal("RCID", def.SubfieldDefinitions[1].Name);
        Assert.Equal(4, def.SubfieldDefinitions[1].Format.Width);
    }

    [Fact]
    public void Ddr_EncodeFieldDefinition_RoundTripsRepeatingGroup()
    {
        var definition = FieldDefinition(
            tag: "SG2D",
            Iso8211DataStructureCode.Vector,
            Iso8211DataTypeCode.Binary,
            "(2b24)",
            repeatingStart: 0,
            fieldName: "2D coordinate",
            ("YCOO", Format(Iso8211SubfieldFormatType.SignedInteger, 4)),
            ("XCOO", Format(Iso8211SubfieldFormatType.SignedInteger, 4)));

        var ddr = Iso8211DataDescriptiveRecordWriter.BuildDdr(new[] { definition });
        var document = new Iso8211DocumentBuilder().AddRecord(ddr).Build();
        var bytes = Iso8211DocumentWriter.Write(document);

        var reparsed = Iso8211DocumentReader.Read(bytes);
        var parsedDdr = Iso8211DataDescriptiveRecordReader.Read(reparsed.Records[0]);

        var def = Assert.Single(parsedDdr.FieldDefinitions);
        Assert.Equal("SG2D", def.Tag);
        Assert.Equal(Iso8211DataStructureCode.Vector, def.DataStructureCode);
        Assert.Equal(0, def.RepeatingSubfieldStartIndex);
        Assert.Equal(2, def.SubfieldDefinitions.Count);
        Assert.Equal("YCOO", def.SubfieldDefinitions[0].Name);
        Assert.Equal("XCOO", def.SubfieldDefinitions[1].Name);
    }

    [Fact]
    public void Ddr_FullDocument_WithDataRecord_RoundTrips()
    {
        var definition = FieldDefinition(
            tag: "FRID",
            Iso8211DataStructureCode.Array,
            Iso8211DataTypeCode.Binary,
            "(b11,b14)",
            repeatingStart: -1,
            ("RCNM", Format(Iso8211SubfieldFormatType.UnsignedInteger, 1)),
            ("RCID", Format(Iso8211SubfieldFormatType.UnsignedInteger, 4)));

        var ddr = Iso8211DataDescriptiveRecordWriter.BuildDdr(new[] { definition });

        var dataRecord = new Iso8211RecordBuilder()
            .AddField(new Iso8211FieldBuilder(definition).AddSubfields((byte)100, (uint)555).Build())
            .Build();

        var document = new Iso8211DocumentBuilder()
            .AddRecord(ddr)
            .AddRecord(dataRecord)
            .Build();

        var bytes = Iso8211DocumentWriter.Write(document);
        var reparsed = Iso8211DocumentReader.Read(bytes);

        Assert.Equal(2, reparsed.Records.Count);
        Assert.True(reparsed.Records[0].IsDataDescriptiveRecord);
        Assert.False(reparsed.Records[1].IsDataDescriptiveRecord);

        var parsedDdr = Iso8211DataDescriptiveRecordReader.Read(reparsed.Records[0]);
        var def = parsedDdr.FieldDefinitions.Single(d => d.Tag == "FRID");

        var fieldReader = new Iso8211FieldReader(def, reparsed.Records[1].GetFieldByTag("FRID")!.Data);
        Assert.Equal(100, fieldReader.GetSubfield<byte>("RCNM"));
        Assert.Equal(555u, fieldReader.GetSubfield<uint>("RCID"));
    }

    #endregion

    #region Helpers

    private static Iso8211SubfieldFormat Format(Iso8211SubfieldFormatType type, int width) =>
        new() { FormatType = type, Width = width };

    private static Iso8211FieldDefinition FieldDefinition(
        string tag,
        Iso8211DataStructureCode structureCode,
        Iso8211DataTypeCode typeCode,
        string formatControls,
        int repeatingStart,
        params (string Name, Iso8211SubfieldFormat Format)[] subfields)
        => FieldDefinition(tag, structureCode, typeCode, formatControls, repeatingStart, fieldName: tag, subfields);

    private static Iso8211FieldDefinition FieldDefinition(
        string tag,
        Iso8211DataStructureCode structureCode,
        Iso8211DataTypeCode typeCode,
        string formatControls,
        int repeatingStart,
        string fieldName,
        params (string Name, Iso8211SubfieldFormat Format)[] subfields)
    {
        var defs = ImmutableArray.CreateBuilder<Iso8211SubfieldDefinition>();
        for (int i = 0; i < subfields.Length; i++)
        {
            defs.Add(new Iso8211SubfieldDefinition
            {
                Name = subfields[i].Name,
                Format = subfields[i].Format,
                Index = i,
                IsRepeating = repeatingStart >= 0 && i >= repeatingStart
            });
        }

        return new Iso8211FieldDefinition
        {
            Tag = tag,
            DataStructureCode = structureCode,
            DataTypeCode = typeCode,
            FieldName = fieldName,
            FormatControls = formatControls,
            SubfieldDefinitions = defs.ToImmutable(),
            RepeatingSubfieldStartIndex = repeatingStart
        };
    }

    #endregion
}
