# P0-25 — Generated Client

**Priority:** P0  
**Source:** [TODO.MD item 25](../TODO.MD) (line 55); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 stable OpenAPI and actor guard.

## Goal

Generate the typed client and hooks from the API OpenAPI document via `scripts/gen-client.sh`; configure orval and the actor-header mutator. Never hand-edit `web/src/api/`. Wire TanStack Query invalidation and separate actor-dependent cached data.

## Implementation

Create scripts/gen-client.sh and orval config; generate web/src/api/ and hooks from live OpenAPI. Add actor-header mutator and TanStack Query keys/invalidation partitioned by actor. Never hand-edit generated files.

## Acceptance

Regenerate client successfully; build web; switch actor and mutate task/proposal without stale private data.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
