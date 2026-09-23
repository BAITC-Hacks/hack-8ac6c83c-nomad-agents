# P0-27 — README Handoff

**Priority:** P0  
**Source:** [TODO.MD item 27](../TODO.MD) (line 57); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 implemented behavior and manual evidence.

## Goal

Update README with the actual architecture, launch/seed commands, exact rubric and catalog rules, prompt/schema and invalid-response handling, both seed profiles, manual scenarios, and remaining limitations. Do not describe sample scoring as the MVP rating.

## Implementation

Update root README with real architecture, Compose/seed commands, rubric detectors and levels, catalog ranking/filtering, AI prompt/schema/failure path, seed profiles, scenarios and limitations. Distinguish stub from live AI.

## Acceptance

A new developer can launch and run the full flow from README; all claimed results match observed behavior.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
