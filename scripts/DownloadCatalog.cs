#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.ProductCatalog/EncDotNet.ProductCatalog.csproj

using EncDotNet.ProductCatalog;

// Parse command line arguments
bool forceRedownload = args.Contains("--force") || args.Contains("-f");

// Define the output folder at the root of the repo
// Use the script's source location to find the repo root
string scriptDir = Path.GetDirectoryName(GetScriptPath())!;
string repoRoot = Path.GetFullPath(Path.Combine(scriptDir, ".."));
string catalogFolder = Path.Combine(repoRoot, ".catalog");

static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

// Ensure the .catalog folder exists
Directory.CreateDirectory(catalogFolder);

Console.WriteLine("Downloading NOAA ENC Product Catalog...");
if (forceRedownload)
{
    Console.WriteLine("Force re-download enabled: all items will be downloaded.");
}

using var catalogClient = new EncProductCatalogClient();
using var httpClient = new HttpClient();

// Get the catalog
var catalog = await catalogClient.GetNoaaCatalogAsync();

Console.WriteLine($"Found {catalog.Cells.Count} charts in catalog.");
Console.WriteLine($"Downloading charts to: {catalogFolder}");

int downloaded = 0;
int skipped = 0;
int errors = 0;

for (int i = 0; i < catalog.Cells.Count; i++)
{
    var cell = catalog.Cells[i];
    var zipUrl = cell.ZipfileLocation;
    var fileName = Path.GetFileName(new Uri(zipUrl).LocalPath);
    var outputPath = Path.Combine(catalogFolder, fileName);

    // Skip if file already exists and force is not enabled
    if (!forceRedownload && File.Exists(outputPath))
    {
        skipped++;
        continue;
    }

    Console.WriteLine($"[{i + 1}/{catalog.Cells.Count}] Downloading {cell.Name} ({fileName})...");

    try
    {
        var zipData = await httpClient.GetByteArrayAsync(zipUrl);
        await File.WriteAllBytesAsync(outputPath, zipData);
        Console.WriteLine($"       Saved to: {outputPath} ({zipData.Length:N0} bytes)");
        downloaded++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"       Error: {ex.Message}");
        errors++;
    }
}

Console.WriteLine();
Console.WriteLine($"Done! Downloaded: {downloaded}, Skipped (cached): {skipped}, Errors: {errors}");
