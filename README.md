# EncDotNet

[![CI](https://github.com/philliphoff/EncDotNet/actions/workflows/ci.yml/badge.svg)](https://github.com/philliphoff/EncDotNet/actions/workflows/ci.yml)
[![Discord](https://img.shields.io/discord/1516327073663156294?label=Discord&logo=discord&logoColor=white)](https://discord.gg/kf6B9EZqqB)

EncDotNet is a set of .NET libraries for
[IHO S-57](https://iho.int/en/s-57-unc) Electronic Navigational Charts (ENCs)
and the [ISO/IEC 8211](https://www.iso.org/standard/7688.html) files they're
encoded in. The libraries read and write ISO 8211, read S-57 cells, apply
update cells, verify exchange sets, and find cells in the NOAA product catalog.
They target .NET 8 and .NET 10.

[EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100) builds on
these libraries to read S-101 and to translate S-57 cells to S-101.

## Packages

| Package | Description |
|---|---|
| [`EncDotNet.Iso8211`](src/EncDotNet.Iso8211/README.md) | Reads, writes and builds ISO/IEC 8211 files: the DDR, records, fields and subfields |
| [`EncDotNet.S57`](src/EncDotNet.S57/README.md) | Reads S-57 cells into records and a typed chart model; applies updates; reads and verifies exchange sets |
| [`EncDotNet.Noaa`](src/EncDotNet.Noaa/README.md) | Finds and downloads U.S. cells from the NOAA ENC product catalog |

`EncDotNet.S57` depends on `EncDotNet.Iso8211`; `EncDotNet.Noaa` is
independent.

## Use the libraries

```bash
dotnet add package EncDotNet.S57
```

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");

foreach (S57FeatureRecord feature in document.GetFeaturesByObjectCode(S57ObjectCode.DEPARE))
{
    Console.WriteLine($"{feature.RecordName}: {feature.Attributes.Count} attributes");
}
```

The [documentation](docs/index.md) has a
[getting started](docs/getting-started.md) quickstart and guides to
[reading S-57 charts](docs/reading-s57-charts.md),
[reading and writing ISO 8211](docs/reading-iso8211.md) and
[downloading NOAA cells](docs/download-noaa-charts.md).

## Chart viewer

The repository also has a legacy desktop viewer, `EncDotNet.ChartViewer`, which
draws S-57 cells on a map. It's kept building but isn't being developed;
chart display work happens in EncDotNet.S100. To run it from a clone:

```bash
dotnet run --project src/EncDotNet.ChartViewer
```

## Build from source

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet build
dotnet test
```

## Scripts

The `scripts/` folder has file-based C# programs for working with NOAA data.
Run them with `dotnet run`:

| Script | Description |
|---|---|
| `DownloadCatalog.cs` | Download every NOAA ENC cell's ZIP file to `.catalog/` |
| `ExpandCatalog.cs` | Extract the downloaded ZIP files to `.expanded/` |
| `ReadCatalog.cs` | Time ISO 8211 parsing of every `.000` file |
| `ReadCatalogS57.cs` | Read every `.000` file as S-57 and report diagnostics |
| `DumpCatalog.cs` | Print a hex dump and parsed view of a `CATALOG.031` file |
| `DumpChart.cs` | Print a hex dump and parsed view of a `.000` file |
| `BuildChartIndex.cs` | Write a JSON index of the cells in every catalog |

```bash
dotnet run scripts/DownloadCatalog.cs
dotnet run scripts/ExpandCatalog.cs
dotnet run scripts/DumpChart.cs .expanded/US5CA11M/ENC_ROOT/US5CA11M/US5CA11M.000
```

## Standards

- [IHO S-57 Edition 3.1](https://iho.int/en/s-57-unc): transfer standard for
  digital hydrographic data.
- [ISO/IEC 8211](https://www.iso.org/standard/7688.html): specification for a
  data descriptive file for information interchange.
- [IHO S-52](https://iho.int/en/s-52-guidance): chart content and display
  aspects of ECDIS.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). For questions, see
[SUPPORT.md](SUPPORT.md); to report a vulnerability, see
[SECURITY.md](SECURITY.md).

## License

MIT. See [LICENSE](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE).
