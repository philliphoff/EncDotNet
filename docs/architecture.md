# Architecture: ENC Standards and API Layers

Electronic Navigational Charts (ENCs) are built on a stack of international standards. EncDotNet mirrors this layered architecture, with each library mapping to a specific standard layer.

## Standards Stack

```
┌─────────────────────────────────────────────────────────────┐
│                    S-52 (Display)                            │
│         Chart symbology, colors, and display rules          │
│                                                             │
│  "How should a depth area be colored?"                      │
│  "What symbol represents a lateral buoy?"                   │
├─────────────────────────────────────────────────────────────┤
│                    S-57 (Data Model)                         │
│     Features, attributes, topology, and spatial records     │
│                                                             │
│  "This is a depth area (DEPARE) with min depth 5m"          │
│  "This buoy (BOYLAT) is at position 37.2°N, 76.1°W"        │
├─────────────────────────────────────────────────────────────┤
│                 ISO/IEC 8211 (Container)                     │
│       Binary encoding format for structured records         │
│                                                             │
│  "Record 42 has 6 fields; field 'VRID' is 5 bytes"          │
│  "The DDR defines field structures and subfield formats"    │
└─────────────────────────────────────────────────────────────┘
```

Each layer builds on the one below it, and the EncDotNet libraries follow the same pattern.

## How the Libraries Map to Standards

| Standard | Library | What It Does |
|----------|---------|--------------|
| **ISO/IEC 8211** | `EncDotNet.Iso8211` | Reads the binary container format — records, fields, and raw byte data |
| **IHO S-57** | `EncDotNet.S57` | Interprets the data model — features, vectors, attributes, and topology |
| **IHO S-52** | *(ChartViewer app)* | Renders charts with symbology and display rules (not a library) |
| **NOAA Products** | `EncDotNet.Noaa` | Accesses the NOAA ENC product catalog for chart discovery and download |

### ISO/IEC 8211 — The Container Format

ISO 8211 is a general-purpose binary format for encoding structured data. It is not specific to nautical charts — it is used by several geospatial standards. An ISO 8211 file consists of:

- A **Data Descriptive Record (DDR)** — a schema record that defines the structure of all fields in the file, including subfield names, types, and sizes
- One or more **Data Records** — each containing a leader, a directory of field entries, and the field data itself

`EncDotNet.Iso8211` provides two ways to read this format:

- **`Iso8211Reader`** — a high-performance, forward-only token reader (`ref struct`) that emits tokens like `StartRecord`, `DirectoryEntry`, `Field`, and `EndRecord` with zero intermediate allocations. Supports streaming for large files.
- **`Iso8211DocumentReader`** — builds a complete in-memory `Iso8211Document` object model containing all records and fields.

At this layer, you see raw tags like `"DSID"`, `"VRID"`, `"SG2D"` and byte arrays — the data has no nautical meaning yet.

### IHO S-57 — The Nautical Data Model

S-57 defines how nautical chart data is encoded *within* ISO 8211 files. It specifies:

- **Features** — real-world objects like depth areas (`DEPARE`), buoys (`BOYLAT`), lights (`LIGHTS`), and soundings (`SOUNDG`), each identified by an object code (`OBJL`)
- **Attributes** — properties of features, such as minimum depth, light color, or buoy shape
- **Spatial records** — geometry stored as isolated nodes (points), connected nodes, edges (polylines), and faces (polygons)
- **Topology** — chain-node topology linking features to spatial records via pointers (FSPT), and edges to nodes (VRPT)
- **Coordinate encoding** — positions stored as integers that must be divided by the Coordinate Multiplication Factor (COMF) to get decimal degrees

`EncDotNet.S57` provides two levels of abstraction:

- **`S57DocumentReader`** → **`S57Document`** — parses an ISO 8211 file into an S-57 domain model with typed feature records, vector records, dataset metadata, and coordinate conversion helpers
- **`S57Chart`** — a higher-level model that categorizes features by geometry (point, line, area, meta) and indexes spatial records by type (isolated nodes, connected nodes, edges, faces), with lookup methods and reverse-pointer indexes

### NOAA ENC Products

NOAA publishes an XML product catalog listing all available U.S. ENC cells with metadata like chart name, scale, coverage, and download URLs.

`EncDotNet.Noaa` provides `EncProductCatalogClient` to download and deserialize this catalog into typed objects (`EncProductCatalog`, `Cell`, `Coverage`, etc.).

## Data Flow

A typical workflow follows the standards stack from bottom to top:

```
  .000 file on disk
        │
        ▼
  ┌─────────────────┐
  │  Iso8211Reader   │  Tokenize binary data
  │        or        │
  │ Iso8211Document- │  Build record/field
  │    Reader        │  object model
  └────────┬────────┘
           │ Iso8211Document
           ▼
  ┌─────────────────┐
  │ S57Document-     │  Parse S-57 semantics:
  │    Reader        │  features, vectors,
  │                  │  attributes, topology
  └────────┬────────┘
           │ S57Document
           ▼
  ┌─────────────────┐
  │   S57Chart       │  Strongly-typed chart:
  │                  │  point/line/area features,
  │                  │  spatial indexes
  └────────┬────────┘
           │
           ▼
      Application
   (display, analysis,
    export, etc.)
```

Each step adds meaning to the raw bytes:

1. **ISO 8211** turns a byte stream into records and fields
2. **S57DocumentReader** turns fields into features, vectors, and metadata
3. **S57Chart** organizes features and spatial records into a navigable, strongly-typed model

## Choosing the Right Layer

| Goal | Use |
|------|-----|
| Parse non-S-57 ISO 8211 files | `Iso8211Reader` or `Iso8211DocumentReader` |
| High-performance scanning of chart files | `Iso8211Reader` (token-level) |
| Read chart features and metadata | `S57DocumentReader` → `S57Document` |
| Work with typed geometry and indexes | `S57Chart.FromFile()` |
| Discover and download NOAA charts | `EncProductCatalogClient` |
| Apply incremental chart updates (.001, .002, …) | `S57Document.ApplyChanges()` |
