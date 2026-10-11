# Versioning and compatibility

This page describes how the EncDotNet packages are versioned, what counts as
their public API, and how breaking changes are caught before they ship. It
applies to `EncDotNet.Iso8211`, `EncDotNet.S57` and `EncDotNet.Noaa`. The
chart viewer isn't a package and has no compatibility commitment.

## Version numbers

The packages follow [Semantic Versioning](https://semver.org/) and share one
version number, set by the release tag (`vMAJOR.MINOR.PATCH`):

| Change | Version part |
|---|---|
| A breaking change to the public API or to documented behavior | Major |
| New public API or behavior that existing code keeps working with | Minor |
| A fix that changes no public API | Patch |

Before 1.0, minor releases may also contain breaking changes. From 1.0 on,
only major releases do.

## Public API

The public API is every `public` and `protected` type and member in the
packages, plus their documented behavior. It doesn't include:

- `internal` types and members, including those visible to the test project.
- Behavior a type's documentation calls unspecified, such as the order of
  items in a collection that isn't documented as ordered.
- The chart viewer (`EncDotNet.ChartViewer`) and the scripts in `scripts/`.

## Supported frameworks

Each package targets .NET 8 and .NET 10. Dropping a target framework is a
breaking change.

## How breaking changes are caught

Each pack compares the new package with its last published release on
nuget.org, using
[package validation](https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview).
It fails the build when a public type or member is removed or changed in an
incompatible way, or when the .NET 8 and .NET 10 builds expose different APIs.
The baseline version is `PackageValidationBaselineVersion` in
[`src/Directory.Build.props`](https://github.com/philliphoff/EncDotNet/blob/main/src/Directory.Build.props).

Run the same check locally before you open a pull request:

```bash
dotnet pack EncDotNet.slnx --configuration Release
```

### Make an intentional breaking change

1. Regenerate the suppression file for the package, which records the
   accepted differences:

   ```bash
   dotnet pack src/EncDotNet.S57/EncDotNet.S57.csproj --configuration Release -p:ApiCompatGenerateSuppressionFile=true
   ```

   This writes `CompatibilitySuppressions.xml` next to the project. Commit it.
2. List the break under **Breaking changes** in the pull request, with its
   effect on [EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100).
3. Release it in the next major version (or, before 1.0, the next minor
   version).

After each release, raise `PackageValidationBaselineVersion` to the version
just published, so APIs added in it are checked too, and delete any
suppression files, since the new baseline already contains those changes.

## Debugging into the packages

The packages ship symbol packages (`.snupkg`) to the NuGet symbol server, and
their PDBs carry [Source Link](https://learn.microsoft.com/dotnet/standard/library-guidance/sourcelink)
information pointing at the exact commit they were built from. To step into
EncDotNet code in Visual Studio or VS Code, turn off **Just My Code** and turn
on the NuGet.org symbol server and Source Link support in the debugger
settings.
