# P0-15 — Live Rating Coach

**Priority:** P0  
**Source:** [TODO.MD item 15](../TODO.MD) (line 36); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 rating response and wizard.

## Goal

Wire the RatingPanel and Coach to backend ratings, including progress markers, breakdown, missing details, next level, quest **Add details** field focus, confirmed delta, and genuine level-up notice. Remove sample scores from the authoritative path.

## Implementation

Render backend rating in RatingPanel and Coach: weighted breakdown, reasons, progress thresholds, missing details, quest Add details focus, confirmed delta, and level-up only on actual threshold crossing.

## Acceptance

S4–S5 show backend values; quest focuses correct card field; no sample-only score or false level-up appears in normal mode.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
