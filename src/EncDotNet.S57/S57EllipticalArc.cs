namespace EncDotNet.S57;

/// <summary>
/// Represents the ellipse coordinates (EL2D) of an elliptical arc
/// (S-57 Part 3, clauses 5.1.4.4 and 7.7.1.10).
/// </summary>
/// <remarks>
/// The ellipse is centred on <see cref="Centre"/>, with its axes through the conjugate diameter
/// points <see cref="MajorAxisPoint"/> and <see cref="MinorAxisPoint"/>, which are taken to be at
/// 90° to each other. The arc is drawn clockwise about the centre from <see cref="Start"/> to
/// <see cref="End"/>. Use <see cref="S57ArcDensifier"/> to turn it into a polyline.
/// </remarks>
public readonly record struct S57EllipticalArc
{
    /// <summary>Gets the start point (STPT), on the arc.</summary>
    public S57Coordinate2D Start { get; init; }

    /// <summary>Gets the centre point (CTPT).</summary>
    public S57Coordinate2D Centre { get; init; }

    /// <summary>Gets the end point (ENPT), on the arc.</summary>
    public S57Coordinate2D End { get; init; }

    /// <summary>Gets the conjugate diameter point on the major axis (CDPM).</summary>
    public S57Coordinate2D MajorAxisPoint { get; init; }

    /// <summary>Gets the conjugate diameter point on the minor axis (CDPR).</summary>
    public S57Coordinate2D MinorAxisPoint { get; init; }
}
