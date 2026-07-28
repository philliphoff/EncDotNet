# EncDotNet — Copilot Instructions

## Domain Context

This project is a .NET 10 library suite and Avalonia desktop application for reading, parsing, and rendering **IHO S-57 Electronic Navigational Charts (ENCs)**. It implements:

- **ISO/IEC 8211** — the binary container format used by S-57 for data encoding
- **IHO S-57** — the International Hydrographic Organization's transfer standard for digital hydrographic data
- **IHO S-52** — specifications for chart content and display aspects of ECDIS (symbolization, colours, display categories, and the Presentation Library)
- **Nautical chart visualization** — rendering ENC data on a Mapsui-based map with Avalonia UI

### Key Standards References

- **IHO S-57 Edition 3.1** — Transfer Standard for Digital Hydrographic Data
- **IHO S-57 Appendix A** — Object Catalogue (defines OBJL codes: DEPARE, SOUNDG, LIGHTS, BOYLAT, etc.)
- **IHO S-57 Appendix B** — Attribute Catalogue (defines ATTL codes and their value domains)
- **ISO/IEC 8211** — Specification for a data descriptive file (the binary container format)
- **IHO S-52** — Chart Content and Display Aspects of ECDIS (display/symbology rules; not yet fully implemented)

### Standards Reference Documents

The `standards/md/` directory contains Markdown versions of the key IHO standards. When working on S-57 parsing, field definitions, object codes, attribute handling, topology, or chart display/symbology, read the relevant document(s) for authoritative reference:

| File | Contents |
|---|---|
| `standards/md/S57v31.md` | S-57 Edition 3.1 main document — Parts 1–3 (general introduction, theoretical data model, data structure including ISO 8211 field/subfield tables and updating rules) |
| `standards/md/S57v31AppendixACh1.md` | Appendix A Chapter 1 — Object Classes (defines every S-57 object class: acronym, code, definition, geometric primitives, and allowed attributes) |
| `standards/md/S57v31AppendixACh2.md` | Appendix A Chapter 2 — Attributes (defines every S-57 attribute: acronym, code, definition, and value domain) |
| `standards/md/S57v31AppendixB1AnnexA.md` | Appendix B.1 Annex A — ENC Product Specification (encoding rules, cell structure, mandatory/optional objects and attributes for ENC) |
| `standards/md/S57v31AppendixB1AnnexD.md` | Appendix B.1 Annex D — ENC Encoding Guide (additional encoding guidance and examples) |
| `standards/md/S52v611.md` | S-52 Edition 6.1.1 — Specifications for Chart Content and Display Aspects of ECDIS (display organization, symbology of areas/lines/points, colour specifications, display screen requirements) |
| `standards/md/S52AnnexA.md` | S-52 Annex A — IHO ECDIS Presentation Library Edition 4.0.3 (symbol catalogue with paper-based symbol descriptions for ECDIS) |

## Project Architecture

| Project | Purpose |
|---|---|
| `EncDotNet.Iso8211` | Low-level ISO 8211 binary format parser (DDR, field definitions, subfield readers) |
| `EncDotNet.Enc` | S-57 domain model (documents, features, vectors, catalogs, charts, topology) |
| `EncDotNet.Noaa` | NOAA ENC product catalog integration (downloading/managing chart data) |
| `EncDotNet.ChartViewer` | Avalonia/Mapsui desktop viewer for rendering charts on an interactive map |
| `EncDotNet.Console` | CLI tools for inspecting and dumping chart data |
| `EndDotNet.UnitTests` | xUnit tests for the parsing and domain libraries |

### Library Dependency Chain

```
Iso8211  →  Enc  →  ChartViewer
                 →  Noaa
                 →  Console
```

## S-57 / ISO 8211 Terminology

These abbreviations appear throughout the codebase as type names, field tags, and subfield names:

### File & Record Structure
- **DDR** — Data Descriptive Record (ISO 8211 schema record at the start of each file)
- **CATALOG.031** — The S-57 exchange set catalog file listing all chart files
- **CATD** — Catalogue Directory record within CATALOG.031

