namespace EncDotNet.S57;

/// <summary>
/// S-57 Object Geographic Primitives (PRIM).
/// </summary>
public enum S57GeometricPrimitive : byte
{
    /// <summary>Point</summary>
    Point = 1,

    /// <summary>Line</summary>
    Line = 2,

    /// <summary>Area</summary>
    Area = 3,

    /// <summary>No geometry (meta objects)</summary>
    None = 255
}
