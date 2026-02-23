using System.Text;
using EncDotNet.Enc;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for the <see cref="S57DocumentReader"/> class and related S-57 types.
/// </summary>
public class S57DocumentReaderTests
{
    #region Test Data Helpers

    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    /// <summary>
    /// Creates a minimal S-57 ISO 8211 document with a DDR and optional data records.
    /// </summary>
    private static byte[] CreateS57Document(params byte[][] dataRecords)
    {
        // Create DDR (Data Descriptive Record) with proper S-57 field definitions
        var ddr = CreateS57Ddr();
        
        // Calculate total size
        var totalSize = ddr.Length;
        foreach (var record in dataRecords)
        {
            totalSize += record.Length;
        }
        
        // Combine all records
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
    /// Creates an S-57 Data Descriptive Record (DDR) with all required field definitions.
    /// </summary>
    private static byte[] CreateS57Ddr()
    {
        // Build all field definitions for DDR
        var fields = new List<(string tag, byte[] data)>();

        // 0001 - File control field (required)
        fields.Add(("0001", CreateDdrFieldData("", "", "()")));

        // DSID - Data Set Identification Field
        // Format: RCNM(b11), RCID(b14), EXPP(b11), INTU(b11), DSNM(A), EDTN(A), UPDN(A), UADT(A), ISDT(A), STED(A), PRSP(b11), PSDN(A), PRED(A), PROF(b11), AGEN(b12), COMT(A)
        fields.Add(("DSID", CreateDdrFieldData(
            "DSID",
            "RCNM!RCID!EXPP!INTU!DSNM!EDTN!UPDN!UADT!ISDT!STED!PRSP!PSDN!PRED!PROF!AGEN!COMT",
            "(b11,b14,b11,b11,A,A,A,A,A,A,b11,A,A,b11,b12,A)")));

        // DSSI - Data Set Structure Information Field
        // Format: DSTR(b11), AALL(b11), NALL(b11), NOMR(b14), NOCR(b14), NOGR(b14), NOLR(b14), NOIN(b14), NOCN(b14), NOED(b14), NOFA(b14)
        fields.Add(("DSSI", CreateDdrFieldData(
            "DSSI",
            "DSTR!AALL!NALL!NOMR!NOCR!NOGR!NOLR!NOIN!NOCN!NOED!NOFA",
            "(b11,b11,b11,b14,b14,b14,b14,b14,b14,b14,b14)")));

        // DSPM - Data Set Parameters Field
        // Format: RCNM(b11), RCID(b14), HDAT(b11), VDAT(b11), SDAT(b11), CSCL(b14), DUNI(b11), HUNI(b11), PUNI(b11), COUN(b11), COMF(b14), SOMF(b14), COMT(A)
        fields.Add(("DSPM", CreateDdrFieldData(
            "DSPM",
            "RCNM!RCID!HDAT!VDAT!SDAT!CSCL!DUNI!HUNI!PUNI!COUN!COMF!SOMF!COMT",
            "(b11,b14,b11,b11,b11,b14,b11,b11,b11,b11,b14,b14,A)")));

        // FRID - Feature Record Identifier Field
        // Format: RCNM(b11), RCID(b14), PRIM(b11), GRUP(b11), OBJL(b12), RVER(b12), RUIN(b11)
        fields.Add(("FRID", CreateDdrFieldData(
            "FRID",
            "RCNM!RCID!PRIM!GRUP!OBJL!RVER!RUIN",
            "(b11,b14,b11,b11,b12,b12,b11)")));

        // FOID - Feature Object Identifier Field
        // Format: AGEN(b12), FIDN(b14), FIDS(b12)
        fields.Add(("FOID", CreateDdrFieldData(
            "FOID",
            "AGEN!FIDN!FIDS",
            "(b12,b14,b12)")));

        // ATTF - Feature Record Attribute Field (repeating)
        // Format: *ATTL(b12), ATVL(A)
        fields.Add(("ATTF", CreateDdrFieldData(
            "ATTF",
            "*ATTL!ATVL",
            "(b12,A)",
            dataStructure: 1))); // Vector (repeating)

        // NATF - Feature Record National Attribute Field (repeating)
        // Format: *ATTL(b12), ATVL(A)
        fields.Add(("NATF", CreateDdrFieldData(
            "NATF",
            "*ATTL!ATVL",
            "(b12,A)",
            dataStructure: 1)));

        // FSPT - Feature Record to Spatial Record Pointer Field (repeating)
        // Format: *NAME(b15), ORNT(b11), USAG(b11), MASK(b11)
        // NAME is 5 bytes (RCNM=1 + RCID=4)
        fields.Add(("FSPT", CreateDdrFieldData(
            "FSPT",
            "*NAME!ORNT!USAG!MASK",
            "(b15,b11,b11,b11)",
            dataStructure: 1)));

        // FFPT - Feature Record to Feature Object Pointer Field (repeating)
        // Format: *LNAM(b18), RIND(b11), COMT(A)
        // LNAM is 8 bytes (AGEN=2 + FIDN=4 + FIDS=2)
        fields.Add(("FFPT", CreateDdrFieldData(
            "FFPT",
            "*LNAM!RIND!COMT",
            "(b18,b11,A)",
            dataStructure: 1)));

        // VRID - Vector Record Identifier Field
        // Format: RCNM(b11), RCID(b14), RVER(b12), RUIN(b11)
        fields.Add(("VRID", CreateDdrFieldData(
            "VRID",
            "RCNM!RCID!RVER!RUIN",
            "(b11,b14,b12,b11)")));

        // ATTV - Vector Record Attribute Field (repeating)
        // Format: *ATTL(b12), ATVL(A)
        fields.Add(("ATTV", CreateDdrFieldData(
            "ATTV",
            "*ATTL!ATVL",
            "(b12,A)",
            dataStructure: 1)));

        // VRPT - Vector Record Pointer Field (repeating)
        // Format: *NAME(b15), ORNT(b11), USAG(b11), TOPI(b11), MASK(b11)
        fields.Add(("VRPT", CreateDdrFieldData(
            "VRPT",
            "*NAME!ORNT!USAG!TOPI!MASK",
            "(b15,b11,b11,b11,b11)",
            dataStructure: 1)));

        // SG2D - 2-D Coordinate Field (repeating)
        // Format: *YCOO(b24), XCOO(b24)
        fields.Add(("SG2D", CreateDdrFieldData(
            "SG2D",
            "*YCOO!XCOO",
            "(b24,b24)",
            dataStructure: 1)));

        // SG3D - 3-D Coordinate (Sounding Array) Field (repeating)
        // Format: *YCOO(b24), XCOO(b24), VE3D(b24)
        fields.Add(("SG3D", CreateDdrFieldData(
            "SG3D",
            "*YCOO!XCOO!VE3D",
            "(b24,b24,b24)",
            dataStructure: 1)));

        // FSPC - Feature Record to Spatial Record Pointer Control Field
        // Format: FSUI(b11), FSIX(b12), NSPT(b12)
        fields.Add(("FSPC", CreateDdrFieldData(
            "FSPC",
            "FSUI!FSIX!NSPT",
            "(b11,b12,b12)")));

        // FFPC - Feature Record to Feature Object Pointer Control Field
        // Format: FFUI(b11), FFIX(b12), NFPT(b12)
        fields.Add(("FFPC", CreateDdrFieldData(
            "FFPC",
            "FFUI!FFIX!NFPT",
            "(b11,b12,b12)")));

        // VRPC - Vector Record Pointer Control Field
        // Format: VPUI(b11), VPIX(b12), NVPT(b12)
        fields.Add(("VRPC", CreateDdrFieldData(
            "VRPC",
            "VPUI!VPIX!NVPT",
            "(b11,b12,b12)")));

        // SGCC - Coordinate Control Field
        // Format: CCUI(b11), CCIX(b12), CCNC(b12)
        fields.Add(("SGCC", CreateDdrFieldData(
            "SGCC",
            "CCUI!CCIX!CCNC",
            "(b11,b12,b12)")));

        return CreateDdrRecord(fields.ToArray());
    }

    /// <summary>
    /// Creates DDR field data with the standard ISO 8211 structure.
    /// </summary>
    private static byte[] CreateDdrFieldData(string fieldName, string subfieldDescriptors, string formatControls, int dataStructure = 0, int dataType = 6)
    {
        using var ms = new MemoryStream();

        // Field controls: data structure code + data type code (2 chars)
        ms.WriteByte((byte)('0' + dataStructure));
        ms.WriteByte((byte)('0' + dataType));

        // Field name and subfield descriptors
        var descriptors = string.IsNullOrEmpty(fieldName) 
            ? subfieldDescriptors 
            : (string.IsNullOrEmpty(subfieldDescriptors) ? fieldName : $"{fieldName}!{subfieldDescriptors}");
        
        if (!string.IsNullOrEmpty(descriptors))
        {
            ms.Write(Encoding.ASCII.GetBytes(descriptors));
        }
        ms.WriteByte(UnitTerminator);

        // Format controls
        ms.Write(Encoding.ASCII.GetBytes(formatControls));
        ms.WriteByte(FieldTerminator);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates a DDR record from field definitions.
    /// </summary>
    private static byte[] CreateDdrRecord((string tag, byte[] data)[] fields)
    {
        // Calculate directory entries
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
        
        // DDR leader: 'L' for leader identifier, field control length = 2
        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3LE1 02{baseAddress:D5}   3304"
        );
        
        var record = new byte[recordLength];
        var offset = 0;
        
        // Copy leader
        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;
        
        // Copy directory entries
        foreach (var entry in directoryEntries)
        {
            Array.Copy(entry, 0, record, offset, entry.Length);
            offset += entry.Length;
        }
        
        // Directory terminator
        record[offset++] = FieldTerminator;
        
        // Copy field data
        foreach (var (_, data) in fields)
        {
            Array.Copy(data, 0, record, offset, data.Length);
            offset += data.Length;
        }
        
        return record;
    }

    /// <summary>
    /// Creates an S-57 DSID (Data Set Identification) record.
    /// </summary>
    private static byte[] CreateDsidRecord(
        byte rcnm = 10,
        uint rcid = 1,
        byte intu = 5,
        string dsnm = "TEST",
        string edtn = "1",
        string updn = "0",
        string uadt = "20250101",
        string isdt = "20250101",
        string sted = "03.1",
        ushort agen = 540)
    {
        // Build DSID field data
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        writer.Write(rcnm);           // RCNM
        writer.Write(rcid);           // RCID
        writer.Write((byte)1);        // EXPP
        writer.Write(intu);           // INTU
        WriteString(writer, dsnm);    // DSNM
        WriteString(writer, edtn);    // EDTN
        WriteString(writer, updn);    // UPDN
        WriteString(writer, uadt);    // UADT
        WriteString(writer, isdt);    // ISDT
        WriteString(writer, sted);    // STED
        writer.Write((byte)1);        // PRSP
        WriteString(writer, "");      // PSDN
        WriteString(writer, "");      // PRED
        writer.Write((byte)1);        // PROF
        writer.Write(agen);           // AGEN
        WriteString(writer, "");      // COMT
        writer.Write((byte)0x1E);     // Field terminator
        
        var dsidData = ms.ToArray();
        
        return CreateDataRecord("DSID", dsidData);
    }

    /// <summary>
    /// Creates an S-57 DSPM (Data Set Parameters) record.
    /// </summary>
    private static byte[] CreateDspmRecord(
        byte rcnm = 20,
        uint rcid = 1,
        byte hdat = 2,
        byte vdat = 17,
        byte sdat = 23,
        uint cscl = 22000,
        byte duni = 1,
        byte huni = 1,
        byte puni = 1,
        byte coun = 1,
        uint comf = 10000000,
        uint somf = 10)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        writer.Write(rcnm);           // RCNM
        writer.Write(rcid);           // RCID
        writer.Write(hdat);           // HDAT
        writer.Write(vdat);           // VDAT
        writer.Write(sdat);           // SDAT
        writer.Write(cscl);           // CSCL
        writer.Write(duni);           // DUNI
        writer.Write(huni);           // HUNI
        writer.Write(puni);           // PUNI
        writer.Write(coun);           // COUN
        writer.Write(comf);           // COMF
        writer.Write(somf);           // SOMF
        WriteString(writer, "");      // COMT
        writer.Write((byte)0x1E);     // Field terminator
        
        var dspmData = ms.ToArray();
        
        return CreateDataRecord("DSPM", dspmData);
    }

