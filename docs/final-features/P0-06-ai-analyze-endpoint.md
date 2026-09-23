# P0-06 — AI Analyze Endpoint

**Priority:** P0  
**Source:** [TODO.MD item 6](../TODO.MD) (line 24); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 tasks, actor guard, ResponsesClient.

## Goal

Implement `POST /api/tasks/{id}/analyze` through the OpenAI Responses API with the strict schema, prompt, and field catalog in §5.2. Only the business owner may call it.

## Implementation

Implement owner-only POST /api/tasks/{id}/analyze through ResponsesClient with §5.2 prompt, field catalog, strict JSON schema, model environment variable, and 20-second timeout.

## Acceptance

A real configured call yields schema-shaped AnalysisDto; another business or a team is denied; response does not modify card fields.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
