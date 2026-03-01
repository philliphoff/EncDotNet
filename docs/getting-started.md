# Getting Started

This guide shows how to install the EncDotNet libraries and use them to read ENC chart data at various levels of abstraction.

## Installation

Install the packages you need from NuGet:

```bash
# For ISO 8211 binary format parsing only
dotnet add package EncDotNet.Iso8211

# For S-57 chart parsing (includes EncDotNet.Iso8211 as a dependency)
dotnet add package EncDotNet.S57

# For downloading NOAA ENC product catalogs
dotnet add package EncDotNet.Noaa
```

## Library Overview

EncDotNet provides three layers of abstraction for working with ENC data:

| Layer | Package | Use When |
|-------|---------|----------|
| **Low-level tokens** | `EncDotNet.Iso8211` | You need maximum performance with minimal allocations, or want to process only specific fields |
| **Object model** | `EncDotNet.Iso8211` | You want a complete in-memory representation of the ISO 8211 structure |
| **S-57 domain model** | `EncDotNet.S57` | You want to work with nautical chart features, geometry, and topology |
| **NOAA catalog** | `EncDotNet.Noaa` | You need to discover and download NOAA ENC charts |

## Reading S-57 Charts (Recommended)

For most use cases, start with the S-57 layer. `S57DocumentReader` parses an ENC file into a strongly-typed domain model:

```csharp
using EncDotNet.S57;

// Read from a file
S57Document document = S57DocumentReader.ReadFromFile("US5VA51M.000");

// Or read asynchronously
S57Document document = await S57DocumentReader.ReadFromFileAsync("US5VA51M.000");

// Inspect dataset metadata
Console.WriteLine($"Dataset: {document.DataSetIdentification?.DataSetName}");
Console.WriteLine($"Scale: 1:{document.DataSetParameters?.CompilationScale}");

// Access features and vectors
Console.WriteLine($"Features: {document.FeatureRecords.Count}");
Console.WriteLine($"Vectors: {document.VectorRecords.Count}");
```

### Querying Features

Features are the primary objects on a chart — depth areas, buoys, lights, soundings, etc. Each feature has an `ObjectCode` identifying its type:

```csharp
using EncDotNet.S57;

var document = S57DocumentReader.ReadFromFile("US5VA51M.000");

// Find all depth area features
var depthAreas = document.GetFeaturesByObjectCode(S57ObjectCode.DEPARE);

foreach (var feature in depthAreas)
{
    Console.WriteLine($"Depth Area: OBJL={feature.ObjectCode}, Primitive={feature.Primitive}");

    // Read attributes (e.g., DRVAL1 = minimum depth, DRVAL2 = maximum depth)
    foreach (var attr in feature.Attributes)
    {
        Console.WriteLine($"  Attribute {attr.Code}: {attr.Value}");
    }
}

// Find all soundings
var soundings = document.GetFeaturesByObjectCode(S57ObjectCode.SOUNDG);
Console.WriteLine($"Sounding clusters: {soundings.Count()}");
```

### Working with Coordinates

Raw coordinates in S-57 are integers. Use the multiplication factors from the document to convert them:

```csharp
using EncDotNet.S57;

var document = S57DocumentReader.ReadFromFile("US5VA51M.000");
int comf = document.CoordinateMultiplicationFactor; // typically 10,000,000
int somf = document.SoundingMultiplicationFactor;   // typically 10

// Read 2D coordinates from a vector record
foreach (var vector in document.VectorRecords)
{
    foreach (var coord in vector.Coordinates2D)
    {
        var (lon, lat) = coord.ToDecimalDegrees(comf);
        Console.WriteLine($"  ({lat:F6}, {lon:F6})");
    }

    // Read 3D sounding coordinates
    foreach (var sounding in vector.Soundings)
    {
        var (lon, lat, depth) = sounding.ToDecimalValues(comf, somf);
        Console.WriteLine($"  ({lat:F6}, {lon:F6}) depth={depth:F1}m");
    }
}
```

## Using the Strongly-Typed Chart Model

`S57Chart` provides a higher-level view that categorizes features by geometry type and indexes spatial records:

