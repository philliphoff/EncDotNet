# ENC.NET

[![CI](https://github.com/philliphoff/EncDotNet/actions/workflows/ci.yml/badge.svg)](https://github.com/philliphoff/EncDotNet/actions/workflows/ci.yml)

A .NET 10 library suite and desktop application for reading, parsing, and rendering [IHO S-57](https://iho.int/en/s-57-unc) Electronic Navigational Charts (ENCs).

EncDotNet implements:

- **ISO/IEC 8211** — the binary container format used by S-57 for data encoding
- **IHO S-57** — the International Hydrographic Organization's transfer standard for digital hydrographic data
- **Nautical chart visualization** — rendering ENC data on an interactive map

## Projects

| Project | Description |
|---|---|
| **EncDotNet.Iso8211** | Low-level ISO 8211 binary format parser (DDR, field definitions, subfield readers) |
| **EncDotNet.S57** | S-57 domain model — documents, features, vectors, catalogs, charts, and topology |
| **EncDotNet.Noaa** | NOAA ENC product catalog client for discovering and downloading chart data |
| **EncDotNet.ChartViewer** | Avalonia/Mapsui desktop viewer for rendering charts on an interactive map |

```
Iso8211  →  S57  →  ChartViewer
                 →  Noaa
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)

### Build

```shell
dotnet build
```

### Run Tests

```shell
dotnet test
```

### Launch the Chart Viewer

```shell
dotnet run --project src/EncDotNet.ChartViewer
```

## Library Usage

### Parsing ISO 8211 Files

`EncDotNet.Iso8211` provides a self-contained parser for the ISO/IEC 8211 binary container format. It can read any ISO 8211 file, not just S-57 charts.

```csharp
using EncDotNet.Iso8211;

// Parse a file
var document = Iso8211DocumentReader.ReadFromFile("chart.000");

// The first record is the Data Descriptive Record (DDR), which defines
// the schema (field tags, subfield names, and data types) for all
// subsequent data records.
var ddr = Iso8211DataDescriptiveRecordReader.Read(document.DataDescriptiveRecord!);

// Iterate data records
foreach (var record in document.DataRecords)
{
    foreach (var field in record.Fields)
    {
        Console.WriteLine($"Field tag: {field.Tag}  ({field.Data.Length} bytes)");

        // Use the DDR field definition to decode subfields
        var fieldDef = ddr.GetFieldDefinition(field.Tag);
        if (fieldDef is not null)
        {
            var reader = new Iso8211FieldReader(fieldDef, field.Data);

            foreach (var subfieldDef in fieldDef.SubfieldDefinitions)
            {
                if (reader.TryGetSubfield<string>(subfieldDef.Name, out var value))
                {
                    Console.WriteLine($"  {subfieldDef.Name} = {value}");
                }
            }
        }
    }
}
```

### Reading S-57 Charts

`EncDotNet.S57` builds on the ISO 8211 parser to provide a typed domain model for S-57 electronic navigational chart data.

```csharp
using EncDotNet.S57;

// Parse an S-57 chart file (.000)
var document = S57DocumentReader.ReadFromFile("US5CA11M.000");

// Access data set metadata
Console.WriteLine($"Dataset: {document.DataSetIdentification?.DataSetName}");
Console.WriteLine($"Scale:   1:{document.DataSetParameters?.CompilationScale}");

// Coordinate multiplication factor — divide raw integer coordinates
// by this value to get decimal degrees (typically 10,000,000)
int comf = document.CoordinateMultiplicationFactor;

// Query features by S-57 object code
var depthAreas = document.GetFeaturesByObjectCode(S57ObjectCode.DEPARE);

foreach (var feature in depthAreas)
{
    Console.WriteLine($"Depth Area {feature.RecordName} — {feature.Primitive}");

    // Feature attributes (e.g. DRVAL1, DRVAL2 for depth range)
    foreach (var attr in feature.Attributes)
    {
        Console.WriteLine($"  ATTL {attr.AttributeCode} = {attr.Value}");
    }

    // Follow spatial pointers to geometry
    foreach (var ptr in feature.SpatialPointers)
    {
        var vector = document.GetVectorRecord(ptr.Name);

        if (vector is not null)
        {
            foreach (var coord in vector.Coordinates2D)
            {
                double lon = coord.X / (double)comf;
                double lat = coord.Y / (double)comf;
                Console.WriteLine($"  ({lat:F6}, {lon:F6})");
            }
        }
    }
}
```

#### Higher-Level Chart Model

`S57Chart` provides indexed, strongly-typed access to features and spatial records:

```csharp
using EncDotNet.S57.Charts;

var chart = S57Chart.FromFile("US5CA11M.000");

// Typed feature collections
Console.WriteLine($"Point features: {chart.PointFeatures.Length}");
Console.WriteLine($"Line features:  {chart.LineFeatures.Length}");
Console.WriteLine($"Area features:  {chart.AreaFeatures.Length}");

// Spatial record dictionaries (keyed by record name)
Console.WriteLine($"Edges:          {chart.Edges.Count}");
Console.WriteLine($"Connected nodes:{chart.ConnectedNodes.Count}");
Console.WriteLine($"Isolated nodes: {chart.IsolatedNodes.Count}");
```

#### Applying Incremental Updates

S-57 supports incremental updates distributed as `.001`, `.002`, etc. files:

```csharp
var baseDoc = S57DocumentReader.ReadFromFile("chart.000");
var update  = S57DocumentReader.ReadFromFile("chart.001");

// Merge insert/delete/modify operations into the base document
var merged = baseDoc.ApplyChanges(update);
```

#### Reading an Exchange Set Catalog

Each S-57 exchange set contains a `CATALOG.031` file listing all chart files and their geographic bounds:

```csharp
using EncDotNet.S57.ExchangeSets;

var catalog = S57CatalogReader.ReadFromFile("ENC_ROOT/CATALOG.031");

foreach (var entry in catalog.Entries)
{
    Console.WriteLine($"{entry.FileName}");
    Console.WriteLine($"  Bounds: ({entry.SouthernmostLatitude}, {entry.WesternmostLongitude}) " +
                      $"to ({entry.NorthernmostLatitude}, {entry.EasternmostLongitude})");
}
```

### Downloading NOAA ENC Charts

`EncDotNet.Noaa` queries the [NOAA ENC product catalog](https://www.charts.noaa.gov/ENCs/ENCProdCat.xml) and can be used to discover and download chart data:

```csharp
using EncDotNet.Noaa;

using var client = new EncProductCatalogClient();
var catalog = await client.GetNoaaCatalogAsync();

Console.WriteLine($"Found {catalog.Cells.Count} charts");

foreach (var cell in catalog.Cells)
{
    Console.WriteLine($"{cell.Name} — Scale 1:{cell.ChartScale}");
    Console.WriteLine($"  Edition {cell.Edition}, Update {cell.UpdateNumber}");
    Console.WriteLine($"  Download: {cell.ZipfileLocation}");
}
```

## Scripts

The `scripts/` folder contains C# scripts for common operations. Run them with `dotnet run`:

| Script | Description |
|---|---|
| `DownloadCatalog.cs` | Download all NOAA ENC chart .zip files to `.catalog/` |
| `ExpandCatalog.cs` | Unzip downloaded charts to `.expanded/` |
| `ReadCatalog.cs` | Benchmark ISO 8211 parsing of all `.000` files |
| `ReadCatalogS57.cs` | Parse all `.000` files as S-57 with diagnostics |
| `DumpCatalog.cs` | Hex dump + parsed view of a `CATALOG.031` file |
| `DumpChart.cs` | Hex dump + parsed view of an ENC `.000` file |
| `BuildChartIndex.cs` | Generate a JSON chart index from all catalogs |

```shell
# Download all NOAA charts
dotnet run scripts/DownloadCatalog.cs

# Expand them
dotnet run scripts/ExpandCatalog.cs

# Dump a single chart
dotnet run scripts/DumpChart.cs .expanded/US5CA11M/ENC_ROOT/US5CA11M.000
```

## Architecture

### S-57 Topology

S-57 uses a **chain-node topology** model:

- **Features** reference spatial records via FSPT (Feature-to-Spatial) pointers
- **Area features** reference **Faces**, which are bounded by **Edges**
- **Edges** are polylines connecting two **Connected Nodes**, with interior coordinate points stored as SG2D fields
- **Isolated Nodes** represent standalone points or sounding clusters

### Coordinate Handling

Raw coordinates in S-57 files are stored as integers. Divide by the **Coordinate Multiplication Factor** (COMF, typically 10,000,000) from the DSPM record to get decimal degrees. Soundings are similarly divided by the **Sounding Multiplication Factor** (SOMF, typically 10).

### Immutable Data Model

All parsed data uses `ImmutableArray<T>` and `ImmutableDictionary<K, V>` for thread-safe, LINQ-friendly access. Records are immutable after construction.

## Standards References

- [IHO S-57 Edition 3.1](https://iho.int/en/s-57-unc) — Transfer Standard for Digital Hydrographic Data
- [ISO/IEC 8211](https://www.iso.org/standard/7688.html) — Specification for a data descriptive file
- [IHO S-52](https://iho.int/en/s-52-guidance) — Chart Content and Display Aspects of ECDIS

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
