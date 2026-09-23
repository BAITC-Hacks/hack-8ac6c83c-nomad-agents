# B7 — Repeatable demo seed and guarded admin route

## Goal

Make a clean local emulator launch immediately usable for the mandatory demo without manual database editing. Protect the destructive seed/reset operations outside local development.

## Starting point and files

`api/Features/Admin/Endpoints.cs` currently returns success without touching Firestore. The spec describes `seed/demo/*.json`, but fixture files are not present in the current scaffold. Own `api/Features/Admin/`, `seed/demo/`, and any dedicated seed loader. Use B1 repositories and B3's scoring service.

## Implement

- Create an idempotent `POST /api/admin/seed?profile=demo`: clear the demo collections safely, then load at least three businesses, two teams, two published cards at contrasting readiness levels, two proposals on one card for side-by-side business choice, and a weak draft text for the live wizard. Repeated seed produces the same IDs/data and no duplicate proposals.
- Fill all card fields and confirmed snapshots consistently. Calculate seeded ratings with the same versioned B3 rules; do not hardcode totals that disagree with fields. Nomad should appear Priority above a Workable Steppe card. Team profiles must include interests, skills, and tech tags.
- Protect admin mutations: allow local emulator/development only, or require a server-side secret checked before mutation. In public deployment, disable them by default. Return a clear failure for unsupported profile values; the `full` profile is outside this selected mandatory-demo scope. A mapped `/reset` route must use the same guard or be unmapped.
- Make `/api/health` report meaningful API/Firestore reachability without leaking configuration or secrets. Avoid claiming Firestore is healthy from a hardcoded string.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. Seed twice against a fresh emulator; `/api/actors` and `/api/catalog` show stable counts/order, two proposals are available to the owning business, and the weak draft can be pasted into a new task. Invalid profile and disabled/public admin mutation fail clearly.

## Boundary

Do not build the full five-of-each dataset, deploy infrastructure, or frontend pages here. B7 owns only demo fixtures, seed/admin guard, and health behavior.
