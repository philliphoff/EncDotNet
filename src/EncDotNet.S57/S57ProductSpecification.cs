namespace EncDotNet.S57;

/// <summary>
/// The product specification codes of the DSID subfield PRSP (S-57 Edition 3.1, Part 3 §7.3.1.1
/// and §1.4.1), for setting the code of a new data set.
/// </summary>
/// <remarks>
/// <para>
/// The values are the exact PRSP codes, so cast between this enum and
/// <see cref="S57DataSetIdentification.ProductSpecification"/> in either direction. To interpret
/// the code of a data set that has been read, use <see cref="S57DataSetIdentification.Product"/>
/// instead: its <see cref="S57ProductKind"/> groups the codes but doesn't hold them, so it can't
/// be cast to a PRSP code.
/// </para>
/// <para>
/// NATO Additional Military Layers (AML) products use the codes 51-57, which
/// <see cref="Aml.AmlProduct"/> names; its values are PRSP codes too. A code this enum doesn't
/// name, such as an AML code, casts to an unnamed value rather than failing.
/// </para>
/// </remarks>
public enum S57ProductSpecification
{
    /// <summary>An Electronic Navigational Chart (<c>ENC</c>, code 1).</summary>
    Enc = 1,

    /// <summary>The IHO Object Catalogue Data Dictionary (<c>ODD</c>, code 2).</summary>
    DataDictionary = 2,

    /// <summary>
    /// An Inland ENC (code 10). This code is defined by the Inland ENC product specification,
    /// not by S-57.
    /// </summary>
    InlandEnc = 10
}
