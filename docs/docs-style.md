# Documentation style guide

This guide covers how to write the EncDotNet developer documentation: the
pages under `docs/`, the per-package `README.md` files under `src/`, and the
repository `README.md`. It follows the
[EncDotNet.S100 documentation style guide](https://github.com/philliphoff/EncDotNet.S100/blob/main/docs/docs-style.md),
so pages from both repositories read the same way when they're published
together.

[Getting started](getting-started.md) and the [documentation home](index.md)
are the reference examples. When this guide doesn't answer a question, do what
they do.

## Principles

1. **Write for someone with a task.** Readers arrive wanting to read a cell,
   apply updates, verify an exchange set or decode an ISO 8211 file. Tell them
   how, in the order they'll do it.
2. **Describe; don't promote.** Say what a feature does and when to use it.
   Don't say how good, fast, easy or complete it is.
3. **Everything you show must work.** Commands run in the order given, and
   code compiles against the current API.
4. **Say it once.** Each fact has one home. Other pages link to it instead of
   restating it.

## Page types

Every page is one of these types. Choose the type before you write, and use its
structure.

### Quickstart

Gets a new reader from nothing to one working result. Example:
[Getting started](getting-started.md).

1. One or two sentences on what the reader will do.
2. **Prerequisites**: what to install or have first. Omit if there are none.
3. Numbered or titled steps that run in order. Show the expected result after
   the step that produces it.
4. **Next steps**: two to five links, each with a few words on what it covers.

Leave options, variations and background to the guides. A quickstart takes
the shortest path that works.

### How-to guide

Solves one specific task for a reader who already has the basics. Example:
[Download NOAA ENC cells](download-noaa-charts.md).

- Title it with the task, as an imperative: "Download NOAA ENC cells".
- Start with a sentence on what the result is and when you'd want it.
- Prerequisites, then numbered steps, then the result.
- Put alternatives and edge cases at the end, under headings that name them.

### Guide

Explains one area of the libraries in depth, with examples. Example:
[Reading S-57 charts](reading-s57-charts.md).

- Open with a short paragraph on what the area covers and which types are the
  entry points.
- Organize sections around what the reader wants to do, such as "Apply update
  cells" or "Verify an exchange set". Don't organize around the type
  hierarchy.
- Each section gives a short explanation, a code example, and any caveats.

### Reference

Lists everything in a surface area, for lookup. Example: the
[S-57 glossary](glossary.md).

- Make it complete and consistently ordered. Tables suit terms, codes and
  options.
- Keep explanation short and link to guides for anything longer.

### Design note

Records why a shipped subsystem works the way it does, for contributors.
Design notes live in `docs/design/`.

- Cover the problem, the constraints, the decision and the alternatives you
  rejected.
- Design notes may discuss internals; the other page types shouldn't.

### Project README

Describes one package in `src/<project>/README.md`. It's also the package's
readme on nuget.org.

- First paragraph: what the package does and when to reference it.
- Then: install, a minimal example, the main entry points, and links to the
  relevant guides.

## Headings

- Use one `#` heading per page, matching the `toc.yml` entry.
- Use sentence case: "Apply update cells", not "Apply Update Cells".
- Make headings describe their content. Task sections use imperatives
  ("Install", "Read a cell"); concept sections use nouns ("Chain-node
  topology").
- Don't skip levels (`##` then `####`).
- Don't use template headings that describe the reader's journey instead of
  the content: "Why it matters", "Quick win", "Deep dive", "Overview" as a
  catch-all, or a closing "Troubleshooting" section that holds general tips.
  Put a real known problem next to the step it affects.
- Heading text becomes the link anchor (`## Apply update cells` becomes
  `#apply-update-cells`). Other pages and the repository README link to
  anchors, so search for links before you rename a heading.

## Voice and tone

- Address the reader as "you". Use present tense and active voice: "The
  reader applies the update", not "The update will be applied".
- Write steps as imperatives: "Run", "Open", "Replace".
- Use contractions ("don't", "isn't", "you'll"). They read as plain speech.
- Keep sentences short. One idea per sentence; one topic per paragraph.
- Don't use exclamation marks, emoji or badges. The repository `README.md`
  may keep its status badges; docs pages and package READMEs don't use them.
- Don't claim speed or ease ("in minutes", "a few lines", "simply", "just",
  "easy"). If something is short, the reader will see that.
- Avoid marketing and filler words, including: *seamless*, *powerful*,
  *robust*, *blazing*, *best-in-class*, *world-class*, *cutting-edge*,
  *batteries-included*, *out of the box*, *on-ramp*, *leverage*, *unlock*,
  *get to value*, *deep dive*.
- Don't use internal language like issue numbers or sprint names in
  reader-facing text. Issue links are fine in design notes and in caveats
  that point to a tracked limitation.

## Names and terms

| Write | Not |
|---|---|
| EncDotNet (this project and its S-57 and ISO 8211 libraries) | ENC.NET, EncDotNet.S100 |
| EncDotNet.S100 (the S-100 project built on these libraries) | EncDotNet |
| `EncDotNet.S57`, `EncDotNet.Iso8211`, `EncDotNet.Noaa` (packages, in code style) | the S57 library, the parser |
| S-57, S-52, S-101, ISO/IEC 8211 (ISO 8211 after first use) | S57, S 57, ISO8211 |
| cell, base cell, update cell, exchange set | chart file, delta, exchangeset |
| feature record, vector record, object class, attribute | object record, feature type |
| `DEPARE`, `SOUNDG`, `DSID`, `SG2D` (S-57 acronyms, in code style) | Depare, depth-area object |
| macOS, Windows, Linux | Mac OS, OSX |

Define an S-57 acronym on first use on a page, or link it to the
[glossary](glossary.md).

## Steps and code

- Use a numbered list for a sequence of actions in one place. Use `###`
  headings for steps that each need a code block or more than a short
  paragraph.
- Use one action per step. After a step with a visible result, say what the
  reader should see.
- Tag every code fence with its language: `bash`, `powershell`, `csharp`,
  `xml`, `json`, `text` for output and diagrams.
- Use one command per line and no `$` prompt. Keep command output in a
  separate `text` block.
- Use placeholders a reader can recognize and replace: `path/to/cell.000`,
  `<version>`. Don't use fake values that look real.
- C# examples must compile against the current API. Copy them from a test or
  script where possible, and keep them as short as the point allows. Show the
  `using` directives the example needs, and don't declare the same variable
  twice in one block.
- Don't show APIs that aren't public, or that need setup the page doesn't
  explain.

## Links

- Use relative links between pages and to package READMEs, so DocFX resolves
  them: `[Reading S-57 charts](reading-s57-charts.md)`.
- Use an absolute GitHub URL for repository files that aren't pages, such as
  source files, scripts, `LICENSE` and folders. Relative links to them break
  when the pages are published inside another site.
- Use `<xref:...>` (for example `<xref:EncDotNet.S57.S57Document>`) to link to
  an API page.
- Make link text say where the link goes: "see [Reading S-57
  charts](reading-s57-charts.md)", not "see [here](reading-s57-charts.md)".
- Link to a page's own topic once, where the reader needs it. Don't repeat the
  same set of links at the top and bottom of a page.

## Alerts, tables and images

- Use DocFX alerts (`> [!NOTE]`, `[!TIP]`, `[!IMPORTANT]`, `[!WARNING]`,
  `[!CAUTION]`) only for information the reader must not miss, placed next to
  the step it affects. Don't open a page with one or use one for decoration.
- Use a table to compare items across the same attributes. Use a list for
  everything else.
- Add a screenshot only when it shows something text can't. Give it alt text
  that says what it shows.
- Use Mermaid diagrams for flows and relationships. Keep them to one idea.

## Check your page

Before you open a pull request:

1. Build the site the way CI does, which fails on broken links and anchors:

   ```bash
   dotnet tool install -g docfx --version 2.78.6
   docfx docfx.json --warningsAsErrors
   ```

2. Preview the output in `_site/` (`docfx docfx.json --serve`) and read the
   page from top to bottom as a new reader.
3. Compile each C# example against the current source, or confirm it against
   a test or script that does the same thing.
4. Search the page for the words in [Voice and tone](#voice-and-tone).
