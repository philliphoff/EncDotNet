using System.Collections.Immutable;
using EncDotNet.ChartViewer;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace EndDotNet.UnitTests;

public class S57LineGeometryBuilderTests
{
    private const int Cmf = 10_000_000; // CoordinateMultiplicationFactor

    #region Helpers

    /// <summary>Creates an S57RecordName for an edge.</summary>
    private static S57RecordName EdgeName(int id) =>
        S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, id);

    /// <summary>Creates an S57RecordName for a connected node.</summary>
    private static S57RecordName NodeName(int id) =>
        S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, id);

    /// <summary>Creates an S57RecordName for a feature.</summary>
    private static S57RecordName FeatureName(int id) =>
        S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, id);

    /// <summary>Converts (lon, lat) decimal degrees to the integer representation used by S-57.</summary>
    private static S57Coordinate2D Coord(double lon, double lat) =>
        new() { X = (int)(lon * Cmf), Y = (int)(lat * Cmf) };

    /// <summary>Creates a connected node spatial record.</summary>
    private static S57ConnectedNode MakeNode(int id, double lon, double lat) =>
        (S57ConnectedNode)S57SpatialRecord.Create(new S57VectorRecord
        {
            RecordName = NodeName(id),
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = ImmutableArray.Create(Coord(lon, lat))
        });

    /// <summary>Creates an edge spatial record.</summary>
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

    /// <summary>Creates a line feature with the given edge references.</summary>
    private static S57LineFeature MakeLineFeature(int id, params S57SpatialPointer[] edgeRefs) =>
        (S57LineFeature)S57TypedFeature.Create(new S57FeatureRecord
        {
            RecordName = FeatureName(id),
            Primitive = S57GeometricPrimitive.Line,
            ObjectCode = S57ObjectCode.COALNE,
            UpdateInstruction = S57UpdateInstruction.Insert,
            SpatialPointers = ImmutableArray.Create(edgeRefs)
        });

    /// <summary>Creates a forward (non-masked) spatial pointer to an edge.</summary>
    private static S57SpatialPointer ForwardEdgeRef(int edgeId) => new()
    {
        Name = EdgeName(edgeId),
        Orientation = S57Orientation.Forward,
        Mask = S57MaskingIndicator.Show
    };

    /// <summary>Creates a reverse (non-masked) spatial pointer to an edge.</summary>
    private static S57SpatialPointer ReverseEdgeRef(int edgeId) => new()
    {
        Name = EdgeName(edgeId),
        Orientation = S57Orientation.Reverse,
        Mask = S57MaskingIndicator.Show
    };

    /// <summary>Creates a masked spatial pointer to an edge.</summary>
    private static S57SpatialPointer MaskedEdgeRef(int edgeId) => new()
    {
        Name = EdgeName(edgeId),
        Orientation = S57Orientation.Forward,
        Mask = S57MaskingIndicator.Mask
    };

    /// <summary>Projects (lon, lat) to Spherical Mercator (same transform the builder uses).</summary>
    private static (double X, double Y) Project(double lon, double lat) =>
        SphericalMercator.FromLonLat(lon, lat);

    /// <summary>Builds an S57Chart with the given nodes and edges.</summary>
    private static S57Chart BuildChart(
        IEnumerable<S57ConnectedNode> nodes,
        IEnumerable<S57Edge> edges)
    {
        var nodeDict = nodes.ToImmutableDictionary(n => n.RecordName);
        var edgeDict = edges.ToImmutableDictionary(e => e.RecordName);

        return new S57Chart(
            connectedNodes: nodeDict,
            edges: edgeDict,
            parameters: new S57DataSetParameters
            {
                CoordinateMultiplicationFactor = Cmf
            });
    }

    #endregion

    [Fact]
    public void ReturnsNull_WhenNoEdgeReferences()
    {
        var chart = BuildChart([], []);
        var feature = MakeLineFeature(1);

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnsSingleLineString_ForSingleEdge()
    {
        // Edge from node 1 (0°,0°) -> node 2 (1°,1°)
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge = MakeEdge(1, 1, 2);
        var chart = BuildChart([n1, n2], [edge]);
        var feature = MakeLineFeature(1, ForwardEdgeRef(1));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.NotNull(result);
        Assert.IsType<LineString>(result);
        var ls = (LineString)result;
        Assert.Equal(2, ls.NumPoints);

        var (x0, y0) = Project(0, 0);
        var (x1, y1) = Project(1, 1);
        Assert.Equal(x0, ls.Coordinates[0].X, 0.01);
        Assert.Equal(y0, ls.Coordinates[0].Y, 0.01);
        Assert.Equal(x1, ls.Coordinates[1].X, 0.01);
        Assert.Equal(y1, ls.Coordinates[1].Y, 0.01);
    }

    [Fact]
    public void ReturnsLineString_WithIntermediatePoints()
    {
        // Edge from node 1 (0°,0°) through intermediate (0.5°,0.5°) to node 2 (1°,1°)
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge = MakeEdge(1, 1, 2, Coord(0.5, 0.5));
        var chart = BuildChart([n1, n2], [edge]);
        var feature = MakeLineFeature(1, ForwardEdgeRef(1));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.NotNull(result);
        var ls = Assert.IsType<LineString>(result);
        Assert.Equal(3, ls.NumPoints);

        var (xMid, yMid) = Project(0.5, 0.5);
        Assert.Equal(xMid, ls.Coordinates[1].X, 0.01);
        Assert.Equal(yMid, ls.Coordinates[1].Y, 0.01);
    }

    [Fact]
    public void ReversesEdgeCoordinates_WhenOrientationIsReverse()
    {
        // Edge from node 1 (0°,0°) to node 2 (1°,1°), referenced in reverse
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge = MakeEdge(1, 1, 2);
        var chart = BuildChart([n1, n2], [edge]);
        var feature = MakeLineFeature(1, ReverseEdgeRef(1));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.NotNull(result);
        var ls = Assert.IsType<LineString>(result);

        // First coordinate should be node 2 (reversed)
        var (x2, y2) = Project(1, 1);
        var (x1, y1) = Project(0, 0);
        Assert.Equal(x2, ls.Coordinates[0].X, 0.01);
        Assert.Equal(y2, ls.Coordinates[0].Y, 0.01);
        Assert.Equal(x1, ls.Coordinates[1].X, 0.01);
        Assert.Equal(y1, ls.Coordinates[1].Y, 0.01);
    }

    [Fact]
    public void MergesContiguousEdges_IntoSingleLineString()
    {
        // Edge 1: node 1 -> node 2, Edge 2: node 2 -> node 3 (contiguous)
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var n3 = MakeNode(3, 2, 0);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 2, 3);
        var chart = BuildChart([n1, n2, n3], [edge1, edge2]);
        var feature = MakeLineFeature(1, ForwardEdgeRef(1), ForwardEdgeRef(2));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.NotNull(result);
        var ls = Assert.IsType<LineString>(result);
        // 3 unique coordinates (shared node 2 is deduplicated)
        Assert.Equal(3, ls.NumPoints);
    }

    [Fact]
    public void ReturnsMultiLineString_ForNonContiguousEdges()
    {
        // Edge 1: node 1 -> node 2, Edge 2: node 3 -> node 4 (gap between them)
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var n3 = MakeNode(3, 5, 5);
        var n4 = MakeNode(4, 6, 6);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 3, 4);
        var chart = BuildChart([n1, n2, n3, n4], [edge1, edge2]);
        var feature = MakeLineFeature(1, ForwardEdgeRef(1), ForwardEdgeRef(2));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.NotNull(result);
        var mls = Assert.IsType<MultiLineString>(result);
        Assert.Equal(2, mls.NumGeometries);
    }

    [Fact]
    public void SkipsMaskedEdges()
    {
        // Edge 1 is visible, Edge 2 is masked, Edge 3 is visible (not contiguous with 1)
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var n3 = MakeNode(3, 2, 2);
        var n4 = MakeNode(4, 3, 3);
        var n5 = MakeNode(5, 5, 5);
        var n6 = MakeNode(6, 6, 6);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 3, 4); // masked
        var edge3 = MakeEdge(3, 5, 6);
        var chart = BuildChart([n1, n2, n3, n4, n5, n6], [edge1, edge2, edge3]);
        var feature = MakeLineFeature(1,
            ForwardEdgeRef(1),
            MaskedEdgeRef(2),
            ForwardEdgeRef(3));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.NotNull(result);
        // Two visible edges that are non-contiguous → MultiLineString
        var mls = Assert.IsType<MultiLineString>(result);
        Assert.Equal(2, mls.NumGeometries);
    }

    [Fact]
    public void ClosedRing_ProperlyClosesLineString()
    {
        // Three edges forming a closed ring: 1→2, 2→3, 3→1
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 0);
        var n3 = MakeNode(3, 0.5, 1);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 2, 3);
        var edge3 = MakeEdge(3, 3, 1);
        var chart = BuildChart([n1, n2, n3], [edge1, edge2, edge3]);
        var feature = MakeLineFeature(1,
            ForwardEdgeRef(1),
            ForwardEdgeRef(2),
            ForwardEdgeRef(3));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.NotNull(result);
        var ls = Assert.IsType<LineString>(result);

        // Closed ring: 4 points — node1, node2, node3, node1 (properly closed)
        Assert.Equal(4, ls.NumPoints);
        Assert.True(ls.Coordinates[0].Equals2D(ls.Coordinates[^1]),
            "First and last coordinates should match for a closed ring");
    }

    [Fact]
    public void IsClosedVisibleEdgeRing_ReturnsFalse_WhenNoVisibleEdges()
    {
        var chart = BuildChart([], []);

        // Feature with only masked edges
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge = MakeEdge(1, 1, 2);
        chart = BuildChart([n1, n2], [edge]);
        var feature = MakeLineFeature(1, MaskedEdgeRef(1));

        Assert.False(S57LineGeometryBuilder.IsClosedVisibleEdgeRing(chart, feature));
    }

    [Fact]
    public void IsClosedVisibleEdgeRing_ReturnsTrue_WhenFirstAndLastNodeMatch()
    {
        // Two edges: 1→2 and 2→1 form a closed ring
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 2, 1);
        var chart = BuildChart([n1, n2], [edge1, edge2]);
        var feature = MakeLineFeature(1, ForwardEdgeRef(1), ForwardEdgeRef(2));

        Assert.True(S57LineGeometryBuilder.IsClosedVisibleEdgeRing(chart, feature));
    }

    [Fact]
    public void IsClosedVisibleEdgeRing_ReturnsTrue_WithReversedLastEdge()
    {
        // Edge 1 forward: 1→2, Edge 2 reversed: treats as 1←2 i.e. end node is node 1
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 1, 2); // begin=1, end=2; reversed → oriented start=2, end=1
        var chart = BuildChart([n1, n2], [edge1, edge2]);
        var feature = MakeLineFeature(1, ForwardEdgeRef(1), ReverseEdgeRef(2));

        Assert.True(S57LineGeometryBuilder.IsClosedVisibleEdgeRing(chart, feature));
    }

    [Fact]
    public void IsClosedVisibleEdgeRing_ReturnsFalse_WhenNotClosed()
    {
        // Edge 1: 1→2, Edge 2: 2→3 (not closed)
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var n3 = MakeNode(3, 2, 2);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 2, 3);
        var chart = BuildChart([n1, n2, n3], [edge1, edge2]);
        var feature = MakeLineFeature(1, ForwardEdgeRef(1), ForwardEdgeRef(2));

        Assert.False(S57LineGeometryBuilder.IsClosedVisibleEdgeRing(chart, feature));
    }

    [Fact]
    public void GetEdgeCoordinates_ReturnsForwardOrder()
    {
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge = MakeEdge(1, 1, 2, Coord(0.5, 0.5));
        var chart = BuildChart([n1, n2], [edge]);

        var coords = S57LineGeometryBuilder.GetEdgeCoordinates(chart, edge, reverse: false);

        Assert.Equal(3, coords.Count);
        var (x0, y0) = Project(0, 0);
        var (x1, y1) = Project(0.5, 0.5);
        var (x2, y2) = Project(1, 1);
        Assert.Equal(x0, coords[0].X, 0.01);
        Assert.Equal(y0, coords[0].Y, 0.01);
        Assert.Equal(x1, coords[1].X, 0.01);
        Assert.Equal(y1, coords[1].Y, 0.01);
        Assert.Equal(x2, coords[2].X, 0.01);
        Assert.Equal(y2, coords[2].Y, 0.01);
    }

    [Fact]
    public void GetEdgeCoordinates_ReturnsReverseOrder()
    {
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge = MakeEdge(1, 1, 2);
        var chart = BuildChart([n1, n2], [edge]);

        var coords = S57LineGeometryBuilder.GetEdgeCoordinates(chart, edge, reverse: true);

        Assert.Equal(2, coords.Count);
        var (x0, y0) = Project(1, 1); // end node comes first when reversed
        var (x1, y1) = Project(0, 0);
        Assert.Equal(x0, coords[0].X, 0.01);
        Assert.Equal(y0, coords[0].Y, 0.01);
        Assert.Equal(x1, coords[1].X, 0.01);
        Assert.Equal(y1, coords[1].Y, 0.01);
    }

    [Fact]
    public void GetEdgeCoordinates_ExcludesEndNode_WhenRequested()
    {
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 1);
        var edge = MakeEdge(1, 1, 2);
        var chart = BuildChart([n1, n2], [edge]);

        var coords = S57LineGeometryBuilder.GetEdgeCoordinates(chart, edge, reverse: false, excludeEndNode: true);

        // Only the beginning node remains
        Assert.Single(coords);
        var (x, y) = Project(0, 0);
        Assert.Equal(x, coords[0].X, 0.01);
        Assert.Equal(y, coords[0].Y, 0.01);
    }

    [Fact]
    public void ReturnsNull_WhenEdgeNotFoundInChart()
    {
        // Feature references an edge that doesn't exist in the chart
        var chart = BuildChart([], []);
        var feature = MakeLineFeature(1, ForwardEdgeRef(99));

        var result = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, feature);

        Assert.Null(result);
    }

    [Fact]
    public void ClosedRing_WithMaskedEdge_StillDetectsClosure()
    {
        // Edges: 1→2 (visible), 2→3 (masked), 3→1 (visible)
        // Visible edges: first start=1, last end=1 → closed
        var n1 = MakeNode(1, 0, 0);
        var n2 = MakeNode(2, 1, 0);
        var n3 = MakeNode(3, 0.5, 1);
        var edge1 = MakeEdge(1, 1, 2);
        var edge2 = MakeEdge(2, 2, 3);
        var edge3 = MakeEdge(3, 3, 1);
        var chart = BuildChart([n1, n2, n3], [edge1, edge2, edge3]);
        var feature = MakeLineFeature(1,
            ForwardEdgeRef(1),
            MaskedEdgeRef(2),
            ForwardEdgeRef(3));

        Assert.True(S57LineGeometryBuilder.IsClosedVisibleEdgeRing(chart, feature));
    }
}
