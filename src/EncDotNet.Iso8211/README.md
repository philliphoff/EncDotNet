# EncDotNet.Iso8211

[![NuGet](https://img.shields.io/nuget/v/EncDotNet.Iso8211)](https://www.nuget.org/packages/EncDotNet.Iso8211)

A .NET 10 parser for the **ISO/IEC 8211** binary container format — the underlying encoding used by IHO S-57 electronic navigational charts, as well as other geospatial and data exchange standards.

## Features

- Parse any ISO 8211 file (not limited to S-57 charts)
- Read the Data Descriptive Record (DDR) to discover field tags, subfield names, and data types
- Decode data records and iterate their fields and subfield values
- Low-allocation `ref struct` reader for streaming large files
- Immutable record types for parsed data (thread-safe, LINQ-friendly)

## Installation

```shell
dotnet add package EncDotNet.Iso8211
```

## Quick Start

```csharp
using EncDotNet.Iso8211;

// Parse a file into an in-memory document
var document = Iso8211DocumentReader.ReadFromFile("chart.000");

// Read the DDR (schema) — defines field tags, subfield names, and data types
var ddr = Iso8211DataDescriptiveRecordReader.Read(document.DataDescriptiveRecord!);

// Iterate data records
foreach (var record in document.DataRecords)
{
    foreach (var field in record.Fields)
    {
        Console.WriteLine($"Field tag: {field.Tag}  ({field.Data.Length} bytes)");

        // Use the DDR field definition to decode subfields
        var fieldDef = ddr.GetFieldDefinition(field.Tag);
        if (fieldDef is not null)
        {
            var reader = new Iso8211FieldReader(fieldDef, field.Data);

            foreach (var subfieldDef in fieldDef.SubfieldDefinitions)
            {
                if (reader.TryGetSubfield<string>(subfieldDef.Name, out var value))
                {
                    Console.WriteLine($"  {subfieldDef.Name} = {value}");
                }
            }
        }
    }
}
```

## Key Types

| Type | Description |
|---|---|
| `Iso8211Document` | An in-memory ISO 8211 document containing the DDR and all data records |
| `Iso8211DocumentReader` | Reads an ISO 8211 file into an `Iso8211Document` |
| `Iso8211DataDescriptiveRecord` | The parsed DDR — field definitions and subfield schemas |
| `Iso8211DataDescriptiveRecordReader` | Parses the raw DDR record into a typed representation |
| `Iso8211FieldReader` | Decodes subfield values from a field's binary data using the DDR schema |
| `Iso8211Reader` | Low-level streaming `ref struct` reader for incremental parsing |
| `Iso8211Record` | A single data record with its leader, directory entries, and fields |
| `Iso8211FieldDefinition` | Defines a field's tag, structure code, data type, and subfield layout |
| `Iso8211SubfieldDefinition` | Defines a subfield's name and format (type, length) |

## Background

[ISO/IEC 8211](https://www.iso.org/standard/7688.html) is a general-purpose binary format for encoding structured data into self-describing files. Each file begins with a **Data Descriptive Record** (DDR) that defines the schema, followed by one or more **data records** conforming to that schema.

The format is used by IHO S-57 for encoding electronic navigational charts, but it is not specific to maritime data — any domain that needs a compact, self-describing binary container can use ISO 8211.

## Related Packages

- [EncDotNet.S57](https://www.nuget.org/packages/EncDotNet.S57) — S-57 domain model built on top of this parser

## License

MIT — see [LICENSE](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE) for details.
