# B8 — End-to-end backend integration and OpenAPI handoff

**Priority: P0 — top priority integration gate.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 7, 14–15.

## Goal

Resolve cross-feature wiring and verify that one running API supports every mandatory backend transition from weak business draft to manual proposal decision. This brief is for integration fixes, not a second implementation of B2–B7.

## Starting point and files

Work after B2–B7. Inspect `api/Program.cs`, feature endpoint mappings, service registrations, DTOs, and the generated OpenAPI document. Coordinate fixes with the owning brief when a service contract differs. Own only shared wiring and small integration corrections; report any larger gap to its feature owner.

## Implement

- Ensure each P0 route in [README](README.md) is mapped once and uses real services/repositories. Unmap unfinished P1/P2 routes (`recommendations`, `rating-preview`, `milestones`, `leaderboard`, public AI-log viewer, standalone score) or return explicit unavailable responses; never leave placeholder success DTOs.
- Check request/response casing, enum values, IDs, validation problem shape, actor headers, ownership behavior, and task/proposal visibility across endpoints. The task and catalog DTOs must expose what the frontend needs: confirmed card, rating/quests, catalog position, proposal status, and previous/current rating for the level-up display.
- Keep built-in OpenAPI accurate for the final mandatory contract; provide a short backend contract note and the OpenAPI URL for frontend agents to regenerate their client. Do not edit frontend or generated client files in this backend brief.
- Confirm fallback analysis works while rating remains identical for identical fields in live/stub AI modes. Ensure no unconfirmed fields are exposed through team/catalog reads and no stub endpoint reports fake success.

## Acceptance

Run `dotnet build api/TaskForge.Api.csproj`. Against the local emulator, manually seed demo, switch actors, create a weak task, analyze 3–7 questions with ≤4 chips, apply answers once, edit the card, confirm and inspect deterministic breakdown/quests, improve and reconfirm for actual delta/level-up, publish, browse and filter catalog by topic and level, submit a proposal, and Select / Reject it as the business. Retry Apply identically and with changed input; verify unchanged retry vs `409`. Seed full and verify all five-of-each persisted minimums. Try wrong actor headers and invalid input. Record observed response/contract differences for frontend handoff and the README scenarios.

## Boundary

Do not implement frontend screens, P1 recommendations/preview/history/AI-log viewer, P2 milestones/leaderboard, or Cloud Run deployment. Do not add automated tests under `AGENTS.md`'s hackathon rule.
