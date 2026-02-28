namespace EncDotNet.S57;

/// <summary>
/// Represents a 3D sounding coordinate in S-57.
/// </summary>
public readonly record struct S57Sounding
{
    /// <summary>Gets the X coordinate (XCOO) or longitude.</summary>
    public int X { get; init; }

    /// <summary>Gets the Y coordinate (YCOO) or latitude.</summary>
    public int Y { get; init; }

    /// <summary>Gets the depth value (VE3D).</summary>
    public int Depth { get; init; }

    /// <summary>
    /// Converts to decimal degrees and depth using the specified multiplication factors.
    /// </summary>
    public (double Longitude, double Latitude, double Depth) ToDecimalValues(
        int coordinateMultiplicationFactor,
        int soundingMultiplicationFactor)
    {
        return (
            (double)X / coordinateMultiplicationFactor,
            (double)Y / coordinateMultiplicationFactor,
            (double)Depth / soundingMultiplicationFactor
        );
    }
}
