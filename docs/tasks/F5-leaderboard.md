# F5 — Leaderboard

Spec: MVP_SPEC.md §8.3 (T4), §6.6. Cut-list item #4 (keep confirmed points on team badge if cut).

## Goal
Simple points leaderboard, lowest priority of the frontend tasks.

## Files to touch
- `web/src/pages/Leaderboard.tsx` (route `/leaderboard`) — table of teams sorted by `points desc`, from `GET /leaderboard`.

## Depends on
None.

## Acceptance checks (manual)
- Order matches `points desc` from the API (no client-side re-sort logic needed beyond trusting API order).
- Updates after a milestone confirmation (F4) via TanStack Query invalidation on `/leaderboard`.

## Do not touch
- `web/src/api/**` (generated).
