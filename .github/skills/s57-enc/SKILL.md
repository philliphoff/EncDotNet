---
name: s57-enc
description: |
  Expert knowledge of IHO S-57 Edition 3.1 and the EncDotNet.S57 package:
  the theoretical data model, feature and vector records, field and subfield
  semantics (DSID, DSSI, DSPM, FRID, FOID, ATTF, NATF, FSPT, FFPT, VRID,
  VRPT, SG2D, SG3D), the object and attribute catalogues (OBJL, ATTL),
  chain-node topology, COMF/SOMF scaling, update cells (.001 and later),
  exchange sets and CATALOG.031, and the ENC Product Specification.
  USE FOR: changes to src/EncDotNet.S57/**, S57DocumentReader, S57Document,
  S57Chart and the typed chart model, ApplyChanges, S57CatalogReader,
  S57ExchangeSetReader, S57ExchangeSetVerifier, S57ObjectCode, attribute
  handling, topology and geometry questions, and S-57 encoding questions.
  DO NOT USE FOR: byte-level ISO 8211 parsing (use iso8211); symbology,
  colours and display categories (use s52-presentation); Mapsui layers
  (use chart-viewer).
---

# S-57 ENC expert

## When engaged

- Tasks touching `src/EncDotNet.S57/**` or the `S57*Tests.cs` files under
  `tests/`.
- Questions about what an S-57 field, subfield, object class or attribute
  means, or how a cell is encoded.
- Geometry and topology problems, wherever they surface.

## Downstream consumers

`EncDotNet.S57` is a published NuGet package. Besides the chart viewer in
this repository, [EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100)
uses it in `EncDotNet.S100.Datasets.S57` to translate S-57 cells to S-101
(`S57ToS101Translator`) and to apply update cells (`S57Document.ApplyChanges`).
Treat every public type and member as a compatibility commitment, and call
out breaking changes and their EncDotNet.S100 impact in the PR description.

## Layers of the model

1. **`S57Document` / `S57DocumentReader`**: a direct mapping from ISO 8211
   records to typed records (`S57FeatureRecord`, `S57VectorRecord`). Records
   are flat, with RCNM/RCID identity (`S57RecordName`).
2. **`S57Chart` (`Charts/`)**: classifies spatial records into
   `S57IsolatedNode`, `S57ConnectedNode`, `S57Edge` and `S57Face`, and
   features into `S57PointFeature`, `S57LineFeature`, `S57AreaFeature` and
   `S57MetaFeature`, indexed by `S57RecordName` in immutable dictionaries.
3. **Exchange sets (`ExchangeSets/`)**: `S57CatalogReader` reads
   `CATALOG.031` (CATD records) into `S57Catalog`; `S57ExchangeSetReader`
   opens an exchange set; `S57ExchangeSetVerifier` checks CRCs, with an
   `IS63SignatureVerifier` seam for S-63 signatures.

## Records

**Feature records** (`S57FeatureRecord`, RCNM 100):

- **OBJL** (`S57ObjectCode`): object class, for example DEPARE = 42,
  LIGHTS = 75, SOUNDG = 129.
- **PRIM** (`S57GeometricPrimitive`): point, line, area or none.
- **GRUP**: 1 = skin of the earth, 2 = all others.
- **Attributes** (`ImmutableArray<S57AttributeValue>`): ATTL/ATVL pairs from
  ATTF and NATF.
- **Spatial pointers** (FSPT, `S57SpatialPointer`) and **feature pointers**
  (FFPT, `S57FeaturePointer`).

**Vector records** (`S57VectorRecord`), by RCNM:

| RCNM | Record | Geometry |
|---|---|---|
| 110 | Isolated node | SG2D position or SG3D soundings, not both |
| 120 | Connected node | One SG2D position at an edge end |
| 130 | Edge | SG2D interior points; VRPT to begin/end nodes and left/right faces |
| 140 | Face | VRPT to edges with USAG and ORNT |

## Chain-node topology

```text
Feature (PRIM=Area) --FSPT--> Face --VRPT--> Edge --VRPT--> Connected node
```

- Faces reference edges with `USAG` exterior (1) or interior (2) and `ORNT`
  forward (1) or reverse (2).
- Edges reference nodes with `TOPI` beginning (1) or end (2), and faces with
  left face (3) or right face (4).
- `MASK` (1 = mask, 2 = show) on FSPT hides an edge from display without
  removing it from the geometry.
- Build area rings by walking edges in orientation order, grouped by usage.

## Coordinates

- SG2D stores Y (latitude) then X (longitude) as integers; SG3D adds depth.
- Divide by **COMF** (`S57DataSetParameters.CoordinateMultiplicationFactor`,
  usually 10,000,000) for decimal degrees, and depths by **SOMF** (usually
  10). Use `S57Chart.ToDecimalDegrees` or `S57Coordinate2D.ToDecimalDegrees`.
- Convert at the boundary where geometry leaves the model; keep integers
  inside it.

## Updates

Update cells (`.001`, `.002`, ...) carry RUIN (insert, delete, modify) and
field update instructions (`S57UpdateInstruction`, `S57FieldUpdateControl`).
`S57Document.ApplyChanges` applies one update; apply them in sequence and
check the update number in DSID. Updating rules are in S-57 Part 3,
clause 8.

## Review checklist

1. Field and subfield tags come from `S57FieldTags` and `S57SubfieldNames`,
   not string literals.
2. Object codes use `S57ObjectCode`, numbered as in Appendix A.
3. Parsed data is immutable: `ImmutableArray<T>`, `ImmutableDictionary<K,V>`,
   records and `readonly record struct`.
4. Byte-level parsing goes through `EncDotNet.Iso8211`; do not re-implement
   field or subfield decoding here.
5. Optional subfields that are missing in real cells (for example S-101-era
   DSID extensions) are modelled as nullable, not as parse failures.
6. Tests build cells in memory with the `EncDotNet.Iso8211` builders. Never
   commit real ENC cells.
7. New public API has XML doc comments citing the S-57 clause it implements.

## Known pitfalls

- ATVL values are strings that may hold an enumeration, a list
  (comma-separated), a float or free text. Interpret them per the attribute
  type in Appendix A Chapter 2.
- National attributes (NATF) use lexical level 1 or 2 (DSSI NALL). Decode
  them with that level, not level 0.
- A feature can reference both edges and faces. Handle edge-only area
  geometry, not just face-based topology.

## Standards reference

Read the relevant file under `standards/md/` before answering spec
questions, and cite the section:

| File | Contents |
|---|---|
| `S57v31.md` | Main document: Parts 1–3 (data model, data structure, field and subfield tables, updating) |
| `S57v31AppendixACh1.md` | Appendix A Ch. 1: object classes (acronym, code, primitives, attributes) |
| `S57v31AppendixACh2.md` | Appendix A Ch. 2: attributes (acronym, code, value domain) |
| `S57v31AppendixB1AnnexA.md` | Appendix B.1 Annex A: ENC Product Specification |
| `S57v31AppendixB1AnnexD.md` | Appendix B.1 Annex D: ENC encoding guide |
