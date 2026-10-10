---
name: s52-presentation
description: |
  Expert knowledge of IHO S-52 Edition 6.1.1 (chart content and display
  aspects of ECDIS) and its Annex A Presentation Library 4.0.3: display
  categories (base, standard, other), viewing groups, display priority and
  draw order, colour tables and day/dusk/night palettes, symbol, line-style
  and area-pattern catalogues, conditional symbology procedures, safety
  contour and depth shading, SCAMIN and text grouping. USE FOR: deciding how
  an S-57 object should look or when it should show; choosing colours,
  symbols or priorities in the chart viewer; reviewing a rendering change
  for display correctness. DO NOT USE FOR: Mapsui/Avalonia mechanics (use
  chart-viewer); S-57 encoding and records (use s57-enc). Full S-52-grade
  portrayal lives in EncDotNet.S100 (S-57 is translated to S-101 and
  portrayed by its Lua catalogue), not in this repository.
---

# S-52 presentation expert

## When engaged

- Choosing how an object class, attribute value or depth band is drawn in
  `src/EncDotNet.ChartViewer`.
- Questions about display categories, priorities, colours, symbols,
  conditional symbology or SCAMIN.

## Scope in this repository

S-52 is a reference here, not a contract. The chart viewer uses simplified
styles and a subset of symbols (see `chart-viewer`), so:

- Use S-52 to pick colours, priorities and visibility rules, and cite the
  section in a code comment when a value comes from it.
- Approximate rather than port: do not add a Presentation Library engine,
  look-up tables or conditional-symbology procedures to the viewer without
  agreeing on it first. That engine already exists in EncDotNet.S100.
- Keep the libraries (`EncDotNet.Iso8211`, `EncDotNet.S57`) free of display
  logic.

## Key concepts

- **Display categories:** Display Base (always shown), Standard Display
  (default), All Other. Viewing groups refine these for user toggles.
- **Display priority and draw order:** priorities 0–9 order objects within
  the radar-over/under planes; area fills draw before lines, lines before
  symbols, symbols before text.
- **Colours:** named colour tokens (for example `DEPDW`, `DEPMS`, `LANDA`)
  resolved through day, dusk and night tables. Pick by token; never by a
  literal RGB value copied from a single palette.
- **Depth shading:** the safety contour and shallow/deep contours divide
  DEPARE into two or four shades.
- **SCAMIN:** hide an object when the display scale is smaller than its
  SCAMIN value.

## Review checklist

1. The change names the S-52 section (or Annex A symbol/colour token) it
   follows.
2. Draw order and priority follow S-52 rather than local preference.
3. Colours come from a palette token, so a dusk or night palette can be
   added later without touching every template.
4. Text and point symbols are never occluded by area fills.

## Standards reference

- `standards/md/S52v611.md`: S-52 Edition 6.1.1 (display organisation,
  symbology of areas, lines and points, colour specifications, display
  screen requirements).
- `standards/md/S52AnnexA.md`: Annex A, Presentation Library 4.0.3 (symbol
  catalogue and paper-chart symbol descriptions). It is large; search it
  for the symbol or token name rather than reading it whole.
