using System.Collections.Generic;
using System.Linq;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Builds NTS geometry objects from S-57 line features.
/// </summary>
public static class S57LineGeometryBuilder
{
    /// <summary>
    /// Creates a <see cref="Geometry"/> (either a <see cref="LineString"/> or
    /// <see cref="MultiLineString"/>) from an S-57 line feature and its chart context.
    /// </summary>
    /// <param name="chart">The S-57 chart providing spatial lookups.</param>
    /// <param name="lineFeature">The line feature to convert.</param>
    /// <param name="edgeCache">Optional projected edge coordinate cache for performance.</param>
    /// <returns>A geometry representing the line feature, or <c>null</c> if the feature has no edges.</returns>
    public static Geometry? CreateLineStringFromLineFeature(S57Chart chart, S57LineFeature lineFeature, ProjectedEdgeCache? edgeCache = null)
    {
        if (!lineFeature.HasEdgeReferences)
            return null;

        var allSegments = new List<List<Coordinate>>();
        var currentSegment = new List<Coordinate>();
        S57RecordName? previousEndNode = null;

        foreach (var edgeRef in lineFeature.EdgeReferences)
        {
            // Skip masked edges — they exist for topological completeness
            // (e.g. boundary-closing edges) but should not be rendered.
            if (edgeRef.Mask == S57MaskingIndicator.Mask)
            {
                previousEndNode = null;
                continue;
            }

            var edge = chart.GetEdge(edgeRef.Name);
            if (edge == null)
                continue;

            bool reverse = edgeRef.Orientation == S57Orientation.Reverse;

            // Oriented start/end nodes for this edge
            var orientedStartNode = reverse ? edge.EndNode : edge.BeginningNode;
            var orientedEndNode = reverse ? edge.BeginningNode : edge.EndNode;

            var edgeCoords = GetEdgeCoordinates(chart, edge, reverse, edgeCache: edgeCache);
            if (edgeCoords.Count == 0)
                continue;

            if (currentSegment.Count > 0)
            {
                // Check if this edge is contiguous with the current segment
                // by comparing node record names (integer-based) rather than
                // projected floating-point coordinates.
                bool contiguous = previousEndNode.HasValue
                    && orientedStartNode.HasValue
                    && previousEndNode.Value == orientedStartNode.Value;

                if (contiguous)
                {
                    // Contiguous — skip the duplicate first coordinate and append
                    for (var i = 1; i < edgeCoords.Count; i++)
                    {
                        currentSegment.Add(edgeCoords[i]);
                    }
                }
                else
                {
                    // Not contiguous — start a new segment
                    if (currentSegment.Count >= 2)
                    {
                        allSegments.Add(currentSegment);
                    }
                    currentSegment = new List<Coordinate>(edgeCoords);
                }
            }
            else
            {
                currentSegment.AddRange(edgeCoords);
            }

            previousEndNode = orientedEndNode;
        }

        // Add the final segment
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

    /// <summary>
    /// Determines whether a line feature's visible (non-masked) edges form a
    /// topologically closed ring by comparing the oriented starting node of the
    /// first visible edge with the oriented ending node of the last visible edge.
    /// </summary>
    internal static bool IsClosedVisibleEdgeRing(S57Chart chart, S57LineFeature lineFeature)
    {
        S57SpatialPointer? firstVisible = null;
        S57SpatialPointer? lastVisible = null;

        foreach (var edgeRef in lineFeature.EdgeReferences)
        {
            if (edgeRef.Mask == S57MaskingIndicator.Mask)
                continue;

            firstVisible ??= edgeRef;
            lastVisible = edgeRef;
        }

        if (firstVisible is not { } first || lastVisible is not { } last)
            return false;

        var firstEdge = chart.GetEdge(first.Name);
        var lastEdge = chart.GetEdge(last.Name);
        if (firstEdge == null || lastEdge == null)
            return false;

        // Get the starting node of the first visible edge (respecting orientation)
        var firstStartNode = first.Orientation == S57Orientation.Reverse
            ? firstEdge.EndNode
            : firstEdge.BeginningNode;

        // Get the ending node of the last visible edge (respecting orientation)
        var lastEndNode = last.Orientation == S57Orientation.Reverse
            ? lastEdge.BeginningNode
            : lastEdge.EndNode;

        if (!firstStartNode.HasValue || !lastEndNode.HasValue)
            return false;

        return firstStartNode.Value == lastEndNode.Value;
    }

    internal static List<Coordinate> GetEdgeCoordinates(
        S57Chart chart, S57Edge edge, bool reverse,
        bool excludeEndNode = false, ProjectedEdgeCache? edgeCache = null)
    {
        // Use the explicit cache if provided, otherwise fall back to the ambient
        // per-chart cache (ConditionalWeakTable). This ensures callers that don't
        // thread the cache explicitly (e.g. template handler delegates) still get
        // the benefit of caching.
        Coordinate[] forward = (edgeCache ?? ProjectedEdgeCache.For(chart))
            .GetOrCompute(chart, edge);

        var coords = new List<Coordinate>(forward);

        if (reverse)
        {
            coords.Reverse();
        }

        // After orientation is applied, remove the last coordinate (the oriented end node)
        // if the caller requested it (e.g. to avoid closing a ring).
        if (excludeEndNode && coords.Count > 0)
        {
            coords.RemoveAt(coords.Count - 1);
        }

        return coords;
    }
}
