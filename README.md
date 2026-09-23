# TaskForge — AI Challenge Coach

Hackathon project by team Nomad Agents. Full target design lives in [`MVP_SPEC.md`](MVP_SPEC.md) (source of truth); this README documents what is actually implemented in this repository right now, plus the planned scope from the spec.
<img width="1919" height="878" alt="image" src="https://github.com/user-attachments/assets/708a63d3-ff99-4ddc-b5bb-7626addd85c7" />



## 1. Project name

**TaskForge** — "AI Challenge Coach: Business Task Readiness & Open Team Selection".

## 2. Brief description

TaskForge helps a **business representative** turn a vague task idea ("we want an app to fix churn") into a clear, complete task description that a **student team** can actually act on, and helps that team find and propose for well-specified tasks. The AI Challenge Coach is meant to flag missing information in a business's draft, ask clarifying questions, and let the backend score the resulting task card 0–100 so business owners can see how ready their task is and teams can prioritize well-specified work over vague ones.

Target users: businesses posting challenges for students (need help writing a clear brief) and student teams browsing a shared catalog of challenges (need to tell which tasks are actually ready to work on).

## 3. Implementation details (what exists in this repository today)

### Backend (`api/`) — .NET 9 Minimal API
- Feature-folder structure per `MVP_SPEC.md` §3 (`api/Features/<Feature>/Endpoints.cs`, `Dtos.cs`).
- **Implemented:** `GET /api/health` and public `GET /api/actors`, which returns the three demo businesses and two teams used by the role switcher.
- Protected API routes resolve `X-Actor-Role` and `X-Actor-Id` once per request. Missing/invalid headers return 400 ValidationProblem, unknown actors return 404, and role mismatches return 403. Reusable guards support business/team and task/proposal ownership checks.
- OpenAPI/Swagger is wired up in Development (`http://localhost:8080/swagger/`, `http://localhost:8080/openapi/v1.json`); the generated document currently lists `/api/health` and `/api/actors`.
- CORS is configurable via `CORS_ORIGINS`; the API binds via `ASPNETCORE_URLS`.
- The B1 domain and repository layer is implemented over the singleton store: actors, tasks, confirmed snapshots, ratings/history, proposals/milestones, and AI logs. It includes revision-safe task updates, atomic proposal/team point updates, and full reset support. No OpenAI integration or task/catalog/proposal/rating HTTP endpoints exist yet.

### Frontend (`web/`) — Vite + React + TypeScript
All F0–F6 pages from `docs/tasks/frontend/` have been built as React pages/components:
- `AppShell`, `RoleSwitcher` — role-switching shell and navigation (`web/src/components/`).
- `NewTaskWizard` + wizard steps (`Draft`, `Clarify`, `Card`, `Rating`, `Published`) — the business task-creation flow (`web/src/pages/business/`, `web/src/components/wizard/`).
- `RatingPanel`, `LevelBadge` — score/level/breakdown/quests display.
- `Catalog`, `TaskView` — shared task catalog and team-facing task detail (`web/src/pages/`).
- `ProposalForm`, `ProposalReview`, `MyProposals` — team proposal submission and business review/decision UI.
- `Leaderboard`, `AiLogs` — additional pages for team points and AI call inspection.
- `Toaster` — error/toast notifications wired to API `ProblemDetails` responses.

### Key behavior actually implemented: automatic "Sample workspace" fallback
The frontend's fetch wrapper (`web/src/lib/http.ts`) switches to a **local demo mode** when the API is unavailable or does not expose `/api/actors`. When the current B0 backend is running, real actor loading works; task/catalog/proposal routes remain unavailable until their backend briefs are implemented.
- All data (businesses, teams, sample tasks, sample proposals, and a simplified sample scoring formula) lives in module memory and is seeded from hardcoded fixtures in `demo.ts`. Reloading clears changes.
- The UI shows a banner on every page: *"Sample workspace · API unavailable. Data and sample scores live only in this tab and reset on reload; they are not official ratings."*
- This lets every page (wizard, catalog, proposals, leaderboard, AI logs) be clicked through end-to-end today, without a running backend, but the scores and rules shown in that mode are **not** the deterministic rubric from `MVP_SPEC.md` §5.3 — they are a simplified placeholder heuristic (`sampleRating` in `demo.ts`), explicitly marked `ratingRulesVersion: "sample-only"`, with no language-accuracy guarantee.
- The sample "Analyze" step never contacts OpenAI; it returns a fixed question template. The AI Logs page in this mode records that local fallback with validation `fallback-stub`, and its prompt/output are explicitly labeled as a template, not a real model response.

