# hack-8ac6c83c-nomad-agents
Hackathon team repository for Nomad Agents

## API baseline (B00)

Requires the .NET 9 SDK for a host run, or Docker with Compose for a container run.
From the repository root:

```sh
dotnet build api/TaskForge.Api.csproj
dotnet run --project api/TaskForge.Api.csproj
```

The default `http` launch profile uses Development and listens at
`http://localhost:8080`. No Firestore emulator or OpenAI key is needed to start
this baseline. `GET http://localhost:8080/api/health` checks only that the API
host is running; `firestore: "not_checked"` means no Firestore probe has run.

For the container path, run `docker compose up --build` from the repository
root. Compose starts the Firestore emulator, API, and web service. To start
only the API and emulator, run `docker compose up --build firestore api`.
The API container runs in Development and publishes port 8080. The emulator is
available at `localhost:8081` from the host and at `firestore:8080` from the
API container. The B00 API does not connect to it yet.
If those host ports are occupied, set `API_HOST_PORT`, `FIRESTORE_HOST_PORT`,
or `WEB_HOST_PORT` before starting Compose.

In Development, Swagger UI is at `http://localhost:8080/swagger/`, and its
built-in OpenAPI document is at `http://localhost:8080/openapi/v1.json`.
The latter is the input URL for the later `scripts/gen-client.sh` integration.
Both endpoints are absent in Production. The document currently describes
only `/api/health`; feature routes in B0–B8 are pending implementation and are
not mapped as working API operations.

The host defaults to port 8080 and allows the Vite origin
`http://localhost:5173`. Set `ASPNETCORE_URLS` to change the bind address and
`CORS_ORIGINS` to a comma-separated list of allowed origins. See
`.env.example` for local settings. Compose reads `.env` automatically if one
exists; the .NET host run can receive the same variables from your shell.
Neither a credential file nor an API key is required for B00.

## Frontend walkthrough (`frontned` branch)

From `web/`, run `npm ci` and `npm run dev`, then open
`http://localhost:5173`. Set `VITE_API_BASE_URL` if the API is at a different
origin. `npm run build` checks TypeScript and creates the production bundle.

The frontend includes the F0–F6 pages: actor switcher, task wizard and owner
detail, rating panel, catalog with topic/level filters, team proposals,
business decisions and milestones, leaderboard, and AI logs. API requests use
`X-Actor-Role` and `X-Actor-Id`. The API in this checkout still exposes only
`/api/health`, so the frontend enters a **Sample workspace** when `/api/actors`
is missing or the API cannot be reached. Its banner stays visible on every
page. Sample tasks, scores, and decisions live only in the browser's
`localStorage` (`taskforge.demo.v1`); they are not persisted to Firestore or
calculated by the backend rating rules. Removing that key restores the sample
seed. The sample scoring is for navigation rehearsal only.

To rehearse the UI, choose **Tamaq Café Chain**, create a weak task, answer the
basic questions, edit and confirm its card twice, then publish. Switch to
**Byte Nomads** to filter the catalog and submit a proposal. Switch back to
Tamaq to select or reject it. The seeded Nomad and Steppe cards provide a
priority/workable catalog comparison and two seeded pending proposals. A
confirmed sample milestone adds ten sample points to the leaderboard.

The basic question template in sample mode never contacts OpenAI. The AI logs
page records that local fallback with `fallback-stub`; its prompt and output
are explicitly labeled as a template. No successful live AI response or
Firestore-backed workflow has been verified in this branch. The current API
contract has only the health route, so generated OpenAPI client integration,
real AI validation/retry, server scoring, real seeds, and full stack acceptance
remain pending backend work. The sample rating heuristic supports English
rehearsal and has no language accuracy guarantee.
