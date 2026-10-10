# EncDotNet.Iso8211

`EncDotNet.Iso8211` reads and writes ISO/IEC 8211 files, the self-describing
binary container used by IHO S-57 and S-101 charts and other data exchange
standards. It decodes the Data Descriptive Record (DDR), data records, fields
and subfield values, writes documents back out, and builds new ones. It has
no knowledge of S-57; reference
[`EncDotNet.S57`](https://www.nuget.org/packages/EncDotNet.S57) instead if
you want chart features. The package targets .NET 8 and .NET 10.

## Install

```bash
dotnet add package EncDotNet.Iso8211
```

## Example: list the fields a file defines

```csharp
using EncDotNet.Iso8211;

Iso8211Document document = Iso8211DocumentReader.ReadFromFile("path/to/file.000");
Iso8211DataDescriptiveRecord ddr = Iso8211DataDescriptiveRecordReader.Read(document.DataDescriptiveRecord!);

foreach (Iso8211FieldDefinition definition in ddr.FieldDefinitions)
{
    Console.WriteLine($"{definition.Tag}: {definition.FieldName} {definition.FormatControls}");
}

Console.WriteLine($"{document.DataRecords.Count()} data records");
```

## Main entry points

| Type | Use it to |
|---|---|
| `Iso8211DocumentReader` | Read a whole file into an `Iso8211Document` of records and fields |
| `Iso8211DataDescriptiveRecordReader` | Decode the DDR into field definitions |
| `Iso8211FieldReader` | Decode subfield values from a field using its definition |
| `Iso8211Reader` | Scan a file token by token without building records (`ref struct`) |
| `Iso8211DocumentWriter` | Write a document to bytes, a stream or a file |
| `Iso8211DocumentBuilder`, `Iso8211RecordBuilder`, `Iso8211FieldBuilder` | Build records and fields to write |
| `Iso8211DataDescriptiveRecordWriter` | Encode field definitions into a DDR |

[Reading and writing ISO 8211](../../docs/reading-iso8211.md) covers each of
these with examples.

## Records longer than 99,999 bytes

A record leader carries its record's length in five numeric characters, so it
can't express a record of 100,000 bytes or more. Producers that emit one, such
as a dense inland ENC cell whose `SG2D` coordinate field runs to six figures,
write `00000` there instead. The reader recognizes that and derives the length
from the record's directory (the field area base address plus the sum of the
field lengths), so `Iso8211Leader.RecordLength` reports the real length.

`Iso8211DocumentWriter` does the same in reverse: a record longer than 99,999
bytes is written with `00000` as its length, so such files round-trip byte for
byte. The field area base address has the same five-digit limit but can't be
left unspecified, because the reader needs it to find the directory. Building
or writing a record whose directory pushes the base address past 99,999
throws `InvalidOperationException`.

## Related packages

- [`EncDotNet.S57`](https://www.nuget.org/packages/EncDotNet.S57): the S-57
  chart model built on this package.

## License

MIT. See [LICENSE](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE).
