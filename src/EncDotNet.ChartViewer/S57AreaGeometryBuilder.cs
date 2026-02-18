using System.Collections.Generic;
using EncDotNet.Enc;
using EncDotNet.Enc.Charts;
using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Builds NTS geometry objects from S-57 area features.
/// </summary>
public static class S57AreaGeometryBuilder
{
    /// <summary>
    /// Creates a <see cref="Geometry"/> (either a <see cref="Polygon"/> or
    /// <see cref="MultiPolygon"/>) from an S-57 area feature and its chart context.
    /// </summary>
    /// <param name="chart">The S-57 chart providing spatial lookups.</param>
    /// <param name="areaFeature">The area feature to convert.</param>
    /// <returns>A geometry representing the area feature, or <c>null</c> if the feature has no spatial data.</returns>
    public static Geometry? CreatePolygonFromAreaFeature(S57Chart chart, S57AreaFeature areaFeature)
    {
        // Full topology (level 3): area references faces
        if (areaFeature.HasFaceReference)
            return CreatePolygonFromFaces(chart, areaFeature);

        // Chain-node topology (level 2): area references edges directly
        if (areaFeature.HasExteriorEdgeReferences)
            return CreatePolygonFromEdges(chart, areaFeature);

        return null;
    }

    internal static Geometry? CreatePolygonFromFaces(S57Chart chart, S57AreaFeature areaFeature)
    {
        var polygons = new List<Polygon>();

        foreach (var faceRef in areaFeature.FaceReferences)
        {
            var face = chart.GetFace(faceRef.Name);
            if (face == null || !face.HasExteriorBoundary)
                continue;

            // Build exterior ring(s)
            var exteriorRings = BuildRingsFromEdges(chart, face.ExteriorBoundary);
            if (exteriorRings.Count == 0)
                continue;

            // Build interior rings (holes) if any — each separate loop is its own hole
            var interiorRings = new List<LinearRing>();
            if (face.HasInteriorBoundaries)
            {
                foreach (var ring in BuildRingsFromEdges(chart, face.InteriorBoundaries))
                {
                    interiorRings.Add(new LinearRing(ring.ToArray()));
                }
            }

            foreach (var extRing in exteriorRings)
            {
                polygons.Add(new Polygon(
                    new LinearRing(extRing.ToArray()),
                    interiorRings.ToArray()));
                interiorRings.Clear(); // assign holes to the first exterior ring only
            }
        }

        return polygons.Count switch
        {
            0 => null,
            1 => polygons[0],
            _ => new MultiPolygon(polygons.ToArray())
        };
    }

    internal static Geometry? CreatePolygonFromEdges(S57Chart chart, S57AreaFeature areaFeature)
    {
        var exteriorRings = BuildRingsFromEdges(chart, areaFeature.ExteriorEdgeReferences);
        if (exteriorRings.Count == 0)
            return null;

        var interiorRings = new List<LinearRing>();
        if (!areaFeature.InteriorEdgeReferences.IsDefaultOrEmpty)
        {
            foreach (var ring in BuildRingsFromEdges(chart, areaFeature.InteriorEdgeReferences))
            {
                interiorRings.Add(new LinearRing(ring.ToArray()));
            }
        }

        if (exteriorRings.Count == 1)
        {
            return new Polygon(
                new LinearRing(exteriorRings[0].ToArray()),
                interiorRings.ToArray());
        }

        // Multiple exterior rings → MultiPolygon
        var polygons = new List<Polygon>();
        for (int i = 0; i < exteriorRings.Count; i++)
        {
            polygons.Add(new Polygon(
                new LinearRing(exteriorRings[i].ToArray()),
                i == 0 ? interiorRings.ToArray() : []));
        }

        return new MultiPolygon(polygons.ToArray());
    }

    internal static List<List<Coordinate>> BuildRingsFromEdges(S57Chart chart, IEnumerable<S57EdgeReference> edgeRefs)
    {
        var rings = new List<List<Coordinate>>();
        var currentRing = new List<Coordinate>();
        S57RecordName? previousEndNode = null;

        foreach (var edgeRef in edgeRefs)
        {
            var edge = chart.GetEdge(edgeRef.EdgeName);
            if (edge == null)
                continue;

            bool reverse = edgeRef.Orientation == S57Orientation.Reverse;

            // Oriented start/end nodes for this edge
            var orientedStartNode = reverse ? edge.EndNode : edge.BeginningNode;
            var orientedEndNode = reverse ? edge.BeginningNode : edge.EndNode;

            var edgeCoords = S57LineGeometryBuilder.GetEdgeCoordinates(chart, edge, reverse);
            if (edgeCoords.Count == 0)
                continue;

            if (currentRing.Count > 0)
            {
                // Check if this edge is contiguous with the current ring
                // by comparing node record names.
                bool contiguous = previousEndNode.HasValue
                    && orientedStartNode.HasValue
                    && previousEndNode.Value == orientedStartNode.Value;

                if (contiguous)
                {
                    // Contiguous — skip the duplicate first coordinate and append
                    for (var i = 1; i < edgeCoords.Count; i++)
                    {
                        currentRing.Add(edgeCoords[i]);
                    }
                }
                else
                {
                    // Not contiguous — close the current ring and start a new one
                    CloseRing(currentRing);
                    if (currentRing.Count >= 4)
                    {
                        rings.Add(currentRing);
                    }

                    currentRing = new List<Coordinate>(edgeCoords);
                }
            }
            else
            {
                currentRing.AddRange(edgeCoords);
            }

            previousEndNode = orientedEndNode;
        }

        // Close and add the final ring
        CloseRing(currentRing);
        if (currentRing.Count >= 4)
        {
            rings.Add(currentRing);
        }

        return rings;
    }

    private static void CloseRing(List<Coordinate> coordinates)
    {
        if (coordinates.Count > 0 && !coordinates[0].Equals2D(coordinates[^1]))
        {
            coordinates.Add(coordinates[0]);
        }
    }
}
