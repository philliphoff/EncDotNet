using System.Globalization;
using EncDotNet.S57.Aml;

namespace EncDotNet.S57;

/// <summary>
/// Identifies the product an S-57 data set conforms to: its kind, its product specification
/// edition, and whether it's a base data set or an update.
/// </summary>
/// <remarks>
/// <para>
/// The identity is derived from the data set identification field (DSID); get it from
/// <see cref="S57DataSetIdentification.Product"/>. The product specification code (PRSP) gives
/// <see cref="Kind"/> and <see cref="AmlProduct"/>, the product specification edition (PRED)
/// gives <see cref="Edition"/> and <see cref="Version"/>, and the application profile (PROF)
/// gives <see cref="IsUpdate"/>.
/// </para>
/// <para>
/// PRSP codes: 1 = ENC, 2 = ODD, 10 = Inland ENC, 51-57 = AML (see
/// <see cref="Aml.AmlProduct"/>).
/// </para>
/// </remarks>
public readonly record struct S57ProductIdentity
{
    private readonly string? _edition;

    /// <summary>Gets the kind of product.</summary>
    public S57ProductKind Kind { get; init; }

    /// <summary>
    /// Gets the AML product when <see cref="Kind"/> is <see cref="S57ProductKind.Aml"/>;
    /// otherwise <see langword="null"/>.
    /// </summary>
    public AmlProduct? AmlProduct { get; init; }

    /// <summary>
    /// Gets the product specification edition (PRED) exactly as recorded, for example
    /// <c>"2.0"</c> for an ENC or <c>"3.0"</c> for an AML 3.0 data set.
    /// </summary>
    public string Edition
    {
        get => _edition ?? string.Empty;
        init => _edition = value;
    }

    /// <summary>
    /// Gets <see cref="Edition"/> parsed as a version, or <see langword="null"/> when it isn't a
    /// version number. An edition with only a major number, such as <c>"3"</c>, parses as 3.0.
    /// </summary>
    public Version? Version { get; init; }

    /// <summary>
    /// Gets a value indicating whether the data set is an update rather than a base data set
    /// (a new edition or re-issue).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The application profile (PROF) decides it when it can: 1 = EN (ENC new) and 2 = ER (ENC
    /// revision) from S-57 Edition 3.1, Part 3 §7.3.1.1, and the AML profiles CLB 4/5, ESB 6/7,
    /// LBO 10/11, MFF 12/13, RAL 16/17 and SBO 18/19, where the first code is the base and the
    /// second the update.
    /// </para>
    /// <para>
    /// AML 3.0 uses PROF 20 for both, and other products may not set PROF, so otherwise the
    /// data set name (DSNM) is used: a numeric file extension other than <c>000</c> marks an
    /// update, following the S-57 Appendix B.1 file naming convention. If the name has no
    /// numeric extension, an exchange purpose (EXPP) of 2 (R, revision) marks an update.
    /// </para>
    /// </remarks>
    public bool IsUpdate { get; init; }

    /// <summary>
    /// Derives the product identity from a data set identification field.
    /// </summary>
    /// <param name="identification">The data set identification field.</param>
    /// <returns>The product identity.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identification"/> is <see langword="null"/>.</exception>
    public static S57ProductIdentity FromDataSetIdentification(S57DataSetIdentification identification)
    {
        ArgumentNullException.ThrowIfNull(identification);

        var prsp = identification.ProductSpecification;
        var kind = prsp switch
        {
            1 => S57ProductKind.Enc,
            2 => S57ProductKind.DataDictionary,
            10 => S57ProductKind.InlandEnc,
            >= (int)Aml.AmlProduct.MFF and <= (int)Aml.AmlProduct.Integrated => S57ProductKind.Aml,
            _ => S57ProductKind.Unknown
        };

        var edition = identification.ProductSpecificationEdition;

        return new S57ProductIdentity
        {
            Kind = kind,
            AmlProduct = kind == S57ProductKind.Aml ? (AmlProduct)prsp : null,
            Edition = edition,
            Version = ParseVersion(edition),
            IsUpdate = IsUpdateDataSet(identification)
        };
    }

    private static Version? ParseVersion(string edition)
    {
        var trimmed = edition.Trim();
        if (Version.TryParse(trimmed, out var version))
            return version;

        return int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out var major)
            ? new Version(major, 0)
            : null;
    }

    private static bool IsUpdateDataSet(S57DataSetIdentification identification)
    {
        switch (identification.ApplicationProfile)
        {
            case 1: // EN
            case 4 or 6 or 10 or 12 or 16 or 18: // AML base profiles
                return false;
            case 2: // ER
            case 5 or 7 or 11 or 13 or 17 or 19: // AML update profiles
                return true;
        }

        var name = identification.DataSetName.Trim();
        var dot = name.LastIndexOf('.');
        if (dot >= 0 &&
            int.TryParse(name.AsSpan(dot + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var extension))
        {
            return extension != 0;
        }

        return identification.ExchangePurpose == 2;
    }
}
