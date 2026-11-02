using System.Text;
using EncDotNet.Iso8211;

namespace EncDotNet.S57;

/// <summary>
/// Provides methods to write an <see cref="S57Document"/> as an S-57 Edition 3.1 base cell
/// (a <c>.000</c> file) in the ISO/IEC 8211 binary implementation.
/// </summary>
/// <remarks>
/// <para>
/// The writer is the inverse of <see cref="S57DocumentReader"/>: a cell it writes reads back as
/// an equal document. It writes, in order:
/// </para>
/// <list type="number">
/// <item><description>The data descriptive record (DDR), with the field definitions of the
/// ENC product specification and the ATTF and NATF lexical levels in their field controls
/// (S-57 Part 3, Annex A and Annex B.2).</description></item>
/// <item><description>The data set general information record: DSID and DSSI (clause 7.3.1).
/// The DSSI record counts are computed from the document's records.</description></item>
/// <item><description>The data set geographic reference record: DSPM (clause 7.3.2).</description></item>
/// <item><description>Each vector record, in document order: VRID, ATTV, VRPT, then SG2D or
/// SG3D (clause 7.7).</description></item>
/// <item><description>Each feature record, in document order: FRID, FOID, ATTF, NATF, FFPT,
/// then FSPT (clause 7.6).</description></item>
/// </list>
/// <para>
/// Every data record starts with the <c>0001</c> record identifier field, numbered from 1.
/// Fields with no content (no attributes, pointers or coordinates) are omitted.
/// </para>
/// <para>
/// The output depends only on the document: the writer reads no clock, so writing the same
/// document always produces the same bytes. Dates such as the issue date (ISDT) are written as
/// the document holds them.
/// </para>
/// <para>
/// Only base cells are supported. Every record must have the update instruction
/// <see cref="S57UpdateInstruction.Insert"/> and no update control fields (FSPC, FFPC, VRPC or
/// SGCC); update cells (ER) aren't written.
/// </para>
/// <para>
/// ATTF values are encoded at the DSSI lexical level <see cref="S57DataSetIdentification.AttfLexicalLevel"/>
/// (0 = ASCII, 1 = ISO 8859-1), and NATF values at <see cref="S57DataSetIdentification.NatfLexicalLevel"/>
/// (0, 1, or 2 = UCS-2). All other character data is ASCII (S-57 Part 3, clause 2.4). A value
/// its lexical level can't represent is an error rather than being replaced.
/// </para>
/// </remarks>
public static class S57DocumentWriter
{
    private const string RecordIdentifierTag = "0001";

    // Object label ranges of the record kinds that DSSI counts (S-57 Appendix A, chapter 1).
    private const int FirstMetaObjectCode = 300;
    private const int FirstCollectionObjectCode = 400;
    private const int FirstCartographicObjectCode = 500;
    private const int LastCartographicObjectCode = 599;

    private static readonly Iso8211WriterOptions WriterOptions = new()
    {
        AutoSizeDirectoryEntries = true,
        BlankDataRecordFieldControlLength = true
    };

    // A cell with NATF at lexical level 2 ends those fields with the UCS-2 field terminator
    // (S-57 Part 3, clause 2.5).
    private static readonly Iso8211WriterOptions Ucs2NationalAttributeWriterOptions = new()
    {
        AutoSizeDirectoryEntries = true,
        BlankDataRecordFieldControlLength = true,
        TwoByteTerminatorFieldTags = new HashSet<string>(StringComparer.Ordinal) { S57FieldTags.NATF }
    };

    /// <summary>
    /// Writes an S-57 document to a byte array.
    /// </summary>
    /// <param name="document">The document to write.</param>
    /// <returns>The encoded base cell.</returns>
    /// <exception cref="ArgumentException">The document can't be encoded as a base cell; the
    /// message names the record and subfield.</exception>
    public static byte[] Write(S57Document document) =>
        Iso8211DocumentWriter.Write(Encode(document), OptionsFor(document));

    /// <summary>
    /// Writes an S-57 document to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="document">The document to write.</param>
    /// <exception cref="ArgumentException">The document can't be encoded as a base cell. Nothing
    /// is written to the stream.</exception>
    public static void Write(Stream stream, S57Document document)
    {
        ArgumentNullException.ThrowIfNull(stream);
        Iso8211DocumentWriter.Write(stream, Encode(document), OptionsFor(document));
    }

