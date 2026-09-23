# P0-14 — Revision Safe Confirm

**Priority:** P0  
**Source:** [TODO.MD item 14](../TODO.MD) (line 35); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 lifecycle and rating service.

## Goal

Make confirmation revision-safe and cache repeated confirmations; do not append rating history for an unchanged confirmation. Expose confirmed score changes and previous/current levels to the wizard.

## Implementation

Capture editable revision before scoring, commit snapshot/hash/rating/confirmedAt atomically only if revision still matches. Cache by canonical confirmed fields plus rules version; keep last ten real history entries and compute delta/previous/current levels.

## Acceptance

Repeat confirm unchanged: source cache and no new history; confirm changed fields gives actual delta; concurrent edit yields conflict and no stale catalog snapshot (S6).

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
