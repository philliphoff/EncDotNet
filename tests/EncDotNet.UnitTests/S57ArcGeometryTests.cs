using System.Text;
using EncDotNet.ChartViewer;
using EncDotNet.Iso8211;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace EncDotNet.UnitTests;

/// <summary>
/// Tests for arc/curve geometry (ARCC, AR2D, EL2D and CT2D): reading and writing it, applying
/// updates to it, densifying it, and building chart geometry from it.
/// </summary>
/// <remarks>
/// The arc cell is synthetic: a half disc south of the equator, bounded by an arc edge from
/// (0.01°, 0°) clockwise about the origin to (-0.01°, 0°) and a straight edge back.
/// </remarks>
public class S57ArcGeometryTests
{
    private const int Comf = 10_000_000;
    private const int Radius = 100_000; // 0.01° at COMF 10,000,000

    private static readonly S57Coordinate2D East = new() { X = Radius, Y = 0 };
    private static readonly S57Coordinate2D West = new() { X = -Radius, Y = 0 };
    private static readonly S57Coordinate2D Origin = new() { X = 0, Y = 0 };

    private static readonly S57Arc SouthArc = new() { Start = East, Centre = Origin, End = West };

    // ---- Write → read ----

    [Fact]
    public void ArcEdge_RoundTripsWithItsArcParameters()
    {
        var cell = ArcCell();

        var read = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        S57DocumentWriterTests.AssertSameDocument(cell, read);
        var curve = Assert.Single(read.GetVectorRecord(EdgeName(3))!.ArcCurves);
        Assert.Equal(S57ArcCurveType.ArcThreePointCentre, curve.Type);
        Assert.Equal(S57ConstructionSurface.Planar, curve.Surface);
        Assert.Equal(2, curve.Order);
        Assert.Equal(25, curve.Resolution);
        Assert.Equal(10, curve.FloatingPointMultiplicationFactor);
        Assert.Equal(2.5, curve.InterpolatedPointResolution);
        Assert.Equal(SouthArc, Assert.Single(curve.Arcs));
    }

    [Fact]
    public void EllipticalArcsAndControlPoints_RoundTrip()
    {
        var cell = ArcCell() with
        {
            VectorRecords =
            [
                .. ArcCell().VectorRecords,
                ArcEdge(5,
                    new S57ArcCurve
                    {
                        Type = S57ArcCurveType.EllipticalArc,
                        Surface = S57ConstructionSurface.Ellipsoidal,
                        EllipticalArcs =
                        [
                            new S57EllipticalArc
                            {
                                Start = new() { X = 200, Y = 0 },
                                Centre = Origin,
                                End = new() { X = -200, Y = 0 },
                                MajorAxisPoint = new() { X = 200, Y = 0 },
                                MinorAxisPoint = new() { X = 0, Y = 100 }
                            }
                        ]
                    },
                    new S57ArcCurve
                    {
                        Type = S57ArcCurveType.PiecewiseBezier,
                        Surface = S57ConstructionSurface.Planar,
                        Order = 3,
                        ControlPoints = [new() { X = 1, Y = 2 }, new() { X = 3, Y = 4 }, new() { X = 5, Y = 6 }]
                    })
            ]
        };

        var read = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        S57DocumentWriterTests.AssertSameDocument(cell, read);
        Assert.Equal(2, read.GetVectorRecord(EdgeName(5))!.ArcCurves.Count);
    }

    [Fact]
    public void ArcCell_WritesTheArcFieldsAfterTheCoordinateFields()
    {
        var bytes = S57DocumentWriter.Write(ArcCell());
        var document = Iso8211DocumentReader.Read(bytes);
        var ddr = document.Records[0];

        Assert.Equal("1600;&   ", FieldControls(ddr, S57FieldTags.ARCC));
        Assert.Equal("2500;&   ", FieldControls(ddr, S57FieldTags.AR2D));
        Assert.Contains("STPT!CTPT!ENPT*YCOO!XCOO", Encoding.ASCII.GetString(ddr.Fields.Single(f => f.Tag == S57FieldTags.AR2D).Data.Span), StringComparison.Ordinal);
        Assert.Contains("VRIDARCCARCCAR2DARCCEL2DARCCCT2D", Encoding.ASCII.GetString(ddr.Fields.Single(f => f.Tag == "0000").Data.Span), StringComparison.Ordinal);

        var edge = document.DataRecords.Single(r => r.Fields.Any(f => f.Tag == S57FieldTags.ARCC));
        Assert.Equal("0001,VRID,VRPT,ARCC,AR2D", string.Join(',', edge.Fields.Select(f => f.Tag)));
    }

