using System.Collections.Immutable;

namespace EncDotNet.Enc;

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