### Data Set Fields
- **DSID** — Data Set Identification field (dataset metadata: name, edition, agency)
- **DSSI** — Data Set Structure Information
- **DSPM** — Data Set Parameters field (coordinate systems, scales, multiplication factors)
- **COMF** — Coordinate Multiplication Factor (divide raw integer coordinates by this to get decimal degrees)
- **SOMF** — Sounding Multiplication Factor (divide raw integer depths by this)
- **CSCL** — Compilation Scale (e.g., 22000 for a 1:22,000 chart)

### Feature Records
- **FRID** — Feature Record Identifier (contains PRIM, GRUP, OBJL)
- **FOID** — Feature Object Identifier (contains FIDN, FIDS)
- **OBJL** — Object Label/Code (the `S57ObjectCode` enum value, e.g., DEPARE=42, SOUNDG=129)
- **PRIM** — Geometric Primitive (Point=1, Line=2, Area=3, None=255)
- **ATTF/NATF** — Feature Record Attribute Field / National Attribute Field
- **ATTL/ATVL** — Attribute Label (numeric code) / Attribute Value (string)
- **FSPT** — Feature-to-Spatial Record Pointer (links features to vector geometry)
- **FFPT** — Feature-to-Feature Pointer (links related features)

### Vector/Spatial Records
- **VRID** — Vector Record Identifier
- **RCNM** — Record Name Code (110=Isolated Node, 120=Connected Node, 130=Edge, 140=Face)
- **RCID** — Record Identification Number
- **SG2D** — 2-D Coordinate field (lat/lon as integers)
- **SG3D** — 3-D Coordinate (Sounding) field (lat/lon/depth as integers)
- **VRPT** — Vector Record Pointer (edge-to-node topology)
- **ORNT** — Orientation (Forward=1, Reverse=2)
- **USAG** — Usage Indicator (Exterior=1, Interior=2)
- **MASK** — Masking Indicator (Mask=1, Show=2)
- **TOPI** — Topology Indicator (Beginning=1, End=2, LeftFace=3, RightFace=4)

### Topology Model
S-57 uses **chain-node topology**:
- **Features** reference spatial records via FSPT pointers
- **Area features** reference **Faces**, which are bounded by **Edges**
- **Edges** are polylines connecting two **Connected Nodes**
- **Isolated Nodes** represent standalone points (or sounding clusters)
- Edge orientation and usage indicators determine interior vs. exterior boundaries

## Coordinate Handling

- Raw coordinates in S-57 files are **integers**; divide by **COMF** (typically 10,000,000) to get lat/lon in decimal degrees.
- Soundings are stored in SG3D fields; divide depth by **SOMF** (typically 10).
- The chart viewer projects to **Spherical Mercator (EPSG:3857)** via `Mapsui.Projections.SphericalMercator`.

## Coding Conventions

- The normative C# rules are in [`docs/coding-style.md`](../docs/coding-style.md)
  and encoded in [`.editorconfig`](../.editorconfig). Follow both when changing
  code.
- Before finishing, run `dotnet format whitespace EncDotNet.slnx` and
  `dotnet format style EncDotNet.slnx --diagnostics IDE0005`; CI verifies both
  with `--verify-no-changes`.
- Target framework: **.NET 10** with nullable reference types and implicit usings enabled.
- Use `ReadOnlySpan<byte>` for parsing hot paths where possible.
- Use `ImmutableArray<T>` and `ImmutableDictionary<K,V>` for parsed record data — records are immutable after construction.
- Field tags are string constants in `S57FieldTags`; subfield names are string constants in `S57SubfieldNames`.
- Object codes use the `S57ObjectCode` enum with values matching IHO S-57 Appendix A numbering.
- The chart viewer uses **Avalonia 11** with **ReactiveUI** for MVVM and **Mapsui 5** for map rendering.
- Chart layers in the viewer are ordered by **compilation scale** (CSCL) — higher scale (less detail) behind lower scale (more detail).
