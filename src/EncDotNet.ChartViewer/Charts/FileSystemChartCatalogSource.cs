using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using EncDotNet.ChartViewer.Baking;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57.Charts;
using EncDotNet.S57.ExchangeSets;
using Microsoft.Extensions.Logging;

namespace EncDotNet.ChartViewer.Charts;

internal sealed class FileSystemChartCatalogSource : IChartCatalogSource
{
    private readonly string _chartIndexPath;
    private readonly string _baseDirectory;
    private readonly ILogger<FileSystemChartCatalogSource> _logger;

    public FileSystemChartCatalogSource(string chartIndexPath, ILogger<FileSystemChartCatalogSource> logger)
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
        var chartPath = Path.Combine(_baseDirectory, entry.Path);
        var chartDirectory = Path.GetDirectoryName(chartPath)!;
        var totalSw = Stopwatch.StartNew();

        // Try loading from baked cache first
        if (ChartBaker.HasValidBakedFile(chartPath))
        {
            try
            {
                var (data, deserializeTime) = await ChartBaker.LoadBakedAsync(chartPath, cancellationToken).ConfigureAwait(false);
                var chartBuildSw = Stopwatch.StartNew();
                var chart = S57Chart.FromDocument(data.Document);

                // Pre-populate the projected edge cache from baked data
                if (data.ProjectedEdgeCoords.Count > 0)
                {
                    var edgeCache = ProjectedEdgeCache.For(chart);
                    edgeCache.Import(chart, data.ProjectedEdgeCoords);
                }

                chartBuildSw.Stop();
                totalSw.Stop();

                _logger.LogInformation(
                    "Loaded {Chart} from baked cache: deserialize={DeserializeMs}ms, chartBuild={ChartBuildMs}ms, total={TotalMs}ms, edges={EdgeCount}",
                    entry.Name, deserializeTime.TotalMilliseconds, chartBuildSw.Elapsed.TotalMilliseconds,
                    totalSw.Elapsed.TotalMilliseconds, data.ProjectedEdgeCoords.Count);

                return chart;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load baked cache for {Chart}, falling back to parse", entry.Name);
            }
        }

        // Parse from source
        var exchangeSet = S57ExchangeSetReader.Read(chartDirectory);

        var parseSw = Stopwatch.StartNew();
        var doc = await exchangeSet.ReadDocumentAsync(chartDirectory, _logger, cancellationToken).ConfigureAwait(false);
        parseSw.Stop();

        var buildSw = Stopwatch.StartNew();
        var parsedChart = S57Chart.FromDocument(doc);
        buildSw.Stop();
        totalSw.Stop();

        _logger.LogInformation(
            "Parsed {Chart} from source: parse={ParseMs}ms, chartBuild={ChartBuildMs}ms, total={TotalMs}ms",
            entry.Name, parseSw.Elapsed.TotalMilliseconds, buildSw.Elapsed.TotalMilliseconds, totalSw.Elapsed.TotalMilliseconds);

        // Bake for next time (fire and forget, don't block chart load)
        // Pre-compute all edge projections so they're included in the baked file.
        _ = Task.Run(async () =>
        {
            try
            {
                var edgeCache = ProjectedEdgeCache.For(parsedChart);
                foreach (var edge in parsedChart.Edges.Values)
                {
                    edgeCache.GetOrCompute(parsedChart, edge);
                }

                var bakedData = new BakedChartData
                {
                    Document = doc,
                    ProjectedEdgeCoords = edgeCache.Export()
                };

                var (path, duration, size) = await ChartBaker.BakeAsync(bakedData, chartPath, CancellationToken.None).ConfigureAwait(false);
                _logger.LogInformation(
                    "Baked {Chart}: {Size:N0} bytes ({Edges} edges) in {Duration}ms → {Path}",
                    entry.Name, size, bakedData.ProjectedEdgeCoords.Count, duration.TotalMilliseconds, path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to bake {Chart}", entry.Name);
            }
        }, CancellationToken.None);

        return parsedChart;
    }
}
