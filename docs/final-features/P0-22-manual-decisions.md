# P0-22 — Manual Decisions

**Priority:** P0  
**Source:** [TODO.MD item 22](../TODO.MD) (line 49); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 proposal repository and owner guard.

## Goal

Implement owner-only proposal comparison and manual Select/Reject decisions. Permit zero or multiple selected teams; never select automatically.

## Implementation

Implement owner comparison list and POST /proposals/{id}/decision with manual Select/Reject. Keep decisions independent so zero or several teams can be selected; never choose on submit.

## Acceptance

Owner sees all teams and fields; selects two or rejects all; non-owner cannot decide; team status reflects explicit choice.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
