# B7 — Repeatable demo seed and guarded admin route

**Priority: P0 — top priority; both seed profiles are submission deliverables.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 9.

## Goal

Make a clean local API process immediately usable for the demo and provide the full source-data minimums for submission. Protect seed/reset operations outside local development.

## Starting point and files

`api/Features/Admin/Endpoints.cs` currently returns success without changing application state. Own `api/Features/Admin/`, `seed/demo/`, `seed/full/`, and any dedicated seed loader. Use B1 in-memory repositories and B3's scoring service.

## Implement

- Create an idempotent `POST /api/admin/seed?profile=demo|full`. `demo` loads three businesses, two teams, two published cards at contrasting readiness levels, two proposals on one card, and Tamaq's weak draft text for the live wizard. `full` includes demo content plus enough in-memory records for at least five distinct drafts, five complete task cards, five team profiles, and five proposals immediately after seeding; live wizard activity never counts toward these minimums. Repeated seed produces the same IDs/data and no duplicate proposals.
- Fill all card fields and confirmed snapshots consistently. Calculate seeded ratings with the same versioned B3 rules; do not hardcode totals that disagree with fields. Nomad should appear Priority above a Workable Steppe card. Team profiles must include interests, skills, and tech tags.
- Protect admin mutations: allow local Development only, or require a server-side secret checked before mutation. In public deployment, disable them by default. Return a clear failure for unsupported profile values. A mapped `/reset` route must use the same guard or be unmapped.
- Keep `/api/health` truthful with `storage: "in_memory"`. Do not imply durable or shared storage.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. Seed `demo` twice in one API process; `/api/actors` and `/api/catalog` show stable counts/order, two proposals are available to the owning business, and the weak draft can be pasted into a new task. Seed `full` twice and verify in-memory counts of at least five drafts, five complete cards, five teams, and five proposals without duplicate IDs; score breakdowns match B3's rules. Restart the API and verify state is empty until seed runs again. Invalid profile and disabled/public admin mutation fail clearly.

## Boundary

Do not deploy infrastructure or build frontend pages here. B7 owns both fixture profiles, seed/admin guard, and health behavior.
