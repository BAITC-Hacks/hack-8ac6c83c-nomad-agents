# P0-03 — Actor Identity Guard

**Priority:** P0  
**Source:** [TODO.MD item 3](../TODO.MD) (line 18); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 repositories and actor DTOs.

## Goal

Return the businesses and teams from `GET /api/actors`; validate `X-Actor-Role` and `X-Actor-Id`, actor existence, ownership, and team-only/business-only operations on protected routes.

## Implementation

Implement GET /api/actors and a shared actor guard for role/id headers, existence, ownership, and role-specific operations. Use store records, not hardcoded frontend IDs.

## Acceptance

Valid business and team actors can use their routes; missing, unknown, wrong-role, and non-owner headers cannot mutate protected resources.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
