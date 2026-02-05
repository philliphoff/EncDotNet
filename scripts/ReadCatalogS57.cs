#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.Enc/EncDotNet.Enc.csproj

using System.Diagnostics;
using EncDotNet.Enc;
using EncDotNet.Enc.Charts;
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

int totalFeatures = 0;
int totalVectors = 0;
int totalFiles = 0;

var stopwatch = Stopwatch.StartNew();

foreach (var file in files)
{
    try
    {
        var document = S57Reader.ReadFromFile(file);
        var chart = S57Chart.FromDocument(document);
        totalFeatures += document.FeatureRecords.Length;
        totalVectors += document.VectorRecords.Length;
        totalFiles++;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        
        Console.WriteLine("=== ERROR ENCOUNTERED ===");
        Console.WriteLine($"File: {file}");
        Console.WriteLine($"Relative path: {Path.GetRelativePath(repoRoot, file)}");
        Console.WriteLine();
        Console.WriteLine($"Exception type: {ex.GetType().FullName}");
        Console.WriteLine($"Message: {ex.Message}");
        Console.WriteLine();
        Console.WriteLine("Stack trace:");
        Console.WriteLine(ex.StackTrace);
        Console.WriteLine();
        
        // Try to provide more diagnostic info by reading the raw ISO 8211 data
        try
        {
            Console.WriteLine("=== ISO 8211 Diagnostic Info ===");
            var iso8211Doc = Iso8211Reader.ReadFromFile(file);
            Console.WriteLine($"Total ISO 8211 records: {iso8211Doc.Records.Length}");
            
            if (iso8211Doc.Records.Length > 0)
            {
                Console.WriteLine();
                Console.WriteLine("First few records:");
                for (int i = 0; i < Math.Min(5, iso8211Doc.Records.Length); i++)
                {
                    var record = iso8211Doc.Records[i];
                    Console.WriteLine($"  Record {i}: {record.Fields.Length} fields");
                    foreach (var field in record.Fields)
                    {
                        Console.WriteLine($"    Field: {field.Tag}, Data length: {field.Data.Length} bytes");
                    }
                }
            }
        }
        catch (Exception isoEx)
        {
            Console.WriteLine($"Could not read ISO 8211 data: {isoEx.Message}");
        }
        
        Console.WriteLine();
        Console.WriteLine("=== File Content Preview (first 512 bytes as hex) ===");
        try
        {
            var bytes = File.ReadAllBytes(file);
            var previewLength = Math.Min(512, bytes.Length);
            Console.WriteLine($"File size: {bytes.Length} bytes");
            Console.WriteLine();
            
            for (int i = 0; i < previewLength; i += 16)
            {
                var lineBytes = bytes.Skip(i).Take(16).ToArray();
                var hex = string.Join(" ", lineBytes.Select(b => b.ToString("X2")));
                var ascii = new string(lineBytes.Select(b => b >= 32 && b < 127 ? (char)b : '.').ToArray());
                Console.WriteLine($"{i:X4}: {hex,-48} {ascii}");
            }
        }
        catch (Exception fileEx)
        {
            Console.WriteLine($"Could not read file: {fileEx.Message}");
        }
        
        Console.WriteLine();
        Console.WriteLine($"Files processed before error: {totalFiles}");
        Console.WriteLine($"Total features read: {totalFeatures}");
        Console.WriteLine($"Total vectors read: {totalVectors}");
        
        // Exit with error code
        Environment.Exit(1);
    }
}

stopwatch.Stop();

var elapsed = stopwatch.Elapsed;
var filesPerSecond = totalFiles / elapsed.TotalSeconds;

Console.WriteLine("=== Summary ===");
Console.WriteLine($"Files processed: {totalFiles}");
Console.WriteLine($"Total features read: {totalFeatures}");
Console.WriteLine($"Total vectors read: {totalVectors}");
Console.WriteLine($"Total time: {elapsed.TotalSeconds:F2} seconds");
Console.WriteLine($"Average: {filesPerSecond:F2} files/second");
