using System.Collections.Immutable;

namespace EncDotNet.Enc;

/// <summary>
/// S-57 Record Name Codes (RCNM) that identify the type of record.
/// </summary>
public static class S57RecordNameCodes
{
    /// <summary>Data Set General Information Record</summary>
    public const int DataSetGeneralInfo = 10;

    /// <summary>Data Set Geographic Reference Record</summary>
    public const int DataSetGeoReference = 20;

    /// <summary>Data Set History Record</summary>
    public const int DataSetHistory = 30;

    /// <summary>Data Set Accuracy Record</summary>
    public const int DataSetAccuracy = 40;

    /// <summary>Catalogue Cross Reference Record</summary>
    public const int CatalogueCrossReference = 50;

    /// <summary>Data Dictionary Definition Record</summary>
    public const int DataDictionaryDefinition = 60;

    /// <summary>Data Dictionary Domain Record</summary>
    public const int DataDictionaryDomain = 70;

    /// <summary>Data Dictionary Schema Record</summary>
    public const int DataDictionarySchema = 80;

    /// <summary>Feature Record</summary>
    public const int Feature = 100;

    /// <summary>Isolated Node (VI)</summary>
    public const int IsolatedNode = 110;

    /// <summary>Connected Node (VC)</summary>
    public const int ConnectedNode = 120;

    /// <summary>Edge (VE)</summary>
    public const int Edge = 130;

    /// <summary>Face (VF)</summary>
    public const int Face = 140;
}

/// <summary>
/// S-57 Object Geographic Primitives (PRIM).
/// </summary>
public enum S57GeometricPrimitive : byte
{
    /// <summary>Point</summary>
    Point = 1,

    /// <summary>Line</summary>
    Line = 2,

    /// <summary>Area</summary>
    Area = 3,

    /// <summary>No geometry (meta objects)</summary>
    None = 255
}

/// <summary>
/// S-57 Relationship Indicator (RIND) - indicates the relationship between a feature and its spatial object.
/// </summary>
public enum S57RelationshipIndicator : byte
{
    /// <summary>Master</summary>
    Master = 1,

    /// <summary>Slave</summary>
    Slave = 2,

    /// <summary>Peer</summary>
    Peer = 3
}

/// <summary>
/// S-57 Orientation (ORNT) - indicates the orientation of a spatial object reference.
/// </summary>
public enum S57Orientation : byte
{
    /// <summary>Forward</summary>
    Forward = 1,

    /// <summary>Reverse</summary>
    Reverse = 2,

    /// <summary>Not applicable (for nodes)</summary>
    NotApplicable = 255
}

/// <summary>
/// S-57 Usage Indicator (USAG) - indicates how a spatial object is used.
/// </summary>
public enum S57UsageIndicator : byte
{
    /// <summary>Exterior</summary>
    Exterior = 1,

    /// <summary>Interior</summary>
    Interior = 2,

    /// <summary>Exterior boundary truncated by data limit</summary>
    ExteriorTruncated = 3,

    /// <summary>Not applicable</summary>
    NotApplicable = 255
}

/// <summary>
/// S-57 Masking Indicator (MASK) - indicates masking behavior.
/// </summary>
public enum S57MaskingIndicator : byte
{
    /// <summary>Mask</summary>
    Mask = 1,

    /// <summary>Show</summary>
    Show = 2,

    /// <summary>Not applicable</summary>
    NotApplicable = 255
}

/// <summary>
/// S-57 Topology Indicator (TOPI) - indicates topology of an edge reference.
/// </summary>
public enum S57TopologyIndicator : byte
{
    /// <summary>Beginning node</summary>
    Beginning = 1,

    /// <summary>End node</summary>
    End = 2,

    /// <summary>Left face</summary>
    LeftFace = 3,

    /// <summary>Right face</summary>
    RightFace = 4,

    /// <summary>Containing face</summary>
    ContainingFace = 5
}

