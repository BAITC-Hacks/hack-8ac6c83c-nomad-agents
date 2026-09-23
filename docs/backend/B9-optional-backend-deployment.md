# B9 — Optional Cloud Run backend deployment

**Priority: P2 — optional after the P0 journey and P1 work are stable.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 10.

## Goal

Prepare and, if separately authorized, deploy the backend API to Cloud Run after the local mandatory flow is stable. This is a stretch brief; the demo can run entirely from Docker Compose.

## Starting point and files

The spec sketches `api/Dockerfile`, `docker-compose.yml`, `.env.example`, and `scripts/deploy-api.sh`; the current scaffold contains `api/Dockerfile` but no proven production build path. Own backend deployment files only. Do not edit or deploy the frontend.

## Implement

- Make the Docker build context coherent with the root `seed/` directory and the API project. Local and hosted builds use the same process-local in-memory store.
- Configure CORS for the actual SPA origin and a non-placeholder OpenAI model/key. Keep credentials out of git and command output; use an appropriate server-side secret mechanism. Public admin mutations must remain disabled or secret-guarded.
- Provide deployment instructions that explicitly state that restart, replacement, and scale-out lose or split data. Limit an optional Cloud Run demo to one instance and include a safe reseed procedure. Local Compose remains the reliable demonstration environment.
- If deployment is authorized, use the prepared script and then manually check health, actor loading, and a harmless catalog read. Otherwise leave a reviewable script and local verification result.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` and local Docker build succeed. Deployment instructions identify the expected image/source context, environment settings, one-instance constraint, and non-durable state. A public request to seed/reset is denied. If deployed, the Cloud Run health/catalog checks succeed without exposing secrets.

## Boundary

No Netlify/frontend deployment, source-code refactor, schema redesign, or production seed. This brief is optional and must not delay B8's local demo readiness.
