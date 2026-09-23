# P0-20 — SPA Navigation

**Priority:** P0  
**Source:** [TODO.MD item 20](../TODO.MD) (line 44); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 app routing and actor context.

## Goal

Fix internal links that currently trigger a full page reload. In Playwright, opening a catalog task through its ordinary link cleared the selected actor and browser-sample task; in-app navigation must preserve the active session and route.

## Implementation

Replace internal full-reload anchors with router navigation in catalog, task cards, proposal links and page actions. Preserve actor context/session on route changes.

## Acceptance

Open catalog task and return using normal links; no reload occurs, actor remains selected, route renders API-backed task.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
