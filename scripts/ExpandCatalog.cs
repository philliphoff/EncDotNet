#!/usr/bin/env dotnet run

using System.IO.Compression;

// Define the folders relative to the repo root
string scriptDir = Path.GetDirectoryName(GetScriptPath())!;
string repoRoot = Path.GetFullPath(Path.Combine(scriptDir, ".."));
string catalogFolder = Path.Combine(repoRoot, ".catalog");
string expandedFolder = Path.Combine(repoRoot, ".expanded");

static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

// Check if catalog folder exists
if (!Directory.Exists(catalogFolder))
{
    Console.WriteLine($"Error: Catalog folder not found: {catalogFolder}");
    Console.WriteLine("Run DownloadCatalog.cs first to download the chart files.");
    return;
}

// Ensure the .expanded folder exists
Directory.CreateDirectory(expandedFolder);

// Get all zip files in the catalog folder
var zipFiles = Directory.GetFiles(catalogFolder, "*.zip");

if (zipFiles.Length == 0)
{
    Console.WriteLine($"No .zip files found in: {catalogFolder}");
    return;
}

Console.WriteLine($"Found {zipFiles.Length} zip files in catalog.");
Console.WriteLine($"Expanding to: {expandedFolder}");
Console.WriteLine();

int expanded = 0;
int skipped = 0;
int errors = 0;

foreach (var zipFile in zipFiles)
{
    var fileName = Path.GetFileNameWithoutExtension(zipFile);
    var outputDir = Path.Combine(expandedFolder, fileName);

    // Skip if already expanded
    if (Directory.Exists(outputDir))
    {
        skipped++;
        continue;
    }

    Console.WriteLine($"Expanding {fileName}...");

    try
    {
        ZipFile.ExtractToDirectory(zipFile, outputDir);
        Console.WriteLine($"       Extracted to: {outputDir}");
        expanded++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"       Error: {ex.Message}");
        errors++;
    }
}

Console.WriteLine();
Console.WriteLine($"Done! Expanded: {expanded}, Skipped (existing): {skipped}, Errors: {errors}");
