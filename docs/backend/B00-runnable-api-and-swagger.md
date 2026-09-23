# B00 — Runnable .NET API and Swagger baseline

**Priority: P0 — first backend task, before B0.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2–3, 7, 10. This prepares the API host and its inspectable contract; B0–B8 add real feature behavior.

## Goal

Turn the existing .NET 10 Minimal API scaffold into a repeatably runnable local service. A developer can start it, reach a truthful health endpoint, inspect its OpenAPI JSON in Swagger UI, and use that document for frontend client generation.

## Starting point and files

`api/TaskForge.Api.csproj`, `api/Program.cs`, and `api/Dockerfile` already exist. `Program.cs` registers built-in OpenAPI and maps it only in Development; no Swagger UI is configured. The feature endpoint files are mostly placeholders. Own the API project/host setup, development-only Swagger UI wiring, basic launch instructions, and minimal Docker/Compose fixes needed to boot. Keep domain models, repositories, scoring, AI, and feature behavior with their later briefs.

## Implement

- Confirm the project restores and builds with the .NET 10 SDK. Keep a single Minimal API entry point and feature endpoint mapping; resolve startup/DI failures so the host can run before real Firestore data or an OpenAI key is available. Do not report a dependency as healthy without checking it.
- Serve the built-in OpenAPI document at `/openapi/v1.json` and a Swagger UI at `/swagger` in local Development. Point the UI at that document and record its URL for the later `scripts/gen-client.sh` integration. Keep these development endpoints disabled or explicitly guarded in production.
- Provide a simple public `GET /api/health` startup check with a truthful response. B7 later adds an actual Firestore reachability check; a hardcoded `firestore: "ok"` is not acceptable. Keep `/api` route prefixes and JSON/ProblemDetails conventions ready for B0.
- Configure local HTTP port `8080`, CORS for the Vite origin, and environment-based settings through `.env.example`. Make `dotnet run --project api/TaskForge.Api.csproj` and the API container in `docker compose up --build` viable local paths. Ensure the local container runs in Development so Swagger UI is reachable there. Document any local emulator requirement and ensure no credential file or key is committed.
- Keep the OpenAPI document limited to mapped routes with honest responses. Placeholder feature routes must be identified for B0–B8 to replace or unmap; they must not be presented as working business functionality.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. Start the API locally and verify `GET http://localhost:8080/api/health` returns success, `GET http://localhost:8080/openapi/v1.json` returns valid OpenAPI JSON, and `http://localhost:8080/swagger` loads Swagger UI with that document. Start the API container with Compose and repeat those checks against port 8080. Record exact run commands and any required local environment variables for B0 and frontend client generation.

## Boundary

Do not implement actor authorization, Firestore serialization, task lifecycle, rating rules, AI analysis, catalog, proposals, seed contents, or frontend pages. No automated tests under `AGENTS.md`'s hackathon rule. B00 establishes a runnable host and contract viewer; B8 verifies the final feature contract.
