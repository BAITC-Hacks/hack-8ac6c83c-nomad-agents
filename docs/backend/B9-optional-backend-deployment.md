# B9 — Optional Cloud Run backend deployment

**Priority: P2 — optional after the P0 journey and P1 work are stable.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 10.

## Goal

Prepare and, if separately authorized, deploy the backend API to Cloud Run after the local mandatory flow is stable. This is a stretch brief; the demo can run entirely from Docker Compose.

## Starting point and files

The spec sketches `api/Dockerfile`, `docker-compose.yml`, `.env.example`, and `scripts/deploy-api.sh`; the current scaffold contains `api/Dockerfile` but no proven production build path. Own backend deployment files only. Do not edit or deploy the frontend.

## Implement

- Make the Docker build context coherent with the root `seed/` directory and the API project. Keep local Compose wired to the Firestore emulator and production configuration wired to native Firestore via B1's builder.
- Configure CORS for the actual SPA origin, a non-placeholder OpenAI model/key, and a service account that can access Firestore. Keep credentials out of git and command output; use an appropriate server-side secret mechanism. Public admin mutations must remain disabled or secret-guarded.
- Provide a backend deployment script/instructions with prerequisites, build, environment variables, and a read-only health check. Avoid destructive database reset/seed in production deployment steps.
- If deployment is authorized, use the prepared script and then manually check health, actor loading, and a harmless catalog read. Otherwise leave a reviewable script and local verification result.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` and local Docker build succeed. Deployment instructions identify the expected image/source context and environment settings. A public request to seed/reset is denied. If deployed, the Cloud Run health/catalog checks succeed without exposing secrets.

## Boundary

No Netlify/frontend deployment, source-code refactor, schema redesign, or production seed. This brief is optional and must not delay B8's local demo readiness.
