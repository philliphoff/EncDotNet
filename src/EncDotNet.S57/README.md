# EncDotNet.S57

[![NuGet](https://img.shields.io/nuget/v/EncDotNet.S57)](https://www.nuget.org/packages/EncDotNet.S57)

A .NET 10 library for reading and working with **IHO S-57 Electronic Navigational Charts (ENCs)** — the international standard for digital hydrographic data used in maritime navigation systems worldwide.

## Features

- Parse S-57 `.000` chart files into a strongly-typed domain model
- Access dataset metadata, feature records, vector/spatial records, and attributes
- Query features by S-57 object code (DEPARE, SOUNDG, LIGHTS, BOYLAT, etc.)
- Navigate chain-node topology: features → faces → edges → connected nodes
- Higher-level `S57Chart` model with indexed, typed feature and spatial record collections
- Apply incremental updates (`.001`, `.002`, …) to base charts
- Read exchange set catalogs (`CATALOG.031`) with geographic bounds
- Read full exchange sets with all referenced chart files
- Immutable data model using `ImmutableArray<T>` and `ImmutableDictionary<K,V>`

## Installation

```shell
dotnet add package EncDotNet.S57
```

This package depends on [EncDotNet.Iso8211](https://www.nuget.org/packages/EncDotNet.Iso8211) for ISO 8211 binary parsing (installed automatically).

## Quick Start

### Reading a Chart File

```csharp
using EncDotNet.S57;

// Parse an S-57 chart file
var document = S57DocumentReader.ReadFromFile("US5CA11M.000");

// Access dataset metadata
Console.WriteLine($"Dataset: {document.DataSetIdentification?.DataSetName}");
Console.WriteLine($"Scale:   1:{document.DataSetParameters?.CompilationScale}");

// Coordinate multiplication factor — divide raw integers by this
// to get decimal degrees (typically 10,000,000)
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

### Higher-Level Chart Model

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

### Applying Incremental Updates

S-57 supports incremental updates distributed as `.001`, `.002`, etc. files:

```csharp
var baseDoc = S57DocumentReader.ReadFromFile("chart.000");
var update  = S57DocumentReader.ReadFromFile("chart.001");

// Merge insert/delete/modify operations into the base document
var merged = baseDoc.ApplyChanges(update);
```

### Reading an Exchange Set Catalog

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

## Key Types

### Core (Document-Level)

| Type | Description |
|---|---|
| `S57Document` | A parsed S-57 file — metadata, feature records, and vector records |
| `S57DocumentReader` | Reads an S-57 `.000` file into an `S57Document` |
| `S57FeatureRecord` | A feature record with object code, primitive type, attributes, and spatial pointers |
| `S57VectorRecord` | A spatial/vector record with coordinates and topology pointers |
| `S57ObjectCode` | Enum of all S-57 object codes (DEPARE, SOUNDG, LIGHTS, etc.) |
| `S57GeometricPrimitive` | Point, Line, Area, or None |

### Charts (Higher-Level Model)

| Type | Description |
|---|---|
| `S57Chart` | Indexed chart model with typed feature and spatial record collections |
| `S57PointFeature` | A point feature with its isolated node location |
| `S57LineFeature` | A line feature with edge references |
| `S57AreaFeature` | An area feature with face/edge boundary topology |
| `S57Edge` | A polyline connecting two connected nodes |
| `S57ConnectedNode` | A node at the junction of edges |
| `S57IsolatedNode` | A standalone point or sounding cluster |

### Exchange Sets

| Type | Description |
|---|---|
| `S57Catalog` | Parsed `CATALOG.031` with entries and geographic bounds |
| `S57CatalogReader` | Reads a catalog file into an `S57Catalog` |
| `S57ExchangeSet` | A complete exchange set with catalog and all referenced charts |
| `S57ExchangeSetReader` | Reads an exchange set directory |

## Background

[IHO S-57](https://iho.int/en/s-57-unc) (Edition 3.1) is the International Hydrographic Organization's transfer standard for digital hydrographic data. It defines how nautical chart data — depth areas, soundings, buoys, lights, coastlines, and hundreds of other maritime features — is encoded into binary files using ISO/IEC 8211 as the container format.

S-57 uses a **chain-node topology** model where features reference spatial records (faces, edges, connected nodes, isolated nodes) to define their geometry. Raw coordinates are stored as integers and must be divided by the **Coordinate Multiplication Factor** (COMF) to get decimal degrees.

## Related Packages

- [EncDotNet.Iso8211](https://www.nuget.org/packages/EncDotNet.Iso8211) — The underlying ISO 8211 binary parser
- [EncDotNet.Noaa](https://www.nuget.org/packages/EncDotNet.Noaa) — NOAA ENC product catalog client for downloading chart data

## License

MIT — see [LICENSE](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE) for details.
