# F0 — App Shell, RoleSwitcher, Actor Context

Spec: MVP_SPEC.md §8.1, §3 (actor headers), §7 (`/actors`).

## Goal
Global app chrome and the demo identity mechanism every other frontend task depends on.

## Files to touch
- `web/src/lib/actor.tsx` — React context storing `{ role: 'business'|'team', actorId }`, persisted to `localStorage`.
- `web/src/lib/http.ts` — orval custom mutator: base URL from `VITE_API_BASE_URL`, injects `X-Actor-Role` / `X-Actor-Id` headers from actor context.
- `web/src/components/RoleSwitcher.tsx` — grouped select (Businesses / Teams), populated from `GET /actors`.
- `web/src/components/AppShell.tsx` / top bar — "TaskForge" name + "AI Challenge Coach" label, RoleSwitcher, nav links that change by current role, "AI logs" link.
- `web/src/components/Toaster` wiring — ProblemDetails `title` + first field error surfaced as toast.
- Router setup (React Router or equivalent) with route stubs for all pages listed in other F-briefs.

## Depends on
Generated client must already exist (`web/src/api/`, from `scripts/gen-client.sh`) — coordinate with Dev B before starting; do not hand-write API calls.

## Acceptance checks (manual)
- Switching role in the selector changes nav links and persists across reload.
- Every mutation call carries `X-Actor-Role`/`X-Actor-Id` (check network tab).
- A forced 400 from the API shows a toast with the ProblemDetails title + first error.
- Business role sees business nav (My Tasks, ...); Team role sees team nav (Catalog, My proposals, Leaderboard).

## Do not touch
- `web/src/api/**` (generated, orval-owned).
- Any backend code.