    [Fact]
    public void CellWithoutArcs_DoesNotDeclareTheArcFields()
    {
        var ddr = Iso8211DocumentReader.Read(S57DocumentWriter.Write(S57WriterTestCells.SmallCell())).Records[0];

        Assert.DoesNotContain(ddr.Fields, f => f.Tag == S57FieldTags.ARCC);
    }

    // ---- ApplyChanges ----

    [Fact]
    public void ApplyChanges_KeepsArcsWhenAnUpdateChangesOnlyAttributes()
    {
        var update = UpdateOf(new S57VectorRecord
        {
            RecordName = EdgeName(3),
            RecordVersion = 2,
            UpdateInstruction = S57UpdateInstruction.Modify,
            Attributes = [new S57AttributeValue(402, "1")]
        });

        var updated = ArcCell().ApplyChanges(update);

        Assert.Equal(SouthArc, Assert.Single(Assert.Single(updated.GetVectorRecord(EdgeName(3))!.ArcCurves).Arcs));
    }

    [Fact]
    public void ApplyChanges_ReplacesArcsWithoutSgcc()
    {
        var northArc = new S57Arc { Start = West, Centre = Origin, End = East };
        var update = UpdateOf(ArcEdge(3, Curve(northArc)) with { UpdateInstruction = S57UpdateInstruction.Modify });

        var updated = ArcCell().ApplyChanges(update);

        Assert.Equal(northArc, Assert.Single(Assert.Single(updated.GetVectorRecord(EdgeName(3))!.ArcCurves).Arcs));
    }

    [Fact]
    public void ApplyChanges_ArcsReplacingSg2dRemoveIt()
    {
        var baseCell = ArcCell() with
        {
            VectorRecords = [.. ArcCell().VectorRecords.Select(v => v.RecordName == EdgeName(3)
                ? v with { ArcCurves = [], Coordinates2D = [new() { X = 0, Y = -Radius }] }
                : v)]
        };
        var update = UpdateOf(ArcEdge(3, Curve(SouthArc)) with { UpdateInstruction = S57UpdateInstruction.Modify });

        var edge = baseCell.ApplyChanges(update).GetVectorRecord(EdgeName(3))!;

        Assert.Empty(edge.Coordinates2D);
        Assert.Single(edge.ArcCurves);
    }

    [Fact]
    public void ApplyChanges_ModifiesArcCoordinatesUnderSgcc()
    {
        var movedCentre = new S57Coordinate2D { X = 0, Y = 10 };
        var update = UpdateOf(new S57VectorRecord
        {
            RecordName = EdgeName(3),
            RecordVersion = 2,
            UpdateInstruction = S57UpdateInstruction.Modify,
            ArcCurves = [new S57ArcCurve { Type = S57ArcCurveType.ArcThreePointCentre, ControlPoints = [movedCentre] }],
            CoordinateControl = new S57FieldUpdateControl { UpdateInstruction = S57UpdateInstruction.Modify, Index = 2, Count = 1 }
        });

        var curve = Assert.Single(ArcCell().ApplyChanges(update).GetVectorRecord(EdgeName(3))!.ArcCurves);

        // The base record's definition keeps its parameters; only the centre point changes.
        Assert.Equal(2, curve.Order);
        Assert.Equal(SouthArc with { Centre = movedCentre }, Assert.Single(curve.Arcs));
    }

