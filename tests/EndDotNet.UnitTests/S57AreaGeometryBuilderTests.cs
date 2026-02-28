using System.Collections.Immutable;
using EncDotNet.ChartViewer;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace EndDotNet.UnitTests;

public class S57AreaGeometryBuilderTests
{
    private const int Cmf = 10_000_000; // CoordinateMultiplicationFactor

    #region Helpers

    private static S57RecordName EdgeName(int id) =>
        S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, id);

    private static S57RecordName NodeName(int id) =>
        S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, id);

    private static S57RecordName FaceName(int id) =>
        S57RecordName.FromRcnmRcid(S57RecordNameCodes.Face, id);

    private static S57RecordName FeatureName(int id) =>
        S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, id);

    private static S57Coordinate2D Coord(double lon, double lat) =>
        new() { X = (int)(lon * Cmf), Y = (int)(lat * Cmf) };

    private static S57ConnectedNode MakeNode(int id, double lon, double lat) =>
        (S57ConnectedNode)S57SpatialRecord.Create(new S57VectorRecord
        {
            RecordName = NodeName(id),
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = ImmutableArray.Create(Coord(lon, lat))
        });

    private static S57Edge MakeEdge(
        int id,
        int beginNodeId,
        int endNodeId,
        params S57Coordinate2D[] intermediatePoints) =>
        (S57Edge)S57SpatialRecord.Create(new S57VectorRecord
        {
            RecordName = EdgeName(id),
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = ImmutableArray.Create(intermediatePoints),
            VectorPointers = ImmutableArray.Create(
                new S57VectorPointer
                {
                    Name = NodeName(beginNodeId),
                    Topology = S57TopologyIndicator.Beginning
                },
                new S57VectorPointer
                {
                    Name = NodeName(endNodeId),
                    Topology = S57TopologyIndicator.End
                })
        });

    private static S57Face MakeFace(
        int id,
        S57VectorPointer[] edgePointers) =>
        (S57Face)S57SpatialRecord.Create(new S57VectorRecord
        {
            RecordName = FaceName(id),
            UpdateInstruction = S57UpdateInstruction.Insert,
            VectorPointers = ImmutableArray.Create(edgePointers)
        });

    private static S57VectorPointer FaceEdgePointer(
        int edgeId,
        S57Orientation orientation,
        S57UsageIndicator usage) => new()
    {
        Name = EdgeName(edgeId),
        Orientation = orientation,
        Usage = usage,
        Mask = S57MaskingIndicator.Show
    };

    private static S57SpatialPointer ExteriorEdgeRef(int edgeId, S57Orientation orientation = S57Orientation.Forward) => new()
    {
        Name = EdgeName(edgeId),
        Orientation = orientation,
        Usage = S57UsageIndicator.Exterior,
        Mask = S57MaskingIndicator.Show
    };

    private static S57SpatialPointer InteriorEdgeRef(int edgeId, S57Orientation orientation = S57Orientation.Forward) => new()
    {
        Name = EdgeName(edgeId),
        Orientation = orientation,
        Usage = S57UsageIndicator.Interior,
        Mask = S57MaskingIndicator.Show
    };

    private static S57SpatialPointer FaceRef(int faceId) => new()
    {
        Name = FaceName(faceId),
        Orientation = S57Orientation.Forward,
        Usage = S57UsageIndicator.Exterior,
        Mask = S57MaskingIndicator.Show
    };

    private static S57AreaFeature MakeAreaFeature(int id, params S57SpatialPointer[] spatialPointers) =>
        (S57AreaFeature)S57TypedFeature.Create(new S57FeatureRecord
        {
            RecordName = FeatureName(id),
            Primitive = S57GeometricPrimitive.Area,
            ObjectCode = S57ObjectCode.DEPARE,
            UpdateInstruction = S57UpdateInstruction.Insert,
            SpatialPointers = ImmutableArray.Create(spatialPointers)
        });

    private static (double X, double Y) Project(double lon, double lat) =>
        SphericalMercator.FromLonLat(lon, lat);

    private static S57Chart BuildChart(
        IEnumerable<S57ConnectedNode> nodes,
        IEnumerable<S57Edge> edges,
        IEnumerable<S57Face>? faces = null)
    {
        var nodeDict = nodes.ToImmutableDictionary(n => n.RecordName);
        var edgeDict = edges.ToImmutableDictionary(e => e.RecordName);
        var faceDict = faces?.ToImmutableDictionary(f => f.RecordName)
            ?? ImmutableDictionary<S57RecordName, S57Face>.Empty;

        return new S57Chart(
            connectedNodes: nodeDict,
            edges: edgeDict,
            faces: faceDict,
            parameters: new S57DataSetParameters
            {
                CoordinateMultiplicationFactor = Cmf
            });
    }

    /// <summary>
    /// Creates a simple triangle: nodes at (0,0), (1,0), (0.5,1) with edges 1→2, 2→3, 3→1.
    /// </summary>
    private static (S57ConnectedNode[] Nodes, S57Edge[] Edges) MakeTriangle(
        int nodeIdStart = 1, int edgeIdStart = 1)
    {
        var n1 = MakeNode(nodeIdStart, 0, 0);
        var n2 = MakeNode(nodeIdStart + 1, 1, 0);
        var n3 = MakeNode(nodeIdStart + 2, 0.5, 1);
        var e1 = MakeEdge(edgeIdStart, nodeIdStart, nodeIdStart + 1);
        var e2 = MakeEdge(edgeIdStart + 1, nodeIdStart + 1, nodeIdStart + 2);
        var e3 = MakeEdge(edgeIdStart + 2, nodeIdStart + 2, nodeIdStart);
        return ([n1, n2, n3], [e1, e2, e3]);
    }

    /// <summary>
    /// Creates a second triangle: nodes at (5,5), (6,5), (5.5,6) with edges 4→5, 5→6, 6→4.
    /// </summary>
    private static (S57ConnectedNode[] Nodes, S57Edge[] Edges) MakeSecondTriangle(
        int nodeIdStart = 4, int edgeIdStart = 4)
    {
        var n1 = MakeNode(nodeIdStart, 5, 5);
        var n2 = MakeNode(nodeIdStart + 1, 6, 5);
        var n3 = MakeNode(nodeIdStart + 2, 5.5, 6);
        var e1 = MakeEdge(edgeIdStart, nodeIdStart, nodeIdStart + 1);
        var e2 = MakeEdge(edgeIdStart + 1, nodeIdStart + 1, nodeIdStart + 2);
        var e3 = MakeEdge(edgeIdStart + 2, nodeIdStart + 2, nodeIdStart);
        return ([n1, n2, n3], [e1, e2, e3]);
    }

    #endregion

    #region CreatePolygonFromAreaFeature — no spatial references

    [Fact]
    public void ReturnsNull_WhenNoSpatialReferences()
    {
        var chart = BuildChart([], []);
        var feature = MakeAreaFeature(1);

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.Null(result);
    }

    #endregion

    #region Chain-node topology (level 2) — exterior edges

    [Fact]
    public void ReturnsPolygon_ForSimpleTriangle_ChainNodeTopology()
    {
        // Three edges forming a closed triangle: 1→2, 2→3, 3→1
        var (nodes, edges) = MakeTriangle();
        var chart = BuildChart(nodes, edges);

        var feature = MakeAreaFeature(1,
            ExteriorEdgeRef(1),
            ExteriorEdgeRef(2),
            ExteriorEdgeRef(3));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        // Closed ring: 4 points (3 vertices + closing point)
        Assert.Equal(4, polygon.ExteriorRing.NumPoints);
        Assert.True(polygon.ExteriorRing.IsClosed);
        Assert.Empty(polygon.InteriorRings);
    }

    [Fact]
    public void ReturnsPolygon_WithReversedEdges_ChainNodeTopology()
    {
        // Triangle with edge 3 reversed: 1→2, 2→3, edge(1→3) reversed = 3→1
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 0);
        var n3 = MakeNode(3, 0.5, 1);
        var e1 = MakeEdge(1, 1, 2);
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 1, 3); // note: begin=1, end=3; reversed → oriented as 3→1
        var chart = BuildChart([n1, n2, n3], [e1, e2, e3]);

        var feature = MakeAreaFeature(1,
            ExteriorEdgeRef(1),
            ExteriorEdgeRef(2),
            ExteriorEdgeRef(3, S57Orientation.Reverse));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        Assert.Equal(4, polygon.ExteriorRing.NumPoints);
        Assert.True(polygon.ExteriorRing.IsClosed);
    }

    [Fact]
    public void ReturnsPolygon_WithIntermediatePoints_ChainNodeTopology()
    {
        // Triangle with intermediate point on edge 1
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 0);
        var n3 = MakeNode(3, 0.5, 1);
        var e1 = MakeEdge(1, 1, 2, Coord(0.5, -0.1)); // intermediate point
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 3, 1);
        var chart = BuildChart([n1, n2, n3], [e1, e2, e3]);

        var feature = MakeAreaFeature(1,
            ExteriorEdgeRef(1),
            ExteriorEdgeRef(2),
            ExteriorEdgeRef(3));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        // 3 node coords + 1 intermediate + closing = 5
        Assert.Equal(5, polygon.ExteriorRing.NumPoints);
        Assert.True(polygon.ExteriorRing.IsClosed);
    }

    [Fact]
    public void ReturnsPolygon_WithInteriorHole_ChainNodeTopology()
    {
        // Outer square: nodes 1-4, edges 1-4
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 2, 0);
        var n3 = MakeNode(3, 2, 2);
        var n4 = MakeNode(4, 0, 2);
        var e1 = MakeEdge(1, 1, 2);
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 3, 4);
        var e4 = MakeEdge(4, 4, 1);

        // Inner triangle (hole): nodes 5-7, edges 5-7
        var n5 = MakeNode(5, 0.5, 0.5);
        var n6 = MakeNode(6, 1.5, 0.5);
        var n7 = MakeNode(7, 1.0, 1.5);
        var e5 = MakeEdge(5, 5, 6);
        var e6 = MakeEdge(6, 6, 7);
        var e7 = MakeEdge(7, 7, 5);

        var chart = BuildChart(
            [n1, n2, n3, n4, n5, n6, n7],
            [e1, e2, e3, e4, e5, e6, e7]);

        var feature = MakeAreaFeature(1,
            ExteriorEdgeRef(1),
            ExteriorEdgeRef(2),
            ExteriorEdgeRef(3),
            ExteriorEdgeRef(4),
            InteriorEdgeRef(5),
            InteriorEdgeRef(6),
            InteriorEdgeRef(7));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        Assert.Equal(5, polygon.ExteriorRing.NumPoints); // 4 nodes + closing
        Assert.Single(polygon.InteriorRings);
        Assert.Equal(4, polygon.InteriorRings[0].NumPoints); // 3 nodes + closing
        Assert.True(polygon.InteriorRings[0].IsClosed);
    }

    #endregion

    #region Chain-node topology — non-contiguous edges (stray line fix)

    [Fact]
    public void ReturnsMultiPolygon_ForNonContiguousExteriorEdges_ChainNodeTopology()
    {
        // Two separate triangles referenced as exterior edges of the same area feature.
        // Without the contiguity fix, these would produce a single malformed ring with
        // a stray line connecting the two triangles.
        var (nodes1, edges1) = MakeTriangle(nodeIdStart: 1, edgeIdStart: 1);
        var (nodes2, edges2) = MakeSecondTriangle(nodeIdStart: 4, edgeIdStart: 4);
        var chart = BuildChart([.. nodes1, .. nodes2], [.. edges1, .. edges2]);

        var feature = MakeAreaFeature(1,
            ExteriorEdgeRef(1),
            ExteriorEdgeRef(2),
            ExteriorEdgeRef(3),
            ExteriorEdgeRef(4),
            ExteriorEdgeRef(5),
            ExteriorEdgeRef(6));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var multi = Assert.IsType<MultiPolygon>(result);
        Assert.Equal(2, multi.NumGeometries);

        // Each sub-polygon should be a closed triangle
        for (int i = 0; i < 2; i++)
        {
            var poly = (Polygon)multi.GetGeometryN(i);
            Assert.Equal(4, poly.ExteriorRing.NumPoints);
            Assert.True(poly.ExteriorRing.IsClosed);
        }
    }

    [Fact]
    public void ReturnsMultiPolygon_NonContiguousInteriorEdges_ProduceSeparateHoles()
    {
        // Large outer square with two separate interior holes
        // Outer: nodes 1-4, edges 1-4
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 10, 0);
        var n3 = MakeNode(3, 10, 10);
        var n4 = MakeNode(4, 0, 10);
        var e1 = MakeEdge(1, 1, 2);
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 3, 4);
        var e4 = MakeEdge(4, 4, 1);

        // First hole: nodes 5-7, edges 5-7
        var n5 = MakeNode(5, 1, 1);
        var n6 = MakeNode(6, 3, 1);
        var n7 = MakeNode(7, 2, 3);
        var e5 = MakeEdge(5, 5, 6);
        var e6 = MakeEdge(6, 6, 7);
        var e7 = MakeEdge(7, 7, 5);

        // Second hole: nodes 8-10, edges 8-10 (non-contiguous with first hole)
        var n8 = MakeNode(8, 6, 6);
        var n9 = MakeNode(9, 8, 6);
        var n10 = MakeNode(10, 7, 8);
        var e8 = MakeEdge(8, 8, 9);
        var e9 = MakeEdge(9, 9, 10);
        var e10 = MakeEdge(10, 10, 8);

        var chart = BuildChart(
            [n1, n2, n3, n4, n5, n6, n7, n8, n9, n10],
            [e1, e2, e3, e4, e5, e6, e7, e8, e9, e10]);

        var feature = MakeAreaFeature(1,
            ExteriorEdgeRef(1),
            ExteriorEdgeRef(2),
            ExteriorEdgeRef(3),
            ExteriorEdgeRef(4),
            InteriorEdgeRef(5),
            InteriorEdgeRef(6),
            InteriorEdgeRef(7),
            InteriorEdgeRef(8),
            InteriorEdgeRef(9),
            InteriorEdgeRef(10));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        Assert.Equal(5, polygon.ExteriorRing.NumPoints);
        // Two separate holes
        Assert.Equal(2, polygon.InteriorRings.Length);
        Assert.True(polygon.InteriorRings[0].IsClosed);
        Assert.True(polygon.InteriorRings[1].IsClosed);
    }

    #endregion

    #region Face topology (level 3)

    [Fact]
    public void ReturnsPolygon_ForSingleFace()
    {
        var (nodes, edges) = MakeTriangle();

        var face = MakeFace(1, [
            FaceEdgePointer(1, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(2, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(3, S57Orientation.Forward, S57UsageIndicator.Exterior)
        ]);

        var chart = BuildChart(nodes, edges, [face]);
        var feature = MakeAreaFeature(1, FaceRef(1));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        Assert.Equal(4, polygon.ExteriorRing.NumPoints);
        Assert.True(polygon.ExteriorRing.IsClosed);
    }

    [Fact]
    public void ReturnsMultiPolygon_ForMultipleFaces()
    {
        var (nodes1, edges1) = MakeTriangle(nodeIdStart: 1, edgeIdStart: 1);
        var (nodes2, edges2) = MakeSecondTriangle(nodeIdStart: 4, edgeIdStart: 4);

        var face1 = MakeFace(1, [
            FaceEdgePointer(1, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(2, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(3, S57Orientation.Forward, S57UsageIndicator.Exterior)
        ]);

        var face2 = MakeFace(2, [
            FaceEdgePointer(4, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(5, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(6, S57Orientation.Forward, S57UsageIndicator.Exterior)
        ]);

        var chart = BuildChart(
            [.. nodes1, .. nodes2],
            [.. edges1, .. edges2],
            [face1, face2]);

        var feature = MakeAreaFeature(1, FaceRef(1), FaceRef(2));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var multi = Assert.IsType<MultiPolygon>(result);
        Assert.Equal(2, multi.NumGeometries);
    }

    [Fact]
    public void ReturnsPolygon_WithInteriorHole_FaceTopology()
    {
        // Outer square
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 2, 0);
        var n3 = MakeNode(3, 2, 2);
        var n4 = MakeNode(4, 0, 2);
        var e1 = MakeEdge(1, 1, 2);
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 3, 4);
        var e4 = MakeEdge(4, 4, 1);

        // Inner triangle (hole)
        var n5 = MakeNode(5, 0.5, 0.5);
        var n6 = MakeNode(6, 1.5, 0.5);
        var n7 = MakeNode(7, 1.0, 1.5);
        var e5 = MakeEdge(5, 5, 6);
        var e6 = MakeEdge(6, 6, 7);
        var e7 = MakeEdge(7, 7, 5);

        var face = MakeFace(1, [
            FaceEdgePointer(1, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(2, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(3, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(4, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(5, S57Orientation.Forward, S57UsageIndicator.Interior),
            FaceEdgePointer(6, S57Orientation.Forward, S57UsageIndicator.Interior),
            FaceEdgePointer(7, S57Orientation.Forward, S57UsageIndicator.Interior)
        ]);

        var chart = BuildChart(
            [n1, n2, n3, n4, n5, n6, n7],
            [e1, e2, e3, e4, e5, e6, e7],
            [face]);

        var feature = MakeAreaFeature(1, FaceRef(1));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        Assert.Equal(5, polygon.ExteriorRing.NumPoints);
        Assert.Single(polygon.InteriorRings);
        Assert.True(polygon.InteriorRings[0].IsClosed);
    }

    [Fact]
    public void Face_NonContiguousExteriorEdges_ProduceMultiPolygon()
    {
        // A face whose exterior edges form two separate closed loops.
        var (nodes1, edges1) = MakeTriangle(nodeIdStart: 1, edgeIdStart: 1);
        var (nodes2, edges2) = MakeSecondTriangle(nodeIdStart: 4, edgeIdStart: 4);

        var face = MakeFace(1, [
            FaceEdgePointer(1, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(2, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(3, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(4, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(5, S57Orientation.Forward, S57UsageIndicator.Exterior),
            FaceEdgePointer(6, S57Orientation.Forward, S57UsageIndicator.Exterior)
        ]);

        var chart = BuildChart(
            [.. nodes1, .. nodes2],
            [.. edges1, .. edges2],
            [face]);

        var feature = MakeAreaFeature(1, FaceRef(1));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var multi = Assert.IsType<MultiPolygon>(result);
        Assert.Equal(2, multi.NumGeometries);
    }

    #endregion

    #region BuildRingsFromEdges

    [Fact]
    public void BuildRingsFromEdges_ReturnsSingleRing_ForContiguousEdges()
    {
        var (nodes, edges) = MakeTriangle();
        var chart = BuildChart(nodes, edges);

        var edgeRefs = new[]
        {
            new S57EdgeReference(EdgeName(1), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(2), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(3), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show)
        };

        var rings = S57AreaGeometryBuilder.BuildRingsFromEdges(chart, edgeRefs);

        Assert.Single(rings);
        Assert.Equal(4, rings[0].Count); // 3 nodes + closing
        Assert.True(rings[0][0].Equals2D(rings[0][^1]));
    }

    [Fact]
    public void BuildRingsFromEdges_ReturnsTwoRings_ForNonContiguousEdges()
    {
        var (nodes1, edges1) = MakeTriangle(nodeIdStart: 1, edgeIdStart: 1);
        var (nodes2, edges2) = MakeSecondTriangle(nodeIdStart: 4, edgeIdStart: 4);
        var chart = BuildChart([.. nodes1, .. nodes2], [.. edges1, .. edges2]);

        var edgeRefs = new[]
        {
            new S57EdgeReference(EdgeName(1), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(2), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(3), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            // Gap — second triangle starts at node 4, not at node 1
            new S57EdgeReference(EdgeName(4), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(5), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(6), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show)
        };

        var rings = S57AreaGeometryBuilder.BuildRingsFromEdges(chart, edgeRefs);

        Assert.Equal(2, rings.Count);
        foreach (var ring in rings)
        {
            Assert.Equal(4, ring.Count);
            Assert.True(ring[0].Equals2D(ring[^1]));
        }
    }

    [Fact]
    public void BuildRingsFromEdges_ReturnsEmpty_WhenNoEdgesFound()
    {
        var chart = BuildChart([], []);

        var edgeRefs = new[]
        {
            new S57EdgeReference(EdgeName(99), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show)
        };

        var rings = S57AreaGeometryBuilder.BuildRingsFromEdges(chart, edgeRefs);

        Assert.Empty(rings);
    }

    [Fact]
    public void BuildRingsFromEdges_ClosesRing_WhenEndDoesNotMatchStart()
    {
        // Two edges that don't form a closed loop: 1→2, 2→3
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 0);
        var n3 = MakeNode(3, 1, 1);
        var n4 = MakeNode(4, 0, 1);
        var e1 = MakeEdge(1, 1, 2);
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 3, 4);
        var e4 = MakeEdge(4, 4, 1);
        var chart = BuildChart([n1, n2, n3, n4], [e1, e2, e3, e4]);

        var edgeRefs = new[]
        {
            new S57EdgeReference(EdgeName(1), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(2), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(3), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            // Omit edge 4 — ring is not closed geometrically
        };

        var rings = S57AreaGeometryBuilder.BuildRingsFromEdges(chart, edgeRefs);

        // Should still produce a ring with closing coordinate appended
        Assert.Single(rings);
        Assert.True(rings[0][0].Equals2D(rings[0][^1]),
            "Ring should be auto-closed");
    }

    [Fact]
    public void BuildRingsFromEdges_HandlesReversedEdges()
    {
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 0);
        var n3 = MakeNode(3, 0.5, 1);
        var e1 = MakeEdge(1, 1, 2);
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 1, 3); // begin=1, end=3; reversed → oriented 3→1
        var chart = BuildChart([n1, n2, n3], [e1, e2, e3]);

        var edgeRefs = new[]
        {
            new S57EdgeReference(EdgeName(1), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(2), S57Orientation.Forward, S57UsageIndicator.Exterior, S57MaskingIndicator.Show),
            new S57EdgeReference(EdgeName(3), S57Orientation.Reverse, S57UsageIndicator.Exterior, S57MaskingIndicator.Show)
        };

        var rings = S57AreaGeometryBuilder.BuildRingsFromEdges(chart, edgeRefs);

        Assert.Single(rings);
        Assert.Equal(4, rings[0].Count);
        Assert.True(rings[0][0].Equals2D(rings[0][^1]));
    }

    #endregion

    #region Coordinate verification

    [Fact]
    public void Polygon_HasCorrectProjectedCoordinates()
    {
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 0);
        var n3 = MakeNode(3, 0.5, 1);
        var e1 = MakeEdge(1, 1, 2);
        var e2 = MakeEdge(2, 2, 3);
        var e3 = MakeEdge(3, 3, 1);
        var chart = BuildChart([n1, n2, n3], [e1, e2, e3]);

        var feature = MakeAreaFeature(1,
            ExteriorEdgeRef(1),
            ExteriorEdgeRef(2),
            ExteriorEdgeRef(3));

        var result = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, feature);

        Assert.NotNull(result);
        var polygon = Assert.IsType<Polygon>(result);
        var coords = polygon.ExteriorRing.Coordinates;

        var (x0, y0) = Project(0, 0);
        var (x1, y1) = Project(1, 0);
        var (x2, y2) = Project(0.5, 1);

        Assert.Equal(x0, coords[0].X, 0.01);
        Assert.Equal(y0, coords[0].Y, 0.01);
        Assert.Equal(x1, coords[1].X, 0.01);
        Assert.Equal(y1, coords[1].Y, 0.01);
        Assert.Equal(x2, coords[2].X, 0.01);
        Assert.Equal(y2, coords[2].Y, 0.01);
        // Closing coordinate matches first
        Assert.Equal(x0, coords[3].X, 0.01);
        Assert.Equal(y0, coords[3].Y, 0.01);
    }

    #endregion
}
