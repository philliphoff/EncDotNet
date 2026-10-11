# Contributing to EncDotNet

Thanks for your interest in contributing. EncDotNet is a set of .NET libraries
for ISO/IEC 8211 and IHO S-57 data. This guide
covers how to build, test and submit changes.

## Prerequisites

- The [.NET 10 SDK](https://dotnet.microsoft.com/download). The libraries also
  target .NET 8; the .NET 10 SDK builds both.
- A Git client.

## Build and test

```bash
git clone https://github.com/philliphoff/EncDotNet.git
cd EncDotNet
dotnet build
dotnet test --configuration Release
```

`TreatWarningsAsErrors` is on, so any compiler, analyzer or NuGet audit
warning fails the build. CI builds and tests on Ubuntu and publishes the
viewer for macOS, Windows and Linux; avoid platform-specific APIs unless
they're gated to a runtime identifier.

## Tests

- Every new public API or bug fix needs an xUnit test under `tests/`.
- Build test input in memory, for example with `Iso8211DocumentBuilder`.
- Tests use xUnit v3. Pass `TestContext.Current.CancellationToken` to async
  calls that take a cancellation token; the xUnit analyzer (xUnit1051) fails
  the build otherwise.
- **Never commit real ENC cells or NOAA downloads.** The `scripts/` that
  download them write to `.catalog/` and `.expanded/`, which Git ignores.

## Performance

Performance is checked in two ways, because elapsed time on shared CI runners
is too noisy to fail a build on, while memory figures are exact.

- **Memory budgets (gate).** `tests/EncDotNet.PerformanceTests` measures the
  bytes each reader, writer and model builder allocates, and the bytes the
  parsed models retain, for a synthetic harbour cell. They run with
  `dotnet test` like any other test and fail when a figure moves outside its
  budget in `PerformanceBudgets.cs`: above it is a regression; well below it
  means the budget must be lowered to lock in the improvement. When a change
  moves a figure on purpose, update the budget to the figure the test prints
  and explain why in the pull request. The project compiles every method fully
  optimized up front (`TieredCompilation` off), because the tiered JIT's
  optimizations change allocation counts while a process runs.
- **Timing benchmarks (informational).** `tests/EncDotNet.Benchmarks` holds
  BenchmarkDotNet benchmarks. On pull requests that touch the libraries, the
  `Benchmarks` workflow runs the `Core` benchmarks for the base and the head
  on the same runner, interleaved, and posts a comparison to the job summary.
  It flags only changes that hold in every round, and never fails the build.

Both use synthetic cells from `tests/Shared/SyntheticS57Cell.cs`, generated
deterministically and shaped like real NOAA cells. The benchmarks also read
real cells when `ENCDOTNET_ENC_ROOT` points at a directory of them; CI
downloads a few with `scripts/DownloadBenchmarkCells.cs`. To run locally:

```bash
dotnet test tests/EncDotNet.PerformanceTests --configuration Release
dotnet run scripts/DownloadBenchmarkCells.cs -- .benchmark-cells
ENCDOTNET_ENC_ROOT=.benchmark-cells dotnet run -c Release --project tests/EncDotNet.Benchmarks -- --anyCategories Core
```

`ENCDOTNET_BENCHMARK_SYNTHETIC` limits the synthetic cells, for example to
`Large`, the cell with a record longer than 99,999 bytes.

## Package management

NuGet versions are managed centrally in `Directory.Packages.props`:

- Don't add `Version` attributes to `.csproj` files.
- Before adding a dependency, run the `gh-advisory-database` security check.
  NuGet audit also fails the build on known advisories.
- Keep the published libraries' dependencies minimal. `EncDotNet.S57`
  depends only on `EncDotNet.Iso8211` and
  `Microsoft.Extensions.Logging.Abstractions`.

## Public API compatibility

`EncDotNet.Iso8211`, `EncDotNet.S57` and `EncDotNet.Noaa` are published to
NuGet, and [EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100)
builds on the first two. Treat their public types and members as a
compatibility commitment: list any removal or signature change under
**Breaking changes** in the pull request, with its effect on EncDotNet.S100.

`dotnet pack` runs package validation against the last published release and
fails on a breaking change. If the break is intentional, follow
[Versioning and compatibility](docs/versioning.md#make-an-intentional-breaking-change)
to record it.

## Skills and instructions

Per-area **skills** live in `.github/skills/<name>/SKILL.md`, and matching
**instructions** in `.github/instructions/` apply to the files they cover.
AI agents load them automatically; they're also a useful briefing for people.
Before a non-trivial change, read the matching skill:

| Area | Skill |
|---|---|
| ISO 8211 leaders, DDR, subfield formats, reader and writer | `iso8211` |
| S-57 records, attributes, topology, updates, exchange sets | `s57-enc` |
| Keeping the legacy chart viewer building | `chart-viewer` |
| Docs pages and package READMEs | `docs-writing` |

Markdown versions of the standards are in `standards/md/`. Cite the section
number in pull requests and in XML doc comments for spec-derived types,
constants and enum values.

## Chart viewer

`src/EncDotNet.ChartViewer` is a legacy desktop viewer. It's kept building
but isn't being developed: limit changes to build fixes, crash fixes and
dependency updates. Chart display work belongs in
[EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100).

## Coding style

The normative rules are in the [C# coding style guide](docs/coding-style.md)
and are encoded in
[`.editorconfig`](https://github.com/philliphoff/EncDotNet/blob/main/.editorconfig).
In brief:

- `PascalCase` for types, methods and properties; `camelCase` for locals and
  parameters; `_camelCase` for private fields.
- File-scoped namespaces, four-space indents, Allman braces.
- Nullable reference types are on everywhere. Avoid `!`; prefer null checks
  or `ArgumentNullException.ThrowIfNull`.
- Every public API has XML doc comments. A missing one fails the build.

CI's format job verifies whitespace, using directives and naming. Fix and
check them locally before you push:

```bash
dotnet format whitespace EncDotNet.slnx
dotnet format style EncDotNet.slnx --diagnostics IDE0005
dotnet format EncDotNet.slnx --diagnostics IDE0005 IDE1006 --severity warn --verify-no-changes
```

`dotnet format` can't fix naming (IDE1006) violations automatically; rename
those by hand.

## Documentation

- Each package has a `README.md` in `src/<project>/`, which is also its
  nuget.org readme. Update it when you add types, remove APIs or change
  behavior.
- Guides live under `docs/`. Add or update a page for user-facing changes.
- Write to the [documentation style guide](docs/docs-style.md): pick a page
  type, use its structure, describe rather than promote, and make sure every
  C# example compiles.
- The Docs workflow builds the site on every pull request with
  `docfx docfx.json --warningsAsErrors`, so a broken link, anchor or xref
  fails the check. Run it locally first:

  ```bash
  dotnet tool install -g docfx --version 2.78.6
  docfx docfx.json --warningsAsErrors
  ```

## Branches and pull requests

1. Create a topic branch off `main`.
2. Keep the change focused on one concern.
3. Make sure `dotnet build`, `dotnet test --configuration Release` and the
   format checks pass locally.
4. Open a pull request against `main` and fill in the
   [pull request template](https://github.com/philliphoff/EncDotNet/blob/main/.github/pull_request_template.md):
   standards alignment, tests, docs, dependencies and breaking changes.
5. CI must pass before review.

## Releases

Maintainers release by running the **Release** workflow
(`.github/workflows/release.yml`) with a patch, minor or major bump. It finds
the highest `vMAJOR.MINOR.PATCH` tag, computes the next version, and pushes a
new tag; nothing is committed to `main`. The tag build in CI takes the version
from the tag name, publishes the packages to NuGet, and creates a GitHub
release with the packages and viewer builds attached. Local and pull request
builds use the `0.0.0-dev` placeholder in `src/Directory.Build.props`.

After each release, raise `PackageValidationBaselineVersion` in
`src/Directory.Build.props` to the version just published and delete any
`CompatibilitySuppressions.xml` files. See
[Versioning and compatibility](docs/versioning.md).

macOS viewer builds are signed and notarized on pushes to `main` and tags
when the `APPLE_DEVELOPER_CERTIFICATE_P12`,
`APPLE_DEVELOPER_CERTIFICATE_PASSWORD`, `APPLE_SIGNING_IDENTITY`, `APPLE_ID`,
`APPLE_TEAM_ID` and `APPLE_APP_PASSWORD` secrets are set. Pull requests and
forks build unsigned.

## Bugs, features and security

Use the [issue templates](https://github.com/philliphoff/EncDotNet/issues/new/choose).
For questions, see [SUPPORT.md](SUPPORT.md). Report security
vulnerabilities privately as described in [SECURITY.md](SECURITY.md), not in a
public issue.

## License

By contributing, you agree that your contributions are licensed under the
[MIT License](https://github.com/philliphoff/EncDotNet/blob/main/LICENSE).