    [Fact]
    public void ApplyChanges_DeletesAWholeArcUnderSgcc()
    {
        var second = new S57Arc { Start = West, Centre = new() { X = -2 * Radius, Y = 0 }, End = new() { X = -3 * Radius, Y = 0 } };
        var baseCell = WithEdge3(Curve(SouthArc, second));
        var update = UpdateOf(new S57VectorRecord
        {
            RecordName = EdgeName(3),
            RecordVersion = 2,
            UpdateInstruction = S57UpdateInstruction.Modify,
            CoordinateControl = new S57FieldUpdateControl { UpdateInstruction = S57UpdateInstruction.Delete, Index = 4, Count = 3 }
        });

        var curve = Assert.Single(baseCell.ApplyChanges(update).GetVectorRecord(EdgeName(3))!.ArcCurves);

        Assert.Equal(SouthArc, Assert.Single(curve.Arcs));
    }

    [Fact]
    public void ApplyChanges_InsertsAWholeArcUnderSgcc()
    {
        var second = new S57Arc { Start = West, Centre = new() { X = -2 * Radius, Y = 0 }, End = new() { X = -3 * Radius, Y = 0 } };
        var update = UpdateOf(new S57VectorRecord
        {
            RecordName = EdgeName(3),
            RecordVersion = 2,
            UpdateInstruction = S57UpdateInstruction.Modify,
            ArcCurves = [Curve(second)],
            CoordinateControl = new S57FieldUpdateControl { UpdateInstruction = S57UpdateInstruction.Insert, Index = 4, Count = 3 }
        });

        var curve = Assert.Single(ArcCell().ApplyChanges(update).GetVectorRecord(EdgeName(3))!.ArcCurves);

        Assert.Equal([SouthArc, second], curve.Arcs);
    }

    [Fact]
    public void ApplyChanges_ThrowsWhenSgccLeavesAPartialArc()
    {
        var update = UpdateOf(new S57VectorRecord
        {
            RecordName = EdgeName(3),
            RecordVersion = 2,
            UpdateInstruction = S57UpdateInstruction.Modify,
            CoordinateControl = new S57FieldUpdateControl { UpdateInstruction = S57UpdateInstruction.Delete, Index = 1, Count = 2 }
        });

        Assert.Throws<InvalidDataException>(() => ArcCell().ApplyChanges(update));
    }

    // ---- Densify ----

    [Fact]
    public void Densify_PutsPointsOnTheCircleAndTurnsClockwise()
    {
        var points = S57ArcDensifier.Densify(SouthArc, 16);

        Assert.Equal(17, points.Count);
        Assert.Equal(East, points[0]);
        Assert.Equal(West, points[^1]);
        foreach (var point in points)
        {
            Assert.InRange(Math.Sqrt(((double)point.X * point.X) + ((double)point.Y * point.Y)), Radius - 1, Radius + 1);

            // Clockwise from east to west passes through the south.
            Assert.True(point.Y <= 0, $"{point} is north of the centre.");
        }

        Assert.Contains(new S57Coordinate2D { X = 0, Y = -Radius }, points);

        // Each step turns clockwise (a decreasing angle).
        for (var i = 1; i < points.Count; i++)
        {
            var cross = ((double)points[i - 1].X * points[i].Y) - ((double)points[i - 1].Y * points[i].X);
            Assert.True(cross < 0, $"Step {i} turns counter-clockwise.");
        }
    }

    [Fact]
    public void Densify_ReversedArcGoesTheOtherWayRound()
    {
        var points = S57ArcDensifier.Densify(new S57Arc { Start = West, Centre = Origin, End = East }, 8);

        Assert.All(points, p => Assert.True(p.Y >= 0));
        Assert.Contains(new S57Coordinate2D { X = 0, Y = Radius }, points);
    }

    [Fact]
    public void Densify_SameStartAndEndIsAFullCircle()
    {
        var points = S57ArcDensifier.Densify(new S57Arc { Start = East, Centre = Origin, End = East }, 4);

        Assert.Equal([East, new() { X = 0, Y = -Radius }, West, new() { X = 0, Y = Radius }, East], points);
    }

