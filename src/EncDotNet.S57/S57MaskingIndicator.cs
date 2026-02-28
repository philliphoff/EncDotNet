namespace EncDotNet.S57;

/// <summary>
/// S-57 Masking Indicator (MASK) - indicates masking behavior.
/// </summary>
public enum S57MaskingIndicator : byte
{
    /// <summary>Mask</summary>
    Mask = 1,

    /// <summary>Show</summary>
    Show = 2,

    /// <summary>Not applicable</summary>
    NotApplicable = 255
}
