# P0-07 — AI Response Validation

**Priority:** P0  
**Source:** [TODO.MD item 7](../TODO.MD) (line 25); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 analyze endpoint.

## Goal

Validate field keys, 3–7 questions, at most four chips per question, suggestions, and extracted values against exact draft evidence. Never put AI suggestions into card fields without a user action.

## Implementation

Validate allowed field keys, 3–7 questions, assigned stable q IDs, at most four clean chips, actionable suggestions, and verbatim extraction value/evidence in raw draft. Treat title as an untrusted suggestion.

## Acceptance

Malformed keys/counts trigger invalid handling; invented extraction is dropped and cannot enter editable card fields.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
