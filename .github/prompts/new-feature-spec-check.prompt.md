---
agent: agent
description: Run a spec-alignment review before implementing a feature.
---

# New feature — spec alignment check

You are reviewing a proposed feature for the EncDotNet repository **before
any implementation work begins**.

## Steps

1. **Identify affected areas.** From the feature description and any files
   or paths mentioned, list every standard touched (ISO/IEC 8211, S-57,
   S-52) and every package touched (`EncDotNet.Iso8211`, `EncDotNet.S57`,
   `EncDotNet.Noaa`, `EncDotNet.ChartViewer`).
2. **Load the matching skills** from `.github/skills/<name>/SKILL.md`:
   `iso8211`, `s57-enc`, `s52-presentation`, `chart-viewer`. For features
   that cross layers, load all of them.
3. **Read the standard.** Find the governing sections in `standards/md/`.
4. For each standard, produce:
   - Section numbers that constrain the design.
   - API shape implications: new types, changes to existing public types,
     and which package owns the change.
   - Encoding details that must be honoured (field and subfield formats,
     lexical levels, update instructions, topology rules).
   - Test fixtures required, built in memory (never real chart files).
   - Compatibility risk for the published packages, including
     [EncDotNet.S100](https://github.com/philliphoff/EncDotNet.S100), which
     consumes `EncDotNet.Iso8211` and `EncDotNet.S57`.
5. **Reconcile** conflicts between the standards and the existing model (for
   example an S-52 display rule that needs data the S-57 model does not
   expose yet).
6. Produce a **final implementation plan** with an ordered task list, and
   only then proceed (or hand off) to coding.

## Output format

```text
## Affected standards and packages
- <standard or package>: <one-line reason>

## Per-standard constraints
### <standard>
- Section refs:
- API implications:
- Encoding details:
- Tests:
- Compatibility risk:

## Reconciliation notes
...

## Implementation plan
1. ...
2. ...
```

Do not write code until this review is complete and acknowledged.
