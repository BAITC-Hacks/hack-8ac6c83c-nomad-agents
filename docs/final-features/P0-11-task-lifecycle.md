# P0-11 — Task Lifecycle

**Priority:** P0  
**Source:** [TODO.MD item 11](../TODO.MD) (line 32); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 task repository, rating service, actor guard.

## Goal

Implement draft creation, field updates, confirm, and publish with the lifecycle rules in §6.1. Publishing requires a title, a confirmed rating, and no unconfirmed changes; low scores must still be publishable.

## Implementation

Implement POST /tasks, PUT /fields, POST /confirm, POST /publish and owner read/mine with §6.1 lifecycle. Validate title and confirmed rating before publish; block unconfirmed edits at publish.

## Acceptance

Create, edit, confirm, publish through API; low-scoring confirmed task publishes; missing title/rating and dirty revision fail clearly.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
