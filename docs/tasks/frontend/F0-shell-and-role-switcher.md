# F0 — App Shell, RoleSwitcher, Actor Context

**Priority: P0 — top priority.** Source: [MVP_SPEC.md](../../../MVP_SPEC.md) §§2, 7, 8.1.

Spec: MVP_SPEC.md §8.1, §3 (actor headers), §7 (`/actors`).

## Goal
Global app chrome and the demo identity mechanism every other frontend task depends on.

## Files to touch
- `web/src/lib/actor.tsx` — React context storing `{ role: 'business'|'team', actorId }`, persisted to `localStorage`.
- `web/src/lib/http.ts` — orval custom mutator: base URL from `VITE_API_BASE_URL`, injects `X-Actor-Role` / `X-Actor-Id` headers from actor context.
- `web/src/components/RoleSwitcher.tsx` — grouped select (Businesses / Teams), populated from `GET /actors`.
- `web/src/components/AppShell.tsx` / top bar — "TaskForge" name + "AI Challenge Coach" label, RoleSwitcher, nav links that change by current role. Add the AI logs link with the P1 viewer; add Leaderboard navigation with P2.
- `web/src/components/Toaster` wiring — ProblemDetails `title` + first field error surfaced as toast.
- Router setup (React Router or equivalent) with route stubs for all pages listed in other F-briefs.

## Depends on
Use the generated client (`web/src/api/`, from `scripts/gen-client.sh`) after the backend OpenAPI contract is ready. If OpenAPI/orval blocks P0 integration for more than 10 minutes, use the spec §12 minimal typed fetch-wrapper fallback for P0 routes and return to generation after the flow works. Never edit generated files directly.

## Acceptance checks (manual)
- Switching role in the selector changes nav links and persists across reload.
- Every mutation call carries `X-Actor-Role`/`X-Actor-Id` (check network tab).
- A forced 400 from the API shows a toast with the ProblemDetails title + first error.
- Business role sees My Tasks and New task; team role sees Catalog and My proposals. The actor selector includes the three demo businesses and seeded teams. Optional links appear only when their P1/P2 pages exist.

## Do not touch
- `web/src/api/**` (generated, orval-owned).
- Any backend code.
