namespace EncDotNet.S57;

/// <summary>
/// The data structure codes of the DSSI subfield DSTR (S-57 Edition 3.1, Part 3 §7.3.1.2 and
/// clause 3.1; Part 2 describes the topology levels).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetIdentification.DataStructure"/> in either direction. A code S-57 doesn't
/// define casts to an unnamed value rather than failing. The ENC product specification requires
/// <see cref="ChainNode"/>.
/// </remarks>
public enum S57DataStructure
{
    /// <summary>Cartographic spaghetti (<c>CS</c>, code 1).</summary>
    CartographicSpaghetti = 1,

    /// <summary>Chain-node (<c>CN</c>, code 2).</summary>
    ChainNode = 2,

    /// <summary>Planar graph (<c>PG</c>, code 3).</summary>
    PlanarGraph = 3,

    /// <summary>Full topology (<c>FT</c>, code 4).</summary>
    FullTopology = 4,

    /// <summary>Topology is not relevant (<c>NO</c>, code 255).</summary>
    TopologyNotRelevant = 255
}
