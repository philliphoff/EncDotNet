namespace EncDotNet.S57;

/// <summary>
/// Contains S-57 subfield name constants.
/// </summary>
internal static class S57SubfieldNames
{
    // Common
    public const string RCNM = "RCNM";  // Record name
    public const string RCID = "RCID";  // Record identification number
    public const string RVER = "RVER";  // Record version
    public const string RUIN = "RUIN";  // Record update instruction

    // DSID subfields
    public const string EXPP = "EXPP";  // Exchange purpose
    public const string INTU = "INTU";  // Intended usage
    public const string DSNM = "DSNM";  // Data set name
    public const string EDTN = "EDTN";  // Edition number
    public const string UPDN = "UPDN";  // Update number
    public const string UADT = "UADT";  // Update application date
    public const string ISDT = "ISDT";  // Issue date
    public const string STED = "STED";  // S-57 edition number
    public const string PRSP = "PRSP";  // Product specification
    public const string PSDN = "PSDN";  // Product specification description
    public const string PRED = "PRED";  // Product specification edition number
    public const string PROF = "PROF";  // Application profile identification
    public const string AGEN = "AGEN";  // Producing agency
    public const string COMT = "COMT";  // Comment
    public const string DSTR = "DSTR";  // Data structure
    public const string AALL = "AALL";  // ATTF lexical level
    public const string NALL = "NALL";  // NATF lexical level

    // DSPM subfields
    public const string HDAT = "HDAT";  // Horizontal geodetic datum
    public const string VDAT = "VDAT";  // Vertical datum
    public const string SDAT = "SDAT";  // Sounding datum
    public const string CSCL = "CSCL";  // Compilation scale
    public const string DUNI = "DUNI";  // Units of depth measurement
    public const string HUNI = "HUNI";  // Units of height measurement
    public const string PUNI = "PUNI";  // Units of positional accuracy
    public const string COUN = "COUN";  // Coordinate units
    public const string COMF = "COMF";  // Coordinate multiplication factor
    public const string SOMF = "SOMF";  // Sounding multiplication factor

    // FRID subfields
    public const string PRIM = "PRIM";  // Object geometric primitive
    public const string GRUP = "GRUP";  // Group
    public const string OBJL = "OBJL";  // Object label/code

    // FOID subfields
    public const string FIDN = "FIDN";  // Feature identification number
    public const string FIDS = "FIDS";  // Feature identification subdivision

    // ATTF/NATF/ATTV subfields
    public const string ATTL = "ATTL";  // Attribute label/code
    public const string ATVL = "ATVL";  // Attribute value

    // FSPT subfields
    public const string NAME = "NAME";  // Name (pointer)
    public const string ORNT = "ORNT";  // Orientation
    public const string USAG = "USAG";  // Usage indicator
    public const string MASK = "MASK";  // Masking indicator

    // FFPT subfields
    public const string LNAM = "LNAM";  // Long name (pointer)
    public const string RIND = "RIND";  // Relationship indicator
    public const string FFUI = "FFUI";  // Feature-to-feature update instruction

    // VRPT subfields
    public const string TOPI = "TOPI";  // Topology indicator

    // SG2D/SG3D subfields
    public const string YCOO = "YCOO";  // Y coordinate
    public const string XCOO = "XCOO";  // X coordinate
    public const string VE3D = "VE3D";  // 3D (sounding) value

    // FSPC subfields
    public const string FSUI = "FSUI";  // Feature to Spatial record update instruction
    public const string FSIX = "FSIX";  // Feature to Spatial record pointer index
    public const string NSPT = "NSPT";  // Number of Spatial record pointers

    // FFPC subfields
    public const string FFIX = "FFIX";  // Feature to Feature record pointer index
    public const string NFPT = "NFPT";  // Number of Feature record pointers

    // VRPC subfields
    public const string VPUI = "VPUI";  // Vector record pointer update instruction
    public const string VPIX = "VPIX";  // Vector record pointer index
    public const string NVPT = "NVPT";  // Number of Vector record pointers

    // SGCC subfields
    public const string CCUI = "CCUI";  // Coordinate update instruction
    public const string CCIX = "CCIX";  // Coordinate index
    public const string CCNC = "CCNC";  // Number of coordinates

    // CATD subfields
    public const string FILE = "FILE";  // File name
    public const string LFIL = "LFIL";  // Long file name
    public const string VOLM = "VOLM";  // Volume
    public const string IMPL = "IMPL";  // Implementation
    public const string SLAT = "SLAT";  // Southernmost latitude
    public const string WLON = "WLON";  // Westernmost longitude
    public const string NLAT = "NLAT";  // Northernmost latitude
    public const string ELON = "ELON";  // Easternmost longitude
    public const string CRCS = "CRCS";  // CRC checksum
}
