namespace EncDotNet.S57;

/// <summary>
/// S-57 Topology Indicator (TOPI) - indicates topology of an edge reference.
/// </summary>
public enum S57TopologyIndicator : byte
{
    /// <summary>Beginning node</summary>
    Beginning = 1,

    /// <summary>End node</summary>
    End = 2,

    /// <summary>Left face</summary>
    LeftFace = 3,

    /// <summary>Right face</summary>
    RightFace = 4,

    /// <summary>Containing face</summary>
    ContainingFace = 5
}
