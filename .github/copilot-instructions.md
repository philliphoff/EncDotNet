# Copilot Instructions for EncDotNet

## Project overview

EncDotNet is a set of .NET libraries for reading, parsing and writing **IHO
S-57 Electronic Navigational Charts (ENCs)** and the **ISO/IEC 8211**
container format they use. The libraries target `net8.0;net10.0`; tests
target `net10.0`. The repository also holds a legacy desktop viewer
(`EncDotNet.ChartViewer`) that is kept building but not developed; chart
display work happens in EncDotNet.S100. `<Nullable>enable</Nullable>` and
`<ImplicitUsings>enable</ImplicitUsings>` apply throughout.

`EncDotNet.Iso8211` and `EncDotNet.S57` are published NuGet packages and are
the foundation of
[EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100), which uses
them to read S-101 and S-401 datasets and to translate S-57 cells to S-101.
Their public APIs are compatibility commitments: call out any breaking change
in the PR description, with its EncDotNet.S100 impact.

## Relevant standards

| Standard | Role in this codebase |
|---|---|
| **ISO/IEC 8211** | Binary container format: leaders, directories, DDR, subfield formats. Read and written by `EncDotNet.Iso8211` |
| **IHO S-57 Edition 3.1** | Transfer standard: data model, feature and vector records, chain-node topology, updating. Modelled by `EncDotNet.S57` |
| **S-57 Appendix A** | Object catalogue (OBJL codes such as DEPARE, SOUNDG, LIGHTS) and attribute catalogue (ATTL codes and value domains) |
| **S-57 Appendix B.1** | ENC Product Specification (Annex A) and encoding guide (Annex D) |
| **IHO S-52 Edition 6.1.1** | ECDIS display rules. Reference only: the legacy viewer approximates them, and full portrayal is in EncDotNet.S100 |

Markdown versions of these documents are in `standards/md/` (PDFs in
`standards/pdf/`). Read the relevant one before changing spec-driven code,
and cite its section.

## Repository layout

```text
src/
  EncDotNet.Iso8211/      # ISO 8211 reader, writer and builders (NuGet)
  EncDotNet.S57/          # S-57 document, chart model, updates, exchange sets (NuGet)
  EncDotNet.Noaa/         # NOAA ENC product catalog client (NuGet)
  EncDotNet.ChartViewer/  # Legacy Avalonia + Mapsui viewer (maintenance only)
tests/
  EndDotNet.UnitTests/    # xUnit tests for the libraries and viewer geometry builders
  EncDotNet.Benchmarks/   # BenchmarkDotNet parsing benchmarks
scripts/                  # File-based C# scripts (`dotnet run scripts/X.cs`)
standards/                # IHO and ISO reference documents (md + pdf)
docs/                     # DocFX conceptual docs
```

Dependency chain: `Iso8211 → S57`, with `Noaa` standalone. The viewer
depends on `S57` and `Noaa`.

## Technology stack

| Concern | Library / tool |
|---|---|
| Language / platform | C#, .NET 10 (libraries also target .NET 8) |
| Legacy viewer | Avalonia 11, ReactiveUI, Mapsui 5, NetTopologySuite, MessagePack |
| Observability | Microsoft.Extensions.Logging, OpenTelemetry |
| Testing | xUnit, coverlet; BenchmarkDotNet |
| Package management | Central Package Management via `Directory.Packages.props` |
| Documentation | DocFX (`docfx.json`) |
| CI/CD | GitHub Actions (`.github/workflows/ci.yml`, `release.yml`, `docs.yml`) |

## Terminology

S-57 and ISO 8211 abbreviations (DDR, DSID, DSPM, COMF, SOMF, FRID, OBJL,
FSPT, VRID, RCNM, SG2D, VRPT, ORNT, USAG, MASK, TOPI and others) are used
directly as type, constant and member names. They are defined in
[`docs/glossary.md`](../docs/glossary.md); the `s57-enc` and `iso8211`
skills explain how the code models them.

## Requirements for changes

