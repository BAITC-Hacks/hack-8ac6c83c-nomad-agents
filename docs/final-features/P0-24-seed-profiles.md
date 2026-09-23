# P0-24 — Seed Profiles

**Priority:** P0  
**Source:** [TODO.MD item 24](../TODO.MD) (line 54); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 store, rubric, admin guard.

## Goal

Add `demo` and `full` seed fixtures and local admin seed/reset. `full` must load at least five drafts, five complete cards, five teams with skills, and five proposals before any user interaction. Seed ratings must be computed by the real rubric.

## Implementation

Add seed/demo and seed/full fixtures, loader, and local POST /api/admin/seed?profile= plus reset. Full profile immediately contains ≥5 drafts, ≥5 complete cards, ≥5 skilled teams, ≥5 proposals; compute ratings with rubric.

## Acceptance

Seed each profile, count all collections before interaction, verify expected levels, reset, and restart to confirm memory clears (S17).

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
