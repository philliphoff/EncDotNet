namespace EncDotNet.Enc;

/// <summary>
/// Represents an S-57 record name, which uniquely identifies an object within a dataset.
/// </summary>
/// <remarks>
/// A record name consists of a record identification number (RCID), a record name (RCNM),
/// and optionally an agency code and feature ID.
/// </remarks>
public readonly record struct S57RecordName
{
    /// <summary>
    /// Gets the record identification number (RCID).
    /// </summary>
    public int RecordId { get; init; }

    /// <summary>
    /// Gets the record name code (RCNM).
    /// </summary>
    public int RecordNameCode { get; init; }

    /// <summary>
    /// Gets the producing agency code (AGEN).
    /// </summary>
    public int AgencyCode { get; init; }

    /// <summary>
    /// Gets the feature identification number (FIDN).
    /// </summary>
    public int FeatureId { get; init; }

    /// <summary>
    /// Gets the feature identification subdivision (FIDS).
    /// </summary>
    public int FeatureSubdivision { get; init; }

    /// <summary>
    /// Creates a record name from the RCNM and RCID values.
    /// </summary>
    public static S57RecordName FromRcnmRcid(int rcnm, int rcid)
    {
        return new S57RecordName
        {
            RecordNameCode = rcnm,
            RecordId = rcid
        };
    }

    /// <summary>
    /// Creates a record name from the long name values (AGEN, FIDN, FIDS).
    /// </summary>
    public static S57RecordName FromLongName(int agencyCode, int featureId, int featureSubdivision)
    {
        return new S57RecordName
        {
            AgencyCode = agencyCode,
            FeatureId = featureId,
            FeatureSubdivision = featureSubdivision
        };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"RCNM={RecordNameCode}, RCID={RecordId}";
    }
}
