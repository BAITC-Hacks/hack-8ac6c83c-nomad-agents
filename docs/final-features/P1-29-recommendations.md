# P1-29 — Recommendations

**Priority:** P1  
**Source:** [TODO.MD item 29](../TODO.MD) (line 62); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 catalog and team tags.

## Goal

**P1:** Rule-based team recommendations using interest/technology tag matches, with matched-tag explanations and no catalog restriction.

## Implementation

Add rule-based GET /api/teams/{id}/recommendations and team UI using interest/topic plus tech-tag matches. Return matched tags and explanation, rank matches, and leave base catalog unrestricted.

## Acceptance

Team sees why each recommendation matches; unmatched and draft-level tasks remain available in ordinary catalog.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
