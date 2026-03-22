using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Caches the projected (Spherical Mercator) coordinates for S-57 edges so that
/// the expensive <see cref="SphericalMercator.FromLonLat"/> computation is performed
/// at most once per edge per chart, regardless of how many features reference it.
/// </summary>
/// <remarks>
/// <para>
/// In S-57 chain-node topology, edges are shared: a single edge may be referenced by
/// multiple area features (e.g. adjacent DEPARE faces), line features (COALNE), and
/// visible-edge outlines. Without caching, each reference recomputes the full
/// ToDecimalDegrees → FromLonLat pipeline for every coordinate in the edge.
/// </para>
/// <para>
/// The cache stores the <b>forward-oriented</b> coordinate array (beginning node,
/// intermediate points, end node). Callers that need reversed or trimmed coordinates
/// build their own list from the cached array.
/// </para>
/// <para>
/// Lifecycle: one instance per <see cref="S57Chart"/>, automatically created via
/// <see cref="For"/>. The <see cref="ConditionalWeakTable{TKey,TValue}"/> ensures
/// that the cache is released when the chart is garbage-collected.
/// </para>
/// </remarks>
public sealed class ProjectedEdgeCache
{
    /// <summary>
    /// Per-chart ambient cache, keyed by <see cref="S57Chart"/> instance.
    /// When a chart is collected, its cache entry is automatically released.
    /// </summary>
    private static readonly ConditionalWeakTable<S57Chart, ProjectedEdgeCache> ChartCaches = new();

    /// <summary>
    /// Returns the <see cref="ProjectedEdgeCache"/> for the given chart,
    /// creating one on first access. The same instance is returned for
    /// the same chart reference, regardless of caller.
    /// </summary>
    public static ProjectedEdgeCache For(S57Chart chart) => ChartCaches.GetOrCreateValue(chart);

    private readonly Dictionary<S57RecordName, Coordinate[]> _cache = new();

    /// <summary>
    /// Returns the forward-oriented projected coordinates for the given edge,
    /// computing and caching them on first access.
    /// </summary>
    /// <param name="chart">The chart providing coordinate conversion factors and node lookups.</param>
    /// <param name="edge">The edge to project.</param>
    /// <returns>
    /// A cached <see cref="Coordinate"/> array in forward orientation:
    /// [beginning node, intermediate points…, end node].
    /// Do <b>not</b> mutate the returned array.
    /// </returns>
    public Coordinate[] GetOrCompute(S57Chart chart, S57Edge edge)
    {
        if (_cache.TryGetValue(edge.RecordName, out var cached))
            return cached;

        var coords = ComputeForwardCoordinates(chart, edge);
        _cache[edge.RecordName] = coords;
        return coords;
    }

    /// <summary>Gets the number of edges currently cached.</summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Computes the forward-oriented projected coordinates for an edge.
    /// </summary>
    internal static Coordinate[] ComputeForwardCoordinates(S57Chart chart, S57Edge edge)
    {
        // Pre-size: begin node + intermediates + end node
        int capacity = (edge.HasBeginningNode ? 1 : 0)
            + edge.IntermediatePoints.Count
            + (edge.HasEndNode ? 1 : 0);

        var list = new List<Coordinate>(capacity);

        if (edge.HasBeginningNode)
        {
            var beginNode = chart.GetConnectedNode(edge.BeginningNode!.Value);
            if (beginNode != null)
            {
                var (lon, lat) = chart.ToDecimalDegrees(beginNode.Position);
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                list.Add(new Coordinate(x, y));
            }
        }

        if (edge.HasIntermediatePoints)
        {
            foreach (var point in edge.IntermediatePoints)
            {
                var (lon, lat) = chart.ToDecimalDegrees(point);
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                list.Add(new Coordinate(x, y));
            }
        }

        if (edge.HasEndNode)
        {
            var endNode = chart.GetConnectedNode(edge.EndNode!.Value);
            if (endNode != null)
            {
                var (lon, lat) = chart.ToDecimalDegrees(endNode.Position);
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                list.Add(new Coordinate(x, y));
            }
        }

        return list.ToArray();
    }
}


