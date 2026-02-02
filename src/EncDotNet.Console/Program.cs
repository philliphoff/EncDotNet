// Using the client

using EncDotNet.ProductCatalog;

using var client = new EncProductCatalogClient();

// Fetch from NOAA's default endpoint
var catalog = await client.GetNoaaCatalogAsync();

Console.WriteLine($"Catalog: {catalog.Header.Title}");
Console.WriteLine($"Valid: {catalog.Header.DtValid}");
Console.WriteLine($"Cells: {catalog.Cells.Count}");

foreach (var cell in catalog.Cells)
{
    Console.WriteLine($"  {cell.Name}: {cell.LongName} ({cell.Status})");
}
