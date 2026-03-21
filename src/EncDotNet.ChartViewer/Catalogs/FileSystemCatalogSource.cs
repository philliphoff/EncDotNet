using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57.Charts;
using EncDotNet.S57.ExchangeSets;
using Microsoft.Extensions.Logging;

namespace EncDotNet.ChartViewer.Catalogs;

internal sealed class FileSystemCatalogSource : ICatalogSource
{
    private readonly string _chartIndexPath;
    private readonly string _baseDirectory;
    private readonly ILogger<FileSystemCatalogSource> _logger;
    private readonly ConcurrentDictionary<string, S57Chart> _chartCache = new();

    public FileSystemCatalogSource(string chartIndexPath, ILogger<FileSystemCatalogSource> logger)
    {
        _chartIndexPath = chartIndexPath;
        _baseDirectory = Path.GetDirectoryName(chartIndexPath) ?? "";
        _logger = logger;
    }

    public async IAsyncEnumerable<ChartIndexEntry> GetCatalogAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(_chartIndexPath, cancellationToken).ConfigureAwait(false);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };

        var entries = JsonSerializer.Deserialize<List<ChartIndexEntry>>(json, options);

        if (entries is null)
            yield break;

        foreach (var entry in entries)
        {
            yield return entry;
        }
    }

    public async Task<S57Chart> GetChartAsync(ChartIndexEntry entry, CancellationToken cancellationToken = default)
    {
        if (_chartCache.TryGetValue(entry.Path, out var cached))
            return cached;

        var chartPath = Path.Combine(_baseDirectory, entry.Path);
        var chartDirectory = Path.GetDirectoryName(chartPath)!;
        var exchangeSet = S57ExchangeSetReader.Read(chartDirectory);
        var chart = await exchangeSet.ReadChartAsync(chartDirectory, _logger, cancellationToken).ConfigureAwait(false);

        _chartCache.TryAdd(entry.Path, chart);
        return chart;
    }
}
