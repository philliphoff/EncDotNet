namespace EncDotNet.S57;

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
    /// <remarks>
    /// FIDN is an unsigned 32-bit number (<c>b14</c>), so a value above <see cref="int.MaxValue"/>
    /// is held as the negative <see cref="int"/> with the same bits. Use <see cref="LongName"/>
    /// for the unsigned value.
    /// </remarks>
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
    /// <param name="agencyCode">The producing agency (AGEN).</param>
    /// <param name="featureId">The feature identification number (FIDN).</param>
    /// <param name="featureSubdivision">The feature identification subdivision (FIDS).</param>
    /// <returns>A record name with the long name values and no RCNM or RCID.</returns>
    public static S57RecordName FromLongName(int agencyCode, int featureId, int featureSubdivision)
    {
        return new S57RecordName
        {
            AgencyCode = agencyCode,
            FeatureId = featureId,
            FeatureSubdivision = featureSubdivision
        };
    }

    /// <summary>
    /// Creates a record name from a feature's long name.
    /// </summary>
    /// <param name="longName">The long name (AGEN, FIDN, FIDS).</param>
    /// <returns>A record name with the long name's values and no RCNM or RCID.</returns>
    /// <remarks>
    /// A FIDN above <see cref="int.MaxValue"/> is stored in <see cref="FeatureId"/> as the
    /// negative <see cref="int"/> with the same bits, so <see cref="LongName"/> gives back
    /// <paramref name="longName"/> for every long name other than all zeros.
    /// </remarks>
    public static S57RecordName FromLongName(S57LongName longName)
    {
        return new S57RecordName
        {
            AgencyCode = longName.AgencyCode,
            FeatureId = unchecked((int)longName.FeatureId),
            FeatureSubdivision = longName.FeatureSubdivision
        };
    }

    /// <summary>
    /// Gets the feature's long name (AGEN, FIDN, FIDS), or <c>null</c> when the record has none,
    /// such as a vector record or a feature record without a FOID field.
    /// </summary>
    public S57LongName? LongName =>
        AgencyCode == 0 && FeatureId == 0 && FeatureSubdivision == 0
            ? null
            : new S57LongName((ushort)AgencyCode, unchecked((uint)FeatureId), (ushort)FeatureSubdivision);

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"RCNM={RecordNameCode}, RCID={RecordId}";
    }
}
