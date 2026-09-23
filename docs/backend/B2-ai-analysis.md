# B2 — AI draft analysis and safe fallback

**Priority: P0 — top priority.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 5.2, 5.4.

## Goal

Provide the one required AI function: detect missing task details, ask at least three relevant questions, and return chips, grounded draft extracts, and improvement suggestions. B4 wires this service into `POST /api/tasks/{id}/analyze`.

## Starting point and files

`api/Infrastructure/OpenAi/ResponsesClient.cs` sends a Responses-shaped body to `/v1/chat/completions`; this must become `POST /v1/responses`. `Schemas.cs` still contains an obsolete score schema and lacks `missingFields`. `api/Features/Ai/Dtos.cs` lacks `missingFields`; `AiLogRepository` already exists. Work in `api/Infrastructure/OpenAi/`, `api/Features/Ai/`, and a focused AI service/validator. Avoid editing `api/Features/Tasks/Endpoints.cs` because B4 owns it.

## Implement

- Send the strict JSON schema from MVP spec §5.2 to the Responses API. Use the configured model/key, 20-second timeout, proper response text extraction, and no secret values in logs. Remove the obsolete LLM score request/output types.
- Validate field keys, 3–7 questions, ≤4 chips per question, concise actions, and assign stable `q1…qN` IDs after validation. Return `missingFields` as known field keys only. A draft extract's **value itself** must be copied from the cited substring of the raw draft; discard unsupported paraphrases. Title is a suggestion requiring business review. Never auto-insert chip choices as company facts.
- On malformed output or API failure, retry once where time permits, then return deterministic questions for missing/weak fields. Keep at least three relevant questions even for a nearly complete card. Mark `source: stub` and let editing continue without OpenAI.
- Persist prompt, input JSON, raw output, validation result/errors, latency, task ID, and model for each attempt/fallback through `AiLogRepository`. `AI_MODE=stub` skips the network call. Expose a service method that B4 can call and a typed `AnalysisDto` for OpenAPI.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. Through B4's endpoint when available, manually analyze a weak draft, a nearly complete draft, an invented extraction, malformed JSON, and stub mode. Confirm the response never inserts unsupported facts and records the attempted input/output without logging the API key.

## Boundary

Do not implement AI scoring, task CRUD, rating calculation, AI-log viewer, or frontend code. B2 owns only analysis service/client/schema/validation/logging.
