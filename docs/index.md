# EncDotNet documentation

EncDotNet reads IHO S-57 Electronic Navigational Charts and the ISO/IEC 8211
files they're encoded in, in .NET. Its libraries read and write ISO 8211, read
S-57 cells, apply update cells, verify exchange sets, and find cells in the
NOAA product catalog. They target .NET 8 and .NET 10.

[EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100) builds on
these libraries to read S-101 and to translate S-57 cells to S-101.

## Get started

[Getting started](getting-started.md) reads a cell and lists its depth areas
in a new console app.

## Packages

| Package | Reads | Reference it to |
|---|---|---|
| [`EncDotNet.Iso8211`](../src/EncDotNet.Iso8211/README.md) | ISO/IEC 8211 | Read, write or build any ISO 8211 file |
| [`EncDotNet.S57`](../src/EncDotNet.S57/README.md) | S-57 Edition 3.1 | Read cells, apply updates, read and verify exchange sets |
| [`EncDotNet.Noaa`](../src/EncDotNet.Noaa/README.md) | NOAA ENC product catalog | Find and download U.S. cells |

## Guides

- [Reading S-57 charts](reading-s57-charts.md): features, attributes,
  coordinates, the typed chart model, update cells and exchange sets.
- [Reading and writing ISO 8211](reading-iso8211.md): documents, the token
  reader, writing and building files.
- [Download NOAA ENC cells](download-noaa-charts.md): find, download and open
  U.S. cells.
- [Architecture](architecture.md): how the packages map to the standards.

## Reference

- [S-57 glossary](glossary.md): the field and record acronyms used in the API.
- [Versioning and compatibility](versioning.md): what counts as public API,
  and how breaking changes are caught.
- [API reference](../api/index.md): every public type and member.

## Contributing

- [How to contribute](../CONTRIBUTING.md)
- [C# coding style](coding-style.md)
- [Documentation style](docs-style.md)
