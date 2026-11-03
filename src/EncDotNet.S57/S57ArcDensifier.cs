namespace EncDotNet.S57;

/// <summary>
/// Turns S-57 arcs (AR2D) and elliptical arcs (EL2D) into polylines.
/// </summary>
/// <remarks>
/// <para>
/// Arcs are drawn clockwise about their centre, from the start point to the end point
/// (S-57 Part 3, clause 5.1.4.4), with X as longitude (east) and Y as latitude (north). When the
/// start and end points are the same, the arc is a full circle. The arc is interpolated in the
/// coordinate plane of the data set, whatever its construction surface.
/// </para>
/// <para>
/// The polylines are in the data set's integer coordinate units: divide by the coordinate
/// multiplication factor (COMF) to get decimal degrees. They start and end exactly on the arc's
/// start and end points, and repeated points are dropped.
/// </para>
/// <para>
/// The curve types (B-splines and Bézier curves, with CT2D control points) aren't interpolated:
/// their control points are joined by straight lines.
/// </para>
/// </remarks>
public static class S57ArcDensifier
{
    /// <summary>
    /// The default tolerance, in decimal degrees: about a metre on the ground. It is the largest
    /// distance between a densified arc and the polyline that replaces it.
    /// </summary>
    public const double DefaultTolerance = 0.00001;

    /// <summary>The most segments a single arc is divided into.</summary>
    public const int MaximumSegmentCount = 4096;

    // The largest angle a segment may subtend, so that a coarse tolerance still keeps the
    // shape of the arc.
    private const double MaximumSegmentAngle = Math.PI / 4;

    private const double FullCircle = 2 * Math.PI;

    /// <summary>
    /// Densifies an arc into a polyline of a given number of segments.
    /// </summary>
    /// <param name="arc">The arc.</param>
    /// <param name="segmentCount">The number of straight segments, from 1 to <see cref="MaximumSegmentCount"/>.</param>
    /// <returns>The polyline, from the arc's start point to its end point.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segmentCount"/> is out of range.</exception>
    public static IReadOnlyList<S57Coordinate2D> Densify(S57Arc arc, int segmentCount)
    {
        CheckSegmentCount(segmentCount);
        return DensifyArc(arc, (_, _) => segmentCount);
    }

    /// <summary>
    /// Densifies an arc into a polyline that is never further than a tolerance from the arc.
    /// </summary>
    /// <param name="arc">The arc.</param>
    /// <param name="tolerance">The largest distance between the arc and the polyline, in decimal
    /// degrees; see <see cref="DefaultTolerance"/>.</param>
    /// <param name="multiplicationFactor">The coordinate multiplication factor (COMF) of the data
    /// set, which converts the tolerance to coordinate units.</param>
    /// <returns>The polyline, from the arc's start point to its end point.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> isn't positive, or
    /// <paramref name="multiplicationFactor"/> isn't positive.</exception>
    public static IReadOnlyList<S57Coordinate2D> Densify(S57Arc arc, double tolerance, int multiplicationFactor)
    {
        var units = ToleranceInUnits(tolerance, multiplicationFactor);
        return DensifyArc(arc, (sweep, radius) => SegmentCount(sweep, radius, units));
    }

    /// <summary>
    /// Densifies an elliptical arc into a polyline of a given number of segments.
    /// </summary>
    /// <param name="arc">The elliptical arc.</param>
    /// <param name="segmentCount">The number of straight segments, from 1 to <see cref="MaximumSegmentCount"/>.</param>
    /// <returns>The polyline, from the arc's start point to its end point.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segmentCount"/> is out of range.</exception>
    public static IReadOnlyList<S57Coordinate2D> Densify(S57EllipticalArc arc, int segmentCount)
    {
        CheckSegmentCount(segmentCount);
        return DensifyEllipse(arc, (_, _) => segmentCount);
    }