    /// <summary>
    /// Creates an S-57 Feature Record (FRID).
    /// </summary>
    private static byte[] CreateFeatureRecord(
        byte rcnm = 100,
        uint rcid = 1,
        byte prim = 1,
        byte grup = 2,
        ushort objl = 75,
        ushort rver = 1,
        byte ruin = 1,
        S57AttributeValue[]? attributes = null,
        S57SpatialPointer[]? spatialPointers = null,
        S57FieldUpdateControl? spatialPointerControl = null,
        S57FeaturePointer[]? featurePointers = null,
        S57FieldUpdateControl? featurePointerControl = null)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        // FRID field
        writer.Write(rcnm);           // RCNM
        writer.Write(rcid);           // RCID
        writer.Write(prim);           // PRIM
        writer.Write(grup);           // GRUP
        writer.Write(objl);           // OBJL
        writer.Write(rver);           // RVER
        writer.Write(ruin);           // RUIN
        writer.Write((byte)0x1E);     // Field terminator
        
        var fridData = ms.ToArray();
        
        // Build fields dictionary
        var fields = new List<(string tag, byte[] data)>
        {
            ("FRID", fridData)
        };
        
        // Add ATTF field if attributes provided
        if (attributes != null && attributes.Length > 0)
        {
            using var attfMs = new MemoryStream();
            using var attfWriter = new BinaryWriter(attfMs);
            
            foreach (var attr in attributes)
            {
                attfWriter.Write((ushort)attr.AttributeCode);
                WriteString(attfWriter, attr.Value);
            }
            attfWriter.Write((byte)0x1E);
            
            fields.Add(("ATTF", attfMs.ToArray()));
        }
        
