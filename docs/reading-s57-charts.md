# Reading S-57 charts

`EncDotNet.S57` reads IHO S-57 Edition 3.1 cells, applies update cells, and
reads and verifies exchange sets. It has two models of a cell:

- <xref:EncDotNet.S57.S57Document>, read with
  <xref:EncDotNet.S57.S57DocumentReader>, maps the file's records directly:
  feature records, vector records and the dataset metadata.
- <xref:EncDotNet.S57.Charts.S57Chart> classifies those records into typed
  point, line, area and meta features and indexed nodes, edges and faces.

Start with `S57Chart` if you want geometry. Use `S57Document` if you want the
records as encoded, or to apply updates yourself.

## Read a cell

`S57DocumentReader` reads from a path, a byte array, a span or a stream, with
async overloads for files and streams. Each method takes an optional
`ILogger` that receives warnings about records it can't fully decode.

```csharp
using EncDotNet.S57;

S57Document document = await S57DocumentReader.ReadFromFileAsync("path/to/cell.000");

S57DataSetIdentification? dsid = document.DataSetIdentification;
Console.WriteLine($"{dsid?.DataSetName} edition {dsid?.EditionNumber}, update {dsid?.UpdateNumber}");
```

`DataSetIdentification` holds the `DSID` field (name, edition, update number,
producing agency, product specification) and `DataSetParameters` holds `DSPM`
(datums, compilation scale, units and multiplication factors).

## Find features

Each <xref:EncDotNet.S57.S57FeatureRecord> has an object class
(`ObjectCode`, from `OBJL`), a geometric primitive (point, line, area or
none), attributes, and pointers to its geometry and to related features.

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");

foreach (S57FeatureRecord light in document.GetFeaturesByObjectCode(S57ObjectCode.LIGHTS))
{
    Console.WriteLine($"{light.RecordName} ({light.Primitive})");

    foreach (S57AttributeValue attribute in light.Attributes)
    {
        Console.WriteLine($"  {attribute.AttributeCode} = {attribute.Value}");
    }
}
```

Attribute codes are the numeric `ATTL` values from S-57 Appendix A,
Chapter 2; for example `COLOUR` is 75 and `VALNMR` (nominal range) is 178.
Values are strings. Depending on the attribute's type they hold an
enumeration value, a comma-separated list, a number or free text. National
attributes (`NATF`) are in `NationalAttributes`.

## Convert coordinates

S-57 stores positions as integers. Divide by the coordinate multiplication
factor (`COMF`, usually 10,000,000) for decimal degrees, and depths by the
sounding multiplication factor (`SOMF`, usually 10).
<xref:EncDotNet.S57.S57Coordinate2D> and <xref:EncDotNet.S57.S57Sounding>
do the division for you:

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");
int comf = document.CoordinateMultiplicationFactor;
int somf = document.SoundingMultiplicationFactor;

foreach (S57VectorRecord vector in document.VectorRecords)
{
    foreach (S57Coordinate2D coordinate in vector.Coordinates2D)
    {
        var (longitude, latitude) = coordinate.ToDecimalDegrees(comf);
        Console.WriteLine($"{vector.RecordName}: {latitude:F6}, {longitude:F6}");
    }

    foreach (S57Sounding sounding in vector.Soundings)
    {
        var (longitude, latitude, depth) = sounding.ToDecimalValues(comf, somf);
        Console.WriteLine($"{vector.RecordName}: {latitude:F6}, {longitude:F6} depth {depth:F1}");
    }
}
```

## Use the typed chart model

`S57Chart` sorts features by primitive and spatial records by kind, and
indexes them by record name. It's built from a file, a stream or an existing
document:

```csharp
using EncDotNet.S57;
using EncDotNet.S57.Charts;

S57Chart chart = S57Chart.FromFile("path/to/cell.000");

Console.WriteLine($"Scale 1:{chart.CompilationScale}");
Console.WriteLine($"{chart.PointFeatures.Count} point, {chart.LineFeatures.Count} line, {chart.AreaFeatures.Count} area features");
Console.WriteLine($"{chart.IsolatedNodes.Count} isolated nodes, {chart.ConnectedNodes.Count} connected nodes, {chart.Edges.Count} edges, {chart.Faces.Count} faces");

if (chart.FeaturesByObjectCode.TryGetValue(S57ObjectCode.SOUNDG, out S57ObjectCodeFeatures? soundings))
{
    foreach (S57PointFeature feature in soundings.Points)
    {
        foreach (S57SpatialPointer pointer in feature.SpatialReferences)
        {
            S57IsolatedNode? node = chart.GetIsolatedNode(pointer.Name);
            Console.WriteLine($"{feature.RecordName}: {node?.Soundings.Count ?? 0} soundings");
        }
    }
}
```

