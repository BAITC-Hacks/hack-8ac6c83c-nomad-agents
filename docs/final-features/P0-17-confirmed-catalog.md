# P0-17 — Confirmed Catalog

**Priority:** P0  
**Source:** [TODO.MD item 17](../TODO.MD) (line 41); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 lifecycle and repositories.

## Goal

Implement `GET /api/catalog` and topics from published **confirmed** snapshots. Sort by rating descending, confirmed time descending, then task ID; compute `#n of m` before filters.

## Implementation

Build GET /api/catalog and /api/catalog/topics from published confirmed snapshots. Rank score descending, confirmedAt descending, task ID; assign global #n of m before filtering.

## Acceptance

Edit a published card without confirming: catalog stays on prior snapshot; sorting and topics match §6.3; equal-score ties are stable.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