        // Add FSPC field if spatial pointer control provided
        if (spatialPointerControl.HasValue)
        {
            using var fspcMs = new MemoryStream();
            using var fspcWriter = new BinaryWriter(fspcMs);
            
            fspcWriter.Write((byte)spatialPointerControl.Value.UpdateInstruction);
            fspcWriter.Write((ushort)spatialPointerControl.Value.Index);
            fspcWriter.Write((ushort)spatialPointerControl.Value.Count);
            fspcWriter.Write((byte)0x1E);
            
            fields.Add(("FSPC", fspcMs.ToArray()));
        }

        // Add FSPT field if spatial pointers provided
        if (spatialPointers != null && spatialPointers.Length > 0)
        {
            using var fsptMs = new MemoryStream();
            using var fsptWriter = new BinaryWriter(fsptMs);
            
            foreach (var ptr in spatialPointers)
            {
                fsptWriter.Write((byte)ptr.Name.RecordNameCode);
                fsptWriter.Write((uint)ptr.Name.RecordId);
                fsptWriter.Write((byte)ptr.Orientation);
                fsptWriter.Write((byte)ptr.Usage);
                fsptWriter.Write((byte)ptr.Mask);
            }
            fsptWriter.Write((byte)0x1E);
            
            fields.Add(("FSPT", fsptMs.ToArray()));
        }

        // Add FFPC field if feature pointer control provided
        if (featurePointerControl.HasValue)
        {
            using var ffpcMs = new MemoryStream();
            using var ffpcWriter = new BinaryWriter(ffpcMs);
            
            ffpcWriter.Write((byte)featurePointerControl.Value.UpdateInstruction);
            ffpcWriter.Write((ushort)featurePointerControl.Value.Index);
            ffpcWriter.Write((ushort)featurePointerControl.Value.Count);
            ffpcWriter.Write((byte)0x1E);
            
            fields.Add(("FFPC", ffpcMs.ToArray()));
        }

        // Add FFPT field if feature pointers provided
        if (featurePointers != null && featurePointers.Length > 0)
        {
            using var ffptMs = new MemoryStream();
            using var ffptWriter = new BinaryWriter(ffptMs);
            
            foreach (var ptr in featurePointers)
            {
                // LNAM: AGEN(2) + FIDN(4) + FIDS(2)
                ffptWriter.Write((ushort)ptr.Name.AgencyCode);
                ffptWriter.Write((uint)ptr.Name.FeatureId);
                ffptWriter.Write((ushort)ptr.Name.FeatureSubdivision);
                ffptWriter.Write((byte)ptr.Relationship);
                WriteString(ffptWriter, ptr.Comment);
            }
            ffptWriter.Write((byte)0x1E);
            
            fields.Add(("FFPT", ffptMs.ToArray()));
        }
        
