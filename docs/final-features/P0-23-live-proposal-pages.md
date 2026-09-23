# P0-23 — Live Proposal Pages

**Priority:** P0  
**Source:** [TODO.MD item 23](../TODO.MD) (line 50); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 proposal and decision APIs, actor client.

## Goal

Wire proposal submission, editing, My Proposals status, and owner review controls to the API. Show field validation errors and refresh affected task, catalog, and proposal views after mutations.

## Implementation

Wire ProposalForm, MyProposals, and ProposalReview to typed API calls. Render field errors and pending-only edit controls; invalidate task, catalog, proposal, and owner views after mutations.

## Acceptance

S7–S9: submit, catch invalid URL, review, select, and observe team status without refresh or stale count.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