## 4. How the solution works (current, demo-mode flow)

1. Open the app and pick a role via the **role switcher** (a business or a student team) — no login.
2. **As a business (e.g. Tamaq Café Chain):** go to *My Tasks* → *New task*, type a weak draft description, and step through the wizard (Clarify → Card → Rating → Publish). "Analyze" returns the sample clarification questions and chips described above; the card can be edited and confirmed twice to see the sample score/level change.
3. The confirmed task appears in the shared **Catalog**, sorted and filterable by topic/level.
4. **As a student team (e.g. Byte Nomads):** switch role, open the catalog, open a task, and submit a **proposal** (idea, plan, timeline, prototype link).
5. **As the business again:** open the task's detail page to compare proposals and manually **Select/Reject** them; confirming a milestone on a selected proposal adds sample points to the **Leaderboard**.
6. The **AI Logs** page shows the recorded local fallback for the Analyze call made in step 2.

Everything above runs entirely against the in-browser sample dataset today; there is no server-side persistence or real scoring behind it yet.

## 5. Technologies

- **Backend:** .NET 9 (Minimal APIs), C#, built-in OpenAPI + Swagger UI, process-local in-memory storage, Docker.
- **Frontend:** React 18, TypeScript, Vite 5.
- **Planned per `MVP_SPEC.md`** (not yet wired in code): in-memory repositories, OpenAI Responses API (mini-tier model, name via `OPENAI_MODEL` env var), TanStack Query, Tailwind CSS + shadcn/ui, and an orval-generated typed API client.
- **Infra:** Docker Compose (API + Vite dev server), `.env` / `.env.example` for local configuration.

## 6. Project architecture

```
┌──────────────┐   HTTPS/JSON   ┌───────────────────────────┐
│ React SPA    │ ─────────────► │ .NET 9 Minimal API         │
│ (Vite, :5173)│  X-Actor-*     │ (Cloud Run target, :8080)  │
│              │ ◄── fallback ─ │  Features/Health/ (only)   │
└──────────────┘   sample data  └───────────────────────────┘
       │ (when API call fails/unimplemented)
       ▼
 web/src/lib/demo.ts  — in-memory sample dataset for the current browser tab

Local dev: docker compose up --build  →  web (vite) + api (in-memory state)
```

- `api/Features/<Feature>/` — one folder per backend feature (`Endpoints.cs`, `Dtos.cs`); `Health` and `Actors` exist today.
- `api/Infrastructure/InMemory/` — singleton process-local state used by backend repositories; all state is cleared on API restart.
- `web/src/components/`, `web/src/pages/` — UI building blocks and routed pages, driven by a hand-rolled path-based router in `web/src/App.tsx` (no routing library).
- `web/src/lib/http.ts` — single fetch wrapper adding `X-Actor-Role`/`X-Actor-Id` headers and handling the API → sample-data fallback described in §3–4.
- `web/src/lib/demo.ts` — the current stand-in for backend business logic (task lifecycle, sample rating, proposals) used only when the real API is unavailable.
- `docker-compose.yml` — orchestrates `api` and `web` for local development.

## 7. Installation and setup

Requires the **.NET 9 SDK** and **Node.js** for a host run, or **Docker + Docker Compose** for a containerized run.

### Option A — Docker Compose (recommended, runs both services)
```sh
cp .env.example .env   # optional, defaults work out of the box
docker compose up --build
```
This starts the API (`localhost:8080`) and web dev server (`localhost:5173`). If either host port is taken, set `API_HOST_PORT` or `WEB_HOST_PORT` in `.env` first. To start only the API, run `docker compose up --build api`.

