#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.Noaa/EncDotNet.Noaa.csproj

using System.IO.Compression;
using System.Security.Cryptography;
using EncDotNet.Noaa;

// ============================================================================
// DownloadBenchmarkCells.cs — Downloads a few real NOAA ENC cells for
// the benchmarks in tests/EncDotNet.Benchmarks.
//
// Usage: dotnet run scripts/DownloadBenchmarkCells.cs -- <output-dir> [cell...]
//
// Without cell names, the script picks one cell per usage band (harbour, approach
// and coastal) from the live NOAA catalog: among active cells with update files,
// the one at the 90th percentile of download size, so the set is substantial but
// typical. Picking by rule rather than by name survives NOAA withdrawing or
// renaming cells, which it does as it re-schemes its coverage.
//
// Each cell is extracted to <output-dir>/<cell>/ with its update files. Point
// ENCDOTNET_ENC_ROOT at <output-dir> to include them in a benchmark run. The
// script writes <output-dir>/cells.txt recording each cell's edition, update
// number and SHA-256, so a benchmark report can say exactly what it measured.
//
// NOAA publishes only the current edition of each cell, so the bytes change as
// NOAA issues updates. That's fine for comparing two builds in the same run,
// which read the same bytes. The exact budget tests use synthetic cells instead.
//
// Never commit the downloaded cells (see CONTRIBUTING.md).
// ============================================================================

// Navigational purpose (usage band), the third character of the cell file name
// (S-57 Appendix B.1, 5.6.4).
(char Band, string Purpose)[] bands = [('5', "harbour"), ('4', "approach"), ('3', "coastal")];
const double sizePercentile = 0.9;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run scripts/DownloadBenchmarkCells.cs -- <output-dir> [cell...]");
    return 1;
}

var outputDirectory = Path.GetFullPath(args[0]);
Directory.CreateDirectory(outputDirectory);

using var catalogClient = new EncProductCatalogClient();
var catalog = await catalogClient.GetNoaaCatalogAsync();
var cellsByName = catalog.Cells.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

string[] cellNames;
if (args.Length > 1)
{
    cellNames = args[1..];
}
else
{
    var picked = new List<string>();
    foreach (var (band, purpose) in bands)
    {
        var candidates = catalog.Cells
            .Where(c => c.Name.Length == 8 && c.Name[2] == band)
            .Where(c => c.UpdateNumber > 0 && c.ZipfileSize > 0)
            .Where(c => !c.Status.Contains("cancel", StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.ZipfileSize)
            .ThenBy(c => c.Name, StringComparer.Ordinal)
            .ToList();
        if (candidates.Count == 0)
        {
            Console.Error.WriteLine($"warning: no active {purpose} cell with updates in the NOAA catalog.");
            continue;
        }

        var cell = candidates[(int)((candidates.Count - 1) * sizePercentile)];
        Console.WriteLine($"Picked {cell.Name} as the {purpose} cell ({candidates.Count} candidates, zip {cell.ZipfileSize:N0} bytes).");
        picked.Add(cell.Name);
    }

    cellNames = [.. picked];
}

using var httpClient = new HttpClient();
var manifest = new List<string>();
var missing = 0;

foreach (var name in cellNames)
{
    if (!cellsByName.TryGetValue(name, out var cell))
    {
        Console.Error.WriteLine($"warning: {name} is not in the NOAA catalog; skipping it.");
        missing++;
        continue;
    }

    var zipBytes = await httpClient.GetByteArrayAsync(cell.ZipfileLocation);
    var cellDirectory = Path.Combine(outputDirectory, cell.Name);
    if (Directory.Exists(cellDirectory))
    {
        Directory.Delete(cellDirectory, recursive: true);
    }

    Directory.CreateDirectory(cellDirectory);

    // NOAA zips hold ENC_ROOT/<cell>/<cell>.000, .001 and so on. Flatten them so each
    // cell's files sit together in one folder.
    using (var archive = new ZipArchive(new MemoryStream(zipBytes)))
    {
        foreach (var entry in archive.Entries)
        {
            if (Path.GetFileNameWithoutExtension(entry.Name).Equals(cell.Name, StringComparison.OrdinalIgnoreCase)
                && Path.GetExtension(entry.Name).Length == 4
                && Path.GetExtension(entry.Name)[1..].All(char.IsAsciiDigit))
            {
                entry.ExtractToFile(Path.Combine(cellDirectory, entry.Name.ToUpperInvariant()), overwrite: true);
            }
        }
    }

    var baseFile = Path.Combine(cellDirectory, $"{cell.Name}.000");
    if (!File.Exists(baseFile))
    {
        Console.Error.WriteLine($"warning: {cell.Name}'s zip has no .000 file; skipping it.");
        Directory.Delete(cellDirectory, recursive: true);
        missing++;
        continue;
    }

    var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(baseFile)));
    var line = $"{cell.Name} edition {cell.Edition} update {cell.UpdateNumber} " +
        $"({new FileInfo(baseFile).Length:N0} bytes, sha256 {hash[..16]})";
    manifest.Add(line);
    Console.WriteLine(line);
}

File.WriteAllLines(Path.Combine(outputDirectory, "cells.txt"), manifest);
return cellNames.Length == 0 || missing == cellNames.Length ? 1 : 0;
