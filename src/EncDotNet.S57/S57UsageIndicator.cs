namespace EncDotNet.S57;

/// <summary>
/// S-57 Usage Indicator (USAG) - indicates how a spatial object is used.
/// </summary>
public enum S57UsageIndicator : byte
{
    /// <summary>Exterior</summary>
    Exterior = 1,

    /// <summary>Interior</summary>
    Interior = 2,

    /// <summary>Exterior boundary truncated by data limit</summary>
    ExteriorTruncated = 3,

    /// <summary>Not applicable</summary>
    NotApplicable = 255
}
