# P0-19 — Live Task Pages

**Priority:** P0  
**Source:** [TODO.MD item 19](../TODO.MD) (line 43); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 catalog, task read/mine APIs and actor client.

## Goal

Wire My Tasks, owner detail, team task view, and catalog to the real API. Show confirmed card content to teams and keep unconfirmed owner edits out of the catalog.

## Implementation

Wire My Tasks, owner TaskDetail, team TaskView, and Catalog to generated API hooks. Show owner editable state and teams only confirmed content; invalidate relevant queries after mutations.

## Acceptance

Switch actors and inspect same task: owner sees edits, team sees confirmed snapshot; publish/update makes list and detail refresh.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
