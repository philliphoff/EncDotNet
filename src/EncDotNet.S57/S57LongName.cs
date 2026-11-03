using System.Buffers.Binary;
using System.Globalization;

namespace EncDotNet.S57;

/// <summary>
/// Represents a feature object identifier, the "Long Name" (LNAM) that identifies a feature
/// object and that feature-to-feature pointers use to refer to it (S-57 Part 3, 4.3 and 4.6).
/// </summary>
/// <remarks>
/// <para>
/// A long name is the feature's producing agency (AGEN), feature identification number (FIDN)
/// and feature identification subdivision (FIDS), from its FOID field. Unlike a record's
/// <see cref="S57RecordName"/> (RCNM and RCID), it stays the same across editions and updates,
/// which is why FFPT pointers use it as their foreign key.
/// </para>
/// <para>
/// In the binary implementation an FFPT pointer encodes the long name as a single 64-bit
/// <c>LNAM</c> subfield: AGEN (<c>b12</c>), FIDN (<c>b14</c>) and FIDS (<c>b12</c>), each
/// little-endian (S-57 Part 3, clauses 4.3, 7.2.2.1 and 7.6.6). Use <see cref="FromLnam"/>,
/// <see cref="ToLnam"/> and <see cref="TryWriteLnam"/> to convert between the two.
/// </para>
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

    /// <summary>
    /// The length of a binary <c>LNAM</c> subfield, in bytes.
    /// </summary>
    public const int LnamLength = 8;

    /// <summary>
    /// Decodes a long name from a binary <c>LNAM</c> subfield (S-57 Part 3, clause 4.3).
    /// </summary>
    /// <param name="lnam">
    /// The <see cref="LnamLength"/> bytes of the subfield: AGEN, FIDN and FIDS, each little-endian.
    /// </param>
    /// <returns>The long name.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="lnam"/> isn't <see cref="LnamLength"/> bytes long.
    /// </exception>
    public static S57LongName FromLnam(ReadOnlySpan<byte> lnam)
    {
        if (lnam.Length != LnamLength)
        {
            throw new ArgumentException(
                $"An LNAM subfield is {LnamLength} bytes long, but {lnam.Length} bytes were given.",
                nameof(lnam));
        }

        return new S57LongName(
            BinaryPrimitives.ReadUInt16LittleEndian(lnam),
            BinaryPrimitives.ReadUInt32LittleEndian(lnam[2..]),
            BinaryPrimitives.ReadUInt16LittleEndian(lnam[6..]));
    }

    /// <summary>
    /// Encodes the long name as a binary <c>LNAM</c> subfield (S-57 Part 3, clause 4.3).
    /// </summary>
    /// <returns>
    /// A new array of <see cref="LnamLength"/> bytes: AGEN, FIDN and FIDS, each little-endian.
    /// </returns>
    public byte[] ToLnam()
    {
        var lnam = new byte[LnamLength];
        TryWriteLnam(lnam);
        return lnam;
    }

    /// <summary>
    /// Tries to encode the long name as a binary <c>LNAM</c> subfield (S-57 Part 3, clause 4.3).
    /// </summary>
    /// <param name="destination">
    /// The span to write to. The first <see cref="LnamLength"/> bytes receive AGEN, FIDN and
    /// FIDS, each little-endian.
    /// </param>
    /// <returns>
    /// <c>true</c> when the long name was written; <c>false</c> when <paramref name="destination"/>
    /// is shorter than <see cref="LnamLength"/> bytes, in which case nothing is written.
    /// </returns>
    public bool TryWriteLnam(Span<byte> destination)
    {
        if (destination.Length < LnamLength)
        {
            return false;
        }

        BinaryPrimitives.WriteUInt16LittleEndian(destination, AgencyCode);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[2..], FeatureId);
        BinaryPrimitives.WriteUInt16LittleEndian(destination[6..], FeatureSubdivision);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"AGEN={AgencyCode}, FIDN={FeatureId}, FIDS={FeatureSubdivision}");
}
