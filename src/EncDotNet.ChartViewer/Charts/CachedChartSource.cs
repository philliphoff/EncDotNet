using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57.Charts;

namespace EncDotNet.ChartViewer.Charts;

/// <summary>
/// Provides cached access to parsed <see cref="S57Chart"/> instances.
/// Delegates to an underlying <see cref="IChartSource"/> on cache miss.
/// </summary>
public sealed class CachedChartSource : IChartSource
{
    private readonly IChartSource _chartSource;
    private readonly ConcurrentDictionary<string, S57Chart> _cache = new();

    public CachedChartSource(IChartSource chartSource)
    {
        _chartSource = chartSource;
    }

    /// <summary>
    /// Returns the chart for the given index entry, using a cached instance if available.
    /// </summary>
    public async Task<S57Chart> GetChartAsync(ChartIndexEntry entry, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(entry.Path, out var cached))
            return cached;

        var chart = await _chartSource.GetChartAsync(entry, cancellationToken).ConfigureAwait(false);

        _cache.TryAdd(entry.Path, chart);
        return chart;
    }
}