### Tests

- Every new public API or bug fix needs an xUnit test under `tests/`.
- Build test input in memory (for example with `Iso8211DocumentBuilder`).
  Never commit real ENC cells or NOAA downloads.
- Run tests with `dotnet test --configuration Release`.

### Documentation

- Each package has a `README.md` in `src/<project>/`, shipped as its NuGet
  readme. Update it when adding types, removing APIs or changing behaviour.
- Conceptual guides live under `docs/`. Add or update pages there for
  user-facing features.
- Follow [`docs/docs-style.md`](../docs/docs-style.md) and build with
  `docfx docfx.json --warningsAsErrors` before finishing; CI does the same.

### Naming and style

- **The normative C# style guide is [`docs/coding-style.md`](../docs/coding-style.md);
  make generated code comply.** It is encoded in
  [`.editorconfig`](../.editorconfig). `EnforceCodeStyleInBuild` is off, so a
  plain `dotnet build` does not flag most style rules; apply the guide
  yourself rather than relying on the build.
- Key rules: `PascalCase` for types, methods and properties; `camelCase` for
  locals and parameters; `_camelCase` for private fields; file-scoped
  namespaces; four-space indents; Allman braces; `System.*` usings first with
  no unused usings.
- S-57 field tags are constants in `S57FieldTags`, subfield names in
  `S57SubfieldNames`, and object codes in the `S57ObjectCode` enum (numbered
  as in Appendix A).
- Parsed data is immutable: `ImmutableArray<T>`, `ImmutableDictionary<K,V>`,
  records. Use `ReadOnlySpan<byte>` on parsing hot paths.
- Avoid `!` null suppression; prefer null checks or
  `ArgumentNullException.ThrowIfNull`.
- All public APIs carry XML doc comments. `GenerateDocumentationFile` and
  `TreatWarningsAsErrors` are on, so a missing comment fails the build.
- Before finishing, run `dotnet format whitespace EncDotNet.slnx` and
  `dotnet format style EncDotNet.slnx --diagnostics IDE0005`. CI verifies
  both with `--verify-no-changes`.

### Package dependencies

- All NuGet versions are managed in `Directory.Packages.props`. Do not add
  `Version` attributes in `.csproj` files.
- Run `gh-advisory-database` security checks before introducing a
  dependency. NuGet audit runs on every build and fails it on advisories.
- Keep the published libraries' dependency footprint small:
  `EncDotNet.S57` depends only on `EncDotNet.Iso8211` and
  `Microsoft.Extensions.Logging.Abstractions`.

### Build

- Build with `dotnet build`. CI builds and tests on Ubuntu and publishes the
  viewer for macOS (arm64), Windows (x64) and Linux (x64); avoid
  platform-specific APIs unless gated to a RID.

## Expertise routing

Per-area **skills** live in `.github/skills/<name>/SKILL.md`, and matching
**instructions files** in `.github/instructions/` apply automatically to the
files they cover. Before designing or implementing a non-trivial change,
load the matching skill(s):

| Signal in task or files touched | Load skill |
|---|---|
| ISO 8211 leaders, DDR, field controls, subfield formats, reader/writer; `src/EncDotNet.Iso8211/**` | `iso8211` |
| S-57 records, fields, object or attribute codes, topology, updates, exchange sets, CATALOG.031; `src/EncDotNet.S57/**` | `s57-enc` |
| Keeping the legacy viewer building; `src/EncDotNet.ChartViewer/**` (maintenance only) | `chart-viewer` |
| Adding, rewriting or reviewing developer docs: `docs/**`, `src/*/README.md`, `README.md` (page types, section structure, tone, compiling examples, DocFX build) | `docs-writing` |

- For changes that cross layers (for example an S-57 change that needs new
  ISO 8211 support), load every affected skill and reconcile them first.
  The `new-feature-spec-check` prompt in `.github/prompts/` walks through
  that review.
- Cite the standard's section number in PR descriptions and in XML doc
  comments for spec-derived constants, enums and members.
