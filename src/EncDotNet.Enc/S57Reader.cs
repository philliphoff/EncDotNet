using System.Collections.Immutable;
using System.Text;
using EncDotNet.Iso8211;

namespace EncDotNet.Enc;

/// <summary>
/// Provides methods to read S-57 Electronic Navigational Chart (ENC) files.
/// </summary>
/// <remarks>
/// <para>
/// S-57 is the IHO (International Hydrographic Organization) standard for the transfer
/// of digital hydrographic data. ENC files are encoded using ISO 8211 format.
/// </para>
/// <para>
/// This reader parses the ISO 8211 encoded data and constructs an S-57 document model
/// containing features, vectors (spatial objects), and associated metadata.
/// </para>
/// </remarks>
public static class S57Reader
{
    /// <summary>
    /// Reads an S-57 document from a byte array.
    /// </summary>
    /// <param name="data">The S-57 data to read.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document Read(byte[] data)
    {
        var iso8211Document = Iso8211Reader.Read(data);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 document from a span of bytes.
    /// </summary>
    /// <param name="data">The S-57 data to read.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document Read(ReadOnlySpan<byte> data)
    {
        var iso8211Document = Iso8211Reader.Read(data);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 document from a file.
    /// </summary>
    /// <param name="path">The path to the S-57 file.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document ReadFromFile(string path)
    {
        var iso8211Document = Iso8211Reader.ReadFromFile(path);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Asynchronously reads an S-57 document from a file.
    /// </summary>
    /// <param name="path">The path to the S-57 file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<S57Document> ReadFromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var iso8211Document = await Iso8211Reader.ReadFromFileAsync(path, cancellationToken).ConfigureAwait(false);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 document from a stream.
    /// </summary>
    /// <param name="stream">The stream containing S-57 data.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document Read(Stream stream)
    {
        var iso8211Document = Iso8211Reader.Read(stream);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Asynchronously reads an S-57 document from a stream.
    /// </summary>
    /// <param name="stream">The stream containing S-57 data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<S57Document> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var iso8211Document = await Iso8211Reader.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Parses an ISO 8211 document into an S-57 document.
    /// </summary>
    private static S57Document ParseDocument(Iso8211Document iso8211Document)
    {
        S57DataSetIdentification? dsid = null;
        S57DataSetParameters? dspm = null;
        var featureRecords = ImmutableArray.CreateBuilder<S57FeatureRecord>();
        var vectorRecords = ImmutableArray.CreateBuilder<S57VectorRecord>();

        foreach (var record in iso8211Document.DataRecords)
        {
            // Check for DSID field
            var dsidField = record.GetFieldByTag(S57FieldTags.DSID);
            if (dsidField != null)
            {
                dsid = ParseDataSetIdentification(record);
                continue;
            }

            // Check for DSPM field
            var dspmField = record.GetFieldByTag(S57FieldTags.DSPM);
            if (dspmField != null)
            {
                dspm = ParseDataSetParameters(record);
                continue;
            }

            // Check for FRID field (Feature Record)
            var fridField = record.GetFieldByTag(S57FieldTags.FRID);
            if (fridField != null)
            {
                var featureRecord = ParseFeatureRecord(record);
                if (featureRecord != null)
                {
                    featureRecords.Add(featureRecord);
                }
                continue;
            }

            // Check for VRID field (Vector Record)
            var vridField = record.GetFieldByTag(S57FieldTags.VRID);
            if (vridField != null)
            {
                var vectorRecord = ParseVectorRecord(record);
                if (vectorRecord != null)
                {
                    vectorRecords.Add(vectorRecord);
                }
            }
        }

        return new S57Document
        {
            DataSetIdentification = dsid,
            DataSetParameters = dspm,
            FeatureRecords = featureRecords.ToImmutable(),
            VectorRecords = vectorRecords.ToImmutable()
        };
    }

    /// <summary>
    /// Parses a Data Set Identification record.
    /// </summary>
    private static S57DataSetIdentification ParseDataSetIdentification(Iso8211Record record)
    {
        var dsidField = record.GetFieldByTag(S57FieldTags.DSID);
        if (dsidField == null)
        {
            throw new InvalidOperationException("DSID field not found in record.");
        }

        // Debug: Log the DSID field data length and first bytes
        System.Diagnostics.Debug.WriteLine($"DSID field data length: {dsidField.Data.Length}");
        if (dsidField.Data.Length > 0)
        {
            var preview = dsidField.Data.Take(Math.Min(60, dsidField.Data.Length)).ToArray();
            System.Diagnostics.Debug.WriteLine($"DSID first bytes (hex): {BitConverter.ToString(preview)}");
        }

        // Check if we have subfields (real S-57 files may use subfield parsing)
        if (!dsidField.Subfields.IsDefaultOrEmpty && dsidField.Subfields.Length > 0)
        {
            System.Diagnostics.Debug.WriteLine($"DSID has {dsidField.Subfields.Length} subfields, using subfield-based parsing");
            return ParseDsidFromSubfields(dsidField);
        }

        var reader = new S57BinaryFieldReader(dsidField.Data);

        // Ensure we have enough data - DSID needs at least 7 bytes for RCNM+RCID+EXPP+INTU
        if (dsidField.Data.Length < 7)
        {
            throw new InvalidOperationException($"DSID field data too short: {dsidField.Data.Length} bytes (expected at least 7)");
        }

        // Parse DSID subfields according to S-57 specification
        var rcnm = reader.ReadUInt8();
        var rcid = reader.ReadUInt32();
        _ = reader.ReadUInt8();  // expp - exchange purpose
        var intu = reader.ReadUInt8();
        var dsnm = reader.ReadString();
        var edtn = reader.ReadString();
        var updn = reader.ReadString();
        var uadt = reader.ReadString();
        var isdt = reader.ReadString();
        var sted = reader.ReadString();
        _ = reader.ReadUInt8();  // prsp - product specification
        _ = reader.ReadString(); // psdn - product specification description
        _ = reader.ReadString(); // pred - product specification edition number
        _ = reader.ReadUInt8();  // prof - application profile identification
        var agen = reader.ReadUInt16();
        var comt = reader.ReadString();

        // Read DSSI field if present for additional info
        var dssiField = record.GetFieldByTag(S57FieldTags.DSSI);
        int dstr = 0, aall = 0, nall = 0;
        if (dssiField != null)
        {
            var dssiReader = new S57BinaryFieldReader(dssiField.Data);
            dstr = dssiReader.ReadUInt8();
            aall = dssiReader.ReadUInt8();
            nall = dssiReader.ReadUInt8();
        }

        return new S57DataSetIdentification
        {
            RecordName = S57RecordName.FromRcnmRcid(rcnm, (int)rcid),
            IntendedUsage = intu,
            DataSetName = dsnm,
            EditionNumber = edtn,
            UpdateNumber = updn,
            UpdateApplicationDate = uadt,
            IssueDate = isdt,
            S57EditionNumber = sted,
            ProducingAgency = agen,
            DataStructure = dstr,
            AttfLexicalLevel = aall,
            NatfLexicalLevel = nall,
            Comment = comt
        };
    }

    /// <summary>
    /// Parses DSID from subfields when the ISO 8211 reader has parsed them.
    /// </summary>
    private static S57DataSetIdentification ParseDsidFromSubfields(Iso8211Field dsidField)
    {
        // When subfields are parsed, each subfield contains one value
        // Order: RCNM, RCID, EXPP, INTU, DSNM, EDTN, UPDN, UADT, ISDT, STED, PRSP, PSDN, PRED, PROF, AGEN, COMT
        var subfields = dsidField.Subfields;
        
        byte rcnm = subfields.Length > 0 && subfields[0].Data.Length > 0 ? subfields[0].Data[0] : (byte)0;
        uint rcid = subfields.Length > 1 && subfields[1].Data.Length >= 4 ? BitConverter.ToUInt32(subfields[1].Data, 0) : 0;
        // Skip EXPP (index 2)
        byte intu = subfields.Length > 3 && subfields[3].Data.Length > 0 ? subfields[3].Data[0] : (byte)0;
        string dsnm = subfields.Length > 4 ? Encoding.ASCII.GetString(subfields[4].Data).TrimEnd('\0') : "";
        string edtn = subfields.Length > 5 ? Encoding.ASCII.GetString(subfields[5].Data).TrimEnd('\0') : "";
        string updn = subfields.Length > 6 ? Encoding.ASCII.GetString(subfields[6].Data).TrimEnd('\0') : "";
        string uadt = subfields.Length > 7 ? Encoding.ASCII.GetString(subfields[7].Data).TrimEnd('\0') : "";
        string isdt = subfields.Length > 8 ? Encoding.ASCII.GetString(subfields[8].Data).TrimEnd('\0') : "";
        string sted = subfields.Length > 9 ? Encoding.ASCII.GetString(subfields[9].Data).TrimEnd('\0') : "";
        // Skip PRSP (index 10), PSDN (index 11), PRED (index 12), PROF (index 13)
        ushort agen = subfields.Length > 14 && subfields[14].Data.Length >= 2 ? BitConverter.ToUInt16(subfields[14].Data, 0) : (ushort)0;
        string comt = subfields.Length > 15 ? Encoding.ASCII.GetString(subfields[15].Data).TrimEnd('\0') : "";

        return new S57DataSetIdentification
        {
            RecordName = S57RecordName.FromRcnmRcid(rcnm, (int)rcid),
            IntendedUsage = intu,
            DataSetName = dsnm,
            EditionNumber = edtn,
            UpdateNumber = updn,
            UpdateApplicationDate = uadt,
            IssueDate = isdt,
            S57EditionNumber = sted,
            ProducingAgency = agen,
            DataStructure = 0,
            AttfLexicalLevel = 0,
            NatfLexicalLevel = 0,
            Comment = comt
        };
    }

    /// <summary>
    /// Parses a Data Set Parameters record.
    /// </summary>
    private static S57DataSetParameters ParseDataSetParameters(Iso8211Record record)
    {
        var dspmField = record.GetFieldByTag(S57FieldTags.DSPM);
        if (dspmField == null)
        {
            throw new InvalidOperationException("DSPM field not found in record.");
        }

        var reader = new S57BinaryFieldReader(dspmField.Data);

        // Parse DSPM subfields according to S-57 specification
        var rcnm = reader.ReadUInt8();
        var rcid = reader.ReadUInt32();
        var hdat = reader.ReadUInt8();
        var vdat = reader.ReadUInt8();
        var sdat = reader.ReadUInt8();
        var cscl = reader.ReadUInt32();
        var duni = reader.ReadUInt8();
        var huni = reader.ReadUInt8();
        var puni = reader.ReadUInt8();
        var coun = reader.ReadUInt8();
        var comf = reader.ReadUInt32();
        var somf = reader.ReadUInt32();
        var comt = reader.ReadString();

        return new S57DataSetParameters
        {
            RecordName = S57RecordName.FromRcnmRcid(rcnm, (int)rcid),
            HorizontalDatum = hdat,
            VerticalDatum = vdat,
            SoundingDatum = sdat,
            CompilationScale = (int)cscl,
            DepthUnits = duni,
            HeightUnits = huni,
            PositionalUnits = puni,
            CoordinateUnits = coun,
            CoordinateMultiplicationFactor = (int)comf,
            SoundingMultiplicationFactor = (int)somf,
            Comment = comt
        };
    }

    /// <summary>
    /// Parses a Feature Record.
    /// </summary>
    private static S57FeatureRecord? ParseFeatureRecord(Iso8211Record record)
    {
        var fridField = record.GetFieldByTag(S57FieldTags.FRID);
        if (fridField == null)
        {
            return null;
        }

        var reader = new S57BinaryFieldReader(fridField.Data);

        // Parse FRID subfields
        var rcnm = reader.ReadUInt8();
        var rcid = reader.ReadUInt32();
        var prim = reader.ReadUInt8();
        var grup = reader.ReadUInt8();
        var objl = reader.ReadUInt16();
        var rver = reader.ReadUInt16();
        var ruin = reader.ReadUInt8();

        var recordName = S57RecordName.FromRcnmRcid(rcnm, (int)rcid);

        // Parse FOID if present (long name)
        var foidField = record.GetFieldByTag(S57FieldTags.FOID);
        if (foidField != null)
        {
            var foidReader = new S57BinaryFieldReader(foidField.Data);
            var agen = foidReader.ReadUInt16();
            var fidn = foidReader.ReadUInt32();
            var fids = foidReader.ReadUInt16();
            recordName = new S57RecordName
            {
                RecordNameCode = rcnm,
                RecordId = (int)rcid,
                AgencyCode = agen,
                FeatureId = (int)fidn,
                FeatureSubdivision = fids
            };
        }

        // Parse ATTF (attributes)
        var attributes = ParseAttributes(record, S57FieldTags.ATTF);

        // Parse NATF (national attributes)
        var nationalAttributes = ParseAttributes(record, S57FieldTags.NATF);

        // Parse FSPT (spatial pointers)
        var spatialPointers = ParseSpatialPointers(record);

        // Parse FFPT (feature pointers)
        var featurePointers = ParseFeaturePointers(record);

        return new S57FeatureRecord
        {
            RecordName = recordName,
            Primitive = (S57GeometricPrimitive)prim,
            Group = grup,
            ObjectCode = objl,
            RecordVersion = rver,
            UpdateInstruction = (S57UpdateInstruction)ruin,
            Attributes = attributes,
            NationalAttributes = nationalAttributes,
            SpatialPointers = spatialPointers,
            FeaturePointers = featurePointers
        };
    }

    /// <summary>
    /// Parses a Vector Record.
    /// </summary>
    private static S57VectorRecord? ParseVectorRecord(Iso8211Record record)
    {
        var vridField = record.GetFieldByTag(S57FieldTags.VRID);
        if (vridField == null)
        {
            return null;
        }

        var reader = new S57BinaryFieldReader(vridField.Data);

        // Parse VRID subfields
        var rcnm = reader.ReadUInt8();
        var rcid = reader.ReadUInt32();
        var rver = reader.ReadUInt16();
        var ruin = reader.ReadUInt8();

        // Parse ATTV (vector attributes)
        var attributes = ParseVectorAttributes(record);

        // Parse VRPT (vector pointers)
        var vectorPointers = ParseVectorPointers(record);

        // Parse SG2D (2D coordinates)
        var coordinates2D = ParseCoordinates2D(record);

        // Parse SG3D (3D soundings)
        var soundings = ParseSoundings(record);

        return new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(rcnm, (int)rcid),
            RecordVersion = rver,
            UpdateInstruction = (S57UpdateInstruction)ruin,
            Attributes = attributes,
            VectorPointers = vectorPointers,
            Coordinates2D = coordinates2D,
            Soundings = soundings
        };
    }

    /// <summary>
    /// Parses attributes from ATTF or NATF fields.
    /// </summary>
    private static ImmutableArray<S57AttributeValue> ParseAttributes(Iso8211Record record, string fieldTag)
    {
        var attributes = ImmutableArray.CreateBuilder<S57AttributeValue>();
        
        foreach (var field in record.GetFieldsByTag(fieldTag))
        {
            var reader = new S57BinaryFieldReader(field.Data);
            
            while (!reader.IsAtEnd)
            {
                try
                {
                    var attl = reader.ReadUInt16();
                    var atvl = reader.ReadString();
                    attributes.Add(new S57AttributeValue(attl, atvl));
                }
                catch
                {
                    break;
                }
            }
        }

        return attributes.ToImmutable();
    }

    /// <summary>
    /// Parses vector attributes from ATTV fields.
    /// </summary>
    private static ImmutableArray<S57AttributeValue> ParseVectorAttributes(Iso8211Record record)
    {
        var attributes = ImmutableArray.CreateBuilder<S57AttributeValue>();
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.ATTV))
        {
            var reader = new S57BinaryFieldReader(field.Data);
            
            while (!reader.IsAtEnd)
            {
                try
                {
                    var attl = reader.ReadUInt16();
                    var atvl = reader.ReadString();
                    attributes.Add(new S57AttributeValue(attl, atvl));
                }
                catch
                {
                    break;
                }
            }
        }

        return attributes.ToImmutable();
    }

    /// <summary>
    /// Parses spatial pointers from FSPT fields.
    /// </summary>
    private static ImmutableArray<S57SpatialPointer> ParseSpatialPointers(Iso8211Record record)
    {
        var pointers = ImmutableArray.CreateBuilder<S57SpatialPointer>();
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.FSPT))
        {
            var reader = new S57BinaryFieldReader(field.Data);
            
            // FSPT contains repeating groups of NAME(8) + ORNT(1) + USAG(1) + MASK(1)
            while (reader.BytesRemaining >= 11)
            {
                try
                {
                    // NAME is 8 bytes: RCNM(1) + RCID(4) + reserved(3)
                    var name = reader.ReadName();
                    var ornt = reader.ReadUInt8();
                    var usag = reader.ReadUInt8();
                    var mask = reader.ReadUInt8();

                    pointers.Add(new S57SpatialPointer
                    {
                        Name = name,
                        Orientation = (S57Orientation)ornt,
                        Usage = (S57UsageIndicator)usag,
                        Mask = (S57MaskingIndicator)mask
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        return pointers.ToImmutable();
    }

    /// <summary>
    /// Parses feature pointers from FFPT fields.
    /// </summary>
    private static ImmutableArray<S57FeaturePointer> ParseFeaturePointers(Iso8211Record record)
    {
        var pointers = ImmutableArray.CreateBuilder<S57FeaturePointer>();
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.FFPT))
        {
            var reader = new S57BinaryFieldReader(field.Data);
            
            // FFPT contains LNAM(8) + RIND(1) + COMT(variable)
            while (reader.BytesRemaining >= 9)
            {
                try
                {
                    // LNAM is 8 bytes: AGEN(2) + FIDN(4) + FIDS(2)
                    var lnam = reader.ReadLongName();
                    var rind = reader.ReadUInt8();
                    var comt = reader.ReadString();

                    pointers.Add(new S57FeaturePointer
                    {
                        Name = lnam,
                        Relationship = (S57RelationshipIndicator)rind,
                        Comment = comt
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        return pointers.ToImmutable();
    }

    /// <summary>
    /// Parses vector pointers from VRPT fields.
    /// </summary>
    private static ImmutableArray<S57VectorPointer> ParseVectorPointers(Iso8211Record record)
    {
        var pointers = ImmutableArray.CreateBuilder<S57VectorPointer>();
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.VRPT))
        {
            var reader = new S57BinaryFieldReader(field.Data);
            
            // VRPT contains repeating groups
            while (reader.BytesRemaining >= 9)
            {
                try
                {
                    var name = reader.ReadName();
                    var ornt = reader.ReadUInt8();
                    var usag = reader.ReadUInt8();
                    var topi = reader.ReadUInt8();
                    var mask = reader.ReadUInt8();

                    pointers.Add(new S57VectorPointer
                    {
                        Name = name,
                        Orientation = (S57Orientation)ornt,
                        Usage = (S57UsageIndicator)usag,
                        Topology = (S57TopologyIndicator)topi,
                        Mask = (S57MaskingIndicator)mask
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        return pointers.ToImmutable();
    }

    /// <summary>
    /// Parses 2D coordinates from SG2D fields.
    /// </summary>
    private static ImmutableArray<S57Coordinate2D> ParseCoordinates2D(Iso8211Record record)
    {
        var coordinates = ImmutableArray.CreateBuilder<S57Coordinate2D>();
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.SG2D))
        {
            var reader = new S57BinaryFieldReader(field.Data);
            
            // SG2D contains repeating groups of YCOO(4) + XCOO(4)
            while (reader.BytesRemaining >= 8)
            {
                try
                {
                    var ycoo = reader.ReadInt32();
                    var xcoo = reader.ReadInt32();

                    coordinates.Add(new S57Coordinate2D
                    {
                        Y = ycoo,
                        X = xcoo
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        return coordinates.ToImmutable();
    }

    /// <summary>
    /// Parses 3D sounding coordinates from SG3D fields.
    /// </summary>
    private static ImmutableArray<S57Sounding> ParseSoundings(Iso8211Record record)
    {
        var soundings = ImmutableArray.CreateBuilder<S57Sounding>();
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.SG3D))
        {
            var reader = new S57BinaryFieldReader(field.Data);
            
            // SG3D contains repeating groups of YCOO(4) + XCOO(4) + VE3D(4)
            while (reader.BytesRemaining >= 12)
            {
                try
                {
                    var ycoo = reader.ReadInt32();
                    var xcoo = reader.ReadInt32();
                    var ve3d = reader.ReadInt32();

                    soundings.Add(new S57Sounding
                    {
                        Y = ycoo,
                        X = xcoo,
                        Depth = ve3d
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        return soundings.ToImmutable();
    }
}

/// <summary>
/// Helper class for reading binary S-57 field data.
/// </summary>
internal ref struct S57BinaryFieldReader
{
    private readonly ReadOnlySpan<byte> _data;
    private int _position;

    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    public S57BinaryFieldReader(byte[] data)
    {
        _data = data.AsSpan();
        _position = 0;
    }

    public S57BinaryFieldReader(ReadOnlySpan<byte> data)
    {
        _data = data;
        _position = 0;
    }

    public bool IsAtEnd => _position >= _data.Length || 
                           (_position < _data.Length && (_data[_position] == FieldTerminator || _data[_position] == UnitTerminator && _position == _data.Length - 1));

    public int BytesRemaining => _data.Length - _position;

    public byte ReadUInt8()
    {
        if (_position >= _data.Length)
        {
            throw new InvalidOperationException("End of data reached.");
        }
        return _data[_position++];
    }

    public ushort ReadUInt16()
    {
        if (_position + 2 > _data.Length)
        {
            throw new InvalidOperationException("Not enough data for UInt16.");
        }
        var value = BitConverter.ToUInt16(_data.Slice(_position, 2));
        _position += 2;
        return value;
    }

    public uint ReadUInt32()
    {
        if (_position + 4 > _data.Length)
        {
            throw new InvalidOperationException("Not enough data for UInt32.");
        }
        var value = BitConverter.ToUInt32(_data.Slice(_position, 4));
        _position += 4;
        return value;
    }

    public int ReadInt32()
    {
        if (_position + 4 > _data.Length)
        {
            throw new InvalidOperationException("Not enough data for Int32.");
        }
        var value = BitConverter.ToInt32(_data.Slice(_position, 4));
        _position += 4;
        return value;
    }

    public string ReadString()
    {
        var start = _position;
        while (_position < _data.Length && 
               _data[_position] != UnitTerminator && 
               _data[_position] != FieldTerminator)
        {
            _position++;
        }

        var length = _position - start;
        var result = length > 0 
            ? Encoding.ASCII.GetString(_data.Slice(start, length)) 
            : string.Empty;

        // Skip the terminator if present
        if (_position < _data.Length && 
            (_data[_position] == UnitTerminator || _data[_position] == FieldTerminator))
        {
            _position++;
        }

        return result;
    }

    /// <summary>
    /// Reads an S-57 NAME field (8 bytes: RCNM(1) + RCID(4) + reserved(3)).
    /// </summary>
    public S57RecordName ReadName()
    {
        if (_position + 5 > _data.Length)
        {
            throw new InvalidOperationException("Not enough data for NAME field.");
        }

        var rcnm = ReadUInt8();
        var rcid = ReadUInt32();

        return S57RecordName.FromRcnmRcid(rcnm, (int)rcid);
    }

    /// <summary>
    /// Reads an S-57 long name field (8 bytes: AGEN(2) + FIDN(4) + FIDS(2)).
    /// </summary>
    public S57RecordName ReadLongName()
    {
        if (_position + 8 > _data.Length)
        {
            throw new InvalidOperationException("Not enough data for LNAM field.");
        }

        var agen = ReadUInt16();
        var fidn = ReadUInt32();
        var fids = ReadUInt16();

        return S57RecordName.FromLongName(agen, (int)fidn, fids);
    }

    /// <summary>
    /// Skips the specified number of bytes.
    /// </summary>
    public void Skip(int count)
    {
        _position += count;
        if (_position > _data.Length)
        {
            _position = _data.Length;
        }
    }
}
