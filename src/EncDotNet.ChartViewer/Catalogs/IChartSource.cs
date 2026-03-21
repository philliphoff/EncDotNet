using System.Threading;
using System.Threading.Tasks;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57.Charts;

namespace EncDotNet.ChartViewer.Catalogs;

public interface IChartSource
{
    Task<S57Chart> GetChartAsync(ChartIndexEntry entry, CancellationToken cancellationToken = default);
}

