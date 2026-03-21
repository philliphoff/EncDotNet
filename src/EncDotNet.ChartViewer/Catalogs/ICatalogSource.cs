using System.Collections.Generic;
using System.Threading;
using EncDotNet.ChartViewer.Models;

namespace EncDotNet.ChartViewer.Catalogs;

public interface ICatalogSource : IChartSource
{
    IAsyncEnumerable<ChartIndexEntry> GetCatalogAsync(CancellationToken cancellationToken = default);
}
