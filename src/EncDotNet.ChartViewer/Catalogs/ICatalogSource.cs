using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncDotNet.ChartViewer.Models;
using EncDotNet.Enc.Charts;

namespace EncDotNet.ChartViewer.Catalogs;

public interface ICatalogSource
{
    IAsyncEnumerable<ChartIndexEntry> GetCatalogAsync(CancellationToken cancellationToken = default);

    Task<S57Chart> GetChartAsync(ChartIndexEntry entry, CancellationToken cancellationToken = default);
}
