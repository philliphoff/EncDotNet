using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EncDotNet.Enc;
using EncDotNet.Enc.Charts;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Factory for creating Mapsui layers from S-57 chart data.
/// </summary>
public static class S57LayerFactory
{
    /// <summary>
    /// Creates a MemoryLayer containing all features from an S-57 chart.
    /// </summary>
    /// <param name="chart">The S-57 chart to render.</param>
    /// <param name="layerName">Optional name for the layer.</param>
    /// <returns>A MemoryLayer containing the chart features.</returns>
    public static MemoryLayer CreateLayer(S57Chart chart, string? layerName = null)
    {
        var features = new List<IFeature>();

        // Add area features (polygons)
        foreach (var areaFeature in chart.AreaFeatures)
        {
            var polygon = CreatePolygonFromAreaFeature(chart, areaFeature);
            if (polygon != null)
            {
                var feature = new GeometryFeature(polygon);
                feature["ObjectCode"] = areaFeature.ObjectCode;
                feature.Styles.Add(CreateAreaStyle(areaFeature.ObjectCode));
                features.Add(feature);
            }
        }

        // Add line features
        foreach (var lineFeature in chart.LineFeatures)
        {
            var lineString = CreateLineStringFromLineFeature(chart, lineFeature);
            if (lineString != null)
            {
                var feature = new GeometryFeature(lineString);
                feature["ObjectCode"] = lineFeature.ObjectCode;
                feature.Styles.Add(CreateLineStyle(lineFeature.ObjectCode));
                features.Add(feature);
            }
        }

        // Add point features
        foreach (var pointFeature in chart.PointFeatures)
        {
            var point = CreatePointFromPointFeature(chart, pointFeature);
            if (point != null)
            {
                var feature = new GeometryFeature(point);
                feature["ObjectCode"] = pointFeature.ObjectCode;
                feature.Styles.Add(CreatePointStyle(pointFeature.ObjectCode));
                features.Add(feature);
            }
        }

        return new MemoryLayer
        {
            Name = layerName ?? "S-57 Features",
            Features = features,
            Style = null // Use per-feature styles
        };
    }

    /// <summary>
    /// Creates a MemoryLayer containing only features matching the specified object codes.
    /// </summary>
    /// <param name="chart">The S-57 chart to render.</param>
    /// <param name="objectCodes">The S-57 object codes to include.</param>
    /// <param name="layerName">Name for the layer.</param>
    /// <returns>A MemoryLayer containing the matching features.</returns>
    public static MemoryLayer CreateLayerForObjectCodes(
        S57Chart chart,
        ImmutableArray<int> objectCodes,
        string layerName)
    {
        var codeSet = objectCodes.ToHashSet();
        var features = new List<IFeature>();

        // Add matching area features
        foreach (var areaFeature in chart.AreaFeatures)
        {
            if (!codeSet.Contains(areaFeature.ObjectCode))
                continue;

            var polygon = CreatePolygonFromAreaFeature(chart, areaFeature);
            if (polygon != null)
            {
                var feature = new GeometryFeature(polygon);
                feature["ObjectCode"] = areaFeature.ObjectCode;
                feature.Styles.Add(CreateAreaStyle(areaFeature.ObjectCode));
                features.Add(feature);
            }
        }

        // Add matching line features
        foreach (var lineFeature in chart.LineFeatures)
        {
            if (!codeSet.Contains(lineFeature.ObjectCode))
                continue;

            var lineString = CreateLineStringFromLineFeature(chart, lineFeature);
            if (lineString != null)
            {
                var feature = new GeometryFeature(lineString);
                feature["ObjectCode"] = lineFeature.ObjectCode;
                feature.Styles.Add(CreateLineStyle(lineFeature.ObjectCode));
                features.Add(feature);
            }
        }

        // Add matching point features
        foreach (var pointFeature in chart.PointFeatures)
        {
            if (!codeSet.Contains(pointFeature.ObjectCode))
                continue;

            var point = CreatePointFromPointFeature(chart, pointFeature);
            if (point != null)
            {
                var feature = new GeometryFeature(point);
                feature["ObjectCode"] = pointFeature.ObjectCode;
                feature.Styles.Add(CreatePointStyle(pointFeature.ObjectCode));
                features.Add(feature);
            }
        }

        return new MemoryLayer
        {
            Name = layerName,
            Features = features,
            Style = null // Use per-feature styles
        };
    }

