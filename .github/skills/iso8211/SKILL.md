---
name: iso8211
description: |
  Expert knowledge of the ISO/IEC 8211 data descriptive file format and the
  EncDotNet.Iso8211 package that reads and writes it: record leaders,
  directories, the Data Descriptive Record (DDR), field controls, subfield
  format codes, repeating groups, delimiters, lexical levels, and records
  longer than 99,999 bytes. USE FOR: changes to src/EncDotNet.Iso8211/**,
  Iso8211Reader, Iso8211DocumentReader, Iso8211DataDescriptiveRecordReader,
  Iso8211FieldReader, Iso8211SubfieldFormat, the writer/builder APIs
  (Iso8211DocumentWriter, Iso8211DocumentBuilder, Iso8211SubfieldEncoder),
  format-control parsing bugs, binary subfield decoding, and reviewing any
  public API change in the package. DO NOT USE FOR: S-57 meaning of fields
  and subfields such as DSID, FRID or VRPT (use s57-enc).
---

# ISO/IEC 8211 expert

## When engaged

- Tasks touching `src/EncDotNet.Iso8211/**` or the `Iso8211*Tests.cs` files
  under `tests/`.
- Parsing failures on real cells that trace back to the leader, directory,
  DDR or a subfield format.
- Any change to the package's public API.

## Downstream consumers

`EncDotNet.Iso8211` is a published NuGet package, not an internal helper.
Besides `EncDotNet.S57`, it is consumed by
[EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100) to read
S-101 and S-401 ISO 8211 datasets, and to read S-57 cells through
`EncDotNet.S100.Datasets.S57`. S-101 data exercises features S-57 does not
(for example `b48` floating-point subfields and UTF-8 text), so:

- Treat every public type and member as a compatibility commitment. Removing
  or changing a signature is a breaking change for EncDotNet.S100.
- Keep the reader product-agnostic. Do not add S-57 or S-101 knowledge to
  this package; it belongs in `EncDotNet.S57` or in EncDotNet.S100.

## File structure

An ISO 8211 file holds one **Data Descriptive Record (DDR)**, which defines
the schema (field tags, subfield names and formats), followed by one or more
**data records** parsed according to that schema.

Each record starts with a 24-byte **leader** (`Iso8211Leader`,
`Iso8211RecordLeader`): record length, interchange level, leader identifier,
field-area start offset, and the entry map (sizes of the tag, length and
position parts of each directory entry). A **directory** of
`Iso8211DirectoryEntry` values follows, giving each field's tag, length and
position in the field area.

Each DDR field (`Iso8211FieldDefinition`) carries:

- a data structure code (`Iso8211DataStructureCode`: elementary, vector,
  array, concatenated);
- a data type code (`Iso8211DataTypeCode`);
- subfield definitions (`Iso8211SubfieldDefinition`) parsed from the field
  controls.

## Subfield formats

`Iso8211SubfieldFormatType` maps the format codes:

| Code | Type | Notes |
|---|---|---|
| `A`, `A(n)` | `CharacterData` | Delimited (variable) or fixed width |
| `I(n)` | `Integer` | ASCII digits |
| `R(n)` | `Real` | ASCII real |
| `B(n)` | `BitString` | `n` is in bits; `Width` is stored in bytes |
| `b1x` | `UnsignedInteger` | `x` = byte width (1, 2, 4), little-endian |
| `b2x` | `SignedInteger` | `x` = byte width (1, 2, 4), little-endian |
| `b4x`, `b5x` | `FloatingPoint` | IEEE 754, `x` = 4 or 8; S-101 uses `b48` |

- **Unit terminator** `0x1F` separates variable-length subfields; **field
  terminator** `0x1E` ends a field.
- A `*` in a format control starts a repeating group (for example the
  coordinate tuples in S-57 `SG2D`/`SG3D`).
- Format controls can nest parenthesized groups and repeat counts, for
  example `(A(2),3(b14))`. Parse them structurally; do not use regular
  expressions.

## Key types

| Type | Role |
|---|---|
| `Iso8211Reader` | Low-allocation token reader over a `ReadOnlySpan<byte>` |
| `Iso8211DocumentReader` | Reads a whole file into an `Iso8211Document` |
| `Iso8211DataDescriptiveRecordReader` | Turns the first record into an `Iso8211DataDescriptiveRecord` |
| `Iso8211FieldReader` | Reads subfield values from a field using its DDR definition |
| `Iso8211Record`, `Iso8211Field` | A data record and one of its fields (tag and raw bytes) |
| `Iso8211DocumentBuilder`, `Iso8211RecordBuilder`, `Iso8211FieldBuilder` | Build documents to write |
| `Iso8211DocumentWriter`, `Iso8211RecordWriter`, `Iso8211SubfieldEncoder` | Encode documents, records and subfields |

## Review checklist

1. Parsing hot paths use `ReadOnlySpan<byte>` and avoid per-subfield
   allocations.
2. Binary integers and floats are little-endian.
3. Character data at lexical levels 0 and 1 is decoded as UTF-8 (a superset
   of the ASCII the standard requires), so S-101 text survives. Level 2 is
   UCS-2 (UTF-16LE) with two-byte terminators.
4. Records longer than 99,999 bytes round-trip: the reader accepts a leader
   record length of `00000` and the writer emits one when needed.
5. Every reader change has a matching writer change (and the reverse), with
   a round-trip test built from an in-memory document.
6. New public API has XML doc comments and a test under `tests/`.
7. Public API removals or signature changes are called out as breaking in
   the PR description, with the EncDotNet.S100 impact.

## Known pitfalls

- Real cells contain fields with optional trailing subfields. Treat a field
  that ends early as missing values, not as corruption.
- Concatenated array descriptors can include leader subfields before the
  repeating group.
- Format-control widths for `B(n)` are in bits but stored in bytes.

## Standards reference

- `standards/md/S57v31.md` Part 3, clause 7 (structure implementation): the
  field and subfield tables and tree structure diagrams. Annex A summarises
  ISO 8211 with ASCII and binary examples.
- ISO/IEC 8211 itself is not in the repository. Use the S-57 annex first,
  and cite the clause when you rely on another source.
