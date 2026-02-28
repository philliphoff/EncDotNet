namespace EncDotNet.S57;

/// <summary>
/// Contains S-57 field tag constants used in ISO 8211 encoded ENC files.
/// </summary>
internal static class S57FieldTags
{
    // Data Set Descriptive Records
    /// <summary>Data Set Identification</summary>
    public const string DSID = "DSID";

    /// <summary>Data Set Structure Information</summary>
    public const string DSSI = "DSSI";

    /// <summary>Data Set Parameter Field</summary>
    public const string DSPM = "DSPM";

    /// <summary>Data Set Projection</summary>
    public const string DSPR = "DSPR";

    /// <summary>Data Set Registration Control</summary>
    public const string DSRC = "DSRC";

    /// <summary>Data Set Accuracy</summary>
    public const string DSAC = "DSAC";

    // Feature Records
    /// <summary>Feature Record Identifier</summary>
    public const string FRID = "FRID";

    /// <summary>Feature Object Identifier</summary>
    public const string FOID = "FOID";

    /// <summary>Feature Record Attribute Field (ASCII)</summary>
    public const string ATTF = "ATTF";

    /// <summary>Feature Record National Attribute Field</summary>
    public const string NATF = "NATF";

    /// <summary>Feature Record to Feature Object Pointer</summary>
    public const string FFPT = "FFPT";

    /// <summary>Feature Record to Spatial Record Pointer</summary>
    public const string FSPT = "FSPT";

    // Vector Records
    /// <summary>Vector Record Identifier</summary>
    public const string VRID = "VRID";

    /// <summary>Vector Record Attribute Field</summary>
    public const string ATTV = "ATTV";

    /// <summary>Vector Record Pointer Field</summary>
    public const string VRPT = "VRPT";

    /// <summary>2D Coordinate Field</summary>
    public const string SG2D = "SG2D";

    /// <summary>3D Coordinate (Sounding) Field</summary>
    public const string SG3D = "SG3D";

    /// <summary>Arc/Curve Definition</summary>
    public const string ARCC = "ARCC";

    // Update Control Fields
    /// <summary>Feature Record to Spatial Record Pointer Control</summary>
    public const string FSPC = "FSPC";

    /// <summary>Feature Record to Feature Object Pointer Control</summary>
    public const string FFPC = "FFPC";

    /// <summary>Vector Record Pointer Control</summary>
    public const string VRPC = "VRPC";

    /// <summary>Coordinate Control</summary>
    public const string SGCC = "SGCC";

    // Catalogue Records
    /// <summary>Catalogue Directory Field</summary>
    public const string CATD = "CATD";

    /// <summary>Catalogue Cross Reference</summary>
    public const string CATX = "CATX";
}
