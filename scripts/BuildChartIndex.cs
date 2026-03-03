#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.S57/EncDotNet.S57.csproj
#:project ../src/EncDotNet.Noaa/EncDotNet.Noaa.csproj

using System.Text.Json;
using EncDotNet.Noaa;
using EncDotNet.S57.ExchangeSets;

// ============================================================================
// BuildChartIndex.cs — Scans an expanded ENC directory and generates a JSON
// chart index file from the CATALOG.031 files.
//
// Usage: dotnet run BuildChartIndex.cs <expanded-dir> [output-file]
//
// Each subfolder in <expanded-dir> is expected to contain an ENC_ROOT/CATALOG.031
// file. The script reads every catalog, extracts chart file entries with their
// geographic bounds, and writes a unified JSON index.
//
// If [output-file] is omitted, the index is written to <expanded-dir>/chart-index.json.
// ============================================================================

if (args.Length < 1)
{
    Console.WriteLine("Usage: dotnet run BuildChartIndex.cs <expanded-dir> [output-file]");
    return;
}

string expandedDir = args[0];

if (!Directory.Exists(expandedDir))
{
    Console.WriteLine($"Error: Directory not found: {expandedDir}");
    return;
}

string outputFile = args.Length >= 2
    ? args[1]
    : Path.Combine(expandedDir, "chart-index.json");

Console.WriteLine($"Scanning: {expandedDir}");
Console.WriteLine();

// Fetch NOAA catalog for more descriptive chart names
Console.WriteLine("Fetching NOAA ENC product catalog...");
var noaaLongNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
try
{
    using var catalogClient = new EncProductCatalogClient();
    var noaaCatalog = await catalogClient.GetNoaaCatalogAsync();
    foreach (var cell in noaaCatalog.Cells)
    {
        if (!string.IsNullOrEmpty(cell.LongName))
        {
            noaaLongNames.TryAdd(cell.Name, cell.LongName);
        }
    }
    Console.WriteLine($"  Loaded {noaaLongNames.Count} NOAA long names.");
}
catch (Exception ex)
{
    Console.WriteLine($"  Warning: Could not fetch NOAA catalog ({ex.Message}). Falling back to LFIL names.");
}
Console.WriteLine();

var entries = new List<ChartEntry>();
int scanned = 0;
int skipped = 0;
int errors = 0;

foreach (string subDir in Directory.EnumerateDirectories(expandedDir).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
{
    string catalogPath = Path.Combine(subDir, "ENC_ROOT", "CATALOG.031");

    if (!File.Exists(catalogPath))
    {
        skipped++;
        continue;
    }

    scanned++;

    try
    {
        var catalog = S57CatalogReader.ReadFromFile(catalogPath);
        string folderName = Path.GetFileName(subDir);

        foreach (var catEntry in catalog.Entries)
        {
            // Only include actual chart data files (.000)
            if (!catEntry.FileName.EndsWith(".000", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Derive the relative path from the FILE field: <folder>/ENC_ROOT/<file>
            string relativePath = Path.Combine(folderName, "ENC_ROOT", catEntry.FileName);

            // Use the chart name from the file name (without extension)
            string chartId = Path.GetFileNameWithoutExtension(catEntry.FileName);

            // Prefer NOAA catalog long name, then LFIL, then filename
            string chartName;
            if (noaaLongNames.TryGetValue(chartId, out var noaaName))
                chartName = noaaName;
            else if (!string.IsNullOrEmpty(catEntry.LongFileName))
                chartName = catEntry.LongFileName;
            else
                chartName = chartId;

            entries.Add(new ChartEntry
            {
                Id = chartId,
                Name = chartName,
                Path = relativePath.Replace('\\', '/'),
                SouthLatitude = catEntry.SouthernmostLatitude,
                WestLongitude = catEntry.WesternmostLongitude,
                NorthLatitude = catEntry.NorthernmostLatitude,
                EastLongitude = catEntry.EasternmostLongitude,
            });
        }
    }
    catch (Exception ex)
    {
        errors++;
        Console.WriteLine($"  ERROR: {Path.GetFileName(subDir)}: {ex.Message}");
    }
}

// Write JSON
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
};

string json = JsonSerializer.Serialize(entries, options);
File.WriteAllText(outputFile, json);

Console.WriteLine($"Catalogs scanned : {scanned}");
Console.WriteLine($"Catalogs skipped : {skipped} (no CATALOG.031)");
Console.WriteLine($"Errors           : {errors}");
Console.WriteLine($"Chart entries    : {entries.Count}");
Console.WriteLine($"Output           : {outputFile}");

// ============================================================================
// Internal model matching the chart viewer's ChartIndexEntry shape
// ============================================================================

class ChartEntry
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public double? SouthLatitude { get; init; }
    public double? WestLongitude { get; init; }
    public double? NorthLatitude { get; init; }
    public double? EastLongitude { get; init; }
}
