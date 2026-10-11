---
name: chart-viewer
description: |
  Maintenance notes for EncDotNet.ChartViewer, the legacy Avalonia + Mapsui
  desktop viewer for S-57 cells. The viewer is kept building but is not
  being developed: chart display work belongs in EncDotNet.S100, which
  translates S-57 to S-101 and portrays it with the S-100 Lua catalogue.
  USE FOR: fixing a build break, dependency update or crash in
  src/EncDotNet.ChartViewer/**, or keeping it compiling after an
  EncDotNet.S57 API change. DO NOT USE FOR: new display features or S-52
  portrayal work (do that in EncDotNet.S100); S-57 semantics (use s57-enc).
---

# Chart viewer (maintenance only)

`EncDotNet.ChartViewer` draws S-57 cells on a Mapsui map with hand-written
styles and SVG symbols that approximate S-52. It isn't published to NuGet
and isn't where chart display is developed. Keep changes to the minimum
needed to keep it building and running:

- Fix build breaks, crashes and dependency updates.
- Update call sites when an `EncDotNet.S57` API changes.
- Don't add features, symbology or S-52 rules here. Raise display work
  against [EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100).
- Don't let viewer needs shape the libraries: keep display logic out of
  `EncDotNet.S57` and `EncDotNet.Iso8211`.

## Where things live

| File | Role |
|---|---|
| `S57LayerFactory.cs` | Builds Mapsui `MemoryLayer`s from an `S57Chart` |
| `S57LayerTemplate.cs`, `S57LayerTemplates.cs` | Per-object-code styles, render order and SCAMIN handling |
| `S57AreaGeometryBuilder.cs`, `S57LineGeometryBuilder.cs` | NTS geometry from S-57 topology (covered by tests) |
| `ProjectedEdgeCache.cs` | Projects each edge to EPSG:3857 once per chart |
| `Baking/ChartBaker.cs` | MessagePack cache of parsed cells |
| `Charts/` | Chart sources and the NOAA package manager |

If a fix touches the geometry builders, keep `S57AreaGeometryBuilderTests`
and `S57LineGeometryBuilderTests` passing; they also exercise S-57
topology handling.