    private static Point? CreatePointFromPointFeature(S57Chart chart, S57PointFeature pointFeature)
    {
        if (!pointFeature.HasSpatialReferences)
            return null;

        var spatialRef = pointFeature.PrimarySpatialReference!.Value;

        // Try isolated node first
        var isolatedNode = chart.GetIsolatedNode(spatialRef.Name);
        if (isolatedNode?.HasPosition == true)
        {
            var (lon, lat) = chart.ToDecimalDegrees(isolatedNode.Position!.Value);
            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            return new Point(x, y);
        }

        // Try connected node
        var connectedNode = chart.GetConnectedNode(spatialRef.Name);
        if (connectedNode != null)
        {
            var (lon, lat) = chart.ToDecimalDegrees(connectedNode.Position);
            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            return new Point(x, y);
        }

        return null;
    }

    private static Geometry? CreateLineStringFromLineFeature(S57Chart chart, S57LineFeature lineFeature)
        => S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, lineFeature);

    private static Polygon? CreatePolygonFromAreaFeature(S57Chart chart, S57AreaFeature areaFeature)
    {
        if (!areaFeature.HasFaceReference)
            return null;

        var face = chart.GetFace(areaFeature.FaceReference!.Value.Name);
        if (face == null || !face.HasExteriorBoundary)
            return null;

        // Build exterior ring
        var exteriorCoords = BuildRingFromEdges(chart, face.ExteriorBoundary);
        if (exteriorCoords.Count < 4) // Minimum for a valid polygon ring
            return null;

        var exteriorRing = new LinearRing(exteriorCoords.ToArray());

        // Build interior rings (holes) if any
        var interiorRings = new List<LinearRing>();
        if (face.HasInteriorBoundaries)
        {
            var interiorCoords = BuildRingFromEdges(chart, face.InteriorBoundaries);
            if (interiorCoords.Count >= 4)
            {
                interiorRings.Add(new LinearRing(interiorCoords.ToArray()));
            }
        }

        return new Polygon(exteriorRing, interiorRings.ToArray());
    }

    private static List<Coordinate> BuildRingFromEdges(S57Chart chart, IEnumerable<S57EdgeReference> edgeRefs)
    {
        var coordinates = new List<Coordinate>();

        foreach (var edgeRef in edgeRefs)
        {
            var edge = chart.GetEdge(edgeRef.EdgeName);
            if (edge == null)
                continue;

            var edgeCoords = GetEdgeCoordinates(chart, edge, edgeRef.Orientation == S57Orientation.Reverse);
            
            // Skip first coordinate if we already have coordinates (to avoid duplicates)
            var startIndex = coordinates.Count > 0 ? 1 : 0;
            for (var i = startIndex; i < edgeCoords.Count; i++)
            {
                coordinates.Add(edgeCoords[i]);
            }
        }

        // Close the ring if necessary
        if (coordinates.Count > 0 && !coordinates[0].Equals2D(coordinates[^1]))
        {
            coordinates.Add(coordinates[0]);
        }

        return coordinates;
    }

    private static List<Coordinate> GetEdgeCoordinates(S57Chart chart, S57Edge edge, bool reverse, bool excludeEndNode = false)
        => S57LineGeometryBuilder.GetEdgeCoordinates(chart, edge, reverse, excludeEndNode);

    private static VectorStyle CreatePointStyle(int objectCode)
    {
        // Basic point styling - can be expanded based on object codes
        return new VectorStyle
        {
            Fill = new Brush(new Color(255, 0, 0, 200)),
            Outline = new Pen(Color.Black, 1)
        };
    }

    private static VectorStyle CreateLineStyle(int objectCode)
    {
        // Different colors for different object types
        var (color, width) = objectCode switch
        {
            // DEPCNT - Depth contour
            43 => (new Color(0, 100, 200, 200), 1),
            // COALNE - Coastline
            30 => (new Color(0, 0, 0, 255), 1),
            // SLCONS - Shoreline construction
            122 => (new Color(100, 100, 100, 255), 1),
            // BRIDGE - Bridge
            12 => (new Color(80, 80, 80, 255), 2),
            // CBLOHD - Cable overhead
            28 => (new Color(160, 0, 160, 200), 1),
            // CBLSUB - Cable submarine
            29 => (new Color(160, 0, 160, 150), 1),
            // PIPSOL - Pipeline submarine/on land
            93 => (new Color(0, 160, 0, 180), 1),
            // FERYRT - Ferry route
            58 => (new Color(160, 0, 160, 180), 1),
            // NAVLNE - Navigation line
            85 => (new Color(200, 0, 200, 180), 1),
            // RECTRC - Recommended track
            96 => (new Color(200, 0, 200, 200), 1),
            // TSSBND - TSS boundary
            144 => (new Color(150, 100, 200, 180), 1),
            // TSELNE - Traffic separation line
            143 => (new Color(150, 100, 200, 180), 1),
            // RIVERS - River
            114 => (new Color(0, 100, 200, 180), 1),
            // CANALS - Canal
            23 => (new Color(0, 100, 200, 180), 1),
            // Default
            _ => (new Color(0, 0, 255, 150), 1)
        };

        return new VectorStyle
        {
            Line = new Pen(color, width)
        };
    }

    private static VectorStyle CreateAreaStyle(int objectCode)
    {
        // Different colors for different object types
        var (fillColor, outlineColor) = objectCode switch
        {
            // LNDARE - Land area
            71 => (new Color(200, 180, 140, 150), new Color(100, 80, 40, 200)),
            // DEPARE - Depth area
            42 => (new Color(180, 220, 255, 100), new Color(0, 100, 200, 150)),
            // SEAARE - Sea area
            112 => (new Color(200, 230, 255, 80), new Color(0, 100, 200, 100)),
            // BUAARE - Built-up area
            25 => (new Color(220, 180, 180, 150), new Color(150, 100, 100, 200)),
            // DRGARE - Dredged area
            47 => (new Color(180, 220, 255, 80), new Color(0, 100, 200, 120)),
            // LAKARE - Lake
            69 => (new Color(170, 210, 255, 120), new Color(0, 80, 180, 150)),
            // DOCARE - Dock area
            45 => (new Color(180, 180, 200, 120), new Color(100, 100, 120, 180)),
            // ACHARE - Anchorage area
            2 => (new Color(200, 200, 255, 80), new Color(100, 100, 200, 150)),
            // RESARE - Restricted area
            120 => (new Color(255, 200, 200, 80), new Color(200, 100, 100, 150)),
            // DMPGRD - Dumping ground
            46 => (new Color(200, 200, 150, 80), new Color(150, 150, 80, 150)),
            // MIPARE - Military practice area
            83 => (new Color(255, 200, 200, 60), new Color(200, 80, 80, 120)),
            // CTNARE - Caution area
            40 => (new Color(255, 230, 180, 80), new Color(200, 150, 50, 150)),
            // FAIRWY - Fairway
            57 => (new Color(200, 220, 255, 60), new Color(100, 140, 200, 120)),
            // TSSLPT - TSS lane part
            145 => (new Color(220, 200, 255, 60), new Color(150, 100, 200, 120)),
            // TSSRON - TSS roundabout
            146 => (new Color(220, 200, 255, 60), new Color(150, 100, 200, 120)),
            // TSEZNE - TSS zone
            147 => (new Color(220, 200, 255, 60), new Color(150, 100, 200, 120)),
            // TSSCRS - TSS crossing
            148 => (new Color(220, 200, 255, 60), new Color(150, 100, 200, 120)),
            // CBLARE - Cable area
            27 => (new Color(220, 180, 255, 60), new Color(160, 100, 200, 120)),
            // PIPARE - Pipeline area
            92 => (new Color(180, 255, 180, 60), new Color(80, 160, 80, 120)),
            // UNSARE - Unsurveyed area
            154 => (new Color(240, 240, 200, 80), new Color(180, 180, 100, 150)),
            // Default
            _ => (new Color(200, 200, 200, 100), new Color(100, 100, 100, 150))
        };

        return new VectorStyle
        {
            Fill = new Brush(fillColor),
            Outline = new Pen(outlineColor, 1)
        };
    }
}
