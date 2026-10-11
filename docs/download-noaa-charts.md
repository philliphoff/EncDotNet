# Download NOAA ENC cells

This page shows how to find U.S. ENC cells in the NOAA product catalog,
download one, and open it with `EncDotNet.S57`. NOAA publishes its cells free
of charge, so they're a convenient source of real data for development and
testing.

## Prerequisites

- A console app that references `EncDotNet.Noaa` and `EncDotNet.S57`:

  ```bash
  dotnet add package EncDotNet.Noaa
  dotnet add package EncDotNet.S57
  ```

- Internet access to `charts.noaa.gov`.

## Find cells in the catalog

<xref:EncDotNet.Noaa.EncProductCatalogClient> downloads and parses the
[NOAA ENC product catalog](https://www.charts.noaa.gov/ENCs/ENCProdCat.xml).
Each <xref:EncDotNet.Noaa.Cell> has a name, a compilation scale, edition and
update numbers, the states, regions and Coast Guard districts it covers, its
coverage polygons and the URL of its ZIP file.

```csharp
using EncDotNet.Noaa;

using var client = new EncProductCatalogClient();
EncProductCatalog catalog = await client.GetNoaaCatalogAsync();

Console.WriteLine($"{catalog.Cells.Count} cells, valid {catalog.Header.DateValid}");

IEnumerable<Cell> harbourCells = catalog.Cells
    .Where(c => c.States.StateList.Contains("MA") && c.ChartScale <= 25000)
    .OrderBy(c => c.Name);

foreach (Cell cell in harbourCells)
{
    Console.WriteLine($"{cell.Name}: {cell.LongName}, 1:{cell.ChartScale}, edition {cell.Edition} update {cell.UpdateNumber}");
}
```

The constructor takes an optional `HttpClient`. Pass one when your app
manages its own, for example from `IHttpClientFactory`.

## Download and open a cell

Each cell's ZIP file holds a single-cell exchange set: an `ENC_ROOT` folder
with `CATALOG.031`, the base cell and any update cells. Download it, extract
it, and read the cell with its updates applied:

```csharp
using System.IO.Compression;
using EncDotNet.Noaa;
using EncDotNet.S57.Charts;
using EncDotNet.S57.ExchangeSets;

using var client = new EncProductCatalogClient();
EncProductCatalog catalog = await client.GetNoaaCatalogAsync();
Cell cell = catalog.Cells.First(c => c.Name == "US5MA1BO");

using var http = new HttpClient();
string zipPath = Path.Combine(Path.GetTempPath(), $"{cell.Name}.zip");
string extractPath = Path.Combine(Path.GetTempPath(), cell.Name);

await File.WriteAllBytesAsync(zipPath, await http.GetByteArrayAsync(cell.ZipfileLocation));
ZipFile.ExtractToDirectory(zipPath, extractPath, overwriteFiles: true);

string encRoot = Path.Combine(extractPath, "ENC_ROOT");
S57Chart chart = S57ExchangeSetReader.Read(encRoot).ReadChart(encRoot);

Console.WriteLine($"{cell.Name}: {chart.AreaFeatures.Count} area features at 1:{chart.CompilationScale}");
```

Replace `US5MA1BO` with a cell name from the previous step.

## Download many cells

The repository's
[`scripts/DownloadCatalog.cs`](https://github.com/philliphoff/EncDotNet/blob/main/scripts/DownloadCatalog.cs)
and
[`scripts/ExpandCatalog.cs`](https://github.com/philliphoff/EncDotNet/blob/main/scripts/ExpandCatalog.cs)
download and extract every cell in the catalog. Run them from a clone of the
repository:

```bash
dotnet run scripts/DownloadCatalog.cs
dotnet run scripts/ExpandCatalog.cs
```

The scripts write to `.catalog/` and `.expanded/` at the repository root,
which Git ignores.

> [!IMPORTANT]
> NOAA ENC cells are for development and testing here. Don't commit them to
> a repository, and check NOAA's terms before you redistribute them.
