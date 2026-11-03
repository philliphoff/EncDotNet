# Writing S-57 charts

`EncDotNet.S57` writes an `S57Document` as an S-57 Edition 3.1 base cell
(`.000`), and an `S57Catalog` as the `CATALOG.031` file of an exchange set.
The writers are the inverses of the readers in
[Reading S-57 charts](reading-s57-charts.md):
<xref:EncDotNet.S57.S57DocumentWriter> writes what
<xref:EncDotNet.S57.S57DocumentReader> reads, and
<xref:EncDotNet.S57.ExchangeSets.S57CatalogWriter> writes what
<xref:EncDotNet.S57.ExchangeSets.S57CatalogReader> reads. Use them to save a
modified cell, to package cells you've built or translated, or to check a
pipeline by writing a cell and reading it back.

## Write a cell

Pass the document to `S57DocumentWriter`. It writes to a byte array, a stream
or a file:

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");
S57DocumentWriter.WriteToFile("path/to/copy.000", document);
```

The copy reads back as an equal document. The writer checks the whole document
before it writes anything, so a document it can't encode throws
`ArgumentException` and leaves the file untouched. The message names the
record and subfield at fault.

The writer puts the records in a fixed order:

1. The data descriptive record (DDR)
2. `DSID` with `DSSI`
3. `DSPM`
4. The vector records
5. The feature records

Within each record, the fields follow the S-57 tree structures. Vector and
feature records keep the order they have in the document. `DSSI`'s record
counts are computed from the records.

The writer reads no clock, so the same document always produces the same
bytes. Dates such as the issue date (`ISDT`) are written as the document
holds them.

## Build a cell from scratch

Every type in the document model has public `init` properties, so you can
build a cell without reading one. A base cell needs a `DSID`, set as
`DataSetIdentification`, and a `DSPM`, set as `DataSetParameters`. Every
record must be an insert (`S57UpdateInstruction.Insert`) with no update
control fields:

```csharp
using EncDotNet.S57;

var document = new S57Document
{
    DataSetIdentification = new S57DataSetIdentification
    {
        RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.DataSetGeneralInfo, 1),
        ExchangePurpose = 1,
        IntendedUsage = 5,
        DataSetName = "XX5EXAMP.000",
        EditionNumber = "1",
        UpdateNumber = "0",
        IssueDate = "20260101",
        S57EditionNumber = "03.1",
        ProductSpecification = 1,
        ApplicationProfile = 1,
        ProducingAgency = 550,
        DataStructure = 2,
        AttfLexicalLevel = 1,
        NatfLexicalLevel = 2
    },
    DataSetParameters = new S57DataSetParameters
    {
        RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.DataSetGeoReference, 1),
        HorizontalDatum = 2,
        CompilationScale = 22000,
        DepthUnits = 1,
        HeightUnits = 1,
        PositionalUnits = 1,
        CoordinateUnits = 1,
        CoordinateMultiplicationFactor = 10_000_000,
        SoundingMultiplicationFactor = 10
    },
    VectorRecords =
    [
        new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, 1),
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = [new S57Coordinate2D { Y = 501_234_567, X = -12_345_678 }]
        }
    ],
    FeatureRecords =
    [
        new S57FeatureRecord
        {
            RecordName = new S57RecordName
            {
                RecordNameCode = S57RecordNameCodes.Feature,
                RecordId = 1,
                AgencyCode = 550,
                FeatureId = 1,
                FeatureSubdivision = 1
            },
            Primitive = S57GeometricPrimitive.Point,
            Group = 2,
            ObjectCode = S57ObjectCode.BOYLAT,
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Attributes = [new S57AttributeValue(116, "Mer du Nord 1")],
            SpatialPointers =
            [
                new S57SpatialPointer
                {
                    Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, 1),
                    Orientation = S57Orientation.NotApplicable,
                    Usage = S57UsageIndicator.NotApplicable,
                    Mask = S57MaskingIndicator.NotApplicable
                }
            ]
        }
    ]
};

byte[] cell = S57DocumentWriter.Write(document);
```

Coordinates and soundings are integers scaled by `COMF` and `SOMF`, as the
reader gives them. A feature's long name (`AGEN`, `FIDN`, `FIDS`) goes in its
`RecordName` and is written as its `FOID` field. A feature without one gets a
`FOID` of zeros.

## Choose lexical levels

`DSSI`'s `AALL` and `NALL` set how attribute values are encoded:

| Field | Level 0 | Level 1 | Level 2 |
|---|---|---|---|
| `ATTF` | ASCII | ISO 8859-1 (Latin-1) | Not allowed |
| `NATF` | ASCII | ISO 8859-1 (Latin-1) | UCS-2 |

The `COMT` comments of `DSID`, `DSPM` and `FFPT` are ISO 8859-1. S-57 makes
them ASCII, but some producers, such as NATO AML cells, put Latin-1 characters
like the degree sign in them, and ISO 8859-1 writes ASCII comments unchanged.
Every other text value is ASCII. A value that its level can't encode, such as
Greek in an `ATTF` value at level 1, throws `ArgumentException` rather than
being replaced. Put text like that in a national attribute with `NALL` set to
2. UCS-2 covers the Basic Multilingual Plane only, so characters outside it,
such as emoji, can't be written at any level.

The writer declares the levels in the DDR, as S-57 requires. It writes a
level 2 `NATF` field with two-byte terminators.

## Package an exchange set

An exchange set holds the cells and a `CATALOG.031` file that lists them
with their CRCs. Compute each cell's CRC with
`S57CatalogWriter.ComputeCrc`, then write the catalog:

```csharp
using EncDotNet.S57;
using EncDotNet.S57.ExchangeSets;

byte[] cell = S57DocumentWriter.Write(document);
Directory.CreateDirectory("path/to/ENC_ROOT/XX5EXAMP");
File.WriteAllBytes("path/to/ENC_ROOT/XX5EXAMP/XX5EXAMP.000", cell);

var catalog = new S57Catalog
{
    Entries =
    [
        new S57CatalogEntry { RecordId = 1, FileName = "CATALOG.031", Volume = "V01X01", Implementation = "ASC" },
        new S57CatalogEntry
        {
            RecordId = 2,
            FileName = "XX5EXAMP\\XX5EXAMP.000",
            Volume = "V01X01",
            Implementation = "BIN",
            SouthernmostLatitude = 50.1,
            WesternmostLongitude = -1.25,
            NorthernmostLatitude = 50.15,
            EasternmostLongitude = -1.2,
            CrcChecksum = S57CatalogWriter.ComputeCrc(cell)
        }
    ]
};

S57CatalogWriter.WriteToFile("path/to/ENC_ROOT/CATALOG.031", catalog);
```

File names in the catalog use `\` as the path separator. An entry with an
empty `RecordName` is written as `CD`. Bounds are written with six decimal
places when that holds them exactly, and an absent bound is written empty.
[Verify an exchange set](reading-s57-charts.md#verify-an-exchange-set) to
check the result.

## Limits

- **Base cells only.** The writer doesn't write update cells (`.001` and
  later): a record whose update instruction isn't `Insert`, or that has an
  update control field (`FSPC`, `FFPC`, `VRPC` or `SGCC`), throws
  `ArgumentException`.
- **No validation against S-58.** The writer encodes the document as it is.
  It doesn't check topology, mandatory attributes or other product
  specification rules.
- **Fields the model doesn't hold aren't written.** These include `DSPR`,
  `DSRC` and `ARCC`.
- **Text is trimmed.** The reader drops trailing spaces from text values, so
  a value that ended with spaces doesn't keep them.
