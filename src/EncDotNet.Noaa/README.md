# EncDotNet.Noaa

[![NuGet](https://img.shields.io/nuget/v/EncDotNet.Noaa)](https://www.nuget.org/packages/EncDotNet.Noaa)

A .NET 10 client for the **NOAA ENC product catalog** — discover and download U.S. electronic navigational chart data published by the [National Oceanic and Atmospheric Administration](https://www.charts.noaa.gov/).

## Features

- Query the NOAA ENC product catalog (XML-based)
- List all available U.S. ENC cells with metadata (name, scale, edition, update number)
- Access geographic coverage, Coast Guard district, region, and state information
- Get direct download URLs for chart ZIP files

## Installation

```shell
dotnet add package EncDotNet.Noaa
```

## Quick Start

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

### Filtering by Region or Scale

```csharp
// Find large-scale harbour charts for a specific area
var harbourCharts = catalog.Cells
    .Where(c => c.ChartScale <= 25000)
    .OrderBy(c => c.Name);

foreach (var cell in harbourCharts)
{
    Console.WriteLine($"{cell.Name} — 1:{cell.ChartScale}");
}
```

### Downloading Chart Data

```csharp
using var httpClient = new HttpClient();

var cell = catalog.Cells.First(c => c.Name == "US5CA11M");

if (cell.ZipfileLocation is not null)
{
    var bytes = await httpClient.GetByteArrayAsync(cell.ZipfileLocation);
    await File.WriteAllBytesAsync($"{cell.Name}.zip", bytes);
}
```

## Key Types

| Type | Description |
|---|---|
| `EncProductCatalogClient` | HTTP client for fetching the NOAA ENC product catalog |
| `EncProductCatalog` | The parsed catalog containing all available ENC cells |
| `Cell` | Metadata for a single ENC cell (name, scale, edition, coverage, download URL) |
| `Coverage` | Geographic coverage polygon for a cell |
| `Vertex` | A lat/lon vertex in a coverage polygon |
| `Panel` | Panel information for a cell |
| `CatalogHeader` | Catalog-level metadata (title, date) |
| `Regions` | Region classification data |
| `States` | U.S. state assignments for cells |
| `CoastGuardDistricts` | Coast Guard district assignments |

## Data Source

This package fetches data from the [NOAA ENC product catalog](https://www.charts.noaa.gov/ENCs/ENCProdCat.xml), which is a publicly available XML feed listing all official U.S. electronic navigational charts. The catalog includes cell metadata, edition/update tracking, geographic coverage, and download links.

## Related Packages

- [EncDotNet.S57](https://www.nuget.org/packages/EncDotNet.S57) — Parse the downloaded S-57 chart files
- [EncDotNet.Iso8211](https://www.nuget.org/packages/EncDotNet.Iso8211) — Low-level ISO 8211 binary parser

## License

MIT — see [LICENSE](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE) for details.
