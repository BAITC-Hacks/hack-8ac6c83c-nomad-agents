# P0-26 — Tailwind shadcn/ui Stack

**Priority:** P0  
**Source:** [TODO.MD item 26](../TODO.MD) (line 56); [MVP_SPEC.md](../../MVP_SPEC.md)  
**Dependencies:** P0 frontend integration.

## Goal

Add the specified Tailwind and shadcn/ui frontend setup, or record an explicit approved deviation from the required stack. The current frontend uses React, Vite, and TypeScript with hand-written styles and fetch calls.

## Implementation

Install/configure Tailwind and shadcn/ui per spec §§2, 8 and preserve current flow styling. If a deviation is approved, record who approved it and exact scope in README.

## Acceptance

Web build succeeds and representative wizard/catalog/decision controls use the configured stack, or approved deviation is documented.

## Handoff

Report changed files, any contract change and client regeneration, build result, manual checks actually run, and remaining blockers. Keep application state in the singleton store; do not add automated tests or commit secrets.
