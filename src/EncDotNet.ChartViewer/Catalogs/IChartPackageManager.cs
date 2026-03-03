using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EncDotNet.ChartViewer.Catalogs;

public sealed record ChartPackage
{
    public required string PackageId { get; init; }

    public required string PackageName { get; init; }

    public int ChartCount { get; init; }

    public bool IsInstalled { get; init; }
}

public sealed record InstallationUpdate
{
    public required string Message { get; init; }

    public int ProgressPercentage { get; init; }
}

public interface IChartPackageManager
{
    IAsyncEnumerable<ChartPackage> GetPackagesAsync(CancellationToken cancellationToken = default);

    Task InstallPackagesAsync(IReadOnlySet<string> packageIds, IProgress<InstallationUpdate> progress, CancellationToken cancellationToken = default);

    Task ReloadIndexAsync(IProgress<InstallationUpdate> progress, CancellationToken cancellationToken = default);
}
