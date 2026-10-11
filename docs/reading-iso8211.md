# Reading and writing ISO 8211

`EncDotNet.Iso8211` reads and writes ISO/IEC 8211 files, the self-describing
binary container that S-57 cells use. It knows nothing about S-57, so you can
use it for any ISO 8211 data. Use `EncDotNet.S57` instead when you want chart
features; see [Reading S-57 charts](reading-s57-charts.md).

There are two ways to read:

- <xref:EncDotNet.Iso8211.Iso8211DocumentReader> reads a whole file into an
  <xref:EncDotNet.Iso8211.Iso8211Document> of records and fields.
- <xref:EncDotNet.Iso8211.Iso8211Reader> is a forward-only `ref struct` that
  returns one token at a time, without building records. Use it to scan large
  files or pick out a few fields.

Install the package with:

```bash
dotnet add package EncDotNet.Iso8211
```

## File structure

An ISO 8211 file starts with a Data Descriptive Record (DDR). The DDR defines
each field that the data records use: its tag, name, subfield names and
subfield formats. Every record, including the DDR, starts with a 24-byte
leader and a directory that gives each field's tag, length and position.

| Format code | Meaning |
|---|---|
| `A`, `A(n)` | Character data, delimited or `n` characters wide |
| `I(n)`, `R(n)` | Integer or real number written as characters |
| `B(n)` | Bit string, `n` bits wide |
| `b1x`, `b2x` | Unsigned or signed little-endian integer, `x` bytes wide |
| `b4x`, `b5x` | IEEE 754 floating-point number, `x` bytes wide |

## Read a file into a document

`Iso8211DocumentReader` returns every record. The first is the DDR; decode it
with <xref:EncDotNet.Iso8211.Iso8211DataDescriptiveRecordReader>, then use its
field definitions to decode subfields with
<xref:EncDotNet.Iso8211.Iso8211FieldReader>:

```csharp
using EncDotNet.Iso8211;

Iso8211Document document = Iso8211DocumentReader.ReadFromFile("path/to/file.000");
Iso8211DataDescriptiveRecord ddr = Iso8211DataDescriptiveRecordReader.Read(document.DataDescriptiveRecord!);

foreach (Iso8211FieldDefinition definition in ddr.FieldDefinitions)
{
    Console.WriteLine($"{definition.Tag}: {definition.FieldName} {definition.FormatControls}");
}

foreach (Iso8211Record record in document.DataRecords.Take(3))
{
    foreach (Iso8211Field field in record.Fields)
    {
        Iso8211FieldDefinition? definition = ddr.GetFieldDefinition(field.Tag);
        if (definition is null)
        {
            continue;
        }

        var reader = new Iso8211FieldReader(definition, field.Data);
        foreach (Iso8211SubfieldDefinition subfield in definition.SubfieldDefinitions)
        {
            if (reader.TryGetSubfield(subfield.Name, out string? value))
            {
                Console.WriteLine($"  {field.Tag}.{subfield.Name} = {value}");
            }
        }
    }
}
```

`GetSubfield<T>` and `TryGetSubfield<T>` convert the value to `T`, such as
`int`, `double` or `string`. For fields with a repeating group, such as a list
of coordinates, `GetSubfieldValues<T>` returns every value of one subfield and
`GetSubfieldGroups` returns each group.

To read a long repeating group, such as the thousands of coordinates in an
edge, look up each subfield's ordinal once with `GetOrdinal`, then read each
repetition by group index and ordinal. This avoids a name lookup and a group
object for every repetition:

```csharp
var reader = new Iso8211FieldReader(ddr.GetFieldDefinition("SG2D")!, field.Data);
int y = reader.GetOrdinal("YCOO");
int x = reader.GetOrdinal("XCOO");

var points = new List<(int Y, int X)>(reader.GroupCount);
for (int group = 0; group < reader.GroupCount; group++)
{
    points.Add((reader.GetSubfield<int>(group, y), reader.GetSubfield<int>(group, x)));
}
```

`GroupCount` counts complete repetitions only. If the field data ends partway
through one, `GetSubfieldGroups` still returns that partial group, and
`TryGetSubfield<T>(group, ordinal, out value)` returns `false` for the
subfields it lacks.

Character data is read as UTF-8 by default. Pass `lexicalLevel: 2` to the
`Iso8211FieldReader` constructor for fields encoded as UCS-2 (UTF-16LE).

