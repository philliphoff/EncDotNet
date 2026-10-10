---
applyTo: "src/EncDotNet.ChartViewer/**,tests/**/S57*GeometryBuilderTests.cs"
---

# Chart viewer editing rules

When modifying `EncDotNet.ChartViewer`:

- Load the `chart-viewer` skill. Load `s52-presentation` as well when the
  change affects how or when something is drawn.
- Follow the existing patterns: a `S57LayerTemplate` per object code,
  `MemoryLayer`s of NTS `GeometryFeature`s, the fixed render-order bands, and
  `MaybeWrapWithScamin` on every feature style.
- Project edges through `ProjectedEdgeCache`; do not project in style
  callbacks.
- Keep display logic in the viewer. Do not add rendering concerns to
  `EncDotNet.S57` or `EncDotNet.Iso8211`.
