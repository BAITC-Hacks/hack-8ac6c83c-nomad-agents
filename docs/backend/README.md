# Backend task queue — MVP priorities

Source of truth: [MVP_SPEC.md](../../MVP_SPEC.md), especially §§2–3, 4–7 and 15. These briefs target the current `api/` scaffold. The scaffold has a .NET project and OpenAPI registration, but its runnable host, Swagger UI, and feature behavior still need verification. **Start with B00, then finish the entire P0 journey before P1 or P2.** Work through the dependencies below. Do not edit frontend files. The product has one AI `analyze` call; the backend owns scoring, levels, quests, and catalog position.

| Priority | Brief | Result | Depends on |
|---|---|---|---|
| P0 first | [B00](B00-runnable-api-and-swagger.md) | Runnable .NET API, truthful health, OpenAPI JSON, Swagger UI | — |
| P0 | [B0](B0-contract-and-actor-guard.md) | API contract, actor guard for role switcher | B00 |
| P0 | [B1](B1-firestore-persistence.md) | Firestore models and repositories | B0 |
| P0 | [B2](B2-ai-analysis.md) | Analyze, questions, chips, suggestions, safe fallback | B0, B1 |
| P0 | [B3](B3-readiness-engine.md) | Deterministic score, breakdown, quests, levels | B1 |
| P0 | [B4](B4-task-lifecycle.md) | Draft, one-time answers, editable card, two confirms, publish | B1, B2, B3 |
| P0 | [B5](B5-catalog.md) | Ranked catalog, topic and level filters, position | B1, B3, B4 |
| P0 | [B6](B6-proposals-and-decisions.md) | Proposal and manual business Select / Reject | B0, B1, B4 |
| P0 | [B7](B7-demo-seed-and-admin.md) | `demo` and `full` fixtures, guarded seed | B1, B3 |
| P0 | [B8](B8-integration-and-contract.md) | End-to-end API and OpenAPI handoff | B2–B7 |
| P2 | [B9](B9-optional-backend-deployment.md) | Optional Cloud Run deployment | B8 |

P0 API surface: `/api/health`, `/api/actors`, task create/read/mine/analyze/answers/fields/confirm/publish, `/api/catalog` with topic and level filters, `/api/catalog/topics`, task proposal upsert/list, team proposal list, proposal decision (`selected|rejected`), and guarded `/api/admin/seed?profile=demo|full`. Preserve the route and response shapes in spec §7. The wizard shows the last confirmed score while editing; `/rating-preview` is P1. Remove unfinished optional routes from the mapped API or make them explicitly unavailable; never return fake success data.

P0 includes `full` seed data and README handoff even if the live demo uses `demo`. P1 begins only after P0 is stable: recommendation ranking, post-publish edits/preview/history, AI-log viewer, and decision Reset. P2 adds milestones, team points, leaderboard, and optional hosting. AI call logs are persisted in P0 so prompt, schema, invalid-output handling, and fallback can be documented even if the P1 viewer is deferred. B9 is P2.

Each coding agent should read the linked brief and `AGENTS.md`, report changed files, run `dotnet build api/TaskForge.Api.csproj`, and perform its brief's manual checks with the Firestore emulator when the dependency is available. Do not add automated tests under the repository's hackathon rule. If a shared contract must change, coordinate the DTO/route update with dependent briefs before implementation.
