# P1-30 — Post Publish Revision

**Priority:** P1  
**Source:** [TODO.MD item 30](../TODO.MD) (line 63); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 lifecycle/rating; optional P1.

## Goal

**P1:** Post-publish card edit/re-score, unconfirmed banner, rating preview clearly marked unawarded, before/after history, and decision Reset before milestones.

## Implementation

Allow owner edits after publish without changing catalog until reconfirm. Show unconfirmed banner, separate unawarded rating preview, before/after history, and Reset decision before milestones per §6.

## Acceptance

Edit published card, inspect unchanged catalog and labelled preview, reconfirm and see delta/history; reset pending before milestone only.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
