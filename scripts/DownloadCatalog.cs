#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.ProductCatalog/EncDotNet.ProductCatalog.csproj

using EncDotNet.ProductCatalog;

// Define the output folder at the root of the repo
// Use the script's source location to find the repo root
string scriptDir = Path.GetDirectoryName(GetScriptPath())!;
string repoRoot = Path.GetFullPath(Path.Combine(scriptDir, ".."));
string catalogFolder = Path.Combine(repoRoot, ".catalog");

static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

// Ensure the .catalog folder exists
Directory.CreateDirectory(catalogFolder);

Console.WriteLine("Downloading NOAA ENC Product Catalog...");

using var catalogClient = new EncProductCatalogClient();
using var httpClient = new HttpClient();

// Get the catalog
var catalog = await catalogClient.GetNoaaCatalogAsync();

Console.WriteLine($"Found {catalog.Cells.Count} charts in catalog.");
Console.WriteLine($"Downloading first 5 charts to: {catalogFolder}");

// Download the first 5 charts
var chartsToDownload = catalog.Cells.Take(5).ToList();

for (int i = 0; i < chartsToDownload.Count; i++)
{
    var cell = chartsToDownload[i];
    var zipUrl = cell.ZipfileLocation;
    var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
    var outputPath = Path.Combine(catalogFolder, fileName);

    Console.WriteLine($"[{i + 1}/5] Downloading {cell.Name} ({fileName})...");

    try
    {
        var zipData = await httpClient.GetByteArrayAsync(zipUrl);
        await File.WriteAllBytesAsync(outputPath, zipData);
        Console.WriteLine($"       Saved to: {outputPath} ({zipData.Length:N0} bytes)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"       Error: {ex.Message}");
    }
}

Console.WriteLine("Done!");
