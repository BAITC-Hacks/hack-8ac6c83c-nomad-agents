# P0-09 — One Time Answers

**Priority:** P0  
**Source:** [TODO.MD item 9](../TODO.MD) (line 27); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 analysis, task repository.

## Goal

Implement one atomic `PUT /answers` per task. Match question IDs and field keys; merge submitted answers into editable fields once; treat an identical retry as a no-op and a changed retry as a conflict directing the user to the card editor.

## Implementation

Implement atomic PUT /api/tasks/{id}/answers using saved analysis question IDs and field keys. Merge accepted user text into editable fields once; save canonical request/result for idempotent retry.

## Acceptance

First apply changes card once, identical retry leaves revision unchanged, changed retry is 409 with card-editor guidance (S16).

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
