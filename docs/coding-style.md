# C# coding style guide

This is the **normative** coding style guide for the C#/.NET code in
EncDotNet. It makes the repository's style explicit rather than requiring
contributors to infer it from surrounding code.

> **Enforcement.** Mechanically checkable rules are encoded in
> [`.editorconfig`](../.editorconfig). CI gates whitespace with `dotnet format
> whitespace --verify-no-changes` and using directives with `dotnet format
> style --diagnostics IDE0005 --verify-no-changes`. Broader style and naming
> rules remain hand-followed because `dotnet format` cannot safely Fix-All
> naming. When this guide and `.editorconfig` disagree, `.editorconfig` wins.

## 1. Guiding principles

1. Prefer consistency over personal preference.
2. Treat compiler and analyzer warnings as failures; fix causes rather than
   suppressing warnings.
3. Model nullability honestly instead of defeating the analyzer.
4. Use `dotnet format` rather than hand-formatting against the rules.

## 2. Files, namespaces, and usings

- Use one top-level type per file and match the file name to the type. Small,
  tightly coupled helpers and partial types are exceptions.
- Use file-scoped namespaces.
- Keep `using` directives outside the namespace, order `System.*` first, and
  remove unused directives.
- Use UTF-8, LF line endings, a trailing newline, and no trailing whitespace.

## 3. Formatting

- Indent C# with four spaces and never tabs.
- Use Allman braces.
- Brace multi-line blocks. Terse, same-line guards may remain unbraced.
- Keep one statement and one declaration per line.
- Keep lines reasonably short, generally 100-120 columns.
- Use expression-bodied members for readable one-liners.

## 4. Naming

| Element | Convention | Example |
|---|---|---|
| Namespace, type, method, property, event, enum member | `PascalCase` | `S57Document`, `TryParse` |
| Interface | `PascalCase` prefixed with `I` | `IRecordReader` |
| Type parameter | `PascalCase` prefixed with `T` | `TRecord`, `TKey` |
| Local variable, parameter | `camelCase` | `recordLength`, `field` |
| Private/internal instance field | `_camelCase` | `_recordReader` |
| Constant, `static readonly` | `PascalCase` | `FieldTerminator` |
| Async method | `PascalCase` suffixed `Async` | `ReadCatalogAsync` |

Do not prefix member access with `this.`. Prefer descriptive names, while
retaining well-known S-57, S-52, and ISO/IEC 8211 domain abbreviations.

## 5. Language features and idioms

- Prefer `var` for locals unless an explicit type materially improves clarity.
- Prefer target-typed `new`, collection expressions, and range/index operators.
- Prefer pattern matching and switch expressions where they read clearly.
- Prefer string interpolation and use an explicit `StringComparison` where
  culture affects semantics.
- Use `is null` and `is not null` for reference equality checks.

## 6. Nullability and argument validation

- Do not use the null-forgiving operator merely to silence the analyzer.
- Validate public entry-point arguments with the standard
  `ArgumentException` helpers.
- Use `Parse`/`TryParse` pairs for parsing and throw the most specific standard
  exception with a useful message.

## 7. Documentation comments

- Public and protected APIs require XML documentation, including parameters,
  return values, and exceptions where applicable.
- Use `<see cref="..."/>` and `<c>...</c>` for references and inline code.
- Comments explain non-obvious rationale or standards-derived invariants; they
  do not narrate self-explanatory implementation.

## 8. Standards-derived code

For constants, enums, attributes, field tags, subfields, and encodings derived
from IHO S-57, IHO S-52, or ISO/IEC 8211, cite the relevant standard and section
in XML documentation when practical.

## 9. Async

- Suffix async methods with `Async` and return `Task`, `Task<T>`, or
  `ValueTask<T>`.
- Accept and honor `CancellationToken` for asynchronous I/O APIs.
- Do not expose `async void` except for event handlers.
- Avoid `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()`.

## 10. Dependencies and build

- Manage NuGet versions centrally in `Directory.Packages.props`.
- Do not use `#pragma warning disable` or `<NoWarn>` without a specific,
  documented justification.
- Keep code cross-platform and gate platform-specific APIs appropriately.

Before pushing, apply and verify the CI-enforced subsets:

```bash
dotnet format whitespace EncDotNet.slnx
dotnet format style EncDotNet.slnx --diagnostics IDE0005
dotnet format whitespace EncDotNet.slnx --verify-no-changes
dotnet format style EncDotNet.slnx --diagnostics IDE0005 --verify-no-changes
```