    /// <summary>
    /// Densifies an elliptical arc into a polyline that is never further than about a tolerance
    /// from the arc.
    /// </summary>
    /// <param name="arc">The elliptical arc.</param>
    /// <param name="tolerance">The largest distance between the arc and the polyline, in decimal
    /// degrees; see <see cref="DefaultTolerance"/>. It is applied to the circle of the
    /// ellipse's major axis, which bounds the distance on the ellipse.</param>
    /// <param name="multiplicationFactor">The coordinate multiplication factor (COMF) of the data
    /// set, which converts the tolerance to coordinate units.</param>
    /// <returns>The polyline, from the arc's start point to its end point.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> isn't positive, or
    /// <paramref name="multiplicationFactor"/> isn't positive.</exception>
    public static IReadOnlyList<S57Coordinate2D> Densify(S57EllipticalArc arc, double tolerance, int multiplicationFactor)
    {
        var units = ToleranceInUnits(tolerance, multiplicationFactor);
        return DensifyEllipse(arc, (sweep, radius) => SegmentCount(sweep, radius, units));
    }

    /// <summary>
    /// Densifies the arcs, elliptical arcs and control points of an arc/curve definition into a
    /// single polyline, in that order.
    /// </summary>
    /// <param name="curve">The arc/curve definition.</param>
    /// <param name="tolerance">The largest distance between an arc and the polyline, in decimal
    /// degrees; see <see cref="DefaultTolerance"/>.</param>
    /// <param name="multiplicationFactor">The coordinate multiplication factor (COMF) of the data set.</param>
    /// <returns>The polyline.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="curve"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> isn't positive, or
    /// <paramref name="multiplicationFactor"/> isn't positive.</exception>
    public static IReadOnlyList<S57Coordinate2D> Densify(S57ArcCurve curve, double tolerance, int multiplicationFactor)
    {
        ArgumentNullException.ThrowIfNull(curve);
        return Densify([curve], tolerance, multiplicationFactor);
    }

    /// <summary>
    /// Densifies a sequence of arc/curve definitions, such as those of an edge, into a single
    /// polyline.
    /// </summary>
    /// <param name="curves">The arc/curve definitions, in order.</param>
    /// <param name="tolerance">The largest distance between an arc and the polyline, in decimal
    /// degrees; see <see cref="DefaultTolerance"/>.</param>
    /// <param name="multiplicationFactor">The coordinate multiplication factor (COMF) of the data set.</param>
    /// <returns>The polyline.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="curves"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> isn't positive, or
    /// <paramref name="multiplicationFactor"/> isn't positive.</exception>
    public static IReadOnlyList<S57Coordinate2D> Densify(IReadOnlyList<S57ArcCurve> curves, double tolerance, int multiplicationFactor)
    {
        ArgumentNullException.ThrowIfNull(curves);
        ToleranceInUnits(tolerance, multiplicationFactor);

        var points = new List<S57Coordinate2D>();
        foreach (var curve in curves)
        {
            foreach (var arc in curve.Arcs)
            {
                AppendDistinct(points, Densify(arc, tolerance, multiplicationFactor));
            }

            foreach (var ellipse in curve.EllipticalArcs)
            {
                AppendDistinct(points, Densify(ellipse, tolerance, multiplicationFactor));
            }

            AppendDistinct(points, curve.ControlPoints);
        }

        return points;
    }

    private static IReadOnlyList<S57Coordinate2D> DensifyArc(S57Arc arc, Func<double, double, int> segmentCount)
    {
        double cx = arc.Centre.X;
        double cy = arc.Centre.Y;
        double sx = arc.Start.X - cx;
        double sy = arc.Start.Y - cy;
        double ex = arc.End.X - cx;
        double ey = arc.End.Y - cy;
        var startRadius = Math.Sqrt((sx * sx) + (sy * sy));
        var endRadius = Math.Sqrt((ex * ex) + (ey * ey));
        if (startRadius == 0 || endRadius == 0)
        {
            return Distinct([arc.Start, arc.End]);
        }

        var startAngle = Math.Atan2(sy, sx);
        var endAngle = Math.Atan2(ey, ex);

        // Clockwise is a decreasing angle; an end point in the start direction closes the circle.
        var sweep = ClockwiseSweep(startAngle, endAngle);
        var count = segmentCount(sweep, Math.Max(startRadius, endRadius));

        var points = new List<S57Coordinate2D>(count + 1) { arc.Start };
        for (var k = 1; k < count; k++)
        {
            var f = (double)k / count;
            var angle = startAngle - (sweep * f);

            // The end point "must be located on the arc", but its radius may differ slightly
            // after rounding, so blend the radius to meet it exactly.
            var radius = startRadius + ((endRadius - startRadius) * f);
            AppendDistinct(points, Round(cx + (radius * Math.Cos(angle)), cy + (radius * Math.Sin(angle))));
        }

        AppendDistinct(points, arc.End);
        return points;
    }

