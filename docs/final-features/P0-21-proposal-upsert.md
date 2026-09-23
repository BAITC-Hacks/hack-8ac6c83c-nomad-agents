# P0-21 — Proposal Upsert

**Priority:** P0  
**Source:** [TODO.MD item 21](../TODO.MD) (line 48); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 published tasks, actor guard and repository.

## Goal

Implement one proposal per team per published task, with idea, plan, timeline, and optional valid HTTP(S) prototype URL. Allow edits only while Pending and allow any number of teams to propose at every readiness level.

## Implementation

Implement team-only PUT /tasks/{id}/proposals/mine, one proposal per team/task, required idea/plan/timeline, optional HTTP(S) URL, edits only while pending. Do not gate by readiness level.

## Acceptance

Two teams can propose to low-score card; duplicate same-team upsert edits pending record; invalid URL and edit after decision fail.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
