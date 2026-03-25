using System.Collections.Generic;
using System.Linq;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
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
    /// <param name="edgeCache">Optional projected edge coordinate cache for performance.</param>
    /// <returns>A geometry representing the area feature, or <c>null</c> if the feature has no spatial data.</returns>
    public static Geometry? CreatePolygonFromAreaFeature(S57Chart chart, S57AreaFeature areaFeature, ProjectedEdgeCache? edgeCache = null)
    {
        // Full topology (level 3): area references faces
        if (areaFeature.HasFaceReference)
            return CreatePolygonFromFaces(chart, areaFeature, edgeCache);

        // Chain-node topology (level 2): area references edges directly
        if (areaFeature.HasExteriorEdgeReferences)
            return CreatePolygonFromEdges(chart, areaFeature, edgeCache);

        return null;
    }

    /// <summary>
    /// Creates line geometry from only the visible (non-masked, non-truncated) edges
    /// of an area feature. Used to render the area outline selectively, suppressing
    /// masked edges and cell boundary edges per S-57 display rules.
    /// </summary>
    /// <param name="chart">The S-57 chart providing spatial lookups.</param>
    /// <param name="areaFeature">The area feature whose visible edges to extract.</param>
    /// <param name="edgeCache">Optional projected edge coordinate cache for performance.</param>
    /// <returns>
    /// A <see cref="LineString"/> or <see cref="MultiLineString"/> for the visible edges,
    /// or <c>null</c> if there are no visible edges.
    /// </returns>
    public static Geometry? CreateVisibleEdgeLinesFromAreaFeature(S57Chart chart, S57AreaFeature areaFeature, ProjectedEdgeCache? edgeCache = null)
    {
        // Collect all edge references from this area feature
        IEnumerable<S57EdgeReference> allEdgeRefs;

        if (areaFeature.HasFaceReference)
        {
            // For face topology, the FSPT MASK on the face reference indicates
            // whether all edges of that face should be masked for this feature.
            // Only include edges from faces whose FSPT MASK is not Mask.
            allEdgeRefs = areaFeature.FaceReferences
                .Where(faceRef => faceRef.Mask != S57MaskingIndicator.Mask)
                .Select(faceRef => chart.GetFace(faceRef.Name))
                .Where(face => face != null)
                .SelectMany(face => face!.ExteriorBoundary.Concat(face.InteriorBoundaries));
        }
        else if (areaFeature.HasExteriorEdgeReferences)
        {
            allEdgeRefs = areaFeature.ExteriorEdgeReferences
                .Concat(areaFeature.InteriorEdgeReferences);
        }
        else
        {
            return null;
        }

        return BuildLinesFromVisibleEdges(chart, allEdgeRefs, edgeCache);
    }

    /// <summary>
    /// Builds line geometry from visible edges, skipping masked and cell-boundary-truncated edges.
    /// Contiguous visible edges are merged into single line strings.
    /// </summary>
    internal static Geometry? BuildLinesFromVisibleEdges(S57Chart chart, IEnumerable<S57EdgeReference> edgeRefs, ProjectedEdgeCache? edgeCache = null)
    {
        var allSegments = new List<List<Coordinate>>();
        var currentSegment = new List<Coordinate>();
        S57RecordName? previousEndNode = null;

        foreach (var edgeRef in edgeRefs)
        {
            // Skip masked edges and cell boundary (ExteriorTruncated) edges
            if (edgeRef.Mask == S57MaskingIndicator.Mask
                || edgeRef.Usage == S57UsageIndicator.ExteriorTruncated)
            {
                // Break contiguity
                if (currentSegment.Count >= 2)
                {
                    allSegments.Add(currentSegment);
                    currentSegment = new List<Coordinate>();
                }
                else
                {
                    currentSegment.Clear();
                }
                previousEndNode = null;
                continue;
            }

            var edge = chart.GetEdge(edgeRef.EdgeName);
            if (edge == null)
                continue;

            bool reverse = edgeRef.Orientation == S57Orientation.Reverse;

            var orientedStartNode = reverse ? edge.EndNode : edge.BeginningNode;
            var orientedEndNode = reverse ? edge.BeginningNode : edge.EndNode;

            var edgeCoords = S57LineGeometryBuilder.GetEdgeCoordinates(chart, edge, reverse, edgeCache: edgeCache);
            if (edgeCoords.Count == 0)
                continue;

            if (currentSegment.Count > 0)
            {
                bool contiguous = previousEndNode.HasValue
                    && orientedStartNode.HasValue
                    && previousEndNode.Value == orientedStartNode.Value;

                // Coordinate proximity fallback (see BuildRingsFromEdges).
                if (!contiguous && currentSegment.Count > 0 && edgeCoords.Count > 0
                    && edgeCoords[0].Equals2D(currentSegment[^1]))
                {
                    contiguous = true;
                }

                if (contiguous)
                {
                    edgeCoords.CopyTo(currentSegment, startIndex: 1);
                }
                else
                {
                    if (currentSegment.Count >= 2)
                    {
                        allSegments.Add(currentSegment);
                    }
                    currentSegment = new List<Coordinate>(edgeCoords.Count);
                    edgeCoords.CopyTo(currentSegment);
                }
            }
            else
            {
                edgeCoords.CopyTo(currentSegment);
            }

            previousEndNode = orientedEndNode;
        }

        if (currentSegment.Count >= 2)
        {
            allSegments.Add(currentSegment);
        }

        if (allSegments.Count == 0)
            return null;

        if (allSegments.Count == 1)
            return new LineString(allSegments[0].ToArray());

        return new MultiLineString(
            allSegments.Select(s => new LineString(s.ToArray())).ToArray());
    }

    internal static Geometry? CreatePolygonFromFaces(S57Chart chart, S57AreaFeature areaFeature, ProjectedEdgeCache? edgeCache = null)
    {
        var polygons = new List<Polygon>();

        foreach (var faceRef in areaFeature.FaceReferences)
        {
            var face = chart.GetFace(faceRef.Name);
            if (face == null || !face.HasExteriorBoundary)
                continue;

            // Build exterior ring(s)
            var exteriorRings = BuildRingsFromEdges(chart, face.ExteriorBoundary, edgeCache);
            if (exteriorRings.Count == 0)
                continue;

            // Build interior rings (holes) if any — each separate loop is its own hole
            var interiorRings = new List<LinearRing>();
            if (face.HasInteriorBoundaries)
            {
                foreach (var ring in BuildRingsFromEdges(chart, face.InteriorBoundaries, edgeCache))
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

    internal static Geometry? CreatePolygonFromEdges(S57Chart chart, S57AreaFeature areaFeature, ProjectedEdgeCache? edgeCache = null)
    {
        var exteriorRings = BuildRingsFromEdges(chart, areaFeature.ExteriorEdgeReferences, edgeCache);
        if (exteriorRings.Count == 0)
            return null;

        var interiorRings = new List<LinearRing>();
        if (areaFeature.InteriorEdgeReferences.Count > 0)
        {
            foreach (var ring in BuildRingsFromEdges(chart, areaFeature.InteriorEdgeReferences, edgeCache))
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

    internal static List<List<Coordinate>> BuildRingsFromEdges(S57Chart chart, IEnumerable<S57EdgeReference> edgeRefs, ProjectedEdgeCache? edgeCache = null)
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

            var edgeCoords = S57LineGeometryBuilder.GetEdgeCoordinates(chart, edge, reverse, edgeCache: edgeCache);
            if (edgeCoords.Count == 0)
                continue;

            if (currentRing.Count > 0)
            {
                // Check if this edge is contiguous with the current ring
                // by comparing node record names.
                bool contiguous = previousEndNode.HasValue
                    && orientedStartNode.HasValue
                    && previousEndNode.Value == orientedStartNode.Value;

                // When node-based contiguity fails, fall back to coordinate
                // proximity. This handles edges with missing node references
                // or mismatched node names that are still geometrically connected,
                // preventing premature ring closure that creates needle artifacts.
                if (!contiguous && currentRing.Count > 0 && edgeCoords.Count > 0
                    && edgeCoords[0].Equals2D(currentRing[^1]))
                {
                    contiguous = true;
                }

                if (contiguous)
                {
                    // Contiguous — skip the duplicate first coordinate and append
                    edgeCoords.CopyTo(currentRing, startIndex: 1);
                }
                else
                {
                    // Not contiguous — close the current ring and start a new one
                    CloseRing(currentRing);
                    if (currentRing.Count >= 4)
                    {
                        rings.Add(currentRing);
                    }

                    currentRing = new List<Coordinate>(edgeCoords.Count);
                    edgeCoords.CopyTo(currentRing);
                }
            }
            else
            {
                edgeCoords.CopyTo(currentRing);
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
