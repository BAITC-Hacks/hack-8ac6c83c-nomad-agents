# P0-10 — Live Wizard Analysis

**Priority:** P0  
**Source:** [TODO.MD item 10](../TODO.MD) (line 28); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 analysis and answers APIs, actor client.

## Goal

Wire the existing wizard to these endpoints. Verify chips, free text, suggestions, extract evidence with **Use**, read-only applied answers, loading, and errors against live API responses.

## Implementation

Connect Draft/Clarify/Card wizard steps to task, analyze, and answers APIs. Keep chips as user-selected options; render free text, suggestions, extracted evidence and explicit Use; show loading, errors, AI/stub source, and read-only applied answers.

## Acceptance

Complete S3–S4 and S11 in live API mode; no suggestion or extracted value appears in the card before the user accepts it.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
