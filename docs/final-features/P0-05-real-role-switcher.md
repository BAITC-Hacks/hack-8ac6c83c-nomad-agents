# P0-05 — Real Role Switcher

**Priority:** P0  
**Source:** [TODO.MD item 5](../TODO.MD) (line 20); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 actors and client integration.

## Goal

Connect the role switcher to real actors and remove the automatic browser-sample fallback from the MVP path once the API is ready. Keep any sample mode explicitly separate and labelled.

## Implementation

Load actor lists from GET /api/actors, persist current choice appropriately, and send X-Actor-Role/Id on protected calls. Remove automatic demo fallback from the normal app; label any explicit sample entry.

## Acceptance

Reload and switch business→team→business without losing real API data or showing another actor's cached private view.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
