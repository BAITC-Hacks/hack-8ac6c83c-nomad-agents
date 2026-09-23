# B6 — Team proposals and manual business decisions

**Priority: P0 — top priority for proposal submission and manual Select / Reject.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 6.5, 7.

## Goal

Let any valid team propose on any published task, then let only the owning business compare and manually select or reject proposals. Decision Reset is P1. No automatic assignment or AI team choice.

## Starting point and files

`api/Features/Proposals/Endpoints.cs` currently returns empty arrays or placeholder proposals. `api/Infrastructure/Firestore/ProposalRepository.cs` already derives a stable task/team ID. Own feature endpoints/DTOs/service; B1 owns repository serialization and transaction helpers.

## Implement

- `PUT /api/tasks/{taskId}/proposals/mine`: team actor only; accept idea (20–2000), plan (20–3000), timeline (3–200), and optional valid HTTP(S) prototype URL. Team ID comes from actor headers, not the request. Upsert one proposal per team/task while status is `pending`; retries update that record rather than creating duplicates. No cap on different teams.
- `GET /api/tasks/{taskId}/proposals`: owning business only, return all proposals with team name/tags and decision state. `GET /api/proposals/mine`: team only, return its proposals and task titles.
- `POST /api/proposals/{proposalId}/decision`: owning business only; P0 accepts `selected|rejected` and an optional reason. Manual selection of zero, one, or several teams is valid. P1 adds Reset to `pending` before any milestone; the full spec allows transitions between selected and rejected until a milestone. Return `409` for illegal state/revision; do not assign a team automatically.
- Keep `proposalCount` accurate for catalog display using a transaction or derive it from the proposal query; do not let retries inflate it. Hide the scaffold's unimplemented milestone and leaderboard routes for this mandatory-demo scope.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. With two seeded teams, manually submit two proposals for one published card, update one pending proposal, reject one and select the other as the business, then inspect both team and business views. Check bad URL/short idea, wrong owner, unpublished task, and another team's proposal edit are rejected. No duplicate proposal or count appears after a retry.

## Boundary

Do not implement milestone points, leaderboard, recommendations, AI matching, or frontend forms in P0. B6 owns proposal HTTP behavior and decision logic; milestones and leaderboard are P2.
