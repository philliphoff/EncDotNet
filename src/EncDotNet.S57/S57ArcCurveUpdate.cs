using System.Collections.Immutable;

namespace EncDotNet.S57;

/// <summary>
/// Applies an update record's arc/curve fields (ARCC, AR2D, EL2D and CT2D) to a base vector
/// record (S-57 Part 3, clause 8.4.3.2 c).
/// </summary>
/// <remarks>
/// <para>
/// The SGCC field addresses the coordinates of AR2D, EL2D and CT2D as one sequence, in field
/// order, as it does those of SG2D (clause 8.4.3.3). The coordinates of the update record's
/// arc/curve fields are spliced into the base record's, and then regrouped into the base
/// record's arc/curve definitions: a modify keeps every definition's shape, and an insert or
/// delete changes the number of coordinates of the definition it falls in, which must stay a
/// whole number of arcs (three points each) or elliptical arcs (five points each).
/// </para>
/// <para>
/// Without SGCC, the update record's arc/curve definitions replace the base record's. SG2D and
/// ARCC are mutually exclusive (clause 5.1.4.4), so an update record that carries SG2D and no
/// arc/curve definitions removes the base record's.
/// </para>
/// </remarks>
internal static class S57ArcCurveUpdate
{
    public static IReadOnlyList<S57ArcCurve> Apply(
        S57VectorRecord baseRecord,
        S57VectorRecord updateRecord,
        Func<ImmutableArray<S57Coordinate2D>, ImmutableArray<S57Coordinate2D>, ImmutableArray<S57Coordinate2D>> splice)
    {
        if (updateRecord.CoordinateControl is null)
        {
            if (updateRecord.ArcCurves.Count > 0)
            {
                return updateRecord.ArcCurves;
            }

            return updateRecord.Coordinates2D.Count > 0 ? [] : baseRecord.ArcCurves;
        }

        if (baseRecord.ArcCurves.Count == 0)
        {
            // The base edge is a straight line (or has SG2D); an update carrying arcs adds them.
            return updateRecord.ArcCurves.Count > 0 ? updateRecord.ArcCurves : baseRecord.ArcCurves;
        }

        var basePoints = Flatten(baseRecord.ArcCurves);
        var updated = splice(basePoints, Flatten(updateRecord.ArcCurves));
        if (updated.Length == basePoints.Length && updated.SequenceEqual(basePoints))
        {
            return baseRecord.ArcCurves;
        }

        return Regroup(baseRecord, updated, updateRecord.CoordinateControl.Value);
    }

    // The coordinates of the arc/curve fields in the order the writer writes them.
    private static ImmutableArray<S57Coordinate2D> Flatten(IReadOnlyList<S57ArcCurve> curves)
    {
        var builder = ImmutableArray.CreateBuilder<S57Coordinate2D>();
        foreach (var curve in curves)
        {
            builder.AddRange(Points(curve));
        }

        return builder.ToImmutable();
    }

    private static IEnumerable<S57Coordinate2D> Points(S57ArcCurve curve)
    {
        foreach (var arc in curve.Arcs)
        {
            yield return arc.Start;
            yield return arc.Centre;
            yield return arc.End;
        }

        foreach (var ellipse in curve.EllipticalArcs)
        {
            yield return ellipse.Start;
            yield return ellipse.Centre;
            yield return ellipse.End;
            yield return ellipse.MajorAxisPoint;
            yield return ellipse.MinorAxisPoint;
        }

        foreach (var point in curve.ControlPoints)
        {
            yield return point;
        }
    }

    private static int PointCount(S57ArcCurve curve) =>
        (curve.Arcs.Count * 3) + (curve.EllipticalArcs.Count * 5) + curve.ControlPoints.Count;

    private static ImmutableArray<S57ArcCurve> Regroup(
        S57VectorRecord baseRecord,
        ImmutableArray<S57Coordinate2D> points,
        S57FieldUpdateControl control)
    {
        var curves = baseRecord.ArcCurves;
        var delta = points.Length - Flatten(curves).Length;

        // The definition whose coordinates the insert or delete falls in. An insert after the
        // last coordinate falls in the last definition.
        var changed = -1;
        if (delta != 0)
        {
            var index = control.Index - 1;
            var start = 0;
            for (var i = 0; i < curves.Count; i++)
            {
                var count = PointCount(curves[i]);
                if (index < start + count || i == curves.Count - 1)
                {
                    changed = i;
                    break;
                }

                start += count;
            }
        }

        var result = ImmutableArray.CreateBuilder<S57ArcCurve>(curves.Count);
        var offset = 0;
        for (var i = 0; i < curves.Count; i++)
        {
            var curve = curves[i];
            if (i != changed)
            {
                result.Add(Rebuild(curve, points.AsSpan(offset, PointCount(curve)), curve.Arcs.Count, curve.EllipticalArcs.Count));
                offset += PointCount(curve);
                continue;
            }

            var count = PointCount(curve) + delta;
            var slice = points.AsSpan(offset, count);
            offset += count;

            // Only a definition with a single kind of coordinate field can grow or shrink
            // unambiguously.
            var kinds = (curve.Arcs.Count > 0 ? 1 : 0) + (curve.EllipticalArcs.Count > 0 ? 1 : 0) + (curve.ControlPoints.Count > 0 ? 1 : 0);
            if (kinds == 1 && curve.Arcs.Count > 0 && count % 3 == 0)
            {
                result.Add(Rebuild(curve, slice, count / 3, 0));
            }
            else if (kinds == 1 && curve.EllipticalArcs.Count > 0 && count % 5 == 0)
            {
                result.Add(Rebuild(curve, slice, 0, count / 5));
            }
            else if (kinds == 1 && curve.ControlPoints.Count > 0)
            {
                result.Add(Rebuild(curve, slice, 0, 0));
            }
            else
            {
                throw new InvalidDataException(
                    $"Update control field {S57FieldTags.SGCC} on record {baseRecord.RecordName} " +
                    $"({control.UpdateInstruction} at index {control.Index}, count {control.Count}) " +
                    "leaves an arc/curve definition with coordinates that are not a whole number of arcs " +
                    "(S-57 Part 3, 8.4.3.3). The update does not match the base cell.");
            }
        }

        return result.MoveToImmutable();
    }

    private static S57ArcCurve Rebuild(S57ArcCurve curve, ReadOnlySpan<S57Coordinate2D> points, int arcCount, int ellipseCount)
    {
        var arcs = new S57Arc[arcCount];
        for (var i = 0; i < arcCount; i++)
        {
            arcs[i] = new S57Arc { Start = points[i * 3], Centre = points[(i * 3) + 1], End = points[(i * 3) + 2] };
        }

        var rest = points[(arcCount * 3)..];
        var ellipses = new S57EllipticalArc[ellipseCount];
        for (var i = 0; i < ellipseCount; i++)
        {
            ellipses[i] = new S57EllipticalArc
            {
                Start = rest[i * 5],
                Centre = rest[(i * 5) + 1],
                End = rest[(i * 5) + 2],
                MajorAxisPoint = rest[(i * 5) + 3],
                MinorAxisPoint = rest[(i * 5) + 4]
            };
        }

        return curve with
        {
            Arcs = ImmutableArray.Create(arcs),
            EllipticalArcs = ImmutableArray.Create(ellipses),
            ControlPoints = ImmutableArray.Create(rest[(ellipseCount * 5)..])
        };
    }
}
