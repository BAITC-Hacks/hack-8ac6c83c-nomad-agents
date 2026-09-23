# P0-28 — Manual Gate

**Priority:** P0  
**Source:** [TODO.MD item 28](../TODO.MD) (line 58); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** All P0 briefs.

## Goal

Run `docker compose up --build`, seed `demo`, and manually verify spec scenarios S1–S9, S11, S14, and S16–S18. Seed `full` and verify its counts. Rehearse the five-minute end-to-end flow twice, including an observed confirmed score increase and level-up, proposal, and business decision.

## Implementation

Run Compose build, seed demo, click S1–S9/S11/S14/S16–S18, seed full and count records. Rehearse §15 five-minute flow twice with actual score increase, level-up, proposal and owner decision.

## Acceptance

Record date, commands, actor/card used, observed scores and outcomes in README; do not mark unrun checks passed.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
