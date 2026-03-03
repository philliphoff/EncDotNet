using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57.ExchangeSets;
using EncDotNet.Noaa;

namespace EncDotNet.ChartViewer.Catalogs;

internal sealed class NoaaChartPackageManager : IChartPackageManager
{
    public async IAsyncEnumerable<ChartPackage> GetPackagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var client = new EncProductCatalogClient();
        var catalog = await client.GetNoaaCatalogAsync(cancellationToken);

        var installedStates = AppDataPaths.LoadDownloadedStates();

        var stateGroups = new Dictionary<string, (int Count, long Size)>();

        foreach (var cell in catalog.Cells)
        {
            var cellStates = cell.States?.StateList ?? [];
            var stateKeys = cellStates.Count > 0 ? cellStates : ["Other"];

            foreach (var state in stateKeys)
            {
                if (!stateGroups.TryGetValue(state, out var group))
                    group = (0, 0);

                stateGroups[state] = (group.Count + 1, group.Size + cell.ZipfileSize);
            }
        }

        foreach (var (state, (count, _)) in stateGroups.OrderBy(kv => kv.Key))
        {
            yield return new ChartPackage
            {
                PackageId = state,
                PackageName = state,
                ChartCount = count,
                IsInstalled = installedStates.Contains(state),
            };
        }
    }

    public async Task InstallPackagesAsync(
        IReadOnlySet<string> packageIds,
        IProgress<InstallationUpdate> progress,
        CancellationToken cancellationToken = default)
    {
        AppDataPaths.EnsureDirectories();

        using var catalogClient = new EncProductCatalogClient();
        var catalog = await catalogClient.GetNoaaCatalogAsync(cancellationToken);

        var previouslyInstalled = AppDataPaths.LoadDownloadedStates();
        var removedStates = previouslyInstalled.Except(packageIds).ToHashSet();
        var addedStates = packageIds.Except(previouslyInstalled).ToHashSet();

        // Phase 1: Remove cells belonging to deselected states
        if (removedStates.Count > 0)
        {
            var cellsToRemove = GetCellsForStates(catalog, removedStates);
            var cellsToKeep = new HashSet<string>(
                GetCellsForStates(catalog, packageIds).Select(c => c.Name));

            foreach (var cell in cellsToRemove)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (cellsToKeep.Contains(cell.Name))
                    continue;

                progress.Report(new InstallationUpdate { Message = $"Removing {cell.Name}..." });

                var zipUrl = cell.ZipfileLocation;
                var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
                var folderName = Path.GetFileNameWithoutExtension(fileName);
                var expandedDir = Path.Combine(AppDataPaths.ExpandedDirectory, folderName);

                if (Directory.Exists(expandedDir))
                    await Task.Run(() => Directory.Delete(expandedDir, recursive: true), cancellationToken);

                var zipPath = Path.Combine(AppDataPaths.CatalogDirectory, fileName);

                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            }
        }

        // Phase 2: Download & extract cells for newly-added states
        if (addedStates.Count > 0)
        {
            var cellsToAdd = GetCellsForStates(catalog, addedStates);

            var alreadyDownloadedCells = new HashSet<string>(
                GetCellsForStates(catalog, previouslyInstalled.Intersect(packageIds).ToHashSet())
                    .Select(c => c.Name));

            var newCells = cellsToAdd.Where(c => !alreadyDownloadedCells.Contains(c.Name)).ToList();

            using var httpClient = new HttpClient();
            int total = newCells.Count;
            int completed = 0;

            foreach (var cell in newCells)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var zipUrl = cell.ZipfileLocation;
                var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
                var outputPath = Path.Combine(AppDataPaths.CatalogDirectory, fileName);

                if (!File.Exists(outputPath))
                {
                    progress.Report(new InstallationUpdate
                    {
                        Message = $"Downloading {cell.Name} ({completed + 1} of {total})...",
                        ProgressPercentage = total > 0 ? (int)((double)completed / total * 60) : 0,
                    });

                    using var response = await httpClient.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                var folderName = Path.GetFileNameWithoutExtension(fileName);
                var expandedDir = Path.Combine(AppDataPaths.ExpandedDirectory, folderName);

                if (!Directory.Exists(expandedDir) && File.Exists(outputPath))
                {
                    progress.Report(new InstallationUpdate
                    {
                        Message = $"Extracting {cell.Name} ({completed + 1} of {total})...",
                        ProgressPercentage = total > 0 ? (int)((double)completed / total * 60) : 0,
                    });

                    await Task.Run(() => ZipFile.ExtractToDirectory(outputPath, expandedDir), cancellationToken);
                }

                completed++;
                progress.Report(new InstallationUpdate
                {
                    Message = $"Extracting {cell.Name} ({completed} of {total})...",
                    ProgressPercentage = total > 0 ? (int)((double)completed / total * 90) : 90,
                });
            }
        }

        // Phase 3: Rebuild chart index
        progress.Report(new InstallationUpdate
        {
            Message = "Building chart index...",
            ProgressPercentage = 90,
        });

        var noaaCatalog = catalog;
        var chartCount = await Task.Run(() => BuildChartIndex(noaaCatalog), cancellationToken);

        AppDataPaths.SaveDownloadedStates(packageIds);

        progress.Report(new InstallationUpdate
        {
            Message = $"Complete. {chartCount} charts prepared.",
            ProgressPercentage = 100,
        });
    }

    public async Task ReloadIndexAsync(
        IProgress<InstallationUpdate> progress,
        CancellationToken cancellationToken = default)
    {
        progress.Report(new InstallationUpdate
        {
            Message = "Fetching NOAA catalog...",
            ProgressPercentage = 10,
        });

        using var catalogClient = new EncProductCatalogClient();
        var catalog = await catalogClient.GetNoaaCatalogAsync(cancellationToken);

        progress.Report(new InstallationUpdate
        {
            Message = "Reloading chart index...",
            ProgressPercentage = 50,
        });

        var chartCount = await Task.Run(() => BuildChartIndex(catalog), cancellationToken);

        progress.Report(new InstallationUpdate
        {
            Message = $"Complete. {chartCount} charts prepared.",
            ProgressPercentage = 100,
        });
    }

    private static List<Cell> GetCellsForStates(EncProductCatalog catalog, IReadOnlyCollection<string> states)
    {
        var stateSet = states as IReadOnlySet<string> ?? states.ToHashSet();
        var cells = new List<Cell>();
        var seen = new HashSet<string>();

        foreach (var cell in catalog.Cells)
        {
            if (seen.Contains(cell.Name))
                continue;

            var cellStates = cell.States?.StateList ?? [];
            bool match = cellStates.Count == 0
                ? stateSet.Contains("Other")
                : cellStates.Any(stateSet.Contains);

            if (match)
            {
                cells.Add(cell);
                seen.Add(cell.Name);
            }
        }

        return cells;
    }

    private static int BuildChartIndex(EncProductCatalog noaaCatalog)
    {
        var expandedDir = AppDataPaths.ExpandedDirectory;
        var entries = new List<ChartIndexEntry>();

        // Build a lookup from cell name to NOAA long name for more descriptive chart titles
        var noaaLongNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in noaaCatalog.Cells)
        {
            if (!string.IsNullOrEmpty(cell.LongName))
            {
                noaaLongNames.TryAdd(cell.Name, cell.LongName);
            }
        }

        foreach (var subDir in Directory.EnumerateDirectories(expandedDir)
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            var catalogPath = Path.Combine(subDir, "ENC_ROOT", "CATALOG.031");

            if (!File.Exists(catalogPath))
                continue;

            try
            {
                var catalog = S57CatalogReader.ReadFromFile(catalogPath);
                var folderName = Path.GetFileName(subDir);

                foreach (var catEntry in catalog.Entries)
                {
                    if (!catEntry.FileName.EndsWith(".000", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var relativePath = Path.Combine(folderName, "ENC_ROOT", catEntry.FileName)
                        .Replace('\\', '/');

                    var chartId = Path.GetFileNameWithoutExtension(catEntry.FileName);

                    // Prefer NOAA catalog long name, then LFIL, then filename
                    string chartName;
                    if (noaaLongNames.TryGetValue(chartId, out var noaaName))
                        chartName = noaaName;
                    else if (!string.IsNullOrEmpty(catEntry.LongFileName))
                        chartName = catEntry.LongFileName;
                    else
                        chartName = chartId;

                    entries.Add(new ChartIndexEntry
                    {
                        Id = chartId,
                        Name = chartName,
                        Path = relativePath,
                        SouthLatitude = catEntry.SouthernmostLatitude,
                        WestLongitude = catEntry.WesternmostLongitude,
                        NorthLatitude = catEntry.NorthernmostLatitude,
                        EastLongitude = catEntry.EasternmostLongitude,
                    });
                }
            }
            catch
            {
                // Skip catalogs that fail to parse
            }
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };

        var json = JsonSerializer.Serialize(entries, options);
        File.WriteAllText(AppDataPaths.ChartIndexPath, json);

        return entries.Count;
    }
}
