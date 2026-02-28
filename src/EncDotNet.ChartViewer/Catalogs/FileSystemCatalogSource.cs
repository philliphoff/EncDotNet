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

namespace EncDotNet.ChartViewer.Catalogs;

internal sealed class FileSystemCatalogSource : ICatalogSource
{
    private readonly string _chartIndexPath;
    private readonly string _baseDirectory;

    public FileSystemCatalogSource(string chartIndexPath)
    {
        _chartIndexPath = chartIndexPath;
        _baseDirectory = Path.GetDirectoryName(chartIndexPath) ?? "";
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
        var exchangeSet = S57ExchangeSetReader.Read(chartDirectory);
        return await exchangeSet.ReadChartAsync(chartDirectory, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
