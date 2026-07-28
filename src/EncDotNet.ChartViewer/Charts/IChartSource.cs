using EncDotNet.ChartViewer.Models;
using EncDotNet.S57.Charts;

namespace EncDotNet.ChartViewer.Charts;

public interface IChartSource
{
    Task<S57Chart> GetChartAsync(ChartIndexEntry entry, CancellationToken cancellationToken = default);
}

