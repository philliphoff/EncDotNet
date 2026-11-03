namespace EncDotNet.S57;

/// <summary>
/// The application profile identification codes of the DSID subfield PROF (S-57 Edition 3.1,
/// Part 3 §7.3.1.1 and §1.4.2).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetIdentification.ApplicationProfile"/> in either direction. NATO Additional
/// Military Layers (AML) data sets use other codes (see
/// <see cref="S57DataSetIdentification.ApplicationProfile"/>), which cast to unnamed values
/// rather than failing.
/// </remarks>
public enum S57ApplicationProfile
{
    /// <summary>ENC new (<c>EN</c>, code 1): a base cell.</summary>
    EncNew = 1,

    /// <summary>ENC revision (<c>ER</c>, code 2): an update cell.</summary>
    EncRevision = 2,

    /// <summary>IHO data dictionary (<c>DD</c>, code 3).</summary>
    DataDictionary = 3
}
