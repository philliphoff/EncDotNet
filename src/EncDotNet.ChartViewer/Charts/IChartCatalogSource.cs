using System.Collections.Generic;
using System.Threading;
using EncDotNet.ChartViewer.Models;

namespace EncDotNet.ChartViewer.Charts;

public interface IChartCatalogSource : IChartSource
{
    IAsyncEnumerable<ChartIndexEntry> GetCatalogAsync(CancellationToken cancellationToken = default);
}