    /// <summary>
    /// Asynchronously writes an S-57 document to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="document">The document to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentException">The document can't be encoded as a base cell. Nothing
    /// is written to the stream.</exception>
    public static Task WriteAsync(Stream stream, S57Document document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return Iso8211DocumentWriter.WriteAsync(stream, Encode(document), OptionsFor(document), cancellationToken);
    }

    /// <summary>
    /// Writes an S-57 document to a file.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="document">The document to write.</param>
    /// <exception cref="ArgumentException">The document can't be encoded as a base cell. The
    /// file isn't created or changed.</exception>
    public static void WriteToFile(string path, S57Document document) =>
        Iso8211DocumentWriter.WriteToFile(path, Encode(document), OptionsFor(document));

    /// <summary>
    /// Asynchronously writes an S-57 document to a file.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="document">The document to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentException">The document can't be encoded as a base cell. The
    /// file isn't created or changed.</exception>
    public static Task WriteToFileAsync(string path, S57Document document, CancellationToken cancellationToken = default) =>
        Iso8211DocumentWriter.WriteToFileAsync(path, Encode(document), OptionsFor(document), cancellationToken);

    // Encodes the document as ISO 8211 records, checking everything before anything is written.
    private static Iso8211Document Encode(S57Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var dsid = document.DataSetIdentification
            ?? throw new ArgumentException("The document has no data set identification (DSID), which a base cell requires.", nameof(document));
        var dspm = document.DataSetParameters
            ?? throw new ArgumentException("The document has no data set parameters (DSPM), which a base cell requires.", nameof(document));

        var attfEncoding = dsid.AttfLexicalLevel switch
        {
            0 => S57FieldEncoder.Ascii,
            1 => S57FieldEncoder.Latin1,
            _ => throw new ArgumentException(
                $"The ATTF lexical level (AALL) is {dsid.AttfLexicalLevel}; ATTF allows only 0 or 1 (S-57 Part 3, clause 2.4).", nameof(document))
        };
        var natfEncoding = dsid.NatfLexicalLevel switch
        {
            0 => S57FieldEncoder.Ascii,
            1 => S57FieldEncoder.Latin1,
            2 => S57FieldEncoder.Ucs2,
            _ => throw new ArgumentException(
                $"The NATF lexical level (NALL) is {dsid.NatfLexicalLevel}; NATF allows only 0, 1 or 2 (S-57 Part 3, clause 2.4).", nameof(document))
        };

        var encoder = new S57FieldEncoder();
        var records = new List<Iso8211Record>(document.VectorRecords.Count + document.FeatureRecords.Count + 3)
        {
            S57DataDescriptiveRecord.Build(dsid.AttfLexicalLevel, dsid.NatfLexicalLevel)
        };

        var recordNumber = 0;
        records.Add(DataSetIdentificationRecord(encoder, ++recordNumber, dsid, document));
        records.Add(DataSetParametersRecord(encoder, ++recordNumber, dspm));

        foreach (var vector in document.VectorRecords)
        {
            records.Add(VectorRecord(encoder, ++recordNumber, vector));
        }

        foreach (var feature in document.FeatureRecords)
        {
            records.Add(FeatureRecord(encoder, ++recordNumber, feature, attfEncoding, natfEncoding));
        }

        return new Iso8211Document { Records = records };
    }