### Option B — Run services directly on the host
Backend:
```sh
dotnet build api/TaskForge.Api.csproj
dotnet run --project api/TaskForge.Api.csproj
```
The API listens at `http://localhost:8080` and needs no database or OpenAI key to start (B00 baseline).

Frontend:
```sh
cd web
npm ci
npm run dev
```
The dev server listens at `http://localhost:5173` and reads `VITE_API_BASE_URL` (defaults to the API on `http://localhost:8080`). `npm run build` type-checks and creates the production bundle.

No credential file or API key is required to run what exists today.

## 8. How to test the solution (example scenario)

1. Start the stack with `docker compose up --build` (or run both services per Option B).
2. Open `http://localhost:5173`.
3. To exercise the browser-only sample journey, run the frontend while the API is unavailable. With the API running, the real actor selector loads but later feature pages remain incomplete until B1–B8 are implemented.
4. Pick **Tamaq Café Chain** (business) from the role switcher, go to *My Tasks* → *New task*, and paste a short weak draft (e.g. "We are a café chain. Customers stop coming back and we don't know why.").
5. Step through the wizard: run Analyze, answer the sample clarification questions, edit the card, and confirm twice to see the sample score/level change.
6. Publish the task and open the **Catalog** — the seeded Nomad Logistics (priority-level) and Steppe Retail (workable-level) cards provide a comparison alongside the one you just published.
7. Switch role to **Byte Nomads** (team), open a published task from the catalog, and submit a proposal (idea, plan, timeline, prototype link) — or inspect the two proposals already seeded on the Nomad task.
8. Switch back to the business role, open the task's detail page, and Select/Reject the proposal; confirming a milestone on a selected proposal adds ten sample points, visible on the **Leaderboard**.
9. Open **AI Logs** to see the recorded `fallback-stub` entry for the Analyze call from step 5.
10. To verify the backend independently of the UI: `curl http://localhost:8080/api/health` and open `http://localhost:8080/swagger/` to see the current (health-only) API surface.

## 9. Data and integrations

- **Current:** the API uses a singleton in-memory store and exposes its storage mode from `/api/health`. All server data is lost on API restart. Until feature endpoints are implemented, sample data lives in browser module memory and resets on reload.
- **Planned per `MVP_SPEC.md`:** repositories over the singleton in-memory store; OpenAI's Responses API (`POST https://api.openai.com/v1/responses`) for the single `analyze` AI call with strict validation, one retry, and deterministic fallback; `demo` and `full` seed profiles reload the in-memory collections.

## 10. Limitations (current version)

- **Backend HTTP scope is currently B0; storage scope is B1.** Health, actor listing, actor resolution, access guards, domain records, and in-memory repositories work. Task, analyze, answers, confirm, publish, catalog, proposal, decision, milestone, leaderboard, and AI-log HTTP endpoints are not implemented.
- **No durable persistence by design.** Backend data is process-local and is cleared by restart, redeployment, or scale-out to another instance.
- **No OpenAI integration.** The "Analyze" step uses a hardcoded stub question template in the frontend, not a real AI call; no successful live AI response has been verified in this branch.
- **No real deterministic rating engine.** The score shown in sample mode is a simplified placeholder heuristic in `web/src/lib/demo.ts`, explicitly not the rubric described in `MVP_SPEC.md` §5.3, and is not cached, versioned, or persisted server-side; it supports English rehearsal only, with no language-accuracy guarantee.
- **No generated API client integration** — the current API contract exposes only the health route, so orval/OpenAPI client generation against real feature endpoints is pending.
- **No authentication.** The server validates the demo actor headers and ownership helpers for correctness, but this role switcher mechanism is intentionally not secure identity.
- **No seed data.** `seed/demo/` and `seed/full/` are empty placeholders.
- **Data is non-durable by design** — API restart clears server state, and browser reload clears the fallback workspace and actor selection.
- **No automated tests**, matching the project's stated hackathon scope.
- **Not deployed yet** (see §11).

## 11. Deployed version

_Not deployed yet — placeholder, to be updated once a live URL exists:_
**`<DEPLOYMENT_URL_PLACEHOLDER>`**
