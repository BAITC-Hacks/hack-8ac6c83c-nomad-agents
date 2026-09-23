# P2-33 — Cloud Run Netlify

**Priority:** P2  
**Source:** [TODO.MD item 33](../TODO.MD) (line 66); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 local delivery; optional P2.

## Goal

**P2:** Cloud Run and Netlify deployment only after the local P0 flow works.

## Implementation

Add Cloud Run API and Netlify SPA deployment config/scripts only after local gate. Configure allowed origin, API base URL, environment secrets, SPA deep-link fallback and health checks.

## Acceptance

Deploy and open deep link; run hosted business→catalog→proposal→decision flow; document actual URLs and any remaining hosting limits.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