    private static Iso8211Record DataSetIdentificationRecord(S57FieldEncoder encoder, int recordNumber, S57DataSetIdentification dsid, S57Document document)
    {
        var record = NewDataRecord(encoder, recordNumber);

        // S-57 Part 3, clause 7.3.1.1.
        encoder.Begin($"{S57FieldTags.DSID} ({dsid.RecordName})")
            .B11(dsid.RecordName.RecordNameCode, S57SubfieldNames.RCNM)
            .B14(unchecked((uint)dsid.RecordName.RecordId), S57SubfieldNames.RCID)
            .B11(dsid.ExchangePurpose == 0 ? 1 : dsid.ExchangePurpose, S57SubfieldNames.EXPP)
            .B11(dsid.IntendedUsage, S57SubfieldNames.INTU)
            .Text(dsid.DataSetName, S57SubfieldNames.DSNM, S57FieldEncoder.Ascii)
            .Text(dsid.EditionNumber, S57SubfieldNames.EDTN, S57FieldEncoder.Ascii)
            .Text(dsid.UpdateNumber, S57SubfieldNames.UPDN, S57FieldEncoder.Ascii)
            .FixedText(dsid.UpdateApplicationDate, 8, S57SubfieldNames.UADT)
            .FixedText(dsid.IssueDate, 8, S57SubfieldNames.ISDT)
            .FixedReal(dsid.S57EditionNumber, 4, S57SubfieldNames.STED)
            .B11(dsid.ProductSpecification, S57SubfieldNames.PRSP)
            .Text(dsid.ProductSpecificationDescription, S57SubfieldNames.PSDN, S57FieldEncoder.Ascii)
            .Text(dsid.ProductSpecificationEdition, S57SubfieldNames.PRED, S57FieldEncoder.Ascii)
            .B11(dsid.ApplicationProfile, S57SubfieldNames.PROF)
            .B12(dsid.ProducingAgency, S57SubfieldNames.AGEN)
            .Text(dsid.Comment, S57SubfieldNames.COMT, S57FieldEncoder.Ascii);
        record.AddField(encoder.ToField(S57FieldTags.DSID));

        // S-57 Part 3, clause 7.3.1.2: the record counts describe the records written here.
        int meta = 0, cartographic = 0, geo = 0, collection = 0;
        foreach (var feature in document.FeatureRecords)
        {
            switch ((int)feature.ObjectCode)
            {
                case >= FirstMetaObjectCode and < FirstCollectionObjectCode: meta++; break;
                case >= FirstCollectionObjectCode and < FirstCartographicObjectCode: collection++; break;
                case >= FirstCartographicObjectCode and <= LastCartographicObjectCode: cartographic++; break;
                default: geo++; break;
            }
        }

        int isolatedNodes = 0, connectedNodes = 0, edges = 0, faces = 0;
        foreach (var vector in document.VectorRecords)
        {
            switch (vector.RecordName.RecordNameCode)
            {
                case S57RecordNameCodes.IsolatedNode: isolatedNodes++; break;
                case S57RecordNameCodes.ConnectedNode: connectedNodes++; break;
                case S57RecordNameCodes.Edge: edges++; break;
                case S57RecordNameCodes.Face: faces++; break;
            }
        }

        encoder.Begin($"{S57FieldTags.DSSI} ({dsid.RecordName})")
            .B11(dsid.DataStructure, S57SubfieldNames.DSTR)
            .B11(dsid.AttfLexicalLevel, S57SubfieldNames.AALL)
            .B11(dsid.NatfLexicalLevel, S57SubfieldNames.NALL)
            .B14(meta, S57SubfieldNames.NOMR)
            .B14(cartographic, S57SubfieldNames.NOCR)
            .B14(geo, S57SubfieldNames.NOGR)
            .B14(collection, S57SubfieldNames.NOLR)
            .B14(isolatedNodes, S57SubfieldNames.NOIN)
            .B14(connectedNodes, S57SubfieldNames.NOCN)
            .B14(edges, S57SubfieldNames.NOED)
            .B14(faces, S57SubfieldNames.NOFA);
        record.AddField(encoder.ToField(S57FieldTags.DSSI));

        return record.Build();
    }

    private static Iso8211Record DataSetParametersRecord(S57FieldEncoder encoder, int recordNumber, S57DataSetParameters dspm)
    {
        var record = NewDataRecord(encoder, recordNumber);

        // S-57 Part 3, clause 7.3.2.1.
        encoder.Begin($"{S57FieldTags.DSPM} ({dspm.RecordName})")
            .B11(dspm.RecordName.RecordNameCode, S57SubfieldNames.RCNM)
            .B14(unchecked((uint)dspm.RecordName.RecordId), S57SubfieldNames.RCID)
            .B11(dspm.HorizontalDatum, S57SubfieldNames.HDAT)
            .B11(dspm.VerticalDatum, S57SubfieldNames.VDAT)
            .B11(dspm.SoundingDatum, S57SubfieldNames.SDAT)
            .B14(dspm.CompilationScale, S57SubfieldNames.CSCL)
            .B11(dspm.DepthUnits, S57SubfieldNames.DUNI)
            .B11(dspm.HeightUnits, S57SubfieldNames.HUNI)
            .B11(dspm.PositionalUnits, S57SubfieldNames.PUNI)
            .B11(dspm.CoordinateUnits, S57SubfieldNames.COUN)
            .B14(dspm.CoordinateMultiplicationFactor, S57SubfieldNames.COMF)
            .B14(dspm.SoundingMultiplicationFactor, S57SubfieldNames.SOMF)
            .Text(dspm.Comment, S57SubfieldNames.COMT, S57FieldEncoder.Ascii);
        record.AddField(encoder.ToField(S57FieldTags.DSPM));

        return record.Build();
    }

