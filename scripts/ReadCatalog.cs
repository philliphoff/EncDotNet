#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.Iso8211/EncDotNet.Iso8211.csproj

using System.Diagnostics;
using EncDotNet.Iso8211;

// Define the folders relative to the repo root
string scriptDir = Path.GetDirectoryName(GetScriptPath())!;
string repoRoot = Path.GetFullPath(Path.Combine(scriptDir, ".."));
string expandedFolder = Path.Combine(repoRoot, ".expanded");

static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

// Check if expanded folder exists
if (!Directory.Exists(expandedFolder))
{
    Console.WriteLine($"Error: Expanded folder not found: {expandedFolder}");
    Console.WriteLine("Run ExpandCatalog.cs first to expand the chart files.");
    return;
}

// Find all .000 files recursively in the expanded folder
var files = Directory.GetFiles(expandedFolder, "*.000", SearchOption.AllDirectories);

if (files.Length == 0)
{
    Console.WriteLine($"No .000 files found in: {expandedFolder}");
    return;
}

Console.WriteLine($"Found {files.Length} .000 files in expanded folder.");
Console.WriteLine();

int totalRecords = 0;
int totalFiles = 0;
int errors = 0;

var stopwatch = Stopwatch.StartNew();

foreach (var file in files)
{
    try
    {
        var document = Iso8211Reader.ReadFromFile(file);
        totalRecords += document.Records.Length;
        totalFiles++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading {Path.GetFileName(file)}: {ex.Message}");
        errors++;
    }
}

stopwatch.Stop();

var elapsed = stopwatch.Elapsed;
var recordsPerSecond = totalRecords / elapsed.TotalSeconds;

Console.WriteLine("=== Summary ===");
Console.WriteLine($"Files processed: {totalFiles}");
Console.WriteLine($"Files with errors: {errors}");
Console.WriteLine($"Total records read: {totalRecords}");
Console.WriteLine($"Total time: {elapsed.TotalSeconds:F2} seconds");
Console.WriteLine($"Average: {recordsPerSecond:F2} records/second");
