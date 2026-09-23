# P0-01 — API Contract

**Priority:** P0  
**Source:** [TODO.MD item 1](../TODO.MD) (line 16); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 foundation; coordinate DTO names before generated client.

## Goal

Map the P0 routes in `MVP_SPEC.md` §7: actors; tasks and wizard mutations; catalog and topics; proposals and decisions; admin seed/reset. Keep DTOs and OpenAPI names consistent with the frontend.

## Implementation

Map every P0 route from spec §7 in feature folders. Use typed request/response DTOs, OpenAPI operation names, and /api prefix. Keep optional P1/P2 routes absent until implemented.

## Acceptance

Inspect OpenAPI and call each P0 route with a valid actor; no required route returns 404 and DTO shapes match §7.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