    private static Iso8211Record VectorRecord(S57FieldEncoder encoder, int recordNumber, S57VectorRecord vector)
    {
        var context = $"{S57FieldTags.VRID} ({vector.RecordName})";
        EnsureBaseRecord(context, vector.UpdateInstruction,
            vector.VectorPointerControl is not null || vector.CoordinateControl is not null);

        var record = NewDataRecord(encoder, recordNumber);

        // S-57 Part 3, clause 7.7.1.1.
        encoder.Begin(context)
            .B11(vector.RecordName.RecordNameCode, S57SubfieldNames.RCNM)
            .B14(unchecked((uint)vector.RecordName.RecordId), S57SubfieldNames.RCID)
            .B12(vector.RecordVersion, S57SubfieldNames.RVER)
            .B11((byte)vector.UpdateInstruction, S57SubfieldNames.RUIN);
        record.AddField(encoder.ToField(S57FieldTags.VRID));

        // ATTV values are basic text, always lexical level 0 (S-57 Part 3, clause 2.4).
        if (vector.Attributes.Count > 0)
        {
            encoder.Begin($"{S57FieldTags.ATTV} ({vector.RecordName})");
            foreach (var attribute in vector.Attributes)
            {
                encoder.B12(attribute.AttributeCode, S57SubfieldNames.ATTL)
                    .Text(attribute.Value, S57SubfieldNames.ATVL, S57FieldEncoder.Ascii);
            }

            record.AddField(encoder.ToField(S57FieldTags.ATTV));
        }

        if (vector.VectorPointers.Count > 0)
        {
            encoder.Begin($"{S57FieldTags.VRPT} ({vector.RecordName})");
            foreach (var pointer in vector.VectorPointers)
            {
                Name(encoder, pointer.Name)
                    .B11((byte)pointer.Orientation, S57SubfieldNames.ORNT)
                    .B11((byte)pointer.Usage, S57SubfieldNames.USAG)
                    .B11((byte)pointer.Topology, S57SubfieldNames.TOPI)
                    .B11((byte)pointer.Mask, S57SubfieldNames.MASK);
            }

            record.AddField(encoder.ToField(S57FieldTags.VRPT));
        }

        if (vector.Coordinates2D.Count > 0)
        {
            encoder.Begin($"{S57FieldTags.SG2D} ({vector.RecordName})");
            foreach (var coordinate in vector.Coordinates2D)
            {
                encoder.B24(coordinate.Y).B24(coordinate.X);
            }

            record.AddField(encoder.ToField(S57FieldTags.SG2D));
        }

        if (vector.Soundings.Count > 0)
        {
            encoder.Begin($"{S57FieldTags.SG3D} ({vector.RecordName})");
            foreach (var sounding in vector.Soundings)
            {
                encoder.B24(sounding.Y).B24(sounding.X).B24(sounding.Depth);
            }

            record.AddField(encoder.ToField(S57FieldTags.SG3D));
        }

        return record.Build();
    }

    private static Iso8211Record FeatureRecord(
        S57FieldEncoder encoder,
        int recordNumber,
        S57FeatureRecord feature,
        Encoding attfEncoding,
        Encoding natfEncoding)
    {
        var context = $"{S57FieldTags.FRID} ({feature.RecordName})";
        EnsureBaseRecord(context, feature.UpdateInstruction,
            feature.SpatialPointerControl is not null || feature.FeaturePointerControl is not null);

        var record = NewDataRecord(encoder, recordNumber);
        var name = feature.RecordName;

        // S-57 Part 3, clause 7.6.1.1.
        encoder.Begin(context)
            .B11(name.RecordNameCode, S57SubfieldNames.RCNM)
            .B14(unchecked((uint)name.RecordId), S57SubfieldNames.RCID)
            .B11((byte)feature.Primitive, S57SubfieldNames.PRIM)
            .B11(feature.Group, S57SubfieldNames.GRUP)
            .B12((int)feature.ObjectCode, S57SubfieldNames.OBJL)
            .B12(feature.RecordVersion, S57SubfieldNames.RVER)
            .B11((byte)feature.UpdateInstruction, S57SubfieldNames.RUIN);
        record.AddField(encoder.ToField(S57FieldTags.FRID));

        // The ENC product specification requires FOID on every feature; a feature without a
        // long name gets one of zeros, which reads back as no long name.
        encoder.Begin($"{S57FieldTags.FOID} ({name})")
            .B12(name.AgencyCode, S57SubfieldNames.AGEN)
            .B14(unchecked((uint)name.FeatureId), S57SubfieldNames.FIDN)
            .B12(name.FeatureSubdivision, S57SubfieldNames.FIDS);
        record.AddField(encoder.ToField(S57FieldTags.FOID));

        if (feature.Attributes.Count > 0)
        {
            record.AddField(Attributes(encoder, S57FieldTags.ATTF, name, feature.Attributes, attfEncoding));
        }

        if (feature.NationalAttributes.Count > 0)
        {
            record.AddField(Attributes(encoder, S57FieldTags.NATF, name, feature.NationalAttributes, natfEncoding));
        }

        if (feature.FeaturePointers.Count > 0)
        {
            encoder.Begin($"{S57FieldTags.FFPT} ({name})");
            foreach (var pointer in feature.FeaturePointers)
            {
                encoder.B12(pointer.Name.AgencyCode, S57SubfieldNames.AGEN)
                    .B14(pointer.Name.FeatureId, S57SubfieldNames.FIDN)
                    .B12(pointer.Name.FeatureSubdivision, S57SubfieldNames.FIDS)
                    .B11((byte)pointer.Relationship, S57SubfieldNames.RIND)
                    .Text(pointer.Comment, S57SubfieldNames.COMT, S57FieldEncoder.Ascii);
            }

            record.AddField(encoder.ToField(S57FieldTags.FFPT));
        }

        if (feature.SpatialPointers.Count > 0)
        {
            encoder.Begin($"{S57FieldTags.FSPT} ({name})");
            foreach (var pointer in feature.SpatialPointers)
            {
                Name(encoder, pointer.Name)
                    .B11((byte)pointer.Orientation, S57SubfieldNames.ORNT)
                    .B11((byte)pointer.Usage, S57SubfieldNames.USAG)
                    .B11((byte)pointer.Mask, S57SubfieldNames.MASK);
            }

            record.AddField(encoder.ToField(S57FieldTags.FSPT));
        }

        return record.Build();
    }

