# EncDotNet.Noaa

`EncDotNet.Noaa` downloads and parses the
[NOAA ENC product catalog](https://www.charts.noaa.gov/ENCs/ENCProdCat.xml),
the public list of official U.S. Electronic Navigational Chart cells. For each
cell it gives you the name, compilation scale, edition and update numbers,
coverage, the states, regions and Coast Guard districts it covers, and the
URL of its ZIP file. Reference it to find and download U.S. cells; read the
downloaded cells with
[`EncDotNet.S57`](https://www.nuget.org/packages/EncDotNet.S57). The package
targets .NET 8 and .NET 10.

## Install

```bash
dotnet add package EncDotNet.Noaa
```

## Example: list harbour-scale cells for a state

```csharp
using EncDotNet.Noaa;

using var client = new EncProductCatalogClient();
EncProductCatalog catalog = await client.GetNoaaCatalogAsync();

foreach (Cell cell in catalog.Cells.Where(c => c.States.StateList.Contains("MA") && c.ChartScale <= 25000))
{
    Console.WriteLine($"{cell.Name}: 1:{cell.ChartScale}, edition {cell.Edition} update {cell.UpdateNumber}, {cell.ZipfileLocation}");
}
```

## Main entry points

| Type | Use it to |
|---|---|
| `EncProductCatalogClient` | Download and parse the catalog; pass your own `HttpClient` if you have one |
| `EncProductCatalog` | Get the catalog header and its cells |
| `Cell` | Get one cell's metadata and download URL |
| `Coverage`, `Panel`, `Vertex` | Get a cell's coverage polygons |
| `States`, `Regions`, `CoastGuardDistricts` | Get the areas a cell is assigned to |
| `CatalogHeader` | Get the catalog's title and validity date |

[Download NOAA ENC cells](../../docs/download-noaa-charts.md) shows how to
download a cell and open it.

## Related packages

- [`EncDotNet.S57`](https://www.nuget.org/packages/EncDotNet.S57): reads the
  downloaded cells.

## License

MIT. See [LICENSE](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE).
