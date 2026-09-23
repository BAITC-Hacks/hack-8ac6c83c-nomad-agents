# Backend task queue — mandatory demo

Source: [MVP_SPEC.md](../../MVP_SPEC.md), especially §§4–7 and §15. These briefs target the **current `api/` scaffold**, not a fresh project. The scaffold compiles but most feature endpoints return placeholders. Work through the dependencies below; independent branches may run in parallel after B0/B1. Do not edit frontend files. The product has one AI `analyze` call; the backend owns the score.

| Brief | Result | Depends on |
|---|---|---|
| [B0](B0-contract-and-actor-guard.md) | Shared API contract, actor guard, error conventions | — |
| [B1](B1-firestore-persistence.md) | Firestore connection, mapped models, repositories | B0 |
| [B2](B2-ai-analysis.md) | Real Responses API analysis, validation, fallback, logs | B0, B1 |
| [B3](B3-readiness-engine.md) | Versioned deterministic rating and quests | B1 |
| [B4](B4-task-lifecycle.md) | Draft, answers, edit, confirm, publish | B1, B2, B3 |
| [B5](B5-catalog.md) | Shared ranked catalog and filters | B1, B3, B4 |
| [B6](B6-proposals-and-decisions.md) | Team proposal and manual business decision | B0, B1, B4 |
| [B7](B7-demo-seed-and-admin.md) | Repeatable demo fixtures and guarded seed | B1, B3 |
| [B8](B8-integration-and-contract.md) | Working API journey and final OpenAPI contract | B2–B7 |
| [B9](B9-optional-backend-deployment.md) | **Optional** Cloud Run backend deployment | B8 |

Mandatory API surface: `/api/health`, `/api/actors`, task create/read/mine/analyze/answers/fields/confirm/publish, `/api/catalog` with topic and level filters, `/api/catalog/topics`, task proposal upsert/list, team proposal list, proposal decision, and guarded `/api/admin/seed`. Preserve existing route shapes where present. The standalone score endpoint and rating preview are unnecessary for this demo; show the last confirmed score while editing. Remove unfinished optional routes from the mapped API or make them explicitly unavailable; never return fake success data. Keep DTO names and route responses consistent with the generated OpenAPI document.

The selected scope excludes recommendation ranking, milestones, leaderboard, AI-log viewer, full five-of-each seed, frontend work, and hosted deployment. `TECHTASK.pdf` lists milestone points and five-of-each source data in its broader requirements; this queue intentionally covers the user's **mandatory demo flow only**. AI call logs are still persisted so prompt, schema, invalid-output handling, and fallback can be demonstrated. B9 is an optional backend-only deployment brief.

Each coding agent should read the linked brief and `AGENTS.md`, report changed files, run `dotnet build api/TaskForge.Api.csproj`, and perform its brief's manual checks with the Firestore emulator when the dependency is available. Do not add automated tests under the repository's hackathon rule. If a shared contract must change, coordinate the DTO/route update with dependent briefs before implementation.
