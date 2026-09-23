# P0-04 — Validation Conflicts Admin Guard

**Priority:** P0  
**Source:** [TODO.MD item 4](../TODO.MD) (line 19); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 route handlers and actor guard.

## Goal

Return validation failures as `Results.ValidationProblem` and conflicts as appropriate ProblemDetails responses. Protect or disable destructive admin routes outside local development.

## Implementation

Validate payloads at route boundaries with Results.ValidationProblem. Return ProblemDetails for stale revisions, duplicate changed answers, and other conflicts. Restrict seed/reset to local development.

## Acceptance

Invalid fields and URL return field errors, changed answer retry returns 409, and destructive admin calls are unavailable outside local development.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
