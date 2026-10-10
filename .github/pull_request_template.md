<!-- Thank you for contributing to EncDotNet! -->

## Summary

<!-- What does this PR change, and why? -->

## Standards alignment

Check each area this PR touches and confirm the matching skill was consulted
(`.github/skills/<name>/SKILL.md`):

- [ ] ISO/IEC 8211 (`iso8211`)
- [ ] S-57 records, topology, updates or exchange sets (`s57-enc`)
- [ ] S-52 display rules (`s52-presentation`)
- [ ] Chart viewer (`chart-viewer`)
- [ ] N/A: build, CI, docs or tooling only

**Standard section references cited in code or docs:**

<!-- e.g. "S-57 Part 3, 7.6.8 for FSPT" -->

## Tests

- [ ] Added or updated xUnit tests under `tests/`
- [ ] Test input is built in memory; no real ENC cells are committed
- [ ] `dotnet test --configuration Release` passes locally

## Documentation

- [ ] Updated the affected package's `src/<project>/README.md`
- [ ] Updated guides under `docs/` if user-facing behavior changed
- [ ] New public APIs have XML doc comments
- [ ] `docfx docfx.json --warningsAsErrors` passes, if docs changed

## Dependencies

- [ ] No new NuGet dependencies, OR versions added to
      `Directory.Packages.props` (not in the `.csproj`)
- [ ] `gh-advisory-database` security check run for any new dependency

## Breaking changes

<!--
List binary or behavioral breaking changes to EncDotNet.Iso8211,
EncDotNet.S57 or EncDotNet.Noaa, and their impact on EncDotNet.S100,
or write "None".
-->
