# P0-12 — Readiness Rubric

**Priority:** P0  
**Source:** [TODO.MD item 12](../TODO.MD) (line 33); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 task fields and store.

## Goal

Implement the versioned, deterministic backend rubric in §§4.2 and 5.3: context/need 20; data/materials 20; expected result 15; success criteria 15; constraints 10; users 10; business connection 10. Score only confirmed fields; AI must never award points.

## Implementation

Implement versioned en-mvp-1 field-specific 0/half/full rules in §5.3 with seven weights 20/20/15/15/10/10/10. Use confirmed fields only for awards; no AI score input.

## Acceptance

Manually check S18 and one zero/half/full case per criterion; same fields and version always yield same total and level.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