```csharp
using EncDotNet.S57.Charts;

// Load directly from a file
var chart = S57Chart.FromFile("US5VA51M.000");

// Or from an existing document
var document = S57DocumentReader.ReadFromFile("US5VA51M.000");
var chart = S57Chart.FromDocument(document);

Console.WriteLine($"Scale: 1:{chart.CompilationScale}");
Console.WriteLine($"Point features: {chart.PointFeatures.Count}");
Console.WriteLine($"Line features: {chart.LineFeatures.Count}");
Console.WriteLine($"Area features: {chart.AreaFeatures.Count}");
Console.WriteLine($"Edges: {chart.Edges.Count}");
Console.WriteLine($"Isolated nodes: {chart.IsolatedNodes.Count}");
Console.WriteLine($"Connected nodes: {chart.ConnectedNodes.Count}");
```

## Reading ISO 8211 Data Directly

### Document Reader (Object Model)

If you need to inspect the raw ISO 8211 structure without S-57 interpretation, use `Iso8211DocumentReader`:

```csharp
using EncDotNet.Iso8211;

var document = Iso8211DocumentReader.ReadFromFile("US5VA51M.000");

Console.WriteLine($"Records: {document.Records.Length}");

foreach (var record in document.Records)
{
    Console.WriteLine($"Record: {record.Fields.Length} fields");

    foreach (var field in record.Fields)
    {
        Console.WriteLine($"  Tag: {field.Tag}, Data: {field.Data.Length} bytes");
    }
}
```

### Token Reader (High Performance)

For maximum throughput with minimal allocations, use `Iso8211Reader` — a forward-only, `ref struct` reader modeled after `System.Text.Json.Utf8JsonReader`:

```csharp
using EncDotNet.Iso8211;

byte[] data = File.ReadAllBytes("US5VA51M.000");
var reader = new Iso8211Reader(data);

while (reader.Read())
{
    switch (reader.TokenType)
    {
        case Iso8211TokenType.StartRecord:
            Console.WriteLine($"Record start ({reader.DirectoryEntryCount} fields)");
            reader.SkipDirectory(); // skip directory entries, go straight to fields
            break;

        case Iso8211TokenType.Field:
            if (reader.TryGetTagString(out string tag))
            {
                Console.WriteLine($"  Field: {tag} ({reader.CurrentLength} bytes)");
            }
            break;

        case Iso8211TokenType.EndRecord:
            Console.WriteLine("Record end");
            break;
    }
}
```

The token reader also supports **streaming** for processing large files incrementally:

```csharp
using EncDotNet.Iso8211;

using var stream = File.OpenRead("US5VA51M.000");
var buffer = new byte[4096];
var state = new Iso8211StreamingReaderState();
int bytesRead;

while ((bytesRead = stream.Read(buffer)) > 0)
{
    bool isFinalBlock = bytesRead < buffer.Length;
    var reader = new Iso8211Reader(buffer.AsSpan(0, bytesRead), isFinalBlock, state);

    while (reader.Read())
    {
        // Process tokens...
    }

    state = reader.GetStreamingState();
}
```

## Downloading NOAA Charts

Use `EncProductCatalogClient` to fetch the official NOAA ENC product catalog:

```csharp
using EncDotNet.Noaa;

using var client = new EncProductCatalogClient();
var catalog = await client.GetNoaaCatalogAsync();

Console.WriteLine($"Catalog valid: {catalog.Header.DateValid}");
Console.WriteLine($"Total cells: {catalog.Cells.Count}");

// Find charts for a specific state
var virginiaCharts = catalog.Cells
    .Where(c => c.States.StateList.Contains("VA"));

foreach (var cell in virginiaCharts)
{
    Console.WriteLine($"{cell.Name}: {cell.LongName} (1:{cell.ChartScale})");
    Console.WriteLine($"  Download: {cell.ZipfileLocation}");
}
```

## Next Steps

- Browse the [API Reference](../api/index.md) for complete type documentation
- See the [S-57 Glossary](glossary.md) for terminology used throughout the codebase
