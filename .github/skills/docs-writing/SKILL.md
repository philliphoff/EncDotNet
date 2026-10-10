---
name: docs-writing
description: |
  Writing and reviewing the EncDotNet developer documentation — pages under
  docs/, per-package src/*/README.md files (also shipped as NuGet readmes)
  and the repository README — to the documentation style guide
  (docs/docs-style.md), which matches EncDotNet.S100's: page types and their
  section structure, plain task-focused tone with no marketing language,
  C# examples that compile against the current API, portable links, DocFX
  alerts and the docfx build check. USE FOR: adding or rewriting a docs page
  or README; restructuring sections; reviewing a docs change for structure,
  tone or accuracy. DO NOT USE FOR: XML doc comments (follow
  docs/coding-style.md); the reference standards under standards/ (they are
  converted IHO documents, not authored docs).
---

# Writing EncDotNet docs

The normative rules are in
[`docs/docs-style.md`](../../../docs/docs-style.md). Read it in full before
writing. This skill adds the working procedure and a review checklist.

The reference examples are
[`docs/getting-started.md`](../../../docs/getting-started.md) (quickstart) and
[`docs/reading-s57-charts.md`](../../../docs/reading-s57-charts.md) (guide).
When unsure, match them.

## Procedure

1. **Pick the page type** (quickstart, how-to guide, guide, reference, design
   note, project README) and use its structure from the style guide. If a page
   mixes types, split it or move material to the page that owns it.
2. **Gather facts from the code, not from other docs.** Other pages may be
   stale.
   - API names and signatures: the source under `src/`, and a test that calls
     them (`tests/`) or a script (`scripts/`).
   - Standard terms and section numbers: `standards/md/`.
3. **Check inbound links before renaming or removing anything.** Search the
   repository for the file name and `#anchor`, including `README.md`,
   `index.md`, `docs/toc.yml` and `src/toc.yml`.
4. **Write**, following the style guide. Prefer linking to an existing page
   over restating it.
5. **Compile the examples.** Paste each C# block into a scratch console
   project that references the packages under `src/` and build it. Fix the
   page, not the scratch project.
6. **Review** with the checklist below.
7. **Build and preview** the site; see
   [Check your page](../../../docs/docs-style.md#check-your-page). Fix every
   warning; CI runs with `--warningsAsErrors`.

## Review checklist

- [ ] The page has one `#` title that matches its `toc.yml` entry.
- [ ] Sections follow the structure for the page type.
- [ ] Headings are in sentence case. No template headings: "Why it matters",
      "Quick win", "Deep dive", or a catch-all "Troubleshooting".
- [ ] No marketing or filler words, emoji, badges or time claims. Quick scan:

      ```bash
      grep -n -i -E "seamless|powerful|robust|blazing|best-in-class|world-class|cutting-edge|batteries-included|out of the box|on-ramp|leverage|unlock|in minutes|simply|easily|deep dive|quick win|why it matters" path/to/page.md
      ```

- [ ] Steps run in the order given. Nothing is used before the step that
      installs or creates it.
- [ ] Code fences have a language tag. Commands have no `$` prompt, and
      output is in a separate block.
- [ ] C# examples compile against the current public API.
- [ ] S-57 acronyms are defined on first use or linked to the glossary.
- [ ] Links are relative for pages, absolute GitHub URLs for non-page files,
      and `<xref:...>` for API types. Link text names the target.
- [ ] Alerts are only used for must-not-miss information, next to the step
      they affect.
- [ ] Renamed headings or removed pages have no remaining inbound links.
- [ ] `docfx docfx.json --warningsAsErrors` succeeds.
