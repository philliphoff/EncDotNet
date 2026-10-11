---
applyTo: "src/EncDotNet.Noaa/**"
---

# NOAA catalog editing rules

When modifying `EncDotNet.Noaa`:

- The package reads the NOAA ENC product catalog
  (`https://www.charts.noaa.gov/ENCs/ENCProdCat.xml`). Model only what the
  catalog publishes; keep chart parsing in `EncDotNet.S57`.
- Accept an `HttpClient` from the caller where possible, and keep network
  access out of unit tests: parse catalog XML from in-memory fixtures.
- This is a published package. Treat public API changes as breaking; say so
  in the PR description.
