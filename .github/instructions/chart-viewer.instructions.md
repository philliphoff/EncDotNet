---
applyTo: "src/EncDotNet.ChartViewer/**"
---

# Chart viewer editing rules

`EncDotNet.ChartViewer` is a legacy app kept for maintenance only:

- Limit changes to build fixes, crash fixes, dependency updates and call-site
  updates after `EncDotNet.S57` API changes. See the `chart-viewer` skill.
- Don't add features or S-52 portrayal here; chart display work belongs in
  EncDotNet.S100.
- Keep display logic out of `EncDotNet.S57` and `EncDotNet.Iso8211`.