    [Fact]
    public void Densify_StaysWithinTheTolerance()
    {
        const double tolerance = 0.00001; // 100 coordinate units at COMF 10,000,000

        var points = S57ArcDensifier.Densify(SouthArc, tolerance, Comf);

        // A chord's furthest distance from the arc is at its midpoint.
        for (var i = 1; i < points.Count; i++)
        {
            var mx = (points[i - 1].X + (double)points[i].X) / 2;
            var my = (points[i - 1].Y + (double)points[i].Y) / 2;
            var deviation = Radius - Math.Sqrt((mx * mx) + (my * my));
            Assert.True(deviation <= (tolerance * Comf) + 1, $"Chord {i} is {deviation} units from the arc.");
        }

        // Half a circle of radius r within tolerance t needs π / (2 acos(1 - t/r)) segments.
        var expected = (int)Math.Ceiling(Math.PI / (2 * Math.Acos(1 - (tolerance * Comf / Radius))));
        Assert.Equal(expected + 1, points.Count);
    }

    [Fact]
    public void Densify_AppliesTheCoordinateMultiplicationFactorToTheTolerance()
    {
        // The same arc in degrees, encoded with a COMF 100 times smaller.
        var coarse = new S57Arc
        {
            Start = new() { X = Radius / 100, Y = 0 },
            Centre = Origin,
            End = new() { X = -Radius / 100, Y = 0 }
        };

        var fine = S57ArcDensifier.Densify(SouthArc, 0.0001, Comf);
        var scaled = S57ArcDensifier.Densify(coarse, 0.0001, Comf / 100);

        Assert.Equal(fine.Count, scaled.Count);
    }

    [Fact]
    public void Densify_EllipticalArcFollowsTheEllipseClockwise()
    {
        var ellipse = new S57EllipticalArc
        {
            Start = new() { X = 2000, Y = 0 },
            Centre = Origin,
            End = new() { X = -2000, Y = 0 },
            MajorAxisPoint = new() { X = 2000, Y = 0 },
            MinorAxisPoint = new() { X = 0, Y = 1000 }
        };

        var points = S57ArcDensifier.Densify(ellipse, 32);

        Assert.Equal(new S57Coordinate2D { X = 2000, Y = 0 }, points[0]);
        Assert.Equal(new S57Coordinate2D { X = -2000, Y = 0 }, points[^1]);
        Assert.Contains(new S57Coordinate2D { X = 0, Y = -1000 }, points);
        foreach (var point in points)
        {
            var value = (point.X / 2000.0 * (point.X / 2000.0)) + (point.Y / 1000.0 * (point.Y / 1000.0));
            Assert.InRange(value, 0.998, 1.002);
            Assert.True(point.Y <= 0);
        }
    }