/// <summary>
/// S-57 Update Instruction (RUIN).
/// </summary>
public enum S57UpdateInstruction : byte
{
    /// <summary>Insert</summary>
    Insert = 1,

    /// <summary>Delete</summary>
    Delete = 2,

    /// <summary>Modify</summary>
    Modify = 3
}

/// <summary>
/// Represents the Data Set Identification field (DSID) from S-57.
/// </summary>
public sealed class S57DataSetIdentification
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the intended usage code (INTU).</summary>
    public int IntendedUsage { get; init; }

    /// <summary>Gets the data set name (DSNM).</summary>
    public string DataSetName { get; init; } = string.Empty;

    /// <summary>Gets the edition number (EDTN).</summary>
    public string EditionNumber { get; init; } = string.Empty;

    /// <summary>Gets the update number (UPDN).</summary>
    public string UpdateNumber { get; init; } = string.Empty;

    /// <summary>Gets the update application date (UADT).</summary>
    public string UpdateApplicationDate { get; init; } = string.Empty;

    /// <summary>Gets the issue date (ISDT).</summary>
    public string IssueDate { get; init; } = string.Empty;

    /// <summary>Gets the edition date (STED).</summary>
    public string S57EditionNumber { get; init; } = string.Empty;

    /// <summary>Gets the producing agency code (PRSP).</summary>
    public int ProducingAgency { get; init; }

    /// <summary>Gets the data structure (DSTR).</summary>
    public int DataStructure { get; init; }

    /// <summary>Gets the lexical level for ATTF (AALL).</summary>
    public int AttfLexicalLevel { get; init; }

    /// <summary>Gets the lexical level for NATF (NALL).</summary>
    public int NatfLexicalLevel { get; init; }

    /// <summary>Gets the comment (COMT).</summary>
    public string Comment { get; init; } = string.Empty;
}

/// <summary>
/// Represents the Data Set Parameter field (DSPM) from S-57.
/// </summary>
public sealed class S57DataSetParameters
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the horizontal geodetic datum (HDAT).</summary>
    public int HorizontalDatum { get; init; }

    /// <summary>Gets the vertical datum (VDAT).</summary>
    public int VerticalDatum { get; init; }

    /// <summary>Gets the sounding datum (SDAT).</summary>
    public int SoundingDatum { get; init; }

    /// <summary>Gets the compilation scale (CSCL).</summary>
    public int CompilationScale { get; init; }

    /// <summary>Gets the units of depth measurement (DUNI).</summary>
    public int DepthUnits { get; init; }

    /// <summary>Gets the units of height measurement (HUNI).</summary>
    public int HeightUnits { get; init; }

    /// <summary>Gets the units of positional accuracy (PUNI).</summary>
    public int PositionalUnits { get; init; }

    /// <summary>Gets the coordinate units (COUN).</summary>
    public int CoordinateUnits { get; init; }

    /// <summary>Gets the coordinate multiplication factor (COMF).</summary>
    public int CoordinateMultiplicationFactor { get; init; }

    /// <summary>Gets the sounding multiplication factor (SOMF).</summary>
    public int SoundingMultiplicationFactor { get; init; }

    /// <summary>Gets the comment (COMT).</summary>
    public string Comment { get; init; } = string.Empty;
}

/// <summary>
/// Represents an S-57 attribute value.
/// </summary>
public readonly struct S57AttributeValue
{
    /// <summary>Gets the attribute code (ATTL).</summary>
    public int AttributeCode { get; init; }

    /// <summary>Gets the attribute value (ATVL).</summary>
    public string Value { get; init; }

    /// <summary>
    /// Creates an attribute value with the specified code and value.
    /// </summary>
    public S57AttributeValue(int attributeCode, string value)
    {
        AttributeCode = attributeCode;
        Value = value;
    }

    /// <inheritdoc/>
    public override string ToString() => $"ATTL={AttributeCode}, ATVL={Value}";
}