Typed features expose `GetAttributeValue(int)` and `GetAttributeValues(int)`
for attribute lookups. Area features list their faces (`FaceReferences`) and,
for cells without full topology, their exterior and interior edges
(`ExteriorEdgeReferences`, `InteriorEdgeReferences`). The
[Architecture](architecture.md) page explains how features, edges, nodes and
faces relate.

## Apply update cells

A base cell (`.000`) is followed by update cells (`.001`, `.002`, and so on)
that insert, delete and modify records. Apply them in order with
<xref:EncDotNet.S57.S57Document.ApplyChanges(EncDotNet.S57.S57Document)>, which
returns a new document and leaves the original unchanged:

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");

foreach (string updatePath in new[] { "path/to/cell.001", "path/to/cell.002" })
{
    document = document.ApplyChanges(S57DocumentReader.ReadFromFile(updatePath));
}

Console.WriteLine($"Now at update {document.DataSetIdentification?.UpdateNumber}");
```

When the cell is in an exchange set, `S57ExchangeSet.ReadDocument` finds and
applies its updates for you; see the next section.

## Read an exchange set

An exchange set is a folder, usually named `ENC_ROOT`, with a `CATALOG.031`
file that lists every file in it.
<xref:EncDotNet.S57.ExchangeSets.S57ExchangeSetReader> finds the catalog, the
base cell and its update cells under that folder, without parsing them:

```csharp
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using EncDotNet.S57.ExchangeSets;

S57ExchangeSet exchangeSet = S57ExchangeSetReader.Read("path/to/ENC_ROOT");

Console.WriteLine($"Base cell: {exchangeSet.BaseCellFileName}");
Console.WriteLine($"Updates:   {string.Join(", ", exchangeSet.UpdateFileNames)}");

// The base cell with every update applied.
S57Chart chart = exchangeSet.ReadChart("path/to/ENC_ROOT");
```

> [!NOTE]
> `S57ExchangeSetReader` describes one cell: it uses the first `.000` file it
> finds under the folder. That matches the single-cell exchange sets NOAA
> distributes. For an exchange set with several cells, read `CATALOG.031` and
> open each cell with `S57DocumentReader`.

To list the catalog's entries and their coverage, read `CATALOG.031` with
<xref:EncDotNet.S57.ExchangeSets.S57CatalogReader>:

```csharp
using EncDotNet.S57.ExchangeSets;

S57Catalog catalog = S57CatalogReader.ReadFromFile("path/to/ENC_ROOT/CATALOG.031");

foreach (S57CatalogEntry entry in catalog.Entries)
{
    Console.WriteLine($"{entry.FileName}: {entry.SouthernmostLatitude}, {entry.WesternmostLongitude} to {entry.NorthernmostLatitude}, {entry.EasternmostLongitude}");
}
```

## Verify an exchange set

The catalog records a CRC-32 checksum for each file. `VerifyAsync` checks
every file against it and reports a result per file instead of throwing:

```csharp
using EncDotNet.S57.ExchangeSets;

S57ExchangeSet exchangeSet = S57ExchangeSetReader.Read("path/to/ENC_ROOT");
S57ExchangeSetVerificationResult result = await exchangeSet.VerifyAsync("path/to/ENC_ROOT");

foreach (S57FileVerificationResult file in result.FileResults)
{
    Console.WriteLine($"{file.FileName}: checksum {file.ChecksumOutcome}, signature {file.SignatureOutcome}");
}

Console.WriteLine(result.AllValid ? "All files verified." : "Verification failed.");
```

Checksum and signature outcomes are reported separately. Checksums are
verified in full. S-63 signature verification isn't implemented yet: the
`IS63SignatureVerifier` extension point exists, and files report
`NotSigned` until it does.
