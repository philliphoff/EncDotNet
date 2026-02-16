using System.Buffers.Binary;
using System.Collections.Immutable;
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
/// containing features, vectors (spatial objects), and associated metadata. The reader
/// uses the Data Descriptive Record (DDR) to understand field structures rather than
/// assuming a fixed binary layout.
/// </para>
/// </remarks>
public static class S57DocumentReader
{
    /// <summary>
    /// Reads an S-57 document from a byte array.
    /// </summary>
    /// <param name="data">The S-57 data to read.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document Read(byte[] data)
    {
        var iso8211Document = Iso8211DocumentReader.Read(data);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 document from a span of bytes.
    /// </summary>
    /// <param name="data">The S-57 data to read.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document Read(ReadOnlySpan<byte> data)
    {
        var iso8211Document = Iso8211DocumentReader.Read(data);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 document from a file.
    /// </summary>
    /// <param name="path">The path to the S-57 file.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document ReadFromFile(string path)
    {
        var iso8211Document = Iso8211DocumentReader.ReadFromFile(path);
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
        var iso8211Document = await Iso8211DocumentReader.ReadFromFileAsync(path, cancellationToken).ConfigureAwait(false);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 document from a stream.
    /// </summary>
    /// <param name="stream">The stream containing S-57 data.</param>
    /// <returns>The parsed S-57 document.</returns>
    public static S57Document Read(Stream stream)
    {
        var iso8211Document = Iso8211DocumentReader.Read(stream);
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
        var iso8211Document = await Iso8211DocumentReader.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
        return ParseDocument(iso8211Document);
    }

    /// <summary>
    /// Parses an ISO 8211 document into an S-57 document.
    /// </summary>
    private static S57Document ParseDocument(Iso8211Document iso8211Document)
    {
        // Parse the Data Descriptive Record (DDR) to get field definitions
        var ddr = iso8211Document.DataDescriptiveRecord is not null
            ? Iso8211DataDescriptiveRecordReader.Read(iso8211Document.DataDescriptiveRecord)
            : null;

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
                dsid = ParseDataSetIdentification(record, ddr);
                continue;
            }

            // Check for DSPM field
            var dspmField = record.GetFieldByTag(S57FieldTags.DSPM);
            if (dspmField != null)
            {
                dspm = ParseDataSetParameters(record, ddr);
                continue;
            }

            // Check for FRID field (Feature Record)
            var fridField = record.GetFieldByTag(S57FieldTags.FRID);
            if (fridField != null)
            {
                var featureRecord = ParseFeatureRecord(record, ddr);
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
                var vectorRecord = ParseVectorRecord(record, ddr);
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
    /// <remarks>
    /// DSID fields contain 16 subfields encoded according to the DDR field definition.
    /// This method reads all subfields using the field reader which interprets the data
    /// based on the DDR-specified formats.
    /// </remarks>
    private static S57DataSetIdentification ParseDataSetIdentification(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var dsidField = record.GetFieldByTag(S57FieldTags.DSID);
        if (dsidField == null)
        {
            throw new InvalidOperationException("DSID field not found in record.");
        }

        byte rcnm;
        uint rcid;
        byte intu;
        string dsnm, edtn, updn, uadt, isdt, sted;
        ushort agen = 0;
        string comt = "";

        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.DSID)
            ?? throw new InvalidOperationException("DDR is required but not available. DSID field definition not found.");

        // Use DDR-based field reader for proper subfield interpretation
        var reader = new Iso8211FieldReader(fieldDef, dsidField.Data);

        rcnm = reader.GetSubfield<byte>(S57SubfieldNames.RCNM);
        rcid = reader.GetSubfield<uint>(S57SubfieldNames.RCID);
        _ = reader.TryGetSubfield<byte>(S57SubfieldNames.EXPP, out _);  // EXPP - exchange purpose (not used)
        intu = reader.GetSubfield<byte>(S57SubfieldNames.INTU);
        dsnm = reader.GetSubfield<string>(S57SubfieldNames.DSNM);
        edtn = reader.GetSubfield<string>(S57SubfieldNames.EDTN);
        updn = reader.GetSubfield<string>(S57SubfieldNames.UPDN);
        uadt = reader.GetSubfield<string>(S57SubfieldNames.UADT);
        isdt = reader.GetSubfield<string>(S57SubfieldNames.ISDT);
        sted = reader.GetSubfield<string>(S57SubfieldNames.STED);

        if (reader.TryGetSubfield<ushort>(S57SubfieldNames.AGEN, out var agenValue))
        {
            agen = agenValue;
        }
        if (reader.TryGetSubfield<string>(S57SubfieldNames.COMT, out var comtValue))
        {
            comt = comtValue;
        }

        // Read DSSI field if present for additional info (data structure, lexical levels)
        var dssiField = record.GetFieldByTag(S57FieldTags.DSSI);
        int dstr = 0, aall = 0, nall = 0;
        if (dssiField != null)
        {
            var dssiFieldDef = ddr?.GetFieldDefinition(S57FieldTags.DSSI)
                ?? throw new InvalidOperationException("DDR is required but not available. DSSI field definition not found.");
            var dssiReader = new Iso8211FieldReader(dssiFieldDef, dssiField.Data);
            dstr = dssiReader.GetSubfield<byte>(S57SubfieldNames.DSTR);
            aall = dssiReader.GetSubfield<byte>(S57SubfieldNames.AALL);
            nall = dssiReader.GetSubfield<byte>(S57SubfieldNames.NALL);
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
    /// Parses a Data Set Parameters record.
    /// </summary>
    private static S57DataSetParameters ParseDataSetParameters(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var dspmField = record.GetFieldByTag(S57FieldTags.DSPM);
        if (dspmField == null)
        {
            throw new InvalidOperationException("DSPM field not found in record.");
        }

        byte rcnm, hdat, vdat, sdat, duni, huni, puni, coun;
        uint rcid, cscl, comf, somf;
        string comt;

        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.DSPM)
            ?? throw new InvalidOperationException("DDR is required but not available. DSPM field definition not found.");

        // Use DDR-based field reader
        var reader = new Iso8211FieldReader(fieldDef, dspmField.Data);

        rcnm = reader.GetSubfield<byte>(S57SubfieldNames.RCNM);
        rcid = reader.GetSubfield<uint>(S57SubfieldNames.RCID);
        hdat = reader.GetSubfield<byte>(S57SubfieldNames.HDAT);
        vdat = reader.GetSubfield<byte>(S57SubfieldNames.VDAT);
        sdat = reader.GetSubfield<byte>(S57SubfieldNames.SDAT);
        cscl = reader.GetSubfield<uint>(S57SubfieldNames.CSCL);
        duni = reader.GetSubfield<byte>(S57SubfieldNames.DUNI);
        huni = reader.GetSubfield<byte>(S57SubfieldNames.HUNI);
        puni = reader.GetSubfield<byte>(S57SubfieldNames.PUNI);
        coun = reader.GetSubfield<byte>(S57SubfieldNames.COUN);
        comf = reader.GetSubfield<uint>(S57SubfieldNames.COMF);
        somf = reader.GetSubfield<uint>(S57SubfieldNames.SOMF);
        comt = reader.TryGetSubfield<string>(S57SubfieldNames.COMT, out var c) ? c : "";

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
    private static S57FeatureRecord? ParseFeatureRecord(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var fridField = record.GetFieldByTag(S57FieldTags.FRID);
        if (fridField == null)
        {
            return null;
        }

        byte rcnm, prim, grup, ruin;
        uint rcid;
        ushort objl, rver;

        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.FRID)
            ?? throw new InvalidOperationException("DDR is required but not available. FRID field definition not found.");

        // Use DDR-based field reader
        var reader = new Iso8211FieldReader(fieldDef, fridField.Data);

        rcnm = reader.GetSubfield<byte>(S57SubfieldNames.RCNM);
        rcid = reader.GetSubfield<uint>(S57SubfieldNames.RCID);
        prim = reader.GetSubfield<byte>(S57SubfieldNames.PRIM);
        grup = reader.GetSubfield<byte>(S57SubfieldNames.GRUP);
        objl = reader.GetSubfield<ushort>(S57SubfieldNames.OBJL);
        rver = reader.GetSubfield<ushort>(S57SubfieldNames.RVER);
        ruin = reader.GetSubfield<byte>(S57SubfieldNames.RUIN);

        var recordName = S57RecordName.FromRcnmRcid(rcnm, (int)rcid);

        // Parse FOID if present (long name)
        var foidField = record.GetFieldByTag(S57FieldTags.FOID);
        if (foidField != null)
        {
            var foidFieldDef = ddr?.GetFieldDefinition(S57FieldTags.FOID)
                ?? throw new InvalidOperationException("DDR is required but not available. FOID field definition not found.");
            ushort agen;
            uint fidn;
            ushort fids;

            var foidReader = new Iso8211FieldReader(foidFieldDef, foidField.Data);
            agen = foidReader.GetSubfield<ushort>(S57SubfieldNames.AGEN);
            fidn = foidReader.GetSubfield<uint>(S57SubfieldNames.FIDN);
            fids = foidReader.GetSubfield<ushort>(S57SubfieldNames.FIDS);

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
        var attributes = ParseAttributes(record, S57FieldTags.ATTF, ddr);

        // Parse NATF (national attributes)
        var nationalAttributes = ParseAttributes(record, S57FieldTags.NATF, ddr);

        // Parse FSPT (spatial pointers)
        var spatialPointers = ParseSpatialPointers(record, ddr);

        // Parse FFPT (feature pointers)
        var featurePointers = ParseFeaturePointers(record, ddr);

        return new S57FeatureRecord
        {
            RecordName = recordName,
            Primitive = (S57GeometricPrimitive)prim,
            Group = grup,
            ObjectCode = (S57ObjectCode)objl,
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
    private static S57VectorRecord? ParseVectorRecord(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var vridField = record.GetFieldByTag(S57FieldTags.VRID);
        if (vridField == null)
        {
            return null;
        }

        byte rcnm, ruin;
        uint rcid;
        ushort rver;

        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.VRID)
            ?? throw new InvalidOperationException("DDR is required but not available. VRID field definition not found.");

        // Use DDR-based field reader
        var reader = new Iso8211FieldReader(fieldDef, vridField.Data);

        rcnm = reader.GetSubfield<byte>(S57SubfieldNames.RCNM);
        rcid = reader.GetSubfield<uint>(S57SubfieldNames.RCID);
        rver = reader.GetSubfield<ushort>(S57SubfieldNames.RVER);
        ruin = reader.GetSubfield<byte>(S57SubfieldNames.RUIN);

        // Parse ATTV (vector attributes)
        var attributes = ParseAttributes(record, S57FieldTags.ATTV, ddr);

        // Parse VRPT (vector pointers)
        var vectorPointers = ParseVectorPointers(record, ddr);

        // Parse SG2D (2D coordinates)
        var coordinates2D = ParseCoordinates2D(record, ddr);

        // Parse SG3D (3D soundings)
        var soundings = ParseSoundings(record, ddr);

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
    private static ImmutableArray<S57AttributeValue> ParseAttributes(Iso8211Record record, string fieldTag, Iso8211DataDescriptiveRecord? ddr)
    {
        var attributes = ImmutableArray.CreateBuilder<S57AttributeValue>();
        var fieldDef = ddr?.GetFieldDefinition(fieldTag);
        
        foreach (var field in record.GetFieldsByTag(fieldTag))
        {
            if (fieldDef == null || !fieldDef.HasRepeatingGroup)
            {
                continue; // No field definition or not a repeating field - skip
            }

            // Use DDR-based field reader with repeating groups
            var reader = new Iso8211FieldReader(fieldDef, field.Data);
            
            foreach (var group in reader.GetSubfieldGroups())
            {
                try
                {
                    var attl = group.GetSubfield<ushort>(S57SubfieldNames.ATTL);
                    var atvl = group.GetSubfield<string>(S57SubfieldNames.ATVL);
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
    private static ImmutableArray<S57SpatialPointer> ParseSpatialPointers(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var pointers = ImmutableArray.CreateBuilder<S57SpatialPointer>();
        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.FSPT);
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.FSPT))
        {
            if (fieldDef == null || !fieldDef.HasRepeatingGroup)
            {
                continue; // No field definition or not a repeating field - skip
            }

            // Use DDR-based field reader with repeating groups
            var reader = new Iso8211FieldReader(fieldDef, field.Data);
            
            foreach (var group in reader.GetSubfieldGroups())
            {
                try
                {
                    // NAME is a composite subfield (5 bytes: RCNM(1) + RCID(4))
                    // Read it as raw bytes and decompose
                    var nameBytes = group.GetSubfieldBytes(S57SubfieldNames.NAME);
                    var name = DecomposeNameField(nameBytes);
                    
                    var ornt = group.GetSubfield<byte>(S57SubfieldNames.ORNT);
                    var usag = group.GetSubfield<byte>(S57SubfieldNames.USAG);
                    var mask = group.GetSubfield<byte>(S57SubfieldNames.MASK);

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
    private static ImmutableArray<S57FeaturePointer> ParseFeaturePointers(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var pointers = ImmutableArray.CreateBuilder<S57FeaturePointer>();
        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.FFPT);
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.FFPT))
        {
            if (fieldDef == null || !fieldDef.HasRepeatingGroup)
            {
                continue; // No field definition or not a repeating field - skip
            }

            // Use DDR-based field reader with repeating groups
            var reader = new Iso8211FieldReader(fieldDef, field.Data);
            
            foreach (var group in reader.GetSubfieldGroups())
            {
                try
                {
                    // LNAM is a composite subfield (8 bytes: AGEN(2) + FIDN(4) + FIDS(2))
                    var lnamBytes = group.GetSubfieldBytes(S57SubfieldNames.LNAM);
                    var lnam = DecomposeLongNameField(lnamBytes);
                    
                    var rind = group.GetSubfield<byte>(S57SubfieldNames.RIND);
                    var comt = group.GetSubfield<string>(S57SubfieldNames.COMT);

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
    private static ImmutableArray<S57VectorPointer> ParseVectorPointers(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var pointers = ImmutableArray.CreateBuilder<S57VectorPointer>();
        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.VRPT);
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.VRPT))
        {
            if (fieldDef == null || !fieldDef.HasRepeatingGroup)
            {
                continue; // No field definition or not a repeating field - skip
            }

            // Use DDR-based field reader with repeating groups
            var reader = new Iso8211FieldReader(fieldDef, field.Data);
            
            foreach (var group in reader.GetSubfieldGroups())
            {
                try
                {
                    // NAME is a composite subfield (5 bytes: RCNM(1) + RCID(4))
                    var nameBytes = group.GetSubfieldBytes(S57SubfieldNames.NAME);
                    var name = DecomposeNameField(nameBytes);
                    
                    var ornt = group.GetSubfield<byte>(S57SubfieldNames.ORNT);
                    var usag = group.GetSubfield<byte>(S57SubfieldNames.USAG);
                    var topi = group.GetSubfield<byte>(S57SubfieldNames.TOPI);
                    var mask = group.GetSubfield<byte>(S57SubfieldNames.MASK);

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
    private static ImmutableArray<S57Coordinate2D> ParseCoordinates2D(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var coordinates = ImmutableArray.CreateBuilder<S57Coordinate2D>();
        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.SG2D);
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.SG2D))
        {
            if (fieldDef == null || !fieldDef.HasRepeatingGroup)
            {
                continue; // No field definition or not a repeating field - skip
            }

            // Use DDR-based field reader with repeating groups
            var reader = new Iso8211FieldReader(fieldDef, field.Data);
            
            foreach (var group in reader.GetSubfieldGroups())
            {
                try
                {
                    var ycoo = group.GetSubfield<int>(S57SubfieldNames.YCOO);
                    var xcoo = group.GetSubfield<int>(S57SubfieldNames.XCOO);

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
    private static ImmutableArray<S57Sounding> ParseSoundings(Iso8211Record record, Iso8211DataDescriptiveRecord? ddr)
    {
        var soundings = ImmutableArray.CreateBuilder<S57Sounding>();
        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.SG3D);
        
        foreach (var field in record.GetFieldsByTag(S57FieldTags.SG3D))
        {
            if (fieldDef == null || !fieldDef.HasRepeatingGroup)
            {
                continue; // No field definition or not a repeating field - skip
            }

            // Use DDR-based field reader with repeating groups
            var reader = new Iso8211FieldReader(fieldDef, field.Data);
            
            foreach (var group in reader.GetSubfieldGroups())
            {
                try
                {
                    var ycoo = group.GetSubfield<int>(S57SubfieldNames.YCOO);
                    var xcoo = group.GetSubfield<int>(S57SubfieldNames.XCOO);
                    var ve3d = group.GetSubfield<int>(S57SubfieldNames.VE3D);

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

    /// <summary>
    /// Decomposes a NAME composite subfield (5 bytes: RCNM(1) + RCID(4)) into an S57RecordName.
    /// </summary>
    private static S57RecordName DecomposeNameField(ReadOnlySpan<byte> data)
    {
        if (data.Length < 5)
        {
            return default;
        }
        
        var rcnm = data[0];
        var rcid = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(1, 4));
        return S57RecordName.FromRcnmRcid(rcnm, (int)rcid);
    }

    /// <summary>
    /// Decomposes a LNAM composite subfield (8 bytes: AGEN(2) + FIDN(4) + FIDS(2)) into an S57RecordName.
    /// </summary>
    private static S57RecordName DecomposeLongNameField(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
        {
            return default;
        }
        
        var agen = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(0, 2));
        var fidn = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(2, 4));
        var fids = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6, 2));
        return S57RecordName.FromLongName(agen, (int)fidn, fids);
    }
}