/// <summary>
/// Represents a feature-to-spatial object pointer (FSPT) in S-57.
/// </summary>
public readonly struct S57SpatialPointer
{
    /// <summary>Gets the name of the spatial record.</summary>
    public S57RecordName Name { get; init; }

    /// <summary>Gets the orientation.</summary>
    public S57Orientation Orientation { get; init; }

    /// <summary>Gets the usage indicator.</summary>
    public S57UsageIndicator Usage { get; init; }

    /// <summary>Gets the masking indicator.</summary>
    public S57MaskingIndicator Mask { get; init; }
}

/// <summary>
/// Represents a feature-to-feature object pointer (FFPT) in S-57.
/// </summary>
public readonly struct S57FeaturePointer
{
    /// <summary>Gets the name of the related feature record.</summary>
    public S57RecordName Name { get; init; }

    /// <summary>Gets the relationship indicator.</summary>
    public S57RelationshipIndicator Relationship { get; init; }

    /// <summary>Gets the comment.</summary>
    public string Comment { get; init; }
}

/// <summary>
/// Represents a spatial record pointer (VRPT) in S-57.
/// </summary>
public readonly struct S57VectorPointer
{
    /// <summary>Gets the name of the spatial record.</summary>
    public S57RecordName Name { get; init; }

    /// <summary>Gets the orientation.</summary>
    public S57Orientation Orientation { get; init; }

    /// <summary>Gets the usage indicator.</summary>
    public S57UsageIndicator Usage { get; init; }

    /// <summary>Gets the topology indicator.</summary>
    public S57TopologyIndicator Topology { get; init; }

    /// <summary>Gets the masking indicator.</summary>
    public S57MaskingIndicator Mask { get; init; }
}

/// <summary>
/// Represents a 2D coordinate in S-57.
/// </summary>
public readonly struct S57Coordinate2D
{
    /// <summary>Gets the X coordinate (XCOO) or longitude.</summary>
    public int X { get; init; }

    /// <summary>Gets the Y coordinate (YCOO) or latitude.</summary>
    public int Y { get; init; }

    /// <summary>
    /// Converts to decimal degrees using the specified multiplication factor.
    /// </summary>
    public (double Longitude, double Latitude) ToDecimalDegrees(int multiplicationFactor)
    {
        return ((double)X / multiplicationFactor, (double)Y / multiplicationFactor);
    }
}

/// <summary>
/// Represents a 3D sounding coordinate in S-57.
/// </summary>
public readonly struct S57Sounding
{
    /// <summary>Gets the X coordinate (XCOO) or longitude.</summary>
    public int X { get; init; }

    /// <summary>Gets the Y coordinate (YCOO) or latitude.</summary>
    public int Y { get; init; }

    /// <summary>Gets the depth value (VE3D).</summary>
    public int Depth { get; init; }

    /// <summary>
    /// Converts to decimal degrees and depth using the specified multiplication factors.
    /// </summary>
    public (double Longitude, double Latitude, double Depth) ToDecimalValues(
        int coordinateMultiplicationFactor,
        int soundingMultiplicationFactor)
    {
        return (
            (double)X / coordinateMultiplicationFactor,
            (double)Y / coordinateMultiplicationFactor,
            (double)Depth / soundingMultiplicationFactor
        );
    }
}

