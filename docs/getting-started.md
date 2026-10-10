# Getting started

In this quickstart you add `EncDotNet.S57` to a console app, read an S-57 cell,
and list its depth areas with their depth ranges and positions.

## Prerequisites

- The [.NET SDK](https://dotnet.microsoft.com/download) 8 or later. The
  packages target .NET 8 and .NET 10.
- An S-57 base cell (a `.000` file). NOAA publishes U.S. cells for free; see
  [Download NOAA ENC cells](download-noaa-charts.md), or download one from the
  [NOAA ENC page](https://charts.noaa.gov/ENCs/ENCs.shtml).

## Create a project

```bash
dotnet new console -o EncQuickstart
cd EncQuickstart
dotnet add package EncDotNet.S57
```

`EncDotNet.S57` brings in `EncDotNet.Iso8211`, which decodes the ISO/IEC 8211
container that S-57 cells use.

## Read a cell

Replace the contents of `Program.cs`:

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");

Console.WriteLine($"Cell:    {document.DataSetIdentification?.DataSetName}");
Console.WriteLine($"Edition: {document.DataSetIdentification?.EditionNumber}");
Console.WriteLine($"Scale:   1:{document.DataSetParameters?.CompilationScale}");
Console.WriteLine($"{document.FeatureRecords.Count} feature records, {document.VectorRecords.Count} vector records");
```

Run it:

```bash
dotnet run
```

You see the cell name from its `DSID` field, the edition, the compilation
scale from `DSPM`, and the record counts.

## List the depth areas

Each feature record has an object class (`OBJL`), such as `DEPARE` for a depth
area, and a list of attributes. Attributes are identified by their numeric
code from the S-57 attribute catalogue: `DRVAL1` (87) and `DRVAL2` (88) hold the
shallow and deep limits of a depth area.

Add this to the end of `Program.cs`:

```csharp
const int Drval1 = 87;
const int Drval2 = 88;

foreach (S57FeatureRecord feature in document.GetFeaturesByObjectCode(S57ObjectCode.DEPARE))
{
    string? shallow = feature.Attributes.FirstOrDefault(a => a.AttributeCode == Drval1).Value;
    string? deep = feature.Attributes.FirstOrDefault(a => a.AttributeCode == Drval2).Value;

    Console.WriteLine($"{feature.RecordName}: {shallow} to {deep}");
}
```

Run it again. Each line names a depth area record and its depth range, in the
units the cell declares in `DSPM` (usually metres).

## Get positions

Coordinates are stored as integers. Divide them by the coordinate
multiplication factor (`COMF`), or let `ToDecimalDegrees` do it. A feature
points to its geometry through spatial pointers (`FSPT`). This prints the
first stored coordinate of each vector record the first five depth areas point
to:

```csharp
int comf = document.CoordinateMultiplicationFactor;

foreach (S57FeatureRecord feature in document.GetFeaturesByObjectCode(S57ObjectCode.DEPARE).Take(5))
{
    foreach (S57SpatialPointer pointer in feature.SpatialPointers)
    {
        S57VectorRecord? vector = document.GetVectorRecord(pointer.Name);
        if (vector is null || vector.Coordinates2D.Count == 0)
        {
            continue;
        }

        var (longitude, latitude) = vector.Coordinates2D[0].ToDecimalDegrees(comf);
        Console.WriteLine($"{feature.RecordName} -> {pointer.Name}: {latitude:F6}, {longitude:F6}");
    }
}
```

## Next steps

- [Reading S-57 charts](reading-s57-charts.md): the typed chart model, update
  cells, exchange sets and verification.
- [Reading and writing ISO 8211](reading-iso8211.md): the container format
  underneath, for S-57 and other ISO 8211 data.
- [Download NOAA ENC cells](download-noaa-charts.md): find and download U.S.
  cells from the NOAA product catalog.
- [Architecture](architecture.md): how the packages map to the standards.
- [S-57 glossary](glossary.md): the field and record acronyms used in the API.
