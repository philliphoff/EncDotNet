namespace EncDotNet.S57;

/// <summary>
/// S-57 Relationship Indicator (RIND) - indicates the relationship between a feature and its spatial object.
/// </summary>
public enum S57RelationshipIndicator : byte
{
    /// <summary>Master</summary>
    Master = 1,

    /// <summary>Slave</summary>
    Slave = 2,

    /// <summary>Peer</summary>
    Peer = 3
}
