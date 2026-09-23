# P0-13 — Rating Explanations

**Priority:** P0  
**Source:** [TODO.MD item 13](../TODO.MD) (line 34); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 readiness rubric.

## Goal

Return criterion scores, weights, evidence/reasons, missing details, ordered quests with point ceilings, level, next threshold, rules version, and cache/source metadata. Identical confirmed fields under one rules version must yield the same result.

## Implementation

Return RatingDto with each criterion weight/score/reason/matchedSignals, up to three missing details, quests sorted by remaining points, level/next threshold, source, rules version and scoredAt.

## Acceptance

Inspect rating response: total equals breakdown sum; quest ceilings equal weight minus score; next-level points are threshold minus total.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
