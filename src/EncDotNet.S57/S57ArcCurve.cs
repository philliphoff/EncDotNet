namespace EncDotNet.S57;

/// <summary>
/// Represents an arc/curve definition (ARCC) of an edge vector record, with the coordinate
/// fields that follow it (S-57 Part 3, clauses 5.1.4.4 and 7.7.1.8 to 7.7.1.11).
/// </summary>
/// <remarks>
/// <para>
/// An edge's coordinates are given either by SG2D or by one or more ARCC fields; the two are
/// mutually exclusive. The <see cref="Type"/> says which coordinate field the definition uses:
/// AR2D (<see cref="Arcs"/>) for <see cref="S57ArcCurveType.ArcThreePointCentre"/>, EL2D
/// (<see cref="EllipticalArcs"/>) for <see cref="S57ArcCurveType.EllipticalArc"/>, and CT2D
/// (<see cref="ControlPoints"/>) for the curve types.
/// </para>
/// <para>
/// Use <see cref="S57ArcDensifier"/> to turn the definition into a polyline.
/// </para>
/// </remarks>
public sealed record S57ArcCurve
{
    /// <summary>Gets the arc/curve type (ATYP).</summary>
    public S57ArcCurveType Type { get; init; }

    /// <summary>Gets the construction surface (SURF).</summary>
    public S57ConstructionSurface Surface { get; init; }

    /// <summary>Gets the curve order (ORDR): the largest exponent of the polynomial, from 1 to 9.</summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets the interpolated point resolution (RESO) as encoded: an integer to divide by
    /// <see cref="FloatingPointMultiplicationFactor"/>.
    /// </summary>
    /// <seealso cref="InterpolatedPointResolution"/>
    public long Resolution { get; init; }

    /// <summary>Gets the floating point multiplication factor (FPMF) of <see cref="Resolution"/>.</summary>
    public long FloatingPointMultiplicationFactor { get; init; }

    /// <summary>
    /// Gets the spacing between interpolated points along the line, in map units (millimetres),
    /// or <c>null</c> when <see cref="FloatingPointMultiplicationFactor"/> is 0.
    /// </summary>
    public double? InterpolatedPointResolution =>
        FloatingPointMultiplicationFactor == 0 ? null : (double)Resolution / FloatingPointMultiplicationFactor;

    /// <summary>Gets the arcs from the AR2D fields that follow the definition.</summary>
    public IReadOnlyList<S57Arc> Arcs { get; init; } = [];

    /// <summary>Gets the elliptical arcs from the EL2D fields that follow the definition.</summary>
    public IReadOnlyList<S57EllipticalArc> EllipticalArcs { get; init; } = [];

    /// <summary>Gets the curve control points from the CT2D fields that follow the definition.</summary>
    /// <remarks>
    /// When the reader finds AR2D or EL2D coordinates that don't make a whole arc, as in an update
    /// record whose SGCC field addresses single coordinates, it keeps them here, in order.
    /// </remarks>
    public IReadOnlyList<S57Coordinate2D> ControlPoints { get; init; } = [];
}