    [Fact]
    public void Densify_RejectsInvalidArguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => S57ArcDensifier.Densify(SouthArc, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => S57ArcDensifier.Densify(SouthArc, 0.0, Comf));
        Assert.Throws<ArgumentOutOfRangeException>(() => S57ArcDensifier.Densify(SouthArc, 0.001, 0));
    }

    // ---- Chart geometry ----

    [Fact]
    public void Edge_DensifiesItsArcs()
    {
        var chart = S57Chart.FromDocument(ArcCell());
        var edge = chart.GetEdge(EdgeName(3))!;

        Assert.True(edge.HasArcCurves);
        Assert.Equal(S57ArcDensifier.Densify(SouthArc, S57ArcDensifier.DefaultTolerance, Comf), edge.IntermediatePoints);
        Assert.True(edge.IntermediatePoints.Count > 10);
    }

    [Fact]
    public void LineGeometry_IncludesTheCurve()
    {
        var chart = S57Chart.FromDocument(ArcCell());
        var line = Assert.Single(chart.LineFeatures);

        var geometry = Assert.IsType<LineString>(S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, line));

        var (_, southY) = SphericalMercator.FromLonLat(0, -0.01);
        Assert.Contains(geometry.Coordinates, c => Math.Abs(c.X) < 1 && Math.Abs(c.Y - southY) < 1);
        Assert.True(geometry.NumPoints > 10);
    }

    [Fact]
    public void AreaGeometry_IncludesTheCurve()
    {
        var chart = S57Chart.FromDocument(ArcCell());
        var area = Assert.Single(chart.AreaFeatures);

        var polygon = Assert.IsType<Polygon>(S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, area));

        // A half disc, not the degenerate straight line between the nodes.
        var (x, y) = SphericalMercator.FromLonLat(0, -0.005);
        Assert.True(polygon.Contains(new Point(x, y)));
        var (rx, _) = SphericalMercator.FromLonLat(0.01, 0);
        Assert.InRange(polygon.Area, 0.99 * Math.PI * rx * rx / 2, 1.01 * Math.PI * rx * rx / 2);
    }

    // ---- Helpers ----

    private static S57RecordName EdgeName(int rcid) => S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, rcid);

    private static S57RecordName NodeName(int rcid) => S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, rcid);

    private static S57ArcCurve Curve(params S57Arc[] arcs) => new()
    {
        Type = S57ArcCurveType.ArcThreePointCentre,
        Surface = S57ConstructionSurface.Planar,
        Order = 2,
        Resolution = 25,
        FloatingPointMultiplicationFactor = 10,
        Arcs = arcs
    };

    private static S57Document WithEdge3(S57ArcCurve curve) => ArcCell() with
    {
        VectorRecords = [.. ArcCell().VectorRecords.Select(v => v.RecordName == EdgeName(3) ? v with { ArcCurves = [curve] } : v)]
    };

    private static S57Document UpdateOf(S57VectorRecord record) => new()
    {
        DataSetIdentification = ArcCell().DataSetIdentification,
        DataSetParameters = ArcCell().DataSetParameters,
        VectorRecords = [record]
    };

    private static S57VectorRecord Node(int rcid, S57Coordinate2D position) => new()
    {
        RecordName = NodeName(rcid),
        RecordVersion = 1,
        UpdateInstruction = S57UpdateInstruction.Insert,
        Coordinates2D = [position]
    };

    private static S57VectorRecord ArcEdge(int rcid, params S57ArcCurve[] curves) =>
        StraightEdge(rcid, 1, 2) with { ArcCurves = curves };

    private static S57VectorRecord StraightEdge(int rcid, int beginning, int end) => new()
    {
        RecordName = EdgeName(rcid),
        RecordVersion = 1,
        UpdateInstruction = S57UpdateInstruction.Insert,
        VectorPointers =
        [
            new S57VectorPointer
            {
                Name = NodeName(beginning),
                Orientation = S57Orientation.NotApplicable,
                Usage = S57UsageIndicator.NotApplicable,
                Topology = S57TopologyIndicator.Beginning,
                Mask = S57MaskingIndicator.NotApplicable
            },
            new S57VectorPointer
            {
                Name = NodeName(end),
                Orientation = S57Orientation.NotApplicable,
                Usage = S57UsageIndicator.NotApplicable,
                Topology = S57TopologyIndicator.End,
                Mask = S57MaskingIndicator.NotApplicable
            }
        ]
    };

    private static S57SpatialPointer EdgePointer(int rcid) => new()
    {
        Name = EdgeName(rcid),
        Orientation = S57Orientation.Forward,
        Usage = S57UsageIndicator.Exterior,
        Mask = S57MaskingIndicator.Show
    };

    private static S57FeatureRecord Feature(int rcid, S57GeometricPrimitive primitive, S57ObjectCode objectCode, params S57SpatialPointer[] pointers) => new()
    {
        RecordName = new S57RecordName
        {
            RecordNameCode = S57RecordNameCodes.Feature,
            RecordId = rcid,
            AgencyCode = 550,
            FeatureId = rcid,
            FeatureSubdivision = 1
        },
        Primitive = primitive,
        Group = 2,
        ObjectCode = objectCode,
        RecordVersion = 1,
        UpdateInstruction = S57UpdateInstruction.Insert,
        SpatialPointers = pointers
    };

    private static S57Document ArcCell() => S57WriterTestCells.SmallCell() with
    {
        VectorRecords =
        [
            Node(1, East),
            Node(2, West),
            ArcEdge(3, Curve(SouthArc)),
            StraightEdge(4, 2, 1)
        ],
        FeatureRecords =
        [
            Feature(1, S57GeometricPrimitive.Line, S57ObjectCode.COALNE, EdgePointer(3)),
            Feature(2, S57GeometricPrimitive.Area, S57ObjectCode.LNDARE, EdgePointer(3), EdgePointer(4))
        ]
    };

    private static string FieldControls(Iso8211Record ddr, string tag) =>
        Encoding.ASCII.GetString(ddr.Fields.Single(f => f.Tag == tag).Data.Span[..9]);
}
