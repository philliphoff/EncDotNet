# Reading AML data sets

NATO Additional Military Layers (AML) cells are S-57 data sets with their own
object catalogue. `EncDotNet.S57` reads them with the same
<xref:EncDotNet.S57.S57DocumentReader> and <xref:EncDotNet.S57.Charts.S57Chart>
it uses for ENC cells, so everything in
[Reading S-57 charts](reading-s57-charts.md) applies to them. This page covers
what's different about AML cells:

- <xref:EncDotNet.S57.S57ProductIdentity>, from
  `S57DataSetIdentification.Product`, says whether a cell is an AML cell, and
  which AML product and edition it is.
- <xref:EncDotNet.S57.Aml.AmlDataSetMetadata> parses the structured comment
  in an AML cell's data set identification field (`DSID`).
- <xref:EncDotNet.S57.S57ObjectCatalogue> gives the AML object class and
  attribute codes their acronyms, names and enumerated values.
- <xref:EncDotNet.S57.Aml.AmlDataSet> puts these together in one view over a
  chart.

## Detect an AML cell

The product specification code (`PRSP`) in `DSID` identifies the product.
Codes 51 to 57 are AML products. `Product.Kind` is `S57ProductKind.Aml` for
them, and `Product.AmlProduct` says which one:

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");
S57ProductIdentity? product = document.DataSetIdentification?.Product;

if (product?.Kind == S57ProductKind.Aml)
{
    Console.WriteLine($"AML {product.Value.AmlProduct} edition {product.Value.Edition}, update: {product.Value.IsUpdate}");
}
```

| `PRSP` | `AmlProduct` | Product |
|---|---|---|
| 51 | `MFF` | Maritime Foundation and Facilities |
| 52 | `RAL` | Routes, Areas and Limits |
| 53 | `LBO` | Large Bottom Objects |
| 54 | `CLB` | Contour Line Bathymetry |
| 55 | `ESB` | Environment, Seabed and Beach |
| 56 | `SBO` | Small Bottom Objects |
| 57 | `Integrated` | AML 3.0, every layer in one data set |

`Edition` is the product specification edition (`PRED`) as recorded, and
`Version` is the same edition parsed as a version number. `IsUpdate` comes
from the application profile (`PROF`), or from the file extension when the
profile doesn't say.

## Read the cell metadata

AML cells put their title, status and security classification in the `DSID`
comment (`COMT`) as `key=value` pairs separated by semicolons, for example
`title=Wrecks; IDO Status = Final; Protective Marking = UNCLASSIFIED`.
`AmlDataSetMetadata.TryParse` reads them:

```csharp
using EncDotNet.S57;
using EncDotNet.S57.Aml;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");

if (AmlDataSetMetadata.TryParse(document.DataSetIdentification?.Comment, out AmlDataSetMetadata? metadata))
{
    Console.WriteLine($"{metadata.Title}: {metadata.ProtectiveMarking}, {metadata.IdoStatus}, owned by {metadata.OwnerAuthority}");

    foreach ((string key, string value) in metadata.Entries)
    {
        Console.WriteLine($"  {key} = {value}");
    }
}
```

Keys are matched without regard to case or spaces. The well-known keys are
properties, which are `null` when the comment doesn't have them, and
`Entries` holds every pair, including keys `AmlDataSetMetadata` doesn't
recognise. `TryParse` returns `false` for a comment that isn't a list of
pairs, such as free text.

> [!IMPORTANT]
> AML data is classified. If your application shows AML data, display the
> cell's protective marking (`ProtectiveMarking`) prominently wherever the
> data is shown.

## Look up AML codes

An S-57 cell encodes object classes (`OBJL`) and attributes (`ATTL`) as
numbers. AML classes and attributes have codes from 20000, which aren't in
the IHO catalogue or in the `S57ObjectCode` enumeration. The AML catalogues
hold them, on top of the IHO entries that AML cells also use, such as
`C_ASSO` and `M_COVR`:

| Catalogue | Edition (`PRED` major number) |
|---|---|
| `S57ObjectCatalogue.Aml10` | 1 |
| `S57ObjectCatalogue.Aml21` | 2 |
| `S57ObjectCatalogue.Aml30` | 3 |

`S57Document.GetObjectCatalogue()` and `S57Chart.GetObjectCatalogue()` choose
the catalogue for a cell's product and edition. When the edition is missing
or unrecognised, the integrated product gets the AML 3.0 catalogue and the
other products get AML 2.1:

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");
S57ObjectCatalogue catalogue = document.GetObjectCatalogue();

foreach (S57FeatureRecord feature in document.FeatureRecords)
{
    S57ObjectClassDefinition? objectClass = catalogue.GetObjectClass(feature.ObjectCode);
    Console.WriteLine($"{objectClass?.Acronym ?? feature.ObjectCode.ToString()}: {objectClass?.Name}");

    foreach (S57AttributeValue value in feature.Attributes)
    {
        S57AttributeDefinition? attribute = catalogue.GetAttribute(value.AttributeCode);
        Console.WriteLine($"  {attribute?.Acronym ?? value.AttributeCode.ToString()} = {value.Value}");
    }
}
```

Acronyms of AML classes and attributes are lower case, such as `atsctl` and
`databa`, and acronym lookups are case-sensitive. The catalogues hold codes,
acronyms, names, attribute types and enumerated values, but not definitions.

## Use the AML data set view

`AmlDataSet` is a view over the `S57Chart` of an AML cell. It doesn't copy the
chart; it adds the cell's product, its metadata and its catalogue, and looks
codes up in that catalogue. Load one from a file or stream, or create one
from a chart or document you already have:

```csharp
using EncDotNet.S57.Aml;
using EncDotNet.S57.Charts;

AmlDataSet dataSet = AmlDataSet.FromFile("path/to/cell.000");

Console.WriteLine($"{dataSet.AmlProduct}, {dataSet.Catalogue.Name}, {dataSet.Metadata?.ProtectiveMarking}");

foreach (S57TypedFeature feature in dataSet.Chart.AllFeatures.Values)
{
    Console.WriteLine($"{dataSet.GetObjectClass(feature)?.Acronym ?? feature.ObjectCode.ToString()}");

    foreach (AmlAttributeValue attribute in dataSet.GetAttributes(feature))
    {
        Console.WriteLine($"  {attribute.Acronym ?? attribute.Code.ToString()} = {attribute.Value.Value}");
    }
}
```

`GetAttributes` and `GetNationalAttributes` pair each of a feature's `ATTF`
and `NATF` values with its `S57AttributeDefinition`, which is `null` for a
code the catalogue doesn't have. `GetObjectClass` and `GetAttribute` look up
a single feature or value.

`Metadata` is `null` when the cell's comment isn't structured. Creating an
`AmlDataSet` from a cell that isn't an AML cell throws `ArgumentException`.
To check first, use `TryCreate`:

```csharp
using EncDotNet.S57;
using EncDotNet.S57.Aml;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");

if (AmlDataSet.TryCreate(document, out AmlDataSet? dataSet))
{
    Console.WriteLine($"{dataSet.Identification.DataSetName} is an AML {dataSet.AmlProduct} cell");
}
```

AML update cells are read and applied like ENC update cells; see
[Apply update cells](reading-s57-charts.md#apply-update-cells). Create the
`AmlDataSet` from the document after you've applied them.

## Read arc edges

Some AML cells, such as Routes, Areas and Limits cells, give edges as arcs
rather than as lists of points. `S57Edge.IntermediatePoints` already holds
them as points, and `S57ArcDensifier` densifies them to a tolerance you
choose. See [Read arc edges](reading-s57-charts.md#read-arc-edges).
