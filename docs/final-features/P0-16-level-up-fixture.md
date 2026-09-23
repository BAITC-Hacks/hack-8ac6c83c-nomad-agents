# P0-16 — Level Up Fixture

**Priority:** P0  
**Source:** [TODO.MD item 16](../TODO.MD) (line 37); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 rubric, seed and wizard.

## Goal

Prepare an English wizard fixture whose first and second confirmations actually produce a score increase across a level threshold. Playwright's sample run stayed at **70 → 70** after adding data and success criteria, so this required demo result is not yet verified.

## Implementation

Prepare an English Tamaq rehearsal card and exact initial/second field values that the real en-mvp-1 rubric scores across one threshold. Keep data and success criteria additions realistic and user-entered.

## Acceptance

Run two confirmations and record actual score/level before and after; score rises and level changes; repeat run gives same result.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