    private static IReadOnlyList<S57Coordinate2D> DensifyEllipse(S57EllipticalArc arc, Func<double, double, int> segmentCount)
    {
        double cx = arc.Centre.X;
        double cy = arc.Centre.Y;
        double ax = arc.MajorAxisPoint.X - cx;
        double ay = arc.MajorAxisPoint.Y - cy;
        double bx = arc.MinorAxisPoint.X - cx;
        double by = arc.MinorAxisPoint.Y - cy;
        var a2 = (ax * ax) + (ay * ay);
        var b2 = (bx * bx) + (by * by);
        var cross = (ax * by) - (ay * bx);
        if (a2 == 0 || b2 == 0 || cross == 0)
        {
            return Distinct([arc.Start, arc.End]);
        }

        // The point at parameter t is C + a cos t + b sin t, with the axes a and b at 90°.
        var startParameter = Parameter(arc.Start);
        var endParameter = Parameter(arc.End);

        // With b counter-clockwise from a, a growing parameter turns counter-clockwise.
        var direction = cross > 0 ? -1.0 : 1.0;
        var sweep = cross > 0
            ? ClockwiseSweep(startParameter, endParameter)
            : ClockwiseSweep(endParameter, startParameter);
        var count = segmentCount(sweep, Math.Sqrt(Math.Max(a2, b2)));

        var points = new List<S57Coordinate2D>(count + 1) { arc.Start };
        for (var k = 1; k < count; k++)
        {
            var t = startParameter + (direction * sweep * k / count);
            var cos = Math.Cos(t);
            var sin = Math.Sin(t);
            AppendDistinct(points, Round(cx + (ax * cos) + (bx * sin), cy + (ay * cos) + (by * sin)));
        }

        AppendDistinct(points, arc.End);
        return points;

        double Parameter(S57Coordinate2D point)
        {
            double dx = point.X - cx;
            double dy = point.Y - cy;
            return Math.Atan2(((dx * bx) + (dy * by)) / b2, ((dx * ax) + (dy * ay)) / a2);
        }
    }

    // The clockwise angle from one direction to another, in (0, 2π].
    private static double ClockwiseSweep(double from, double to)
    {
        var sweep = (from - to) % FullCircle;
        if (sweep <= 0)
        {
            sweep += FullCircle;
        }

        return sweep;
    }

    // The fewest segments whose chords stay within the tolerance of a circle of the radius:
    // a chord subtending an angle θ is at most r(1 - cos(θ/2)) from the arc.
    private static int SegmentCount(double sweep, double radius, double tolerance)
    {
        var angle = tolerance >= radius
            ? MaximumSegmentAngle
            : Math.Min(2 * Math.Acos(1 - (tolerance / radius)), MaximumSegmentAngle);
        var count = Math.Ceiling(sweep / angle);
        return (int)Math.Clamp(count, 1, MaximumSegmentCount);
    }

    private static double ToleranceInUnits(double tolerance, int multiplicationFactor)
    {
        if (!(tolerance > 0) || double.IsInfinity(tolerance))
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "The tolerance must be a positive number of decimal degrees.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(multiplicationFactor);
        return tolerance * multiplicationFactor;
    }

    private static void CheckSegmentCount(int segmentCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(segmentCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(segmentCount, MaximumSegmentCount);
    }

    private static S57Coordinate2D Round(double x, double y) => new()
    {
        X = (int)Math.Clamp(Math.Round(x, MidpointRounding.AwayFromZero), int.MinValue, int.MaxValue),
        Y = (int)Math.Clamp(Math.Round(y, MidpointRounding.AwayFromZero), int.MinValue, int.MaxValue)
    };

    private static List<S57Coordinate2D> Distinct(IEnumerable<S57Coordinate2D> points)
    {
        var result = new List<S57Coordinate2D>();
        AppendDistinct(result, points);
        return result;
    }

    private static void AppendDistinct(List<S57Coordinate2D> points, IEnumerable<S57Coordinate2D> more)
    {
        foreach (var point in more)
        {
            AppendDistinct(points, point);
        }
    }

    private static void AppendDistinct(List<S57Coordinate2D> points, S57Coordinate2D point)
    {
        if (points.Count == 0 || points[^1] != point)
        {
            points.Add(point);
        }
    }
}
