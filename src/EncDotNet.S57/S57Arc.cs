namespace EncDotNet.S57;

/// <summary>
/// Represents the arc coordinates (AR2D) of an arc 3 point centre
/// (S-57 Part 3, clauses 5.1.4.4 and 7.7.1.9).
/// </summary>
/// <remarks>
/// The arc starts at <see cref="Start"/>, which also gives its radius, and is drawn clockwise
/// about <see cref="Centre"/> until it reaches the direction of <see cref="End"/>. When the start
/// and end points are the same, the arc is a full circle. Use
/// <see cref="S57ArcDensifier"/> to turn it into a polyline.
/// </remarks>
public readonly record struct S57Arc
{
    /// <summary>Gets the start point (STPT), on the arc.</summary>
    public S57Coordinate2D Start { get; init; }

    /// <summary>Gets the centre point (CTPT).</summary>
    public S57Coordinate2D Centre { get; init; }

    /// <summary>Gets the end point (ENPT), on the arc.</summary>
    public S57Coordinate2D End { get; init; }
}
