namespace EncDotNet.S57;

/// <summary>
/// S-57 Arc/Curve type (ATYP): how the coordinates of an <see cref="S57ArcCurve"/> are
/// interpolated (S-57 Part 3, clauses 5.1.4.4 and 7.7.1.8).
/// </summary>
public enum S57ArcCurveType : byte
{
    /// <summary>
    /// Arc 3 point centre (<c>C</c>): a circular arc drawn clockwise about a centre point,
    /// from a start point to an end point, given by AR2D fields.
    /// </summary>
    ArcThreePointCentre = 1,

    /// <summary>
    /// Elliptical arc (<c>E</c>): an elliptical arc drawn clockwise about a centre point,
    /// given by EL2D fields.
    /// </summary>
    EllipticalArc = 2,

    /// <summary>Uniform B-spline (<c>U</c>), with control points given by CT2D fields.</summary>
    UniformBSpline = 3,

    /// <summary>Piecewise Bézier (<c>B</c>), with control points given by CT2D fields.</summary>
    PiecewiseBezier = 4,

    /// <summary>Non-uniform rational B-spline (<c>N</c>), with control points given by CT2D fields.</summary>
    NonUniformRationalBSpline = 5
}
