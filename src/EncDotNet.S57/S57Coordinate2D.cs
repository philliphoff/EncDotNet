namespace EncDotNet.S57;

/// <summary>
/// Represents a 2D coordinate in S-57.
/// </summary>
public readonly record struct S57Coordinate2D
{
    /// <summary>Gets the X coordinate (XCOO) or longitude.</summary>
    public int X { get; init; }

    /// <summary>Gets the Y coordinate (YCOO) or latitude.</summary>
    public int Y { get; init; }

    /// <summary>
    /// Converts to decimal degrees using the specified multiplication factor.
    /// </summary>
    public (double Longitude, double Latitude) ToDecimalDegrees(int multiplicationFactor)
    {
        return ((double)X / multiplicationFactor, (double)Y / multiplicationFactor);
    }
}
