# AGENTS.md — TaskForge

Spec: docs/MVP_SPEC.md (source of truth). Briefs: docs/tasks/*.md.

## Stack
.NET 9 Minimal API (api/), React+Vite+TS+Tailwind+shadcn (web/), Firestore (emulator locally), OpenAI Responses API.

## Rules
- Contract-first: change API → regenerate client (scripts/gen-client.sh). Never edit web/src/api/.
- Feature folders in api/Features/<Feature>/ : Endpoints.cs, Dtos.cs, Service.cs.
- Firestore access only through repositories in api/Infrastructure/Firestore.
- AI calls only through api/Infrastructure/OpenAi/ResponsesClient + validators in Features/Ai.
- Never invent card content in AI prompts/stubs. Never auto-select teams.
- Validation errors → Results.ValidationProblem. No exceptions for control flow.
- No tests (hackathon). Keep changes small; run `docker compose up` and click the flow.
- Never commit .env or keys.

## Commands
docker compose up --build | scripts/gen-client.sh | curl -X POST localhost:8080/api/admin/seed?profile=demo
