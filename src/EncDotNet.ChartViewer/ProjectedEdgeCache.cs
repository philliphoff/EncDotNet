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
    /// Exports the cache contents as a dictionary of record IDs to flat coordinate arrays
    /// (alternating x, y values) for serialization.
    /// </summary>
    internal Dictionary<int, double[]> Export()
    {
        var result = new Dictionary<int, double[]>(_cache.Count);
        foreach (var (name, coords) in _cache)
        {
            var flat = new double[coords.Length * 2];
            for (int i = 0; i < coords.Length; i++)
            {
                flat[i * 2] = coords[i].X;
                flat[i * 2 + 1] = coords[i].Y;
            }
            result[name.RecordId] = flat;
        }
        return result;
    }

    /// <summary>
    /// Pre-populates the cache from previously baked coordinate data.
    /// </summary>
    /// <param name="chart">The chart whose edges to match.</param>
    /// <param name="bakedCoords">
    /// Dictionary of edge record ID to flat coordinate arrays (alternating x, y values).
    /// </param>
    internal void Import(S57Chart chart, Dictionary<int, double[]> bakedCoords)
    {
        foreach (var (recordId, flat) in bakedCoords)
        {
            var coords = new Coordinate[flat.Length / 2];
            for (int i = 0; i < coords.Length; i++)
            {
                coords[i] = new Coordinate(flat[i * 2], flat[i * 2 + 1]);
            }

            // Find the edge record name with this ID (RCNM=130 for edges)
            var name = new S57RecordName { RecordId = recordId, RecordNameCode = 130 };
            _cache[name] = coords;
        }
    }

    /// <summary>
    /// Computes the forward-oriented projected coordinates for an edge.
    /// </summary>
    internal static Coordinate[] ComputeForwardCoordinates(S57Chart chart, S57Edge edge)
    {
        // Pre-size: begin node + intermediates + end node
        int capacity = (edge.HasBeginningNode ? 1 : 0)
            + edge.IntermediatePoints.Count
            + (edge.HasEndNode ? 1 : 0);

        var array = new Coordinate[capacity];
        int index = 0;

        if (edge.HasBeginningNode)
        {
            var beginNode = chart.GetConnectedNode(edge.BeginningNode!.Value);
            if (beginNode != null)
            {
                var (lon, lat) = chart.ToDecimalDegrees(beginNode.Position);
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                array[index++] = new Coordinate(x, y);
            }
        }

        if (edge.HasIntermediatePoints)
        {
            foreach (var point in edge.IntermediatePoints)
            {
                var (lon, lat) = chart.ToDecimalDegrees(point);
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                array[index++] = new Coordinate(x, y);
            }
        }

        if (edge.HasEndNode)
        {
            var endNode = chart.GetConnectedNode(edge.EndNode!.Value);
            if (endNode != null)
            {
                var (lon, lat) = chart.ToDecimalDegrees(endNode.Position);
                var (x, y) = SphericalMercator.FromLonLat(lon, lat);
                array[index++] = new Coordinate(x, y);
            }
        }

        // Trim if some nodes were not found (rare)
        return index == array.Length ? array : array[..index];
    }
}


