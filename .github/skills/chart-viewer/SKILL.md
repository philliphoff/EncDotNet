---
name: chart-viewer
description: |
  Working knowledge of EncDotNet.ChartViewer (the Sound Charts desktop app):
  how S-57 features become Mapsui layers on an Avalonia map — layer
  templates, area and line geometry builders, Spherical Mercator projection,
  render order, SCAMIN visibility, SVG point symbols, compilation-scale
  layer ordering, the baked-chart cache and NOAA chart management.
  USE FOR: changes to src/EncDotNet.ChartViewer/**, S57LayerFactory,
  S57LayerTemplate, S57LayerTemplates, S57AreaGeometryBuilder,
  S57LineGeometryBuilder, ScaminStyleHelper, ProjectedEdgeCache, ChartBaker,
  view models and views, adding or restyling an object class, NTS geometry
  construction, Mapsui styling questions. DO NOT USE FOR: what the S-52
  display rules require (use s52-presentation); S-57 record semantics or
  topology rules (use s57-enc).
---

# Chart viewer (Avalonia + Mapsui)

## When engaged

- Tasks touching `src/EncDotNet.ChartViewer/**`, or the geometry-builder
  tests (`S57AreaGeometryBuilderTests.cs`, `S57LineGeometryBuilderTests.cs`).
- Adding, restyling or reordering an S-57 object class on the map.

The viewer approximates S-52 with hand-written styles and SVG symbols; it
does not run the S-52 Presentation Library. Load `s52-presentation` when a
change should follow the display rules, and this skill for how to build it.

## Stack

- Avalonia 11 with ReactiveUI (`ViewModelBase` derives from
  `ReactiveObject`) and compiled bindings by default.
- Mapsui 5 (`Mapsui.Avalonia`, `Mapsui.Nts`) with NetTopologySuite
  geometries.
- `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging`
  and OpenTelemetry exporters (`ChartViewerDiagnostics`, `DiagnosticOptions`).
- MessagePack for the baked-chart cache.

## Where things live

| File | Role |
|---|---|
| `S57LayerFactory.cs` | Builds `MemoryLayer`s from an `S57Chart`, grouped by object code |
| `S57LayerTemplate.cs` | Per-object-code area, line and point handlers, `RenderOrder`, `MaxVisible`, SCAMIN wrapping |
| `S57LayerTemplates.cs` | Registry of templates: colours, symbols and attribute-driven styling |
| `S57AreaGeometryBuilder.cs` | `Polygon`/`MultiPolygon` from face-based or edge-only area topology |
| `S57LineGeometryBuilder.cs` | `LineString`/`MultiLineString` from edges, honouring ORNT and MASK |
| `ProjectedEdgeCache.cs` | Projects each edge to EPSG:3857 once per chart |
| `ScaminThemeStyle.cs` | `ScaminStyleHelper`: hides features beyond their SCAMIN scale |
| `Baking/ChartBaker.cs` | Caches the parsed document and projected edges to skip re-parsing |
| `Charts/` | Chart sources, catalog sources and the NOAA package manager |
| `Assets/ChartSymbols/` | Embedded SVG symbols, loaded as `embedded://` image sources |

## Conventions

- **Layers:** every chart layer is a `MemoryLayer` of `GeometryFeature`s
  wrapping NTS geometries.
- **Projection:** project with `SphericalMercator.FromLonLat` after
  converting with COMF (see `s57-enc`); go through `ProjectedEdgeCache` for
  edges rather than projecting again.
- **Render order** (lower draws first): land 100, water 200, depth area
  300, area overlay 400, line 500, label 600, point 700. Within a layer,
  set the `"FeatureOrder"` feature property; `S57LayerFactory` sorts on
  it ascending (DEPARE stores `-depth` so deeper areas draw first).
- **Chart order:** charts stack by compilation scale (CSCL); smaller-scale
  charts draw behind larger-scale ones. `MaxVisible` is derived from CSCL
  and, per feature, SCAMIN.
- **Styles:** `VectorStyle` for fills and strokes, `LabelStyle` for text
  such as soundings (converted to the selected `DepthUnit`), `ImageStyle`
  for SVG symbols. Wrap every feature style with
  `S57LayerTemplate.MaybeWrapWithScamin`.
- **New object class:** add a template in `S57LayerTemplates` using the
  `AreaStyle`, `LineStyle`, `PointStyle` and `ImagePointStyle` helpers,
  pick its render order, and add a `S57FeatureCategory` value
  (`Models/S57FeatureCategory.cs`) if users should be able to hide it.

## Review checklist

1. Geometry builders handle both face-based and edge-only areas, interior
   rings (USAG = 2), reversed edges (ORNT = 2) and masked edges.
2. No per-frame allocation or projection in style callbacks.
3. Baked-cache changes bump or validate the cache format so stale files are
   rebuilt, not mis-read.
4. Geometry-builder changes have tests built from in-memory charts.
5. Never commit downloaded NOAA cells; the `.catalog/` and `.expanded/`
   folders are ignored for that reason.