    private static Iso8211Field Attributes(
        S57FieldEncoder encoder,
        string tag,
        S57RecordName name,
        IReadOnlyList<S57AttributeValue> attributes,
        Encoding encoding)
    {
        encoder.Begin($"{tag} ({name})");
        foreach (var attribute in attributes)
        {
            encoder.B12(attribute.AttributeCode, S57SubfieldNames.ATTL)
                .Text(attribute.Value, S57SubfieldNames.ATVL, encoding);
        }

        // A field at lexical level 2 ends with the UCS-2 field terminator (S-57 Part 3, clause 2.5).
        return encoder.ToField(tag, twoByteTerminator: ReferenceEquals(encoding, S57FieldEncoder.Ucs2));
    }

    // The NAME subfield of VRPT and FSPT: RCNM (b11) then RCID (b14), as one B(40) subfield.
    private static S57FieldEncoder Name(S57FieldEncoder encoder, S57RecordName name) =>
        encoder.B11(name.RecordNameCode, S57SubfieldNames.RCNM)
            .B14(unchecked((uint)name.RecordId), S57SubfieldNames.RCID);

    // Only Encode checks the document, so this runs after it and can rely on the DSID.
    private static Iso8211WriterOptions OptionsFor(S57Document document) =>
        document.DataSetIdentification?.NatfLexicalLevel == 2 ? Ucs2NationalAttributeWriterOptions : WriterOptions;

    private static void EnsureBaseRecord(string context, S57UpdateInstruction instruction, bool hasControlField)
    {
        if (instruction != S57UpdateInstruction.Insert)
        {
            throw new ArgumentException(
                $"{context}: the record update instruction (RUIN) is {instruction}, but a base cell holds only Insert records. Update cells aren't supported.");
        }

        if (hasControlField)
        {
            throw new ArgumentException(
                $"{context}: the record has an update control field (FSPC, FFPC, VRPC or SGCC), which only update cells use. Update cells aren't supported.");
        }
    }

    // A data record leader as S-57 requires it (S-57 Part 3, Annex A, table A.3), starting with
    // the record identifier field.
    private static Iso8211RecordBuilder NewDataRecord(S57FieldEncoder encoder, int recordNumber)
    {
        var builder = new Iso8211RecordBuilder(WriterOptions)
        {
            InterchangeLevel = ' ',
            LeaderIdentifier = 'D',
            InlineCodeExtensionIndicator = ' ',
            VersionNumber = ' ',
            ApplicationIndicator = ' ',
            ExtendedCharacterSetIndicator = "   "
        };

        // The record identifier is a b12, so a cell with more than 65,535 records wraps it.
        builder.AddField(encoder.Begin(RecordIdentifierTag).B12(recordNumber & 0xFFFF, RecordIdentifierTag).ToField(RecordIdentifierTag));
        return builder;
    }
}
