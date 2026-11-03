namespace EncDotNet.S57;

/// <summary>
/// The kind of product an S-57 data set conforms to, derived from the product specification
/// code (PRSP) in the data set identification field (DSID).
/// </summary>
public enum S57ProductKind
{
    /// <summary>The product specification code is absent or not recognized.</summary>
    Unknown = 0,

    /// <summary>An IHO Electronic Navigational Chart (PRSP 1, S-57 Edition 3.1, Part 3 §7.3.1.1).</summary>
    Enc = 1,

    /// <summary>An IHO Object Catalogue Data Dictionary (PRSP 2, S-57 Edition 3.1, Part 3 §7.3.1.1).</summary>
    DataDictionary = 2,

    /// <summary>An Inland ENC (PRSP 10).</summary>
    InlandEnc = 3,

    /// <summary>A NATO Additional Military Layers product (PRSP 51-57); see <see cref="Aml.AmlProduct"/>.</summary>
    Aml = 4
}