/// <summary>
/// Base class for S-57 feature records.
/// </summary>
public sealed class S57FeatureRecord
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the object geometric primitive (PRIM).</summary>
    public S57GeometricPrimitive Primitive { get; init; }

    /// <summary>Gets the group code (GRUP).</summary>
    public int Group { get; init; }

    /// <summary>Gets the object label/code (OBJL).</summary>
    public int ObjectCode { get; init; }

    /// <summary>Gets the record version (RVER).</summary>
    public int RecordVersion { get; init; }

    /// <summary>Gets the record update instruction (RUIN).</summary>
    public S57UpdateInstruction UpdateInstruction { get; init; }

    /// <summary>Gets the object attributes from ATTF field.</summary>
    public ImmutableArray<S57AttributeValue> Attributes { get; init; }

    /// <summary>Gets the national attributes from NATF field.</summary>
    public ImmutableArray<S57AttributeValue> NationalAttributes { get; init; }

    /// <summary>Gets the spatial pointers from FSPT field.</summary>
    public ImmutableArray<S57SpatialPointer> SpatialPointers { get; init; }

    /// <summary>Gets the feature pointers from FFPT field.</summary>
    public ImmutableArray<S57FeaturePointer> FeaturePointers { get; init; }
}

/// <summary>
/// Represents an S-57 vector (spatial) record.
/// </summary>
public sealed class S57VectorRecord
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the record version (RVER).</summary>
    public int RecordVersion { get; init; }

    /// <summary>Gets the record update instruction (RUIN).</summary>
    public S57UpdateInstruction UpdateInstruction { get; init; }

    /// <summary>Gets the attributes from ATTV field.</summary>
    public ImmutableArray<S57AttributeValue> Attributes { get; init; }

    /// <summary>Gets the vector record pointers from VRPT field.</summary>
    public ImmutableArray<S57VectorPointer> VectorPointers { get; init; }

    /// <summary>Gets the 2D coordinates from SG2D field.</summary>
    public ImmutableArray<S57Coordinate2D> Coordinates2D { get; init; }

    /// <summary>Gets the 3D sounding coordinates from SG3D field.</summary>
    public ImmutableArray<S57Sounding> Soundings { get; init; }
}

/// <summary>
/// Represents a complete S-57 Electronic Navigational Chart (ENC) document.
/// </summary>
public sealed class S57Document
{
    /// <summary>Gets the data set identification (DSID field).</summary>
    public S57DataSetIdentification? DataSetIdentification { get; init; }

    /// <summary>Gets the data set parameters (DSPM field).</summary>
    public S57DataSetParameters? DataSetParameters { get; init; }

    /// <summary>Gets all feature records.</summary>
    public ImmutableArray<S57FeatureRecord> FeatureRecords { get; init; }

    /// <summary>Gets all vector (spatial) records.</summary>
    public ImmutableArray<S57VectorRecord> VectorRecords { get; init; }

    /// <summary>
    /// Gets a feature record by its record name.
    /// </summary>
    public S57FeatureRecord? GetFeatureRecord(S57RecordName name)
    {
        return FeatureRecords.FirstOrDefault(r =>
            r.RecordName.RecordNameCode == name.RecordNameCode &&
            r.RecordName.RecordId == name.RecordId);
    }

    /// <summary>
    /// Gets a vector record by its record name.
    /// </summary>
    public S57VectorRecord? GetVectorRecord(S57RecordName name)
    {
        return VectorRecords.FirstOrDefault(r =>
            r.RecordName.RecordNameCode == name.RecordNameCode &&
            r.RecordName.RecordId == name.RecordId);
    }

    /// <summary>
    /// Gets all feature records with the specified object code.
    /// </summary>
    public IEnumerable<S57FeatureRecord> GetFeaturesByObjectCode(int objectCode)
    {
        return FeatureRecords.Where(r => r.ObjectCode == objectCode);
    }

    /// <summary>
    /// Gets the coordinate multiplication factor for converting integer coordinates to decimal degrees.
    /// </summary>
    public int CoordinateMultiplicationFactor =>
        DataSetParameters?.CoordinateMultiplicationFactor ?? 10000000;

    /// <summary>
    /// Gets the sounding multiplication factor for converting integer soundings to real values.
    /// </summary>
    public int SoundingMultiplicationFactor =>
        DataSetParameters?.SoundingMultiplicationFactor ?? 10;
}
