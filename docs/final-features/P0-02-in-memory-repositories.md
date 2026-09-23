# P0-02 — In Memory Repositories

**Priority:** P0  
**Source:** [TODO.MD item 2](../TODO.MD) (line 17); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 API contract.

## Goal

Implement repositories over the singleton `InMemoryDataStore` for businesses, teams, tasks, proposals, ratings, and AI logs. All authoritative application state must remain in that process-local store.

## Implementation

Put business, team, task, proposal, rating history, and AI log collections in one singleton InMemoryDataStore. Access them through feature repositories with atomic mutation methods.

## Acceptance

Seed, create, and read records in one API process; restart the API and verify process-local state clears.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
