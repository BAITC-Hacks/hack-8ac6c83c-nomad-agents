# P0-08 — AI Retry Stub Logs

**Priority:** P0  
**Source:** [TODO.MD item 8](../TODO.MD) (line 26); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 analyze endpoint and store.

## Goal

On an invalid response or call failure, retry once, then return the local question stub. Record each attempt and fallback in AI logs; show the correct AI/basic badge in the wizard.

## Implementation

For malformed/failed AI calls retry once, log each attempt with validation/error/latency, then return deterministic 3–5 question stub and source=stub. AI_MODE=stub skips network and logs fallback.

## Acceptance

Force malformed output and a failed retry; verify two attempts plus fallback in store and a basic-mode badge; stub answers remain usable.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