        return CreateDataRecordMultiField(fields.ToArray());
    }

    /// <summary>
    /// Creates an S-57 Vector Record (VRID).
    /// </summary>
    private static byte[] CreateVectorRecord(
        byte rcnm = 110,
        uint rcid = 1,
        ushort rver = 1,
        byte ruin = 1,
        S57Coordinate2D[]? coordinates = null,
        S57Sounding[]? soundings = null,
        S57VectorPointer[]? vectorPointers = null,
        S57FieldUpdateControl? vectorPointerControl = null,
        S57FieldUpdateControl? coordinateControl = null)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        // VRID field
        writer.Write(rcnm);           // RCNM
        writer.Write(rcid);           // RCID
        writer.Write(rver);           // RVER
        writer.Write(ruin);           // RUIN
        writer.Write((byte)0x1E);     // Field terminator
        
        var vridData = ms.ToArray();
        
        // Build fields dictionary
        var fields = new List<(string tag, byte[] data)>
        {
            ("VRID", vridData)
        };
        
        // Add VRPC field if vector pointer control provided
        if (vectorPointerControl.HasValue)
        {
            using var vrpcMs = new MemoryStream();
            using var vrpcWriter = new BinaryWriter(vrpcMs);
            
            vrpcWriter.Write((byte)vectorPointerControl.Value.UpdateInstruction);
            vrpcWriter.Write((ushort)vectorPointerControl.Value.Index);
            vrpcWriter.Write((ushort)vectorPointerControl.Value.Count);
            vrpcWriter.Write((byte)0x1E);
            
            fields.Add(("VRPC", vrpcMs.ToArray()));
        }

        // Add VRPT field if vector pointers provided
        if (vectorPointers != null && vectorPointers.Length > 0)
        {
            using var vrptMs = new MemoryStream();
            using var vrptWriter = new BinaryWriter(vrptMs);
            
            foreach (var ptr in vectorPointers)
            {
                vrptWriter.Write((byte)ptr.Name.RecordNameCode);
                vrptWriter.Write((uint)ptr.Name.RecordId);
                vrptWriter.Write((byte)ptr.Orientation);
                vrptWriter.Write((byte)ptr.Usage);
                vrptWriter.Write((byte)ptr.Topology);
                vrptWriter.Write((byte)ptr.Mask);
            }
            vrptWriter.Write((byte)0x1E);
            
            fields.Add(("VRPT", vrptMs.ToArray()));
        }

        // Add SGCC field if coordinate control provided
        if (coordinateControl.HasValue)
        {
            using var sgccMs = new MemoryStream();
            using var sgccWriter = new BinaryWriter(sgccMs);
            
            sgccWriter.Write((byte)coordinateControl.Value.UpdateInstruction);
            sgccWriter.Write((ushort)coordinateControl.Value.Index);
            sgccWriter.Write((ushort)coordinateControl.Value.Count);
            sgccWriter.Write((byte)0x1E);
            
            fields.Add(("SGCC", sgccMs.ToArray()));
        }

        // Add SG2D field if coordinates provided
        if (coordinates != null && coordinates.Length > 0)
        {
            using var sg2dMs = new MemoryStream();
            using var sg2dWriter = new BinaryWriter(sg2dMs);
            
            foreach (var coord in coordinates)
            {
                sg2dWriter.Write(coord.Y);
                sg2dWriter.Write(coord.X);
            }
            sg2dWriter.Write((byte)0x1E);
            
            fields.Add(("SG2D", sg2dMs.ToArray()));
        }
        
        // Add SG3D field if soundings provided
        if (soundings != null && soundings.Length > 0)
        {
            using var sg3dMs = new MemoryStream();
            using var sg3dWriter = new BinaryWriter(sg3dMs);
            
            foreach (var snd in soundings)
            {
                sg3dWriter.Write(snd.Y);
                sg3dWriter.Write(snd.X);
                sg3dWriter.Write(snd.Depth);
            }
            sg3dWriter.Write((byte)0x1E);
            
            fields.Add(("SG3D", sg3dMs.ToArray()));
        }
        
        return CreateDataRecordMultiField(fields.ToArray());
    }

    /// <summary>
    /// Creates a data record with a single field.
    /// </summary>
    private static byte[] CreateDataRecord(string tag, byte[] fieldData)
    {
        return CreateDataRecordMultiField((tag, fieldData));
    }

    /// <summary>
    /// Creates a data record with multiple fields.
    /// </summary>
    private static byte[] CreateDataRecordMultiField(params (string tag, byte[] data)[] fields)
    {
        var fieldTerminator = (byte)0x1E;
        
        // Calculate directory entries
        var directoryEntries = new List<byte[]>();
        var currentPosition = 0;
        
        foreach (var (tag, data) in fields)
        {
            var entry = Encoding.ASCII.GetBytes($"{tag}{data.Length:D3}{currentPosition:D3}");
            directoryEntries.Add(entry);
            currentPosition += data.Length;
        }
        
        var directorySize = directoryEntries.Sum(e => e.Length);
        var baseAddress = 24 + directorySize + 1; // +1 for directory terminator
        var totalFieldSize = fields.Sum(f => f.data.Length);
        var recordLength = baseAddress + totalFieldSize;
        
        var leader = Encoding.ASCII.GetBytes(
            $"{recordLength:D5}3DE1 00{baseAddress:D5}   3304"
        );
        
        var record = new byte[recordLength];
        var offset = 0;
        
        // Copy leader
        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;
        
        // Copy directory entries
        foreach (var entry in directoryEntries)
        {
            Array.Copy(entry, 0, record, offset, entry.Length);
            offset += entry.Length;
        }
        
        // Directory terminator
        record[offset++] = fieldTerminator;
        
        // Copy field data
        foreach (var (_, data) in fields)
        {
            Array.Copy(data, 0, record, offset, data.Length);
            offset += data.Length;
        }
        
        return record;
    }

    /// <summary>
    /// Writes a string followed by a unit terminator.
    /// </summary>
    private static void WriteString(BinaryWriter writer, string value)
    {
        writer.Write(Encoding.ASCII.GetBytes(value));
        writer.Write((byte)0x1F); // Unit terminator
    }

    #endregion

    #region S57Document Tests

    [Fact]
    public void Read_EmptyDocument_ReturnsDocumentWithNoRecords()
    {
        // Arrange
        var data = CreateS57Document();
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.NotNull(document);
        Assert.Null(document.DataSetIdentification);
        Assert.Null(document.DataSetParameters);
        Assert.Empty(document.FeatureRecords);
        Assert.Empty(document.VectorRecords);
    }

    [Fact]
    public void Read_DocumentWithDsid_ParsesDataSetIdentification()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(
            rcnm: 10,
            rcid: 1,
            intu: 5,
            dsnm: "US5WA51M",
            edtn: "2",
            updn: "1",
            uadt: "20250115",
            isdt: "20250101",
            sted: "03.1",
            agen: 540
        );
        var data = CreateS57Document(dsidRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.NotNull(document.DataSetIdentification);
        Assert.Equal(10, document.DataSetIdentification.RecordName.RecordNameCode);
        Assert.Equal(1, document.DataSetIdentification.RecordName.RecordId);
        Assert.Equal(5, document.DataSetIdentification.IntendedUsage);
        Assert.Equal("US5WA51M", document.DataSetIdentification.DataSetName);
        Assert.Equal("2", document.DataSetIdentification.EditionNumber);
        Assert.Equal("1", document.DataSetIdentification.UpdateNumber);
        Assert.Equal("20250115", document.DataSetIdentification.UpdateApplicationDate);
        Assert.Equal("20250101", document.DataSetIdentification.IssueDate);
        Assert.Equal("03.1", document.DataSetIdentification.S57EditionNumber);
        Assert.Equal(540, document.DataSetIdentification.ProducingAgency);
    }

    [Fact]
    public void Read_DocumentWithDspm_ParsesDataSetParameters()
    {
        // Arrange
        var dspmRecord = CreateDspmRecord(
            rcnm: 20,
            rcid: 1,
            hdat: 2,
            vdat: 17,
            sdat: 23,
            cscl: 22000,
            duni: 1,
            huni: 1,
            puni: 1,
            coun: 1,
            comf: 10000000,
            somf: 10
        );
        var data = CreateS57Document(dspmRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.NotNull(document.DataSetParameters);
        Assert.Equal(20, document.DataSetParameters.RecordName.RecordNameCode);
        Assert.Equal(1, document.DataSetParameters.RecordName.RecordId);
        Assert.Equal(2, document.DataSetParameters.HorizontalDatum);
        Assert.Equal(17, document.DataSetParameters.VerticalDatum);
        Assert.Equal(23, document.DataSetParameters.SoundingDatum);
        Assert.Equal(22000, document.DataSetParameters.CompilationScale);
        Assert.Equal(1, document.DataSetParameters.DepthUnits);
        Assert.Equal(1, document.DataSetParameters.HeightUnits);
        Assert.Equal(1, document.DataSetParameters.PositionalUnits);
        Assert.Equal(1, document.DataSetParameters.CoordinateUnits);
        Assert.Equal(10000000, document.DataSetParameters.CoordinateMultiplicationFactor);
        Assert.Equal(10, document.DataSetParameters.SoundingMultiplicationFactor);
    }

    [Fact]
    public void Read_DocumentWithFeatureRecord_ParsesFeatureRecord()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(
            rcnm: 100,
            rcid: 42,
            prim: (byte)S57GeometricPrimitive.Point,
            grup: 2,
            objl: 75, // BUOYAGE
            rver: 1,
            ruin: (byte)S57UpdateInstruction.Insert
        );
        var data = CreateS57Document(featureRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Single(document.FeatureRecords);
        var feature = document.FeatureRecords[0];
        Assert.Equal(100, feature.RecordName.RecordNameCode);
        Assert.Equal(42, feature.RecordName.RecordId);
        Assert.Equal(S57GeometricPrimitive.Point, feature.Primitive);
        Assert.Equal(2, feature.Group);
        Assert.Equal(S57ObjectCode.LIGHTS, feature.ObjectCode);
        Assert.Equal(1, feature.RecordVersion);
        Assert.Equal(S57UpdateInstruction.Insert, feature.UpdateInstruction);
    }

    [Fact]
    public void Read_DocumentWithFeatureRecordAndAttributes_ParsesAttributes()
    {
        // Arrange
        var attributes = new[]
        {
            new S57AttributeValue(1, "TestValue1"),
            new S57AttributeValue(2, "TestValue2"),
            new S57AttributeValue(116, "RED")
        };
        var featureRecord = CreateFeatureRecord(
            rcnm: 100,
            rcid: 1,
            prim: 1,
            grup: 2,
            objl: 75,
            rver: 1,
            ruin: 1,
            attributes: attributes
        );
        var data = CreateS57Document(featureRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Single(document.FeatureRecords);
        var feature = document.FeatureRecords[0];
        Assert.Equal(3, feature.Attributes.Length);
        Assert.Equal(1, feature.Attributes[0].AttributeCode);
        Assert.Equal("TestValue1", feature.Attributes[0].Value);
        Assert.Equal(2, feature.Attributes[1].AttributeCode);
        Assert.Equal("TestValue2", feature.Attributes[1].Value);
        Assert.Equal(116, feature.Attributes[2].AttributeCode);
        Assert.Equal("RED", feature.Attributes[2].Value);
    }

    [Fact]
    public void Read_DocumentWithVectorRecord_ParsesVectorRecord()
    {
        // Arrange
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.IsolatedNode,
            rcid: 100,
            rver: 1,
            ruin: (byte)S57UpdateInstruction.Insert
        );
        var data = CreateS57Document(vectorRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Single(document.VectorRecords);
        var vector = document.VectorRecords[0];
        Assert.Equal(S57RecordNameCodes.IsolatedNode, vector.RecordName.RecordNameCode);
        Assert.Equal(100, vector.RecordName.RecordId);
        Assert.Equal(1, vector.RecordVersion);
        Assert.Equal(S57UpdateInstruction.Insert, vector.UpdateInstruction);
    }

    [Fact]
    public void Read_DocumentWithVectorRecordAnd2DCoordinates_ParsesCoordinates()
    {
        // Arrange
        var coordinates = new[]
        {
            new S57Coordinate2D { X = -1225000000, Y = 475000000 },
            new S57Coordinate2D { X = -1225100000, Y = 475100000 }
        };
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.Edge,
            rcid: 50,
            rver: 1,
            ruin: 1,
            coordinates: coordinates
        );
        var data = CreateS57Document(vectorRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Single(document.VectorRecords);
        var vector = document.VectorRecords[0];
        Assert.Equal(2, vector.Coordinates2D.Length);
        Assert.Equal(-1225000000, vector.Coordinates2D[0].X);
        Assert.Equal(475000000, vector.Coordinates2D[0].Y);
        Assert.Equal(-1225100000, vector.Coordinates2D[1].X);
        Assert.Equal(475100000, vector.Coordinates2D[1].Y);
    }

    [Fact]
    public void Read_DocumentWithVectorRecordAndSoundings_ParsesSoundings()
    {
        // Arrange
        var soundings = new[]
        {
            new S57Sounding { X = -1225000000, Y = 475000000, Depth = 150 },
            new S57Sounding { X = -1225100000, Y = 475100000, Depth = 200 }
        };
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.IsolatedNode,
            rcid: 60,
            rver: 1,
            ruin: 1,
            soundings: soundings
        );
        var data = CreateS57Document(vectorRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Single(document.VectorRecords);
        var vector = document.VectorRecords[0];
        Assert.Equal(2, vector.Soundings.Length);
        Assert.Equal(-1225000000, vector.Soundings[0].X);
        Assert.Equal(475000000, vector.Soundings[0].Y);
        Assert.Equal(150, vector.Soundings[0].Depth);
        Assert.Equal(-1225100000, vector.Soundings[1].X);
        Assert.Equal(475100000, vector.Soundings[1].Y);
        Assert.Equal(200, vector.Soundings[1].Depth);
    }

    [Fact]
    public void Read_DocumentWithMultipleRecords_ParsesAllRecords()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(dsnm: "TESTDATA");
        var dspmRecord = CreateDspmRecord(cscl: 50000, comf: 10000000);
        var featureRecord1 = CreateFeatureRecord(rcid: 1, objl: 75);
        var featureRecord2 = CreateFeatureRecord(rcid: 2, objl: 159);
        var vectorRecord1 = CreateVectorRecord(rcnm: S57RecordNameCodes.IsolatedNode, rcid: 1);
        var vectorRecord2 = CreateVectorRecord(rcnm: S57RecordNameCodes.Edge, rcid: 2);
        
        var data = CreateS57Document(
            dsidRecord, dspmRecord, 
            featureRecord1, featureRecord2,
            vectorRecord1, vectorRecord2
        );
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.NotNull(document.DataSetIdentification);
        Assert.Equal("TESTDATA", document.DataSetIdentification.DataSetName);
        Assert.NotNull(document.DataSetParameters);
        Assert.Equal(50000, document.DataSetParameters.CompilationScale);
        Assert.Equal(2, document.FeatureRecords.Length);
        Assert.Equal(2, document.VectorRecords.Length);
    }

    #endregion

    #region S57Document Helper Methods Tests

    [Fact]
    public void GetFeatureRecord_ByRecordName_ReturnsMatchingRecord()
    {
        // Arrange
        var featureRecord1 = CreateFeatureRecord(rcnm: 100, rcid: 1, objl: 75);
        var featureRecord2 = CreateFeatureRecord(rcnm: 100, rcid: 2, objl: 159);
        var data = CreateS57Document(featureRecord1, featureRecord2);
        var document = S57DocumentReader.Read(data);
        
        // Act
        var targetName = S57RecordName.FromRcnmRcid(100, 2);
        var result = document.GetFeatureRecord(targetName);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.RecordName.RecordId);
        Assert.Equal(S57ObjectCode.WRECKS, result.ObjectCode);
    }

    [Fact]
    public void GetFeatureRecord_NonExistentName_ReturnsNull()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(rcnm: 100, rcid: 1, objl: 75);
        var data = CreateS57Document(featureRecord);
        var document = S57DocumentReader.Read(data);
        
        // Act
        var targetName = S57RecordName.FromRcnmRcid(100, 999);
        var result = document.GetFeatureRecord(targetName);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetVectorRecord_ByRecordName_ReturnsMatchingRecord()
    {
        // Arrange
        var vectorRecord1 = CreateVectorRecord(rcnm: S57RecordNameCodes.IsolatedNode, rcid: 1);
        var vectorRecord2 = CreateVectorRecord(rcnm: S57RecordNameCodes.Edge, rcid: 2);
        var data = CreateS57Document(vectorRecord1, vectorRecord2);
        var document = S57DocumentReader.Read(data);
        
        // Act
        var targetName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 2);
        var result = document.GetVectorRecord(targetName);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(S57RecordNameCodes.Edge, result.RecordName.RecordNameCode);
        Assert.Equal(2, result.RecordName.RecordId);
    }

    [Fact]
    public void GetFeaturesByObjectCode_ReturnsMatchingRecords()
    {
        // Arrange
        var featureRecord1 = CreateFeatureRecord(rcnm: 100, rcid: 1, objl: 75);
        var featureRecord2 = CreateFeatureRecord(rcnm: 100, rcid: 2, objl: 159);
        var featureRecord3 = CreateFeatureRecord(rcnm: 100, rcid: 3, objl: 75);
        var data = CreateS57Document(featureRecord1, featureRecord2, featureRecord3);
        var document = S57DocumentReader.Read(data);
        
        // Act
        var results = document.GetFeaturesByObjectCode(S57ObjectCode.LIGHTS).ToArray();
        
        // Assert
        Assert.Equal(2, results.Length);
        Assert.All(results, r => Assert.Equal(S57ObjectCode.LIGHTS, r.ObjectCode));
    }

    [Fact]
    public void CoordinateMultiplicationFactor_WithDspm_ReturnsValue()
    {
        // Arrange
        var dspmRecord = CreateDspmRecord(comf: 10000000);
        var data = CreateS57Document(dspmRecord);
        var document = S57DocumentReader.Read(data);
        
        // Act
        var result = document.CoordinateMultiplicationFactor;
        
        // Assert
        Assert.Equal(10000000, result);
    }

    [Fact]
    public void CoordinateMultiplicationFactor_WithoutDspm_ReturnsDefault()
    {
        // Arrange
        var data = CreateS57Document();
        var document = S57DocumentReader.Read(data);
        
        // Act
        var result = document.CoordinateMultiplicationFactor;
        
        // Assert
        Assert.Equal(10000000, result); // Default value
    }

    [Fact]
    public void SoundingMultiplicationFactor_WithDspm_ReturnsValue()
    {
        // Arrange
        var dspmRecord = CreateDspmRecord(somf: 100);
        var data = CreateS57Document(dspmRecord);
        var document = S57DocumentReader.Read(data);
        
        // Act
        var result = document.SoundingMultiplicationFactor;
        
        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void SoundingMultiplicationFactor_WithoutDspm_ReturnsDefault()
    {
        // Arrange
        var data = CreateS57Document();
        var document = S57DocumentReader.Read(data);
        
        // Act
        var result = document.SoundingMultiplicationFactor;
        
        // Assert
        Assert.Equal(10, result); // Default value
    }

    #endregion

    #region S57RecordName Tests

    [Fact]
    public void S57RecordName_FromRcnmRcid_CreatesCorrectly()
    {
        // Act
        var name = S57RecordName.FromRcnmRcid(100, 42);
        
        // Assert
        Assert.Equal(100, name.RecordNameCode);
        Assert.Equal(42, name.RecordId);
        Assert.Equal(0, name.AgencyCode);
        Assert.Equal(0, name.FeatureId);
        Assert.Equal(0, name.FeatureSubdivision);
    }

    [Fact]
    public void S57RecordName_FromLongName_CreatesCorrectly()
    {
        // Act
        var name = S57RecordName.FromLongName(540, 12345, 1);
        
        // Assert
        Assert.Equal(540, name.AgencyCode);
        Assert.Equal(12345, name.FeatureId);
        Assert.Equal(1, name.FeatureSubdivision);
    }

    [Fact]
    public void S57RecordName_Equality_SameValues_ReturnsTrue()
    {
        // Arrange
        var name1 = S57RecordName.FromRcnmRcid(100, 42);
        var name2 = S57RecordName.FromRcnmRcid(100, 42);
        
        // Assert
        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
        Assert.False(name1 != name2);
    }

    [Fact]
    public void S57RecordName_Equality_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var name1 = S57RecordName.FromRcnmRcid(100, 42);
        var name2 = S57RecordName.FromRcnmRcid(100, 43);
        
        // Assert
        Assert.NotEqual(name1, name2);
        Assert.False(name1 == name2);
        Assert.True(name1 != name2);
    }

    [Fact]
    public void S57RecordName_GetHashCode_SameValues_ReturnsSameHash()
    {
        // Arrange
        var name1 = S57RecordName.FromRcnmRcid(100, 42);
        var name2 = S57RecordName.FromRcnmRcid(100, 42);
        
        // Assert
        Assert.Equal(name1.GetHashCode(), name2.GetHashCode());
    }

    [Fact]
    public void S57RecordName_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var name = S57RecordName.FromRcnmRcid(100, 42);
        
        // Act
        var result = name.ToString();
        
        // Assert
        Assert.Equal("RCNM=100, RCID=42", result);
    }

    #endregion

    #region S57AttributeValue Tests

    [Fact]
    public void S57AttributeValue_Constructor_SetsProperties()
    {
        // Act
        var attr = new S57AttributeValue(116, "RED");
        
        // Assert
        Assert.Equal(116, attr.AttributeCode);
        Assert.Equal("RED", attr.Value);
    }

    [Fact]
    public void S57AttributeValue_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var attr = new S57AttributeValue(116, "RED");
        
        // Act
        var result = attr.ToString();
        
        // Assert
        Assert.Equal("ATTL=116, ATVL=RED", result);
    }

    #endregion

    #region S57Coordinate2D Tests

    [Fact]
    public void S57Coordinate2D_ToDecimalDegrees_ConvertsCorrectly()
    {
        // Arrange
        var coord = new S57Coordinate2D { X = -1225000000, Y = 475000000 };
        var multiplicationFactor = 10000000;
        
        // Act
        var (longitude, latitude) = coord.ToDecimalDegrees(multiplicationFactor);
        
        // Assert
        Assert.Equal(-122.5, longitude, 6);
        Assert.Equal(47.5, latitude, 6);
    }

    #endregion

    #region S57Sounding Tests

    [Fact]
    public void S57Sounding_ToDecimalValues_ConvertsCorrectly()
    {
        // Arrange
        var sounding = new S57Sounding { X = -1225000000, Y = 475000000, Depth = 150 };
        var coordFactor = 10000000;
        var soundingFactor = 10;
        
        // Act
        var (longitude, latitude, depth) = sounding.ToDecimalValues(coordFactor, soundingFactor);
        
        // Assert
        Assert.Equal(-122.5, longitude, 6);
        Assert.Equal(47.5, latitude, 6);
        Assert.Equal(15.0, depth, 6);
    }

    #endregion

    #region S57GeometricPrimitive Tests

    [Fact]
    public void S57GeometricPrimitive_Values_AreCorrect()
    {
        Assert.Equal(1, (byte)S57GeometricPrimitive.Point);
        Assert.Equal(2, (byte)S57GeometricPrimitive.Line);
        Assert.Equal(3, (byte)S57GeometricPrimitive.Area);
        Assert.Equal(255, (byte)S57GeometricPrimitive.None);
    }

    #endregion

    #region S57RecordNameCodes Tests

    [Fact]
    public void S57RecordNameCodes_FeatureCode_IsCorrect()
    {
        Assert.Equal(100, S57RecordNameCodes.Feature);
    }

    [Fact]
    public void S57RecordNameCodes_VectorCodes_AreCorrect()
    {
        Assert.Equal(110, S57RecordNameCodes.IsolatedNode);
        Assert.Equal(120, S57RecordNameCodes.ConnectedNode);
        Assert.Equal(130, S57RecordNameCodes.Edge);
        Assert.Equal(140, S57RecordNameCodes.Face);
    }

    [Fact]
    public void S57RecordNameCodes_DataSetCodes_AreCorrect()
    {
        Assert.Equal(10, S57RecordNameCodes.DataSetGeneralInfo);
        Assert.Equal(20, S57RecordNameCodes.DataSetGeoReference);
        Assert.Equal(30, S57RecordNameCodes.DataSetHistory);
        Assert.Equal(40, S57RecordNameCodes.DataSetAccuracy);
    }

    #endregion

    #region Read Method Variants Tests

    [Fact]
    public void Read_ByteArray_ReturnsDocument()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(dsnm: "BYTEARRAY");
        var data = CreateS57Document(dsidRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.NotNull(document.DataSetIdentification);
        Assert.Equal("BYTEARRAY", document.DataSetIdentification.DataSetName);
    }

    [Fact]
    public void Read_ReadOnlySpan_ReturnsDocument()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(dsnm: "SPANTEST");
        var data = CreateS57Document(dsidRecord);
        
        // Act
        var document = S57DocumentReader.Read(data.AsSpan());
        
        // Assert
        Assert.NotNull(document.DataSetIdentification);
        Assert.Equal("SPANTEST", document.DataSetIdentification.DataSetName);
    }

    [Fact]
    public void Read_Stream_ReturnsDocument()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(dsnm: "STREAMTEST");
        var data = CreateS57Document(dsidRecord);
        
        // Act
        S57Document document;
        using (var stream = new MemoryStream(data))
        {
            document = S57DocumentReader.Read(stream);
        }
        
        // Assert
        Assert.NotNull(document.DataSetIdentification);
        Assert.Equal("STREAMTEST", document.DataSetIdentification.DataSetName);
    }

    [Fact]
    public async Task ReadAsync_Stream_ReturnsDocument()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(dsnm: "ASYNCSTREAM");
        var data = CreateS57Document(dsidRecord);
        
        // Act
        S57Document document;
        using (var stream = new MemoryStream(data))
        {
            document = await S57DocumentReader.ReadAsync(stream);
        }
        
        // Assert
        Assert.NotNull(document.DataSetIdentification);
        Assert.Equal("ASYNCSTREAM", document.DataSetIdentification.DataSetName);
    }

    [Fact]
    public void ReadFromFile_ValidFile_ReturnsDocument()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(dsnm: "FILETEST");
        var data = CreateS57Document(dsidRecord);
        var tempFile = Path.GetTempFileName();
        
        try
        {
            File.WriteAllBytes(tempFile, data);
            
            // Act
            var document = S57DocumentReader.ReadFromFile(tempFile);
            
            // Assert
            Assert.NotNull(document.DataSetIdentification);
            Assert.Equal("FILETEST", document.DataSetIdentification.DataSetName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFileAsync_ValidFile_ReturnsDocument()
    {
        // Arrange
        var dsidRecord = CreateDsidRecord(dsnm: "ASYNCFILE");
        var data = CreateS57Document(dsidRecord);
        var tempFile = Path.GetTempFileName();
        
        try
        {
            await File.WriteAllBytesAsync(tempFile, data);
            
            // Act
            var document = await S57DocumentReader.ReadFromFileAsync(tempFile);
            
            // Assert
            Assert.NotNull(document.DataSetIdentification);
            Assert.Equal("ASYNCFILE", document.DataSetIdentification.DataSetName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region Update Control Field Tests

    [Fact]
    public void Read_FeatureRecordWithFspc_ParsesSpatialPointerControl()
    {
        // Arrange
        var fspc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Insert,
            Index = 3,
            Count = 2
        };
        var spatialPointers = new[]
        {
            new S57SpatialPointer
            {
                Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 10),
                Orientation = S57Orientation.Forward,
                Usage = S57UsageIndicator.Exterior,
                Mask = S57MaskingIndicator.Show
            },
            new S57SpatialPointer
            {
                Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 11),
                Orientation = S57Orientation.Reverse,
                Usage = S57UsageIndicator.Exterior,
                Mask = S57MaskingIndicator.Show
            }
        };
        var featureRecord = CreateFeatureRecord(
            rcnm: 100,
            rcid: 1,
            prim: 3,
            objl: (ushort)S57ObjectCode.DEPARE,
            ruin: (byte)S57UpdateInstruction.Modify,
            spatialPointerControl: fspc,
            spatialPointers: spatialPointers
        );
        var data = CreateS57Document(featureRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        Assert.Single(document.FeatureRecords);
        var feature = document.FeatureRecords[0];
        Assert.NotNull(feature.SpatialPointerControl);
        Assert.Equal(S57UpdateInstruction.Insert, feature.SpatialPointerControl.Value.UpdateInstruction);
        Assert.Equal(3, feature.SpatialPointerControl.Value.Index);
        Assert.Equal(2, feature.SpatialPointerControl.Value.Count);
        Assert.Equal(2, feature.SpatialPointers.Length);
    }

    [Fact]
    public void Read_FeatureRecordWithFfpc_ParsesFeaturePointerControl()
    {
        // Arrange
        var ffpc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Delete,
            Index = 1,
            Count = 3
        };
        var featureRecord = CreateFeatureRecord(
            rcnm: 100,
            rcid: 5,
            prim: 1,
            objl: (ushort)S57ObjectCode.LIGHTS,
            ruin: (byte)S57UpdateInstruction.Modify,
            featurePointerControl: ffpc
        );
        var data = CreateS57Document(featureRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        Assert.Single(document.FeatureRecords);
        var feature = document.FeatureRecords[0];
        Assert.NotNull(feature.FeaturePointerControl);
        Assert.Equal(S57UpdateInstruction.Delete, feature.FeaturePointerControl.Value.UpdateInstruction);
        Assert.Equal(1, feature.FeaturePointerControl.Value.Index);
        Assert.Equal(3, feature.FeaturePointerControl.Value.Count);
    }

    [Fact]
    public void Read_FeatureRecordWithBothControls_ParsesBothControls()
    {
        // Arrange
        var fspc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Modify,
            Index = 2,
            Count = 1
        };
        var ffpc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Insert,
            Index = 5,
            Count = 4
        };
        var featureRecord = CreateFeatureRecord(
            rcnm: 100,
            rcid: 1,
            ruin: (byte)S57UpdateInstruction.Modify,
            spatialPointerControl: fspc,
            featurePointerControl: ffpc
        );
        var data = CreateS57Document(featureRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        var feature = document.FeatureRecords[0];
        Assert.NotNull(feature.SpatialPointerControl);
        Assert.Equal(S57UpdateInstruction.Modify, feature.SpatialPointerControl.Value.UpdateInstruction);
        Assert.Equal(2, feature.SpatialPointerControl.Value.Index);
        Assert.Equal(1, feature.SpatialPointerControl.Value.Count);

        Assert.NotNull(feature.FeaturePointerControl);
        Assert.Equal(S57UpdateInstruction.Insert, feature.FeaturePointerControl.Value.UpdateInstruction);
        Assert.Equal(5, feature.FeaturePointerControl.Value.Index);
        Assert.Equal(4, feature.FeaturePointerControl.Value.Count);
    }

    [Fact]
    public void Read_FeatureRecordWithoutControls_HasNullControlProperties()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(rcnm: 100, rcid: 1, objl: 75);
        var data = CreateS57Document(featureRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        var feature = document.FeatureRecords[0];
        Assert.Null(feature.SpatialPointerControl);
        Assert.Null(feature.FeaturePointerControl);
    }

    [Fact]
    public void Read_VectorRecordWithVrpc_ParsesVectorPointerControl()
    {
        // Arrange
        var vrpc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Insert,
            Index = 1,
            Count = 2
        };
        var vectorPointers = new[]
        {
            new S57VectorPointer
            {
                Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 5),
                Orientation = S57Orientation.Forward,
                Usage = S57UsageIndicator.NotApplicable,
                Topology = S57TopologyIndicator.Beginning,
                Mask = S57MaskingIndicator.NotApplicable
            },
            new S57VectorPointer
            {
                Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 6),
                Orientation = S57Orientation.Forward,
                Usage = S57UsageIndicator.NotApplicable,
                Topology = S57TopologyIndicator.End,
                Mask = S57MaskingIndicator.NotApplicable
            }
        };
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.Edge,
            rcid: 42,
            ruin: (byte)S57UpdateInstruction.Modify,
            vectorPointerControl: vrpc,
            vectorPointers: vectorPointers
        );
        var data = CreateS57Document(vectorRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        Assert.Single(document.VectorRecords);
        var vector = document.VectorRecords[0];
        Assert.NotNull(vector.VectorPointerControl);
        Assert.Equal(S57UpdateInstruction.Insert, vector.VectorPointerControl.Value.UpdateInstruction);
        Assert.Equal(1, vector.VectorPointerControl.Value.Index);
        Assert.Equal(2, vector.VectorPointerControl.Value.Count);
        Assert.Equal(2, vector.VectorPointers.Length);
    }

    [Fact]
    public void Read_VectorRecordWithSgcc_ParsesCoordinateControl()
    {
        // Arrange
        var sgcc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Insert,
            Index = 6,
            Count = 3
        };
        var coordinates = new[]
        {
            new S57Coordinate2D { X = 100, Y = 200 },
            new S57Coordinate2D { X = 150, Y = 250 },
            new S57Coordinate2D { X = 200, Y = 300 }
        };
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.Edge,
            rcid: 10,
            ruin: (byte)S57UpdateInstruction.Modify,
            coordinateControl: sgcc,
            coordinates: coordinates
        );
        var data = CreateS57Document(vectorRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        Assert.Single(document.VectorRecords);
        var vector = document.VectorRecords[0];
        Assert.NotNull(vector.CoordinateControl);
        Assert.Equal(S57UpdateInstruction.Insert, vector.CoordinateControl.Value.UpdateInstruction);
        Assert.Equal(6, vector.CoordinateControl.Value.Index);
        Assert.Equal(3, vector.CoordinateControl.Value.Count);
        Assert.Equal(3, vector.Coordinates2D.Length);
    }

    [Fact]
    public void Read_VectorRecordWithSgccForSoundings_ParsesCoordinateControl()
    {
        // Arrange
        var sgcc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Delete,
            Index = 2,
            Count = 1
        };
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.IsolatedNode,
            rcid: 20,
            ruin: (byte)S57UpdateInstruction.Modify,
            coordinateControl: sgcc
        );
        var data = CreateS57Document(vectorRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        var vector = document.VectorRecords[0];
        Assert.NotNull(vector.CoordinateControl);
        Assert.Equal(S57UpdateInstruction.Delete, vector.CoordinateControl.Value.UpdateInstruction);
        Assert.Equal(2, vector.CoordinateControl.Value.Index);
        Assert.Equal(1, vector.CoordinateControl.Value.Count);
    }

    [Fact]
    public void Read_VectorRecordWithBothControls_ParsesBothControls()
    {
        // Arrange
        var vrpc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Modify,
            Index = 1,
            Count = 1
        };
        var sgcc = new S57FieldUpdateControl
        {
            UpdateInstruction = S57UpdateInstruction.Insert,
            Index = 10,
            Count = 5
        };
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.Edge,
            rcid: 30,
            ruin: (byte)S57UpdateInstruction.Modify,
            vectorPointerControl: vrpc,
            coordinateControl: sgcc
        );
        var data = CreateS57Document(vectorRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        var vector = document.VectorRecords[0];
        Assert.NotNull(vector.VectorPointerControl);
        Assert.Equal(S57UpdateInstruction.Modify, vector.VectorPointerControl.Value.UpdateInstruction);
        Assert.Equal(1, vector.VectorPointerControl.Value.Index);

        Assert.NotNull(vector.CoordinateControl);
        Assert.Equal(S57UpdateInstruction.Insert, vector.CoordinateControl.Value.UpdateInstruction);
        Assert.Equal(10, vector.CoordinateControl.Value.Index);
        Assert.Equal(5, vector.CoordinateControl.Value.Count);
    }

    [Fact]
    public void Read_VectorRecordWithoutControls_HasNullControlProperties()
    {
        // Arrange
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.Edge,
            rcid: 1
        );
        var data = CreateS57Document(vectorRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        var vector = document.VectorRecords[0];
        Assert.Null(vector.VectorPointerControl);
        Assert.Null(vector.CoordinateControl);
    }

    [Theory]
    [InlineData(S57UpdateInstruction.Insert)]
    [InlineData(S57UpdateInstruction.Delete)]
    [InlineData(S57UpdateInstruction.Modify)]
    public void Read_SgccWithAllUpdateInstructions_ParsesCorrectly(S57UpdateInstruction instruction)
    {
        // Arrange
        var sgcc = new S57FieldUpdateControl
        {
            UpdateInstruction = instruction,
            Index = 1,
            Count = 1
        };
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.Edge,
            rcid: 1,
            ruin: (byte)S57UpdateInstruction.Modify,
            coordinateControl: sgcc
        );
        var data = CreateS57Document(vectorRecord);

        // Act
        var document = S57DocumentReader.Read(data);

        // Assert
        var control = document.VectorRecords[0].CoordinateControl;
        Assert.NotNull(control);
        Assert.Equal(instruction, control.Value.UpdateInstruction);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Read_FeatureRecordWithEmptyAttributes_ReturnsEmptyAttributeArray()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(
            rcnm: 100,
            rcid: 1,
            objl: 75,
            attributes: null
        );
        var data = CreateS57Document(featureRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Single(document.FeatureRecords);
        Assert.Empty(document.FeatureRecords[0].Attributes);
    }

    [Fact]
    public void Read_VectorRecordWithNoCoordinates_ReturnsEmptyCoordinateArray()
    {
        // Arrange
        var vectorRecord = CreateVectorRecord(
            rcnm: S57RecordNameCodes.IsolatedNode,
            rcid: 1,
            coordinates: null
        );
        var data = CreateS57Document(vectorRecord);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Single(document.VectorRecords);
        Assert.Empty(document.VectorRecords[0].Coordinates2D);
        Assert.Empty(document.VectorRecords[0].Soundings);
    }

    [Fact]
    public void Read_MultipleFeatureRecords_PreservesOrder()
    {
        // Arrange
        var featureRecord1 = CreateFeatureRecord(rcnm: 100, rcid: 1, objl: 10);
        var featureRecord2 = CreateFeatureRecord(rcnm: 100, rcid: 2, objl: 20);
        var featureRecord3 = CreateFeatureRecord(rcnm: 100, rcid: 3, objl: 30);
        var data = CreateS57Document(featureRecord1, featureRecord2, featureRecord3);
        
        // Act - Call S57Reader.Read FIRST
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Equal(3, document.FeatureRecords.Length);
        Assert.Equal(1, document.FeatureRecords[0].RecordName.RecordId);
        Assert.Equal(2, document.FeatureRecords[1].RecordName.RecordId);
        Assert.Equal(3, document.FeatureRecords[2].RecordName.RecordId);
    }

    [Fact]
    public void Read_DifferentVectorTypes_ParsesCorrectly()
    {
        // Arrange
        var isolatedNode = CreateVectorRecord(rcnm: S57RecordNameCodes.IsolatedNode, rcid: 1);
        var connectedNode = CreateVectorRecord(rcnm: S57RecordNameCodes.ConnectedNode, rcid: 2);
        var edge = CreateVectorRecord(rcnm: S57RecordNameCodes.Edge, rcid: 3);
        var face = CreateVectorRecord(rcnm: S57RecordNameCodes.Face, rcid: 4);
        var data = CreateS57Document(isolatedNode, connectedNode, edge, face);
        
        // Act
        var document = S57DocumentReader.Read(data);
        
        // Assert
        Assert.Equal(4, document.VectorRecords.Length);
        Assert.Equal(S57RecordNameCodes.IsolatedNode, document.VectorRecords[0].RecordName.RecordNameCode);
        Assert.Equal(S57RecordNameCodes.ConnectedNode, document.VectorRecords[1].RecordName.RecordNameCode);
        Assert.Equal(S57RecordNameCodes.Edge, document.VectorRecords[2].RecordName.RecordNameCode);
        Assert.Equal(S57RecordNameCodes.Face, document.VectorRecords[3].RecordName.RecordNameCode);
    }

    #endregion
}
