# AGENTS for agent-rules-books

This file defines the full-strength working rules for maintaining this repository.

## Scope and intent

- This repository publishes AI-agent rule sets derived from programming books and adjacent sources.
- The repository is organized around three strengths of rule delivery: `full`, `mini`, and `nano`.
- The goal is decision-faithful compression and release, not generic summary writing.
- Project guidance here controls repository maintenance; book files control the behavior prescribed by a given book.

## Repository map

- `<book>/<book>.md` is the canonical public full rule set for that book.
- `<book>/<book>.mini.md` and `<book>/<book>.nano.md` are the released compressed variants.
- `_rule-workbench/<book>/full.md` mirrors the canonical full source for workbench use and must not become a competing source of truth.
- `_rule-workbench/<book>/mini.md`, `nano.md`, and `traceability.md` are the active workbench deliverables for compression work.
- `docs/ADDING_THE_BOOK.md` defines the new-book import flow.
- `_rule-workbench/PROCESS.md` defines compression rules and validation criteria.
- `_rule-workbench/RELEASE.md` defines how workbench outputs become released artifacts.
- `_rule-workbench/CHECK_COMPATIBILITY.md` defines compatibility analysis and matrix maintenance.
- `docs/USAGE.md` defines how the repo's rule strengths should be used across Codex, Claude Code, Cursor, and retrieval-based setups.

## Priority and behavior

- Treat explicit repository workflow in local docs as authoritative over ad hoc preferences.
- Keep changes minimal, targeted, and reversible unless the task is explicitly a larger rework.
- Prefer local evidence from repository files over memory, reputation, or generic software-engineering folklore.
- Preserve the distinction between repository process rules and the content of a book-derived rule set.
- When a task is ambiguous, choose the action that preserves source fidelity, traceability, and reproducibility.

## Source-grounded content rules

- Do not invent book rules that are not supported by the source extraction or current repository process.
- Do not smooth away a book's distinctive bias just because a more generic phrasing feels cleaner.
- Do not import rules from another book to "improve" the current one unless the task is explicitly comparative.
- Preserve modal strength appropriately: obligations should stay stronger than defaults, and prohibitions should remain explicit.
- Keep anti-patterns, trigger rules, tradeoff rules, and stop conditions visible when they materially change agent behavior.
- If evidence is missing, state the gap and stop short of unsupported claims.

## Book addition workflow

- Use lowercase kebab-case for book slugs and directories.
- For a new book, follow `docs/ADDING_THE_BOOK.md` in order:
  - extract the complete outline and operational rules
  - expand until materially complete
  - produce a full rule file in this repository's full standard
  - review it for source fidelity and modal accuracy
  - place the approved file in `_rule-workbench/<book>/full.md`
  - run the compression workflow from `_rule-workbench/PROCESS.md`
  - run the release workflow from `_rule-workbench/RELEASE.md`
- Do not skip the review step between generation and import.

## Compression rules

- Treat the canonical public full file as the source of truth.
- Do not edit workbench `full.md` as if it were an independent document.
- Keep `mini.md` decision-equivalent to the full source, not sentence-equivalent.
- Keep `mini.md` rich enough to preserve book thesis, decision-changing rules, conflict resolvers, and repeated local disciplines that agents commonly miss.
- Keep `nano.md` small enough for compact always-on fallback use while preserving the smallest reminders that block known bad shortcuts.
- Preserve enough book-specific vocabulary that compressed files still feel like the source book rather than a generic style guide.
- Use the required standard shape for compressed files:
  - when to use
  - primary bias to correct
  - decision rules
  - trigger rules
  - final checklist
- Keep required headings consistent with the workbench rules, including the `# OBEY {book name} by {author name}` format for book rule files.

## Traceability rules

- Every retained `mini` rule must be traceable through `traceability.md` with `M*` identifiers.
- Every retained `nano` rule must be traceable through `traceability.md` with `N*` identifiers.
- Each retained rule must point back to the relevant source sections and line ranges in the full file.
- Every omitted or merged source rule must be marked as covered or intentionally lost.
- Do not describe omissions vaguely; name where the operational effect survived or state that it was intentionally dropped.

## Process-bug discipline

- Before patching a compressed book after review, decide whether the issue is a book-specific miss or a reusable process bug.
- If the lesson generalizes across books, update `_rule-workbench/PROCESS.md` first and then re-apply the improved process.
- Do not hand-sculpt one book to match reviewer taste when the actual defect is the compression method.

## Release rules

- Do not modify canonical public full files during release.
- Release only by copying `_rule-workbench/<book>/mini.md` to `<book>/<book>.mini.md` and `_rule-workbench/<book>/nano.md` to `<book>/<book>.nano.md`.
- Keep workbench-only files such as `traceability.md` and workbench `full.md` out of the public release surface unless the task explicitly changes that policy.
- Update `README.md` whenever the released surface changes so the release matrix still matches the actual files.
- Use the repository's deterministic metrics:
  - line count: physical lines
  - file size: raw bytes
  - rule count: actionable Markdown list items according to `_rule-workbench/RELEASE.md`
- Verify links and metrics after release; do not leave README drift behind.

## Compatibility-work rules

- Use canonical `mini` files as the primary evidence base for compatibility judgments.
- Read enough of both sides to identify active pressure before scoring overlap, conflict, or complementarity.
- Cite specific line ranges from both `mini` files; broad whole-file citations are not acceptable.
- Use canonical alphabetical pair paths under `docs/compatibility/<earlier-slug>/<later-slug>.md`.
- Do not create reverse duplicate pair files.
- When compatibility work is active, create and maintain `_rule-workbench/compatibility-pair-workqueue.md`.
- Analyze one unordered pair at a time; do not batch half-finished pair analysis.
- For high-risk pairs named in `_rule-workbench/CHECK_COMPATIBILITY.md`, require external context before claiming `Status: reviewed`.
- Keep the matrix verdict and the detailed pair file verdict synchronized.

## Editing discipline

- Prefer small, precise documentation edits over repo-wide rewrites.
- Keep wording consistent with the repository's existing tone: operational, source-grounded, and tool-agnostic where possible.
- Do not rename slugs, paths, or published files casually; the docs and README link structure depend on them.
- When a public-facing repo convention changes, update the corresponding process or usage document in the same change.

## Stop conditions

- Stop if the relevant source file, process file, or released artifact cannot be found.
- Stop if line citations would no longer be trustworthy and you have not refreshed them.
- Stop if a change would blur canonical full versus workbench versus released file roles.
- Stop if a compatibility verdict depends on reputation, memory, or vague intuition instead of cited local evidence.
- Stop if README metrics or release links would be left stale.

## Final checklist

- Did I use the correct source of truth for this task?
- Did I preserve book-specific bias instead of rewriting it into generic advice?
- Did I update compression, traceability, release, and docs in the right order for this change?
- Did I keep public artifacts, workbench artifacts, and helper docs in their intended places?
- Did I verify any README matrix, metrics, links, or compatibility verdicts affected by the change?
