# P1-31 — AI Log Viewer

**Priority:** P1  
**Source:** [TODO.MD item 31](../TODO.MD) (line 64); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 AI log persistence; optional P1.

## Goal

**P1:** Real AI log read endpoint and expandable viewer showing prompt, input, output, validation, errors, and latency. Document these even if the viewer is deferred.

## Implementation

Expose bounded GET /api/ai-logs?taskId= and expandable UI for prompt, input, raw output, validation errors, source and latency. Apply actor/access rules before returning sensitive task data.

## Acceptance

Force success and fallback, then inspect corresponding log entries in viewer; README documents log shape even if UI is deferred.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
