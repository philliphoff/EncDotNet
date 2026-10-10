# EncDotNet.S57

`EncDotNet.S57` reads IHO S-57 Edition 3.1 Electronic Navigational Chart
(ENC) cells. It gives you the dataset metadata, feature and vector records,
and attributes as encoded, and a typed chart model of point, line and area
features with indexed nodes, edges and faces. It also applies update cells
(`.001`, `.002`, ...) and reads and verifies exchange sets. Reference it to
read S-57 data in .NET 8 or .NET 10. It depends on
[`EncDotNet.Iso8211`](https://www.nuget.org/packages/EncDotNet.Iso8211),
which it installs for you.

## Install

```bash
dotnet add package EncDotNet.S57
```

## Example: read a cell and list its depth areas

```csharp
using EncDotNet.S57;

S57Document document = S57DocumentReader.ReadFromFile("path/to/cell.000");

Console.WriteLine($"{document.DataSetIdentification?.DataSetName} at 1:{document.DataSetParameters?.CompilationScale}");

foreach (S57FeatureRecord feature in document.GetFeaturesByObjectCode(S57ObjectCode.DEPARE))
{
    Console.WriteLine($"{feature.RecordName}: {feature.Attributes.Count} attributes, {feature.SpatialPointers.Count} spatial pointers");
}
```

## Main entry points

### Records as encoded

| Type | Use it to |
|---|---|
| `S57DocumentReader` | Read a cell from a file, stream or bytes into an `S57Document` |
| `S57Document` | Get metadata (`DSID`, `DSPM`), feature records and vector records; apply an update cell with `ApplyChanges` |
| `S57FeatureRecord` | Get a feature's object class, primitive, attributes and pointers |
| `S57VectorRecord` | Get a node's or edge's coordinates, soundings and topology pointers |
| `S57ObjectCode` | Identify object classes (`DEPARE`, `SOUNDG`, `LIGHTS`, ...) by their S-57 code |

### Typed chart model (`EncDotNet.S57.Charts`)

| Type | Use it to |
|---|---|
| `S57Chart` | Get features grouped by primitive and object class, and nodes, edges and faces indexed by record name |
| `S57PointFeature`, `S57LineFeature`, `S57AreaFeature`, `S57MetaFeature` | Work with a feature and its geometry references |
| `S57IsolatedNode`, `S57ConnectedNode`, `S57Edge`, `S57Face` | Work with spatial records |

### Exchange sets (`EncDotNet.S57.ExchangeSets`)

| Type | Use it to |
|---|---|
| `S57ExchangeSetReader` | Find the catalog, base cell and update cells of a single-cell exchange set |
| `S57ExchangeSet` | Read the cell with its updates applied; verify the exchange set |
| `S57CatalogReader` | Read `CATALOG.031` entries and their coverage |
| `S57ExchangeSetVerifier` | Check each file's CRC-32 against the catalog |

[Reading S-57 charts](../../docs/reading-s57-charts.md) covers each of these
with examples.

## Exchange set verification outcomes

`S57ExchangeSet.VerifyAsync` reports a checksum outcome and a signature
outcome for each file, as `S57VerificationOutcome` values. It doesn't throw
for files that fail. S-63 signature verification isn't implemented yet;
`IS63SignatureVerifier` and `S63TrustAnchorOptions` are the extension point,
and files report `NotSigned` until it is.

| `S57VerificationOutcome` | Meaning |
|---|---|
| `Ok` | The check passed |
| `NoChecksum` | The catalog entry has no CRC to check against |
| `ChecksumMismatch` | The file's CRC doesn't match the catalog |
| `FileMissing` | The file isn't on disk |
| `NotSigned` | The file has no digital signature |
| `SignatureInvalid`, `CertificateUntrusted`, `CertificateExpired`, `CertificateNotFound` | S-63 signature outcomes |
| `Error` | An unexpected error occurred during verification |

## Related packages

- [`EncDotNet.Iso8211`](https://www.nuget.org/packages/EncDotNet.Iso8211):
  the ISO 8211 reader and writer this package uses.
- [`EncDotNet.Noaa`](https://www.nuget.org/packages/EncDotNet.Noaa): finds
  and downloads U.S. cells from the NOAA product catalog.

## License

MIT. See [LICENSE](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE).