## Read a large file record by record

`Iso8211DocumentReader.EnumerateRecords` returns one record at a time instead
of building the whole document, so memory use doesn't grow with the size of the
file. Each record can be released once you have processed it:

```csharp
using EncDotNet.Iso8211;

byte[] data = File.ReadAllBytes("path/to/file.000");
Iso8211DataDescriptiveRecord? ddr = null;
int fieldCount = 0;

foreach (Iso8211Record record in Iso8211DocumentReader.EnumerateRecords(data))
{
    if (record.IsDataDescriptiveRecord)
    {
        ddr = Iso8211DataDescriptiveRecordReader.Read(record);
        continue;
    }

    fieldCount += record.Fields.Count;
}
```

Each field's `Data` is a `ReadOnlyMemory<byte>` slice of the data read, not a
copy. `EnumerateRecords` and `Read(ReadOnlyMemory<byte>)` slice the memory you
pass, so don't modify it while you use the records. The overloads that take a
`byte[]`, a span, a file or a stream read from a private copy, so you can reuse
your buffer straight away.

## Scan a file token by token

`Iso8211Reader` works like `System.Text.Json.Utf8JsonReader`: call `Read()`
until it returns `false`, and check `TokenType` after each call.

```csharp
using EncDotNet.Iso8211;

byte[] data = File.ReadAllBytes("path/to/file.000");
var reader = new Iso8211Reader(data);
var tagCounts = new Dictionary<string, int>();

while (reader.Read())
{
    switch (reader.TokenType)
    {
        case Iso8211TokenType.StartRecord:
            reader.SkipDirectory();
            break;

        case Iso8211TokenType.Field:
            string tag = reader.GetTagString();
            tagCounts[tag] = tagCounts.GetValueOrDefault(tag) + 1;
            break;
    }
}

foreach (var (tag, count) in tagCounts.OrderBy(pair => pair.Key))
{
    Console.WriteLine($"{tag}: {count}");
}
```

While the reader is on a field, `ValueSpan` holds the field's bytes and
`GetValueString()` decodes them as text. `SkipRecord()` moves to the next
record.

The reader can also take input in blocks: construct it with
`isFinalBlock: false` and pass the state from `GetCurrentState()` to the next
reader along with the bytes the previous one didn't consume. Most callers read
whole files, which avoids managing that state.

## Write a document

<xref:EncDotNet.Iso8211.Iso8211DocumentWriter> writes an `Iso8211Document`
to a byte array, stream or file. A document read from a file writes back
byte for byte when the source is canonically encoded:

```csharp
using EncDotNet.Iso8211;

Iso8211Document document = Iso8211DocumentReader.ReadFromFile("path/to/file.000");
Iso8211DocumentWriter.WriteToFile("path/to/copy.000", document);
```

Records longer than 99,999 bytes can't state their length in the five-digit
leader field. The writer writes `00000` there, as producers do, and the
reader derives the real length from the directory.

## Build a file from scratch

Describe each field with an <xref:EncDotNet.Iso8211.Iso8211FieldDefinition>,
encode the definitions into a DDR, then build data records whose fields follow
those definitions:

```csharp
using EncDotNet.Iso8211;

var identifier = new Iso8211FieldDefinition
{
    Tag = "IDNT",
    FieldName = "Identifier",
    DataStructureCode = Iso8211DataStructureCode.Vector,
    DataTypeCode = Iso8211DataTypeCode.MixedDataTypes,
    FormatControls = "(A(2),b14)",
    SubfieldDefinitions =
    [
        new Iso8211SubfieldDefinition
        {
            Name = "AGEN",
            Index = 0,
            Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = 2 },
        },
        new Iso8211SubfieldDefinition
        {
            Name = "NUMB",
            Index = 1,
            Format = new Iso8211SubfieldFormat { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = 4 },
        },
    ],
};

Iso8211Record ddr = Iso8211DataDescriptiveRecordWriter.BuildDdr([identifier]);

Iso8211Record record = new Iso8211RecordBuilder()
    .AddField(new Iso8211FieldBuilder(identifier).AddSubfields("US", 42u))
    .Build();

Iso8211Document document = new Iso8211DocumentBuilder()
    .AddRecord(ddr)
    .AddRecord(record)
    .Build();

byte[] bytes = Iso8211DocumentWriter.Write(document);
```

Build the DDR and the data fields from the same definitions, so the data
matches the schema the DDR declares.
