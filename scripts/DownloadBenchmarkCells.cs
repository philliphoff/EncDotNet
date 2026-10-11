#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.Noaa/EncDotNet.Noaa.csproj

using System.IO.Compression;
using System.Security.Cryptography;
using EncDotNet.Noaa;

// ============================================================================
// DownloadBenchmarkCells.cs — Downloads a fixed set of real NOAA ENC cells for
// the benchmarks in tests/EncDotNet.Benchmarks.
//
// Usage: dotnet run scripts/DownloadBenchmarkCells.cs -- <output-dir> [cell...]
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

// A harbour, an approach and a coastal cell, all with update files. Together they
// cover the range of record counts and edge lengths in typical NOAA cells.
string[] defaultCells = ["US5WA50M", "US4MD81M", "US3WA01M"];

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run scripts/DownloadBenchmarkCells.cs -- <output-dir> [cell...]");
    return 1;
}

var outputDirectory = Path.GetFullPath(args[0]);
var cellNames = args.Length > 1 ? args[1..] : defaultCells;
Directory.CreateDirectory(outputDirectory);

using var catalogClient = new EncProductCatalogClient();
var catalog = await catalogClient.GetNoaaCatalogAsync();
var cellsByName = catalog.Cells.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

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
return missing == cellNames.Length ? 1 : 0;
