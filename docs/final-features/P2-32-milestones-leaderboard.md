# P2-32 — Milestones Leaderboard

**Priority:** P2  
**Source:** [TODO.MD item 32](../TODO.MD) (line 65); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 proposal decisions; optional P2.

## Goal

**P2:** Confirmed milestones with idempotent +10 team points, decision lock, leaderboard, and optional rating-history sparkline.

## Implementation

Add owner-confirmed milestones for selected proposals, idempotent +10 team points, decision lock after first milestone, leaderboard and optional rating-history sparkline.

## Acceptance

Confirm same milestone twice and award only +10 once; locked decision rejects reversal; leaderboard order/points reflect persisted store.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
