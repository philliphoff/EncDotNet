namespace EncDotNet.S57.Aml;

/// <summary>
/// NATO Additional Military Layers (AML) products, identified by the product specification
/// code (PRSP) in the data set identification field (DSID).
/// </summary>
/// <remarks>
/// Codes 51-56 are listed in the UKHO AML product specifications and in the 2018 AML guidance
/// document. Code 57 identifies the AML 3.0 integrated product, which carries every layer in
/// one data set and isn't listed in the 2018 guidance document.
/// </remarks>
public enum AmlProduct
{
    /// <summary>Maritime Foundation and Facilities (PRSP 51).</summary>
    MFF = 51,

    /// <summary>Routes, Areas and Limits (PRSP 52).</summary>
    RAL = 52,

    /// <summary>Large Bottom Objects (PRSP 53).</summary>
    LBO = 53,

    /// <summary>Contour Line Bathymetry (PRSP 54).</summary>
    CLB = 54,

    /// <summary>Environment, Seabed and Beach (PRSP 55).</summary>
    ESB = 55,

    /// <summary>Small Bottom Objects (PRSP 56).</summary>
    SBO = 56,

    /// <summary>The AML 3.0 integrated product (PRSP 57).</summary>
    Integrated = 57
}
