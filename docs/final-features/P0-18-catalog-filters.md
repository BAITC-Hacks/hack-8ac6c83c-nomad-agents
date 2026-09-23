# P0-18 — Catalog Filters

**Priority:** P0  
**Source:** [TODO.MD item 18](../TODO.MD) (line 42); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 confirmed catalog.

## Goal

Support topic and multi-level filters, including visible draft-level cards and proposals at every level. Return score, level, position, business, tags, and proposal count; retain priority highlighting.

## Implementation

Accept topic and multi-level filters while preserving global position. Include draft-level published cards and allow proposals at every level. Return score, level, business, tags, position and proposalCount; mark priority.

## Acceptance

S1–S2: Workable filter retains #2 of 2; low-score published task remains visible and proposal-enabled.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
