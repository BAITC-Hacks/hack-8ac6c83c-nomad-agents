# hack-8ac6c83c-nomad-agents
Hackathon team repository for Nomad Agents

## API baseline (B00)

Requires the .NET 10 SDK for a host run, or Docker with Compose for a container run.
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
