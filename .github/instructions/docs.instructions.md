---
applyTo: "docs/**/*.md,src/**/README.md,README.md,index.md"
---

# Documentation editing rules

When adding or editing developer documentation, follow
[`docs/docs-style.md`](../../docs/docs-style.md). For a new page or a
substantial rewrite, load the `docs-writing` skill. In particular:

- Choose the page type first (quickstart, how-to guide, guide, reference,
  design note or project README) and use its section structure.
- Describe what things do. Don't promote them: no marketing words, emoji,
  badges or claims about speed or ease.
- Don't use the "Why it matters / Quick win / Deep dive" template headings or
  a catch-all "Troubleshooting" section.
- Every command and code example must work in the order shown, against the
  current API. Compile C# examples before you commit them.
- Link pages relatively, repository files that aren't pages by absolute
  GitHub URL, and API types with `<xref:...>`.
- Build with `docfx docfx.json --warningsAsErrors` before opening a pull
  request.
