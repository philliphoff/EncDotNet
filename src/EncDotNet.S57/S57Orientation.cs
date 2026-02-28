namespace EncDotNet.S57;

/// <summary>
/// S-57 Orientation (ORNT) - indicates the orientation of a spatial object reference.
/// </summary>
public enum S57Orientation : byte
{
    /// <summary>Forward</summary>
    Forward = 1,

    /// <summary>Reverse</summary>
    Reverse = 2,

    /// <summary>Not applicable (for nodes)</summary>
    NotApplicable = 255
}
