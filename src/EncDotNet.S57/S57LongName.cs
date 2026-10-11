using System.Globalization;

namespace EncDotNet.S57;

/// <summary>
/// Represents a feature object identifier, the "Long Name" (LNAM) that identifies a feature
/// object and that feature-to-feature pointers use to refer to it (S-57 Part 3, 4.3 and 4.6).
/// </summary>
/// <remarks>
/// A long name is the feature's producing agency (AGEN), feature identification number (FIDN)
/// and feature identification subdivision (FIDS), from its FOID field. Unlike a record's
/// <see cref="S57RecordName"/> (RCNM and RCID), it stays the same across editions and updates,
/// which is why FFPT pointers use it as their foreign key.
/// </remarks>
public readonly record struct S57LongName
{
    /// <summary>
    /// Initializes a new instance of the <see cref="S57LongName"/> struct.
    /// </summary>
    /// <param name="agencyCode">The producing agency (AGEN).</param>
    /// <param name="featureId">The feature identification number (FIDN).</param>
    /// <param name="featureSubdivision">The feature identification subdivision (FIDS).</param>
    public S57LongName(ushort agencyCode, uint featureId, ushort featureSubdivision)
    {
        AgencyCode = agencyCode;
        FeatureId = featureId;
        FeatureSubdivision = featureSubdivision;
    }

    /// <summary>Gets the producing agency code (AGEN).</summary>
    public ushort AgencyCode { get; init; }

    /// <summary>Gets the feature identification number (FIDN).</summary>
    public uint FeatureId { get; init; }

    /// <summary>Gets the feature identification subdivision (FIDS).</summary>
    public ushort FeatureSubdivision { get; init; }

    /// <inheritdoc/>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"AGEN={AgencyCode}, FIDN={FeatureId}, FIDS={FeatureSubdivision}");
}
