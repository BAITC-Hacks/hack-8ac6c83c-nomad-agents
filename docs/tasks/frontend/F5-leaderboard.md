# F5 — Leaderboard

**Priority: P2 — optional after the complete P0 flow.** Source: [MVP_SPEC.md](../../../MVP_SPEC.md) §§2, 6.6, 8.3.

Spec: MVP_SPEC.md §8.3 (T4), §6.6.

## Goal
Simple points leaderboard, lowest priority of the frontend tasks.

## Files to touch
- `web/src/pages/Leaderboard.tsx` (route `/leaderboard`) — table of teams sorted by `points desc`, from `GET /leaderboard`.

## Depends on
- B10's P2 milestone/leaderboard slice and its generated API contract; F0 actor context and F4 milestone action. I0 owns client generation. Do not enable navigation until the real endpoint is available.

## Acceptance checks (manual)
- Order matches `points desc` from the API (no client-side re-sort logic needed beyond trusting API order).
- Updates after a milestone confirmation (F4) via TanStack Query invalidation on `/leaderboard`.

## Do not touch
- `web/src/api/**` (generated).
