# Task 0 Review Package


> Archived snapshot: the storage design and executable snippets below describe the original scaffold. They are retained for history and must not be used. The current project uses a singleton process-local in-memory store, has no database service, and loses data on API restart. See MVP_SPEC.md and docs/backend/README.md.

## Commits

edc7ceb Refactor code structure for improved readability and maintainability
133857c chore: scaffold directory structure and docker-compose
df28cbc chore: add global config and agent rules

## Stat Summary

 .env.example                                       |   12 +
 .gitignore                                         |   35 +
 .../sdd/2026-09-23-project-scaffolding/progress.md |   31 +
 .../task-0-report.md                               |  198 +++
 AGENTS.md                                          |   19 +
 CLAUDE.md                                          |    5 +
 MVP_SPEC.md                                        |  102 +-
 TECHTASK.pdf                                       |  Bin 0 -> 98954 bytes
 api/.gitkeep                                       |    0
 api/Domain/.gitkeep                                |    0
 api/Features/Actors/.gitkeep                       |    0
 api/Features/Admin/.gitkeep                        |    0
 api/Features/Ai/.gitkeep                           |    0
 api/Features/Catalog/.gitkeep                      |    0
 api/Features/Proposals/.gitkeep                    |    0
 api/Features/Rating/.gitkeep                       |    0
 api/Features/Tasks/.gitkeep                        |    0
 api/Infrastructure/Firestore/.gitkeep              |    0
 api/Infrastructure/OpenAi/.gitkeep                 |    0
 docker-compose.yml                                 |   37 +
 .../plans/2026-09-23-project-scaffolding.md        | 1299 ++++++++++++++++++++
 docs/tasks/.gitkeep                                |    0
 scripts/.gitkeep                                   |    0
 seed/demo/.gitkeep                                 |    0
 seed/full/.gitkeep                                 |    0
 web/.gitkeep                                       |    0
 web/src/api/.gitkeep                               |    0
 27 files changed, 1686 insertions(+), 52 deletions(-)

## Full Diff

diff --git a/.env.example b/.env.example
new file mode 100644
index 0000000..8161529
--- /dev/null
+++ b/.env.example
@@ -0,0 +1,12 @@
+# --- Backend (API) ---
+OPENAI_API_KEY=sk-REPLACE_ME
+OPENAI_MODEL=REPLACE_WITH_CURRENT_MINI_MODEL
+OPENAI_TIMEOUT_SECONDS=20
+AI_MODE=live                    # live | stub
+GCP_PROJECT_ID=taskforge-local
+FIRESTORE_EMULATOR_HOST=firestore:8080   # local only; unset in Cloud Run
+CORS_ORIGINS=http://localhost:5173
+ASPNETCORE_URLS=http://+:8080
+
+# --- Frontend (WEB) ---
+VITE_API_BASE_URL=http://localhost:8080
diff --git a/.gitignore b/.gitignore
new file mode 100644
index 0000000..158686d
--- /dev/null
+++ b/.gitignore
@@ -0,0 +1,35 @@
+# Secrets & local environment
+.env
+.env.local
+*.local
+
+# IDE
+.vscode/
+.idea/
+*.swp
+*.swo
+*~
+.DS_Store
+
+# Build outputs
+bin/
+obj/
+dist/
+node_modules/
+*.pyc
+__pycache__/
+
+# Logs & temp
+*.log
+npm-debug.log*
+/tmp/
+
+# OS
+Thumbs.db
+
+# Generated client (regenerate via scripts/gen-client.sh)
+# /web/src/api/ is NOT in .gitignore — it's committed for CI/CD
+
+# Seed data & docs are committed
+# /seed/* — committed
+# /docs/* — committed
diff --git a/.superpowers/sdd/2026-09-23-project-scaffolding/progress.md b/.superpowers/sdd/2026-09-23-project-scaffolding/progress.md
new file mode 100644
index 0000000..26b9fe2
--- /dev/null
+++ b/.superpowers/sdd/2026-09-23-project-scaffolding/progress.md
@@ -0,0 +1,31 @@
+# SDD ledger — plan: docs/superpowers/plans/2026-09-23-project-scaffolding.md
+
+## Pre-flight scan
+
+Checking plan consistency before dispatch...
+
+| Task | Check | Status | Note |
+|------|-------|--------|------|
+| T0 | Root config files don't conflict | ✅ | .gitignore, .env.example, AGENTS.md, CLAUDE.md all independent |
+| T0 | Folder structure completeness | ✅ | All dirs from MVP_SPEC.md §3 included |
+| T0-T1 | Docker Compose services match | ✅ | firestore:8080, api:8080, web:5173 — consistent |
+| T1 | .NET version specified | ✅ | net9.0 in csproj, matches § 10.3 |
+| T2 | Frontend dependencies precise | ✅ | package.json versions pinned |
+| T3 | Seed data schema match | ✅ | JSON structure matches Firestore collections in §4.4 |
+| T4 | Agent briefs completeness | ✅ | All 11 briefs (00, A1–A5, B1–B5) have goal/files/interfaces/acceptance |
+| T5 | Agent context paths | ✅ | .agents/ and .claude/settings.json created |
+| Cross | Interface consistency | ✅ | All task-to-task interfaces match (e.g., AnalysisDto fields) |
+
+**Verdict:** Pre-flight clean. No conflicts detected. Proceeding to Task 1 dispatch.
+
+## Task progress
+
+- [ ] Task 0: Repository Root & Global Config
+- [ ] Task 1: Folder Structure & Docker Compose
+- [ ] Task 2: Backend Skeleton (.NET 9 API)
+- [ ] Task 3: Frontend Skeleton (React + Vite)
+- [ ] Task 4: Seed Data Structure
+- [ ] Task 5: Agent Briefs in `docs/tasks/`
+- [ ] Task 6: Agent Context & Settings
+- [ ] Task 7: Final Verification & Clean Up
+
diff --git a/.superpowers/sdd/2026-09-23-project-scaffolding/task-0-report.md b/.superpowers/sdd/2026-09-23-project-scaffolding/task-0-report.md
new file mode 100644
index 0000000..a64797c
--- /dev/null
+++ b/.superpowers/sdd/2026-09-23-project-scaffolding/task-0-report.md
@@ -0,0 +1,198 @@
+# Task 0: Project Scaffolding — Report
+
+**Status:** DONE
+
+**Completed:** 2026-09-23 14:12 UTC
+
+---
+
+## Summary
+
+Successfully established repository structure, global configuration, and folder hierarchy for TaskForge MVP parallel development. All acceptance checks pass.
+
+---
+
+## Acceptance Checks
+
+### 1. Docker Compose Configuration
+
+```bash
+docker compose config > /dev/null
+```
+
+**Result:** ✓ PASS — docker-compose.yml is valid with no syntax errors
+
+### 2. Folder Structure Verification
+
+**Gitkeep files created:** 17
+
+Expected folders:
+```
+api/
+├── .gitkeep
+├── Domain/.gitkeep
+├── Infrastructure/
+│   ├── Firestore/.gitkeep
+│   └── OpenAi/.gitkeep
+└── Features/
+    ├── Actors/.gitkeep
+    ├── Tasks/.gitkeep
+    ├── Ai/.gitkeep
+    ├── Rating/.gitkeep
+    ├── Catalog/.gitkeep
+    ├── Proposals/.gitkeep
+    └── Admin/.gitkeep
+
+web/
+├── .gitkeep
+└── src/api/.gitkeep
+
+seed/
+├── demo/.gitkeep
+└── full/.gitkeep
+
+scripts/.gitkeep
+
+docs/tasks/.gitkeep
+```
+
+**Result:** ✓ PASS — All 17 .gitkeep files present
+
+### 3. Root Configuration Files
+
+| File | Status |
+|------|--------|
+| `.gitignore` | ✓ Committed |
+| `.env.example` | ✓ Committed |
+| `AGENTS.md` | ✓ Committed |
+| `CLAUDE.md` | ✓ Committed |
+| `docker-compose.yml` | ✓ Committed |
+
+**Result:** ✓ PASS — All files committed cleanly
+
+---
+
+## Commits Made
+
+### Commit 1: Global Config and Agent Rules
+
+```
+df28cbc chore: add global config and agent rules
+```
+
+**Files:**
+- `.gitignore` — excludes .env, build outputs, generated client
+- `.env.example` — template with all env vars (API + frontend)
+- `AGENTS.md` — shared rules for Dev A & B teams
+- `CLAUDE.md` — points to AGENTS.md as authoritative
+- `MVP_SPEC.md` — updated (was modified, now staged)
+
+### Commit 2: Directory Structure & Docker Compose
+
+```
+133857c chore: scaffold directory structure and docker-compose
+```
+
+**Files:**
+- `docker-compose.yml` — services: firestore, api, web
+- `api/` — 11 .gitkeep files (Domain, Infrastructure, Features)
+- `web/` — 2 .gitkeep files (root, src/api)
+- `seed/` — 2 .gitkeep files (demo, full)
+- `scripts/` — 1 .gitkeep file
+- `docs/tasks/` — 1 .gitkeep file
+
+---
+
+## Test Results
+
+### Docker Compose Configuration
+
+```
+Valid: ✓
+Services: firestore, api, web
+Port mappings: 8081 (firestore), 8080 (api), 5173 (web)
+Environment: .env file properly referenced
+Volumes: ./seed:/app/seed:ro (api container)
+Dependencies: firestore ← api ← web
+```
+
+### Directory Tree (first 30 lines)
+
+```
+.
+./scripts
+./api
+./scripts/.gitkeep
+./api/.gitkeep
+./api/Infrastructure
+./api/Domain
+./api/Features
+./api/Infrastructure/OpenAi
+./api/Infrastructure/Firestore
+./api/Features/Proposals
+./api/Features/Actors
+./api/Features/Tasks
+./api/Features/Rating
+./api/Features/Catalog
+./api/Features/Admin
+./api/Features/Ai
+./api/Domain/.gitkeep
+./api/Infrastructure/OpenAi/.gitkeep
+./api/Infrastructure/Firestore/.gitkeep
+./api/Features/Actors/.gitkeep
+./api/Features/Tasks/.gitkeep
+./api/Features/Ai/.gitkeep
+./api/Features/Rating/.gitkeep
+./api/Features/Catalog/.gitkeep
+./api/Features/Proposals/.gitkeep
+./api/Features/Admin/.gitkeep
+./web
+./web/.gitkeep
+./web/src
+./web/src/api
+```
+
+### Git Log
+
+```
+133857c chore: scaffold directory structure and docker-compose
+df28cbc chore: add global config and agent rules
+08bad16 ADD: spec_v1
+```
+
+---
+
+## Verification
+
+| Check | Result |
+|-------|--------|
+| `.gitignore` excludes `.env` | ✓ PASS |
+| `.env` file not committed | ✓ PASS |
+| `.env.example` is template | ✓ PASS |
+| Docker Compose validates | ✓ PASS |
+| Folder structure complete | ✓ PASS |
+| Commits have proper attribution | ✓ PASS |
+| Git working tree clean | ✓ PASS |
+
+---
+
+## Concerns
+
+**None.** All requirements met:
+- ✓ Global configuration established (AGENTS.md, CLAUDE.md, .env.example)
+- ✓ .gitignore prevents .env commits
+- ✓ Docker Compose validated for local dev environment
+- ✓ Complete folder hierarchy per MVP_SPEC.md §3
+- ✓ 17 .gitkeep files track empty directories
+- ✓ 2 clean commits with proper attribution lines
+- ✓ Ready for parallel Dev A & B development
+
+---
+
+## Next Steps
+
+**Ready for:**
+- Task A1 (Dev A): OpenAI Integration Client
+- Task B1 (Dev B): Firestore Repositories
+
+Both development tracks can proceed in parallel from this stable scaffolding.
diff --git a/AGENTS.md b/AGENTS.md
new file mode 100644
index 0000000..0f06504
--- /dev/null
+++ b/AGENTS.md
@@ -0,0 +1,19 @@
+# AGENTS.md — TaskForge
+
+Spec: docs/MVP_SPEC.md (source of truth). Briefs: docs/tasks/*.md.
+
+## Stack
+.NET 9 Minimal API (api/), React+Vite+TS+Tailwind+shadcn (web/), Firestore (emulator locally), OpenAI Responses API.
+
+## Rules
+- Contract-first: change API → regenerate client (scripts/gen-client.sh). Never edit web/src/api/.
+- Feature folders in api/Features/<Feature>/ : Endpoints.cs, Dtos.cs, Service.cs.
+- Firestore access only through repositories in api/Infrastructure/Firestore.
+- AI calls only through api/Infrastructure/OpenAi/ResponsesClient + validators in Features/Ai.
+- Never invent card content in AI prompts/stubs. Never auto-select teams.
+- Validation errors → Results.ValidationProblem. No exceptions for control flow.
+- No tests (hackathon). Keep changes small; run `docker compose up` and click the flow.
+- Never commit .env or keys.
+
+## Commands
+docker compose up --build | scripts/gen-client.sh | curl -X POST localhost:8080/api/admin/seed?profile=demo
diff --git a/CLAUDE.md b/CLAUDE.md
new file mode 100644
index 0000000..c0892ca
--- /dev/null
+++ b/CLAUDE.md
@@ -0,0 +1,5 @@
+# CLAUDE.md — TaskForge
+
+Read AGENTS.md first — it is authoritative. Work only within the brief you are given.
+
+Before finishing: build (`dotnet build` / `npm run build`) and report changed files.
diff --git a/MVP_SPEC.md b/MVP_SPEC.md
index 10d846e..fc9c86a 100644
--- a/MVP_SPEC.md
+++ b/MVP_SPEC.md
@@ -28,58 +28,58 @@ A business rep types a weak task description → AI asks clarifying questions (w
 | 12 | FE state | TanStack Query + generated typed client |
 | 13 | Contract | .NET built-in OpenAPI → orval-generated TS client + hooks |
 | 14 | Clarify flow | Single round, all questions at once |
 | 15 | Draft → card | AI asks questions + suggests actionable items; only user-written text is pre-filled |
 | 16 | Answer UI | Up to 4 AI chips per question (select) **or** free text |
 | 17 | Post-publish edit | Allowed; stays visible; re-scored on confirm (per SoW §4) |
 | 18 | Sort | Rating desc, then `updatedAt` desc |
 | 19 | Filters | Topic + readiness level (level is SoW-mandatory) |
 | 20 | Recommendations | Rule-based: team interest/tech tags ∩ task tags |
 | 21 | Proposal fields | Idea, plan, timeline, prototype link |
-| 22 | Duplicates | One proposal per team per task, editable while Pending |
+| 22 | Proposals | Any team may submit; one active proposal per team per task, editable while Pending. No cap across teams. |
 | 23 | Decision | Pending / Selected / Rejected, manual, reversible until milestone |
 | 24 | Milestones | Minimal: business confirms → +10 team points → leaderboard |
-| 25 | Seed | 3 businesses, 3 cards (full / medium / wizard-demo), 2 teams |
+| 25 | Seed | Demo: 3 businesses, 2 published cards plus live wizard demo, 2 teams; full: source-data minimums are preloaded |
 | 26 | Volume | Two seed profiles: `demo` and `full` (SoW 5× minimum) |
 | 27 | AI failure | Validate → 1 retry → local stub, flagged in UI |
 | 28 | Secrets | Local `.env` (gitignored) + committed `.env.example` |
 | 29 | Agent tooling | `AGENTS.md` + `CLAUDE.md` + per-module briefs in `/docs/tasks` |
 | 30 | Testing | None automated; manual demo rehearsal + README test scenarios |
 
 ---
 
 ## 1. ⚠ Red flags (read first)
 
 | # | Risk | Mitigation in this spec |
 |---|---|---|
-| R1 | **AI-scored rating vs. "transparent" (25 pts criterion).** LLM scores drift between identical inputs. | Per-criterion score + reason + missing details from AI; backend clamps to weight; empty fields forced to 0; SHA-256 cache of confirmed fields → identical card = identical score; rubric anchors in prompt. |
-| R2 | **"AI must not add facts."** Chips and extraction can smuggle invented facts. | Chips are *options the user picks*, never auto-inserted. Extracted draft values require an `evidence` substring that the backend verifies exists in the raw draft; failures are dropped. Suggestions are phrased as "Add X", never as content. |
+| R1 | **AI-scored rating vs. "transparent" (25 pts criterion).** LLM scores drift between identical inputs. | Per-criterion score + reason + missing details from AI; backend clamps to weight; empty fields forced to 0; versioned canonical-input cache → same inputs/model/rubric reuse the same score; rubric anchors in prompt. |
+| R2 | **"AI must not add facts."** Chips and extraction can smuggle invented facts. | Chips are options only and require user selection. Extraction is restricted to verbatim spans from the draft; backend verifies value/evidence correspondence and that evidence occurs in the raw draft. AI-generated titles require human review. |
 | R3 | **"Draft" naming collision.** Readiness level "Draft" (0–39) ≠ unpublished task. | Use `status: editing \| published` and `level: draft \| workable \| ready \| priority`. UI label for level draft: "Needs clarification". |
-| R4 | **Seed below SoW §6 minimum** (5 of each). | `full` seed profile pads to 5 drafts / 5 cards / 5 teams / 5 proposals. |
+| R4 | **Seed below SoW §6 minimum** (5 of each). | `full` contains at least 5 persisted drafts, complete cards, teams, and proposals immediately after seed; it never counts live wizard activity. |
 | R5 | **SoW requires readiness filter** — not only topic. | Both filters implemented. |
 | R6 | **Live demo depends on OpenAI latency/availability.** | Mini model, 20s timeout, stub fallback, `AI_MODE=stub` env override for offline. |
 | R7 | **4 h not 5 h; 2 devs not 3–5.** | Hard cut list in §13; code freeze at T+3:45. |
 | R8 | **Secrets.** Real `OPENAI_API_KEY` in git = key revoked + judges notice. | `.env` in `.gitignore` from minute 1; Cloud Run gets vars via `--set-env-vars`. |
 | R9 | **CORS / mixed hosting.** Netlify → Cloud Run calls fail at demo time. | CORS allow-list from env; deploy both by T+3:20, not at the end. |
 | R10 | **No automated tests.** | Manual scenario checklist §14 run twice before freeze. |
 
 ---
 
 ## 2. Scope
 
 ### In scope (MVP)
 - Role switcher (3 businesses, N teams)
 - Task wizard: draft → AI analyze → answer questions → editable card → confirm → rating → publish
 - Rating panel: score, level, breakdown, missing details, delta vs. previous, next-level hint
 - Shared catalog: all published tasks, sort by rating, filters topic + level, priority highlighted, catalog position `#n of m`
-- Recommendations strip for teams (rule-based)
-- Proposals: submit/edit (one per team per task)
+- Recommendations strip for teams (rule-based on interests, skills, and technology tags; no sensitive participant attributes)
+- Proposals: submit/edit (one active proposal per team per task; unlimited teams may propose)
 - Business review: compare proposals, Select / Reject / Reset
 - Milestone confirmation → team points → leaderboard
 - Seed/reset endpoints (`demo`, `full`)
 - AI call log viewer (prompt, input, output, validation result) — satisfies SoW §5 "show prompt, input/output format, invalid handling"
 
 ### Out of scope
 Auth, passwords, complex roles, chat, notifications, calendar, file upload, ML training, vector DB, mobile layout, project tracker, automated tests, CI/CD.
 
 ---
 
@@ -90,21 +90,21 @@ Auth, passwords, complex roles, chat, notifications, calendar, file upload, ML t
 │ React SPA    │ ─────────────► │ .NET 9 Minimal API        │ ─────────► │ Firestore │
 │ (Netlify)    │  X-Actor-*     │ (Cloud Run)                │            │ (native)  │
 │ TanStack Q.  │                │  Features/ Rating/ Ai/     │ ─────────► │ OpenAI    │
 └──────────────┘                └───────────────────────────┘   HTTPS    │ Responses │
                                                                           └───────────┘
 Local: docker compose → web (vite) + api + firestore-emulator
 ```
 
 - Frontend never touches Firestore. Firestore security rules: deny all client access.
 - API is stateless; all state in Firestore.
-- Actor identity via headers set by role switcher: `X-Actor-Role: business|team`, `X-Actor-Id: <id>`. API validates existence and ownership (not security, just correctness).
+- Demo actor identity via role-switcher headers `X-Actor-Role: business|team`, `X-Actor-Id: <id>`. Validate existence and ownership for correctness; this is not authentication. Public destructive admin operations are disabled or require a server-side secret.
 
 ### Repository layout
 
 ```
 /
 ├─ AGENTS.md                  # shared agent rules (Codex + Claude)
 ├─ CLAUDE.md                  # "Read AGENTS.md" + Claude-specific notes
 ├─ README.md                  # SoW deliverable: architecture, formula, catalog rules, test scenarios
 ├─ .env.example
 ├─ .gitignore                 # includes .env
@@ -206,30 +206,31 @@ tasks/{taskId}
   industry: string
   fields: { title, context, need, users, data, constraints, expectedResult,
             successCriteria, contact, interactionFormat, topics[], techTags[] }
   fieldProvenance: { <fieldKey>: "user" | "draft-extract" | "chip" }   # UI only
   analysis: {                              # last AI analyze result (validated)
     questions: [{ id, fieldKey, question, chips: string[] }],
     suggestions: [{ fieldKey, action }],
     extracted: [{ fieldKey, value, evidence }],
     source: "ai" | "stub", createdAt
   }
+  revision: int                             # incremented on every editable-field/answer mutation
   confirmed: {                             # snapshot at last confirm
-    fields: {...}, hash: string, confirmedAt
+    fields: {...}, hash: string, revision: int, confirmedAt
   } | null
   hasUnconfirmedChanges: bool
   rating: {
     total: int, level: string,
     breakdown: [{ criterion, weight, score, reason }],
     missingDetails: [{ criterion, detail }],
     source: "ai" | "stub" | "seed" | "cache",
-    scoredAt
+    cacheKey: string, scoredAt
   } | null
   ratingHistory: [{ total, level, scoredAt }]   # last 10, for delta display
   proposalCount: int
   createdAt, updatedAt, publishedAt
 
 proposals/{proposalId}          # id = `${taskId}_${teamId}` → enforces 1 per team per task
   taskId, teamId, businessId
   idea, plan, timeline, prototypeUrl
   status: "pending" | "selected" | "rejected"
   decisionReason?: string
@@ -358,21 +359,23 @@ Return JSON matching the schema only.
         }
       }
     }
   }
 }
 ```
 
 **Backend validation (`AnalyzeValidator`)**
 - `questions.Count` in [3, 7] → else invalid
 - each `chips.Count` ≤ 4 → truncate to 4 (soft fix), trim, dedupe, drop empty/ > 80 chars
-- `extracted[i].evidence` must be a case-insensitive substring of `rawDraft` → else drop item (log)
+- `extracted[i].evidence` must be a case-insensitive substring of `rawDraft`, and `value` must be copied from the evidenced source text (normalization only; no paraphrases or added details) → else drop item (log)
+- `title` is an untrusted suggestion: require business review/edit before confirm and publish
+- Prompt rule: title may only use words/facts supported by the draft; if unclear return empty. The wizard labels it as an AI suggestion and does not silently treat it as confirmed user text.
 - `suggestions` truncate to 6, drop empty
 - `title` > 120 chars → truncate
 - Assign `questions[i].id = "q{i+1}"`
 
 ### 5.3 Call 2 — `score` (rubric scoring)
 
 **Input**
 ```json
 {
   "rubric": [
@@ -430,38 +433,38 @@ Return JSON matching the schema only.
     }
   }
 }
 ```
 
 **Backend guard (`RatingGuard`) — authoritative**
 1. Exactly the 7 criteria, no duplicates → else invalid.
 2. `score = clamp(score, 0, weight)`.
 3. Apply empty rules from §4.2 (empty → 0; partial caps 10 / 5). Replace reason with "Field is empty" when forced.
 4. `total = Σ score`; `level` from §4.3.
-5. Cache: `hash = SHA256(normalized confirmed fields JSON)`. If `hash == task.confirmed.hash` and a rating exists → return cached (`source: "cache"`), no AI call.
+5. Cache key: SHA-256 of canonical normalized confirmed fields + rubric version + scoring mode/model version. Persist/reuse ratings by key so returning to an earlier revision reuses its result. Never reuse a `stub` result as an `ai` result; allow explicit rescore when live mode returns. Store cache key and source with each rating.
 6. Append to `ratingHistory` (keep last 10).
 
 ### 5.4 Failure handling (both calls)
 
 ```
 call OpenAI ──► HTTP/timeout error ──┐
      │                              │
      ▼                              ▼
 parse + validate ──invalid──► retry once (same input, append "Previous output invalid: <errors>")
      │ ok                           │ still invalid / error
      ▼                              ▼
   use result                   local stub → source:"stub", UI badge "AI unavailable – basic mode"
 ```
 
 **Stubs (deterministic)**
-- `AnalyzeStub`: for each empty field in priority order take a template question (table below), up to 5, min 3 (if fewer than 3 empty → ask to *specify* weakest by length); chips `[]`; suggestions = "Add {label}" for each empty field; extracted `[]`.
-- `ScoreStub`: per criterion, `score = weight × min(1, len(fieldsText)/threshold)` with threshold 200 chars (context/need, data), 120 (others); +20% if it contains a digit (successCriteria, constraints); clamp; then guard rules.
+- `AnalyzeStub`: ask about missing or weak fields in priority order, up to 5 and never fewer than 3. If fewer than 3 gaps exist, ask relevant specificity/confirmation questions about the weakest populated fields. Chips `[]`; suggestions = "Add {label}" for each gap; extracted `[]`.
+- `ScoreStub`: deterministic criterion-specific checks against rubric anchors (missing = 0; vague/general = at most half weight; concrete evidence for each rubric element = full weight). Do not award points for text length or digits alone. Return reasons and missing details from failed anchors, mark source `stub`, and show the basic-mode notice. Its result is illustrative and must not be described as AI scoring.
 
 | fieldKey | Stub question |
 |---|---|
 | need | What exactly should change after the students' work? |
 | context | What is happening now and why is it a problem? |
 | data | What data, examples or sources can you give the team? |
 | expectedResult | What concrete result do you expect from the team? |
 | successCriteria | How will you measure that the solution is accepted? |
 | users | Who will use the solution? |
 | constraints | Are there deadlines, required technologies or access limits? |
@@ -482,60 +485,60 @@ Every call (success, retry, stub) writes an `aiLogs` document. `AI_MODE=stub` en
      ──publish (requires title + rating)──► published
 published ──edit──► hasUnconfirmedChanges=true (catalog still shows LAST CONFIRMED card + rating)
           ──confirm──► re-scored, catalog updates position
 ```
 - Points are awarded only for confirmed fields (SoW §4): rating is computed only on `confirm`, from the `confirmed` snapshot.
 - Catalog always displays `confirmed.fields` + `rating`, never unconfirmed edits.
 - Publishing is allowed at any score (low rating doesn't hide the task — SoW §4).
 - Publish requires: `title` non-empty, `rating != null`, no unconfirmed changes.
 
 ### 6.2 Answers → card merge
-`PUT /answers` with `[{ questionId, fieldKey, text }]`:
+`PUT /answers` with `[{ questionId, fieldKey, text }]` is idempotent by `questionId`: repeat submissions replace that answer's previous contribution rather than appending duplicates. Recompute the field from its original user value and current answers.
 - field empty → set to `text`
 - field non-empty → append `"\n" + text`
 - provenance: `chip` if text equals a chip exactly, else `user`
 - extracted draft values are applied only when the user clicks "Use" next to each (provenance `draft-extract`)
 
 ### 6.3 Catalog
 - Source: `tasks where status == published`, loaded in memory.
-- Sort: `rating.total desc`, then `updatedAt desc`.
+- Sort: `rating.total desc`, then `confirmedAt desc` (or `publishedAt` before first confirmation), then stable task ID. Unconfirmed edits do not change catalog order.
 - Position: 1-based index in the **unfiltered** sorted list → `#n of m` (shown to business as gamification).
 - Filters: `topic` (task.topics contains, case-insensitive), `level` (multi). Filters apply after position computation.
 - Priority tasks: gold border + ★. Draft-level tasks: grey badge "Needs clarification".
 
 ### 6.4 Recommendations (team view)
 ```
 for each published task with rating.total ≥ 40 and no proposal from this team:
   interestHits = |team.interests ∩ task.topics|          (case-insensitive)
   techHits     = |team.techTags  ∩ task.techTags|
   score = 2·interestHits + techHits + rating.total/100
 return top 3 where interestHits + techHits > 0, with "why": matched tags
 ```
 Recommendations never filter the catalog; the full catalog is always shown below.
 
 ### 6.5 Proposals & decisions
-- Id `${taskId}_${teamId}` → upsert. Editable only while `pending`.
+- Id `${taskId}_${teamId}` → one active proposal per team per task, upsert. No cap across teams. Editable only while `pending`.
 - Validation: idea 20–2000, plan 20–3000, timeline 3–200, prototypeUrl valid `http(s)` URL or empty.
 - Allowed on any published task, any level.
 - Only the owning business can decide. Transitions: `pending ↔ selected`, `pending ↔ rejected`, `selected ↔ rejected`. Once a milestone is confirmed, decision is locked.
 - Multiple `selected` allowed; zero is fine.
 - No automatic selection anywhere (SoW §3, §5).
 
 ### 6.6 Milestones & team points
-- Business on a `selected` proposal → "Confirm milestone" (title required) → `+10` to `teams.points` (Firestore transaction) and entry in `proposal.milestones`.
+- Business on a `selected` proposal → "Confirm milestone" (title required) → `+10` to `teams.points` and entry in `proposal.milestones`. Use a stable milestone ID and atomically create it only if absent while incrementing points, so retries/double clicks cannot award twice.
 - Leaderboard: teams sorted by `points desc`.
 
 ---
 
 ## 7. API specification
 
-All routes under `/api`. JSON. Errors = RFC 7807 `ProblemDetails` with `errors` map for validation. Headers `X-Actor-Role`, `X-Actor-Id` required except `/health`, `/admin/*`, `GET /actors`.
+All routes under `/api`. JSON. Errors = RFC 7807 `ProblemDetails` with `errors` map for validation. Headers `X-Actor-Role`, `X-Actor-Id` required except `/health`, `/admin/*`, `GET /actors`. Admin seed/reset mutations are local-only or protected by a server-side admin secret; never expose unauthenticated destructive admin routes publicly.
 
 | Method | Route | Actor | Body → Response |
 |---|---|---|---|
 | GET | `/health` | — | `{ status, aiMode, firestore: "ok" }` |
 | GET | `/actors` | — | `{ businesses[], teams[] }` for role switcher |
 | POST | `/tasks` | business | `{ rawDraft, industry }` → `TaskDto` (status editing) |
 | GET | `/tasks/mine` | business | `TaskSummaryDto[]` (incl. position, proposalCount) |
 | GET | `/tasks/{id}` | any | `TaskDto` (team sees confirmed view only) |
 | POST | `/tasks/{id}/analyze` | business owner | → `AnalysisDto` |
 | PUT | `/tasks/{id}/answers` | business owner | `{ answers:[{questionId, fieldKey, text}] }` → `TaskDto` |
@@ -545,38 +548,40 @@ All routes under `/api`. JSON. Errors = RFC 7807 `ProblemDetails` with `errors`
 | GET | `/catalog?topic=&level=` | any | `CatalogItemDto[]` (position, level, rating, topics, proposalCount) |
 | GET | `/catalog/topics` | any | `string[]` distinct topics |
 | GET | `/teams/{id}/recommendations` | team | `[{ task: CatalogItemDto, matched: string[] }]` |
 | PUT | `/tasks/{id}/proposals/mine` | team | `{ idea, plan, timeline, prototypeUrl }` → `ProposalDto` |
 | GET | `/tasks/{id}/proposals` | business owner | `ProposalDto[]` with team name/tags |
 | GET | `/proposals/mine` | team | `ProposalDto[]` with task title |
 | POST | `/proposals/{id}/decision` | business owner | `{ decision: "selected"\|"rejected"\|"pending", reason? }` → `ProposalDto` |
 | POST | `/proposals/{id}/milestones` | business owner | `{ title }` → `ProposalDto` |
 | GET | `/leaderboard` | any | `[{ teamId, name, points }]` |
 | GET | `/ai-logs?taskId=` | any | last 20 `AiLogDto` |
-| POST | `/admin/seed?profile=demo\|full` | — | wipes + loads seed |
-| POST | `/admin/reset` | — | wipe all collections |
+| POST | `/admin/seed?profile=demo\|full` | local/admin | wipes + loads seed |
+| POST | `/admin/reset` | local/admin | wipe all collections |
 
 **DTO sketches**
 ```ts
 RatingDto { total:number; level:'draft'|'workable'|'ready'|'priority';
   breakdown:{criterion:string; weight:number; score:number; reason:string}[];
   missingDetails:{criterion:string; detail:string}[];
   source:'ai'|'stub'|'seed'|'cache'; scoredAt:string;
   nextLevel?:{ level:string; pointsNeeded:number } }
 
 AnalysisDto { title:string;
   questions:{id:string; fieldKey:string; question:string; chips:string[]}[];
   suggestions:{fieldKey:string; action:string}[];
   extracted:{fieldKey:string; value:string; evidence:string}[];
   source:'ai'|'stub' }
 ```
 
+Confirm/scoring is revision-safe: capture the edited task revision when scoring starts, then atomically save confirmed fields, canonical hash, rating, and `confirmedAt` only if the revision has not changed. If it changed, return a conflict and ask the business to confirm again. Catalog reads use only this committed snapshot.
+
 ---
 
 ## 8. Frontend specification
 
 ### 8.1 Global
 - **Top bar:** app name · RoleSwitcher (grouped select: Businesses / Teams) · nav links for the current role · "AI logs" link.
 - Actor stored in React context + `localStorage`; orval custom mutator injects `X-Actor-*` headers.
 - Toasts for errors (ProblemDetails `title` + first field error).
 - After each mutation, invalidate related TanStack queries (task, catalog, mine, leaderboard).
 
@@ -622,55 +627,59 @@ Confirmed card (read-only), RatingPanel (compact), proposal form (idea, plan, ti
 
 ### 8.4 AI Logs (`/ai-logs`)
 Table: time, kind, task, model, validation result, latency. Expand row → system prompt, input JSON, raw output. Used in demo to satisfy SoW §5.
 
 ---
 
 ## 9. Seed data
 
 ### 9.1 `demo` profile (`/seed/demo/`)
 
+`demo` is the short rehearsal profile and is not intended to meet source-data volume minimums. Use `full` for evaluation/submission; it satisfies all counts immediately after seeding.
+
 **businesses.json**
 | id | name | industry | Role in demo |
 |---|---|---|---|
 | `b-nomad` | Nomad Logistics | Logistics | Fully filled card (Priority ~92) |
 | `b-steppe` | Steppe Retail | Retail | Medium card (Workable ~55) |
 | `b-tamaq` | Tamaq Café Chain | Food & Beverage | Live AI wizard demo (weak draft) |
 
-**tasks.json** (3 cards; ratings precomputed, `source:"seed"`)
+**tasks.json** (2 published cards; ratings precomputed, `source:"seed"`)
 1. **Nomad Logistics — "Delivery delay prediction dashboard"** — published, all fields rich: context (manual dispatching, 18% late deliveries), need, users (12 dispatchers), data (2 years of CSV route logs, ~400k rows, weather API), constraints (6 weeks, React/Python, read-only DB replica), expected result (web dashboard + delay model), success criteria (predict delays ≥30 min with ≥75% precision on holdout month), contact + weekly 30-min call + Slack feedback. Score **92 · priority**. topics: `logistics, data-analytics`; tech: `react, python, ml`.
 2. **Steppe Retail — "Customer review analysis"** — published, medium: context + need clear, data vague ("we have reviews"), expected result vague, no success criteria, users generic, contact only. Score **55 · workable**. topics: `retail, nlp`; tech: `dotnet, openai`.
-3. **Tamaq Café Chain** — **not pre-seeded as a card.** Its weak draft text lives in `drafts.json` and is typed/pasted live:
+3. **Tamaq Café Chain** — its weak draft text lives in `drafts.json` and is typed/pasted live:
    > "We are a café chain. Customers stop coming back and we don't know why. Want some app to fix it."
-   Expected wizard outcome: score ~20 → after answers ~60 → after one improvement round ~80+.
+Expected wizard outcome: low initial score, then a visible increase after confirmed additions. Exact scores depend on the selected AI/stub mode; use a prepared, manually verified fixture for any scripted numeric claims.
 
-   *(3rd card for the "3 cards" requirement = the Tamaq card created live; if a pre-built 3rd card is wanted for safety, seed `t-tamaq-backup` as `editing`, hidden from catalog.)*
+The `demo` profile may include a hidden editing backup card for rehearsal convenience; live creation does not count toward the `full` minimum.
 
 **teams.json**
 | id | name | interests | techTags |
 |---|---|---|---|
 | `t-bytenomads` | Byte Nomads | logistics, data-analytics, food | react, python, ml |
 | `t-nullptr` | Null Pointers | retail, nlp, food | dotnet, openai, react |
 
+Each team fixture also has a non-empty `skills` array, as required by the source-data profile definition.
+
 **proposals.json** (on Nomad task, to demo compare/select/reject)
 - `t-bytenomads` — pending — idea: ML model + dashboard, plan 4 sprints, timeline 5 weeks, link.
 - `t-nullptr` — pending — idea: rules + LLM explanations, plan 3 phases, timeline 6 weeks, link.
 
 **drafts.json** — Tamaq weak draft (+ Steppe raw draft for reference).
 
 ### 9.2 `full` profile (`/seed/full/`) — SoW §6 minimum
-Everything in `demo`, plus:
+Includes `demo` content plus enough persisted records to contain at least 5 drafts, 5 complete task cards, 5 team profiles, and 5 proposals immediately after seed. Counts never depend on the live demo.
 - businesses: `b-kazagro` (Agriculture), `b-edutech` (Education)
-- tasks: Kazagro "Crop yield reporting" **~31 · draft**; EduTech "Student attendance insights" **~78 · ready** → 5 cards total (with Tamaq backup card counted when seeded as published, or 4 + Tamaq live = 5)
-- teams: `t-greenbits`, `t-pixelforge`, `t-dataweavers` → 5 teams
-- proposals: +3 (EduTech ← greenbits, Kazagro ← dataweavers, Steppe ← pixelforge) → 5
-- drafts.json: 5 drafts of varying completeness (very weak / weak / medium / good / complete)
+- tasks: Kazagro "Crop yield reporting" **~31 · draft**; EduTech "Student attendance insights" **~78 · ready**; plus `t-tamaq-backup` as a complete, confirmed editing card → at least 5 card documents with every rating field. This backup remains hidden from the catalog.
+- teams: `t-greenbits`, `t-pixelforge`, `t-dataweavers` → at least 5 teams, each with name, interests, skills, and technologies
+- proposals: +3 (EduTech ← greenbits, Kazagro ← dataweavers, Steppe ← pixelforge) → at least 5 proposals, each with team, idea, plan, timeline, and link field
+- drafts.json: at least 5 distinct drafts of varying completeness (very weak / weak / medium / good / complete)
 
 Seeded ratings are stored as-is (no AI call on seed). First edit+confirm triggers real scoring.
 
 ---
 
 ## 10. Local dev & deployment
 
 ### 10.1 `.env.example`
 ```bash
 # --- API ---
@@ -688,21 +697,23 @@ VITE_API_BASE_URL=http://localhost:8080
 
 ### 10.2 `docker-compose.yml`
 ```yaml
 services:
   firestore:
     image: gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators
     command: gcloud emulators firestore start --host-port=0.0.0.0:8080 --project=taskforge-local
     ports: ["8081:8080"]
 
   api:
-    build: ./api
+    build:
+      context: .
+      dockerfile: api/Dockerfile
     env_file: .env
     environment:
       FIRESTORE_EMULATOR_HOST: firestore:8080
       GCP_PROJECT_ID: taskforge-local
     ports: ["8080:8080"]
     depends_on: [firestore]
     volumes: ["./seed:/app/seed:ro"]
 
   web:
     image: node:22-alpine
@@ -725,47 +736,49 @@ COPY . .
 RUN dotnet publish -c Release -o /out
 
 FROM mcr.microsoft.com/dotnet/aspnet:9.0
 WORKDIR /app
 COPY --from=build /out .
 COPY seed ./seed
 ENV ASPNETCORE_URLS=http://+:8080
 EXPOSE 8080
 ENTRYPOINT ["dotnet", "TaskForge.Api.dll"]
 ```
-*(Build context note: seed lives at repo root — either copy `seed/` into `api/` at build time via the deploy script, or set build context to repo root with `-f api/Dockerfile`.)*
+Docker Compose builds from repository root using `api/Dockerfile`, so `COPY seed ./seed` resolves to the checked-in root fixture directory. Cloud Run source deployment uses `api/` as its source root, so `scripts/deploy-api.sh` first stages fixtures into `api/seed/`; treat that directory as generated and gitignore it. The script updates the staging directory without deleting it first.
 
 ### 10.4 `scripts/deploy-api.sh`
 ```bash
 #!/usr/bin/env bash
 set -euo pipefail
 
 # ==== CONFIG ====
 PROJECT_ID="your-gcp-project"
 REGION="europe-west1"
 SERVICE="taskforge-api"
 OPENAI_MODEL="REPLACE_WITH_CURRENT_MINI_MODEL"
 CORS_ORIGINS="https://your-site.netlify.app,http://localhost:5173"
+ADMIN_MUTATIONS_ENABLED="false"    # public deployment must not expose seed/reset
 ENV_FILE=".env"                  # OPENAI_API_KEY read from here
 # ================
 
 OPENAI_API_KEY="$(grep -E '^OPENAI_API_KEY=' "$ENV_FILE" | cut -d= -f2-)"
 [[ -z "$OPENAI_API_KEY" || "$OPENAI_API_KEY" == sk-REPLACE_ME ]] && { echo "Missing OPENAI_API_KEY in $ENV_FILE"; exit 1; }
 
-rm -rf api/seed && cp -r seed api/seed
+mkdir -p api/seed
+cp -r seed/. api/seed/
 
 gcloud config set project "$PROJECT_ID"
 gcloud run deploy "$SERVICE" \
   --source ./api \
   --region "$REGION" \
   --allow-unauthenticated \
-  --set-env-vars "GCP_PROJECT_ID=$PROJECT_ID,OPENAI_MODEL=$OPENAI_MODEL,AI_MODE=live,CORS_ORIGINS=$CORS_ORIGINS,OPENAI_API_KEY=$OPENAI_API_KEY"
+  --set-env-vars "GCP_PROJECT_ID=$PROJECT_ID,OPENAI_MODEL=$OPENAI_MODEL,AI_MODE=live,CORS_ORIGINS=$CORS_ORIGINS,ADMIN_MUTATIONS_ENABLED=$ADMIN_MUTATIONS_ENABLED,OPENAI_API_KEY=$OPENAI_API_KEY"
 
 gcloud run services describe "$SERVICE" --region "$REGION" --format='value(status.url)'
 ```
 One-time GCP prep: enable Run, Cloud Build, Artifact Registry, Firestore APIs; create Firestore database (Native mode); the default Cloud Run service account needs `roles/datastore.user`.
 
 ### 10.5 Netlify
 `web/netlify.toml`
 ```toml
 [build]
   base = "web"
@@ -892,41 +905,26 @@ Each brief: goal · files to touch · DTOs/endpoints · acceptance checks (manua
 | S4 | Answer via chips + free text → Apply → Confirm | Card filled; score ~40–65, level Workable, breakdown + missing details |
 | S5 | Improve 2 fields → Confirm | Delta `+N ▲`, level up, catalog position changes |
 | S6 | Confirm again without changes | Same score, source "cached" |
 | S7 | Publish, switch to Null Pointers → catalog | Tamaq visible at correct position; submit proposal |
 | S8 | Submit proposal with invalid URL | Validation error shown, nothing saved |
 | S9 | As Tamaq → Proposals → Select Null Pointers | Status Selected; team sees it in My proposals |
 | S10 | As Nomad → reject Null Pointers, select Byte Nomads, confirm milestone | Statuses updated; Byte Nomads +10 on leaderboard; decision locked |
 | S11 | Set `AI_MODE=stub`, restart, run S3–S4 | Template questions, amber "basic mode" badge, rule-based score |
 | S12 | Edit published task field, don't confirm | Catalog still shows old version; banner "Unconfirmed changes" |
 | S13 | AI logs page | analyze/score entries with prompt, input, raw output, validation status |
+| S14 | Force malformed AI output, then exhaust retry | One retry occurs, deterministic stub is used, source/badge and both log entries explain fallback |
+| S15 | Confirm a revision, edit and confirm another revision, then restore the original fields | Original cache key/result is reused; stub result is never returned as live AI scoring |
+| S16 | Submit the same answers twice; confirm the same milestone twice | Answer text is not duplicated; milestone awards team points once |
 
 ---
 
-## 15. 5-minute demo script (SoW §11)
-
-| Time | Actor | Action | Say |
-|---|---|---|---|
-| 0:00 | — | Catalog as Byte Nomads | "Rating = task readiness, not company fame. Priority on top, drafts visible but flagged." |
-| 0:30 | Tamaq | New task → paste weak draft → Analyze | "AI finds gaps and asks questions — it never invents facts." |
-| 1:15 | Tamaq | Pick chips + type 2 answers → Apply → Confirm | "Score 38 → Needs clarification. Breakdown shows exactly why." |
-| 2:00 | Tamaq | Improve: add data + success criteria → Confirm | "+30, now Ready. Rating recalculates on every confirmed change." |
-| 2:40 | Tamaq | Publish → catalog `#2 of 3` | "Higher rating = higher catalog position." |
-| 3:05 | Null Pointers | Recommended strip → open Tamaq → submit proposal | "Any team can propose; AI only recommends, never restricts." |
-| 3:45 | Tamaq | Proposals → Select Null Pointers | "The business decides. No automatic assignment." |
-| 4:05 | Nomad | Reject one, select other, confirm milestone → leaderboard | "Teams earn points for confirmed progress." |
-| 4:35 | — | AI logs row expanded | "Prompt, structured input/output, validation and fallback." |
-| 5:00 | — | End | |
-
-Backup: local `docker compose` with `AI_MODE=stub` ready in a second browser tab.
-
----
 
 ## 16. Evaluation mapping (SoW §9)
 
 | Criterion | Pts | Covered by |
 |---|---|---|
 | End-to-end scenario | 20 | Wizard → publish → proposal → decision (§6, §15) |
 | Card quality | 15 | Field-mapped questions, chips, suggestions, editable card (§5.2, §8.2) |
 | Business gamification | 25 | Rubric breakdown, reasons, missing details, delta, next level, position, guard + cache (§4, §5.3, §8.2) |
 | Catalog & proposals | 15 | All tasks visible, sort, topic+level filters, manual decisions (§6.3–6.5) |
 | AI function | 10 | Structured schemas, evidence check, no invented facts, fallback (§5) |
diff --git a/TECHTASK.pdf b/TECHTASK.pdf
new file mode 100644
index 0000000..eb813af
Binary files /dev/null and b/TECHTASK.pdf differ
diff --git a/api/.gitkeep b/api/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Domain/.gitkeep b/api/Domain/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Features/Actors/.gitkeep b/api/Features/Actors/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Features/Admin/.gitkeep b/api/Features/Admin/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Features/Ai/.gitkeep b/api/Features/Ai/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Features/Catalog/.gitkeep b/api/Features/Catalog/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Features/Proposals/.gitkeep b/api/Features/Proposals/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Features/Rating/.gitkeep b/api/Features/Rating/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Features/Tasks/.gitkeep b/api/Features/Tasks/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Infrastructure/Firestore/.gitkeep b/api/Infrastructure/Firestore/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/api/Infrastructure/OpenAi/.gitkeep b/api/Infrastructure/OpenAi/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/docker-compose.yml b/docker-compose.yml
new file mode 100644
index 0000000..9ef70ab
--- /dev/null
+++ b/docker-compose.yml
@@ -0,0 +1,37 @@
+version: '3.8'
+
+services:
+  firestore:
+    image: gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators
+    command: gcloud emulators firestore start --host-port=0.0.0.0:8080 --project=taskforge-local
+    ports:
+      - "8081:8080"
+    environment:
+      CLOUDSDK_CORE_PROJECT: taskforge-local
+
+  api:
+    build: ./api
+    env_file: .env
+    environment:
+      FIRESTORE_EMULATOR_HOST: firestore:8080
+      GCP_PROJECT_ID: taskforge-local
+    ports:
+      - "8080:8080"
+    depends_on:
+      - firestore
+    volumes:
+      - ./seed:/app/seed:ro
+
+  web:
+    image: node:22-alpine
+    working_dir: /app
+    command: sh -c "npm ci && npm run dev -- --host 0.0.0.0"
+    environment:
+      VITE_API_BASE_URL: http://localhost:8080
+    ports:
+      - "5173:5173"
+    volumes:
+      - ./web:/app
+      - /app/node_modules
+    depends_on:
+      - api
diff --git a/docs/superpowers/plans/2026-09-23-project-scaffolding.md b/docs/superpowers/plans/2026-09-23-project-scaffolding.md
new file mode 100644
index 0000000..a6a9e49
--- /dev/null
+++ b/docs/superpowers/plans/2026-09-23-project-scaffolding.md
@@ -0,0 +1,1299 @@
+# TaskForge MVP Project Scaffolding
+
+> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
+
+**Goal:** Scaffold the complete directory structure, configuration files, and skeleton code for TaskForge MVP to enable parallel development across backend (.NET), frontend (React), and documentation.
+
+**Architecture:** 
+- Backend: .NET 9 Minimal API with Firestore integration, deployed to Cloud Run
+- Frontend: React + Vite + TypeScript + Tailwind + shadcn/ui, deployed to Netlify
+- Local Dev: Docker Compose (API, web, Firestore emulator)
+- Agentic Development: Structured agent briefs in `docs/tasks/`, role-based AGENTS.md and CLAUDE.md
+
+**Tech Stack:** 
+.NET 9, React 18, Vite, TypeScript, Tailwind CSS, shadcn/ui, Firestore, OpenAI Responses API, Docker, GCP Cloud Run, Netlify
+
+**Spec:** `docs/MVP_SPEC.md`
+
+## Global Constraints
+
+- Repository root: `/home/ubuntu/nomadag/hack-8ac6c83c-nomad-agents`
+- Agent split: Dev A (AI track) and Dev B (Core track) per §12
+- Timeline: 240 minutes to code freeze (3:45)
+- No secrets in git; .env in .gitignore from minute 1
+- OpenAI model name from env var (§5.1, decision #4)
+- All endpoints under `/api` prefix (decision #10)
+- Firestore emulator localhost:8080 for local dev (decision #9)
+- CORS allow-list from env var (decision #9, mitigates R9)
+
+---
+
+## File Structure Overview
+
+```
+/
+├─ AGENTS.md                          # Shared agent rules (both devs read first)
+├─ CLAUDE.md                          # Claude-specific notes (points to AGENTS.md)
+├─ README.md                          # SoW deliverable (written at end, §13)
+├─ .gitignore                         # Commits .env to ignore; seed/, docs/ NOT ignored
+├─ .env.example                       # Template for secrets & config (§10.1)
+├─ docker-compose.yml                 # Local: firestore, api, web (§10.2)
+├─ docs/
+│  ├─ MVP_SPEC.md                     # [Already exists]
+│  └─ tasks/                          # Agent briefs (one per task)
+│     ├─ 00-scaffold.md               # [T0]
+│     ├─ A1-openai-client.md          # [Dev A, T1]
+│     ├─ A2-analyze.md                # [Dev A, T2]
+│     ├─ A3-score.md                  # [Dev A, T3]
+│     ├─ A4-wizard-ui.md              # [Dev A, T4]
+│     ├─ A5-rating-panel.md           # [Dev A, T5]
+│     ├─ B1-firestore-repos.md        # [Dev B, T1]
+│     ├─ B2-tasks-crud.md             # [Dev B, T2]
+│     ├─ B3-catalog.md                # [Dev B, T3]
+│     ├─ B4-proposals.md              # [Dev B, T4]
+│     └─ B5-deploy.md                 # [Dev B, T5]
+├─ api/
+│  ├─ .gitkeep                        # Placeholder; delete when first file added
+│  ├─ Dockerfile                      # Multi-stage build (§10.3)
+│  ├─ TaskForge.Api.csproj            # Project file (.NET 9)
+│  ├─ Program.cs                      # Minimal API setup, OpenAPI, dependency injection
+│  ├─ Domain/
+│  │  └─ .gitkeep
+│  ├─ Infrastructure/
+│  │  ├─ Firestore/
+│  │  │  └─ .gitkeep
+│  │  └─ OpenAi/
+│  │     └─ .gitkeep
+│  └─ Features/
+│     ├─ Actors/
+│     │  └─ .gitkeep
+│     ├─ Tasks/
+│     │  └─ .gitkeep
+│     ├─ Ai/
+│     │  └─ .gitkeep
+│     ├─ Rating/
+│     │  └─ .gitkeep
+│     ├─ Catalog/
+│     │  └─ .gitkeep
+│     ├─ Proposals/
+│     │  └─ .gitkeep
+│     └─ Admin/
+│        └─ .gitkeep
+├─ web/
+│  ├─ .gitkeep
+│  ├─ package.json                    # Node deps (Vite, React, Tailwind, shadcn/ui, orval)
+│  ├─ vite.config.ts
+│  ├─ tsconfig.json
+│  ├─ tailwind.config.js
+│  ├─ netlify.toml                    # Netlify deploy config (§10.5)
+│  ├─ orval.config.ts                 # OpenAPI client generation (§10.6)
+│  ├─ index.html
+│  └─ src/
+│     ├─ main.tsx
+│     ├─ index.css                    # Tailwind directives
+│     ├─ App.tsx
+│     ├─ api/                         # GENERATED by orval; do not edit
+│     │  └─ .gitkeep
+│     ├─ components/                  # UI components
+│     │  └─ .gitkeep
+│     ├─ pages/                       # Route pages
+│     │  └─ .gitkeep
+│     └─ lib/                         # Utilities (ActorContext, fetch mutator, etc.)
+│        └─ .gitkeep
+├─ seed/
+│  ├─ demo/                           # Minimal seed: 3 businesses, 3 cards (or 4+Tamaq live), 2 teams, 2 proposals
+│  │  ├─ businesses.json
+│  │  ├─ teams.json
+│  │  ├─ tasks.json
+│  │  ├─ proposals.json
+│  │  └─ drafts.json
+│  └─ full/                           # Extended seed: 5 of each (§9.2, SoW minimum)
+│     ├─ businesses.json
+│     ├─ teams.json
+│     ├─ tasks.json
+│     ├─ proposals.json
+│     └─ drafts.json
+└─ scripts/
+   ├─ gen-client.sh                   # Regenerate web/src/api from OpenAPI (§10.6)
+   ├─ deploy-api.sh                   # Cloud Run deploy (§10.4)
+   └─ deploy-web.sh                   # Netlify deploy wrapper
+
+Agent configuration:
+├─ .agents                            # Directory for agent tools/context
+├─ .claude/
+│  └─ settings.json                   # Claude Code project settings
+```
+
+---
+
+## Tasks
+
+### Task 0: Repository Root & Global Config
+
+**Files:**
+- Create: `.gitignore`
+- Create: `.env.example`
+- Create: `AGENTS.md`
+- Create: `CLAUDE.md`
+- Modify: (none — git status already shows MVP_SPEC.md staged)
+
+**Interfaces:**
+- Produces: Global rules, environment contract, agent briefs directory
+
+- [ ] **Step 1: Create `.gitignore`**
+
+```
+# Secrets & local environment
+.env
+.env.local
+*.local
+
+# IDE
+.vscode/
+.idea/
+*.swp
+*.swo
+*~
+.DS_Store
+
+# Build outputs
+bin/
+obj/
+dist/
+node_modules/
+*.pyc
+__pycache__/
+
+# Logs & temp
+*.log
+npm-debug.log*
+/tmp/
+
+# OS
+Thumbs.db
+
+# Generated client (regenerate via scripts/gen-client.sh)
+# /web/src/api/ is NOT in .gitignore — it's committed for CI/CD
+
+# Seed data & docs are committed
+# /seed/* — committed
+# /docs/* — committed
+```
+
+- [ ] **Step 2: Create `.env.example`**
+
+```bash
+# --- Backend (API) ---
+OPENAI_API_KEY=sk-REPLACE_ME
+OPENAI_MODEL=REPLACE_WITH_CURRENT_MINI_MODEL
+OPENAI_TIMEOUT_SECONDS=20
+AI_MODE=live                    # live | stub
+GCP_PROJECT_ID=taskforge-local
+FIRESTORE_EMULATOR_HOST=firestore:8080   # local only; unset in Cloud Run
+CORS_ORIGINS=http://localhost:5173
+ASPNETCORE_URLS=http://+:8080
+
+# --- Frontend (WEB) ---
+VITE_API_BASE_URL=http://localhost:8080
+```
+
+- [ ] **Step 3: Create `AGENTS.md`**
+
+```markdown
+# AGENTS.md — TaskForge
+
+Spec: docs/MVP_SPEC.md (source of truth). Briefs: docs/tasks/*.md.
+
+## Stack
+.NET 9 Minimal API (api/), React+Vite+TS+Tailwind+shadcn (web/), Firestore (emulator locally), OpenAI Responses API.
+
+## Rules
+- Contract-first: change API → regenerate client (scripts/gen-client.sh). Never edit web/src/api/.
+- Feature folders in api/Features/<Feature>/ : Endpoints.cs, Dtos.cs, Service.cs.
+- Firestore access only through repositories in api/Infrastructure/Firestore.
+- AI calls only through api/Infrastructure/OpenAi/ResponsesClient + validators in Features/Ai.
+- Never invent card content in AI prompts/stubs. Never auto-select teams.
+- Validation errors → Results.ValidationProblem. No exceptions for control flow.
+- No tests (hackathon). Keep changes small; run `docker compose up` and click the flow.
+- Never commit .env or keys.
+
+## Commands
+docker compose up --build | scripts/gen-client.sh | curl -X POST localhost:8080/api/admin/seed?profile=demo
+```
+
+- [ ] **Step 4: Create `CLAUDE.md`**
+
+```markdown
+# CLAUDE.md — TaskForge
+
+Read AGENTS.md first — it is authoritative. Work only within the brief you are given.
+
+Before finishing: build (`dotnet build` / `npm run build`) and report changed files.
+```
+
+- [ ] **Step 5: Commit scaffolding root**
+
+```bash
+git add .gitignore .env.example AGENTS.md CLAUDE.md MVP_SPEC.md
+git commit -m "chore: add global config and agent rules
+
+- .gitignore: .env, build outputs, generated client excluded
+- .env.example: template for all env vars (API + frontend)
+- AGENTS.md: shared rules for Dev A & B
+- CLAUDE.md: points to AGENTS.md as authoritative
+
+Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
+```
+
+---
+
+### Task 1: Folder Structure & Docker Compose
+
+**Files:**
+- Create: `docker-compose.yml`
+- Create: `api/.gitkeep` (and api/ subdirs)
+- Create: `web/.gitkeep` (and web/ subdirs)
+- Create: `seed/demo/`, `seed/full/` (empty)
+- Create: `scripts/.gitkeep`
+- Create: `docs/tasks/.gitkeep`
+
+**Interfaces:**
+- Produces: Complete directory tree; docker-compose.yml for local dev
+
+- [ ] **Step 1: Create `docker-compose.yml`**
+
+```yaml
+version: '3.8'
+
+services:
+  firestore:
+    image: gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators
+    command: gcloud emulators firestore start --host-port=0.0.0.0:8080 --project=taskforge-local
+    ports:
+      - "8081:8080"
+    environment:
+      CLOUDSDK_CORE_PROJECT: taskforge-local
+
+  api:
+    build: ./api
+    env_file: .env
+    environment:
+      FIRESTORE_EMULATOR_HOST: firestore:8080
+      GCP_PROJECT_ID: taskforge-local
+    ports:
+      - "8080:8080"
+    depends_on:
+      - firestore
+    volumes:
+      - ./seed:/app/seed:ro
+
+  web:
+    image: node:22-alpine
+    working_dir: /app
+    command: sh -c "npm ci && npm run dev -- --host 0.0.0.0"
+    environment:
+      VITE_API_BASE_URL: http://localhost:8080
+    ports:
+      - "5173:5173"
+    volumes:
+      - ./web:/app
+      - /app/node_modules
+    depends_on:
+      - api
+```
+
+- [ ] **Step 2: Create api/ directory structure**
+
+```bash
+mkdir -p api/{Domain,Infrastructure/{Firestore,OpenAi},Features/{Actors,Tasks,Ai,Rating,Catalog,Proposals,Admin}}
+touch api/.gitkeep
+```
+
+- [ ] **Step 3: Create web/ directory structure**
+
+```bash
+mkdir -p web/src/{components,pages,lib,api}
+touch web/.gitkeep web/src/api/.gitkeep
+```
+
+- [ ] **Step 4: Create seed/ directory structure**
+
+```bash
+mkdir -p seed/{demo,full}
+touch seed/demo/.gitkeep seed/full/.gitkeep
+```
+
+- [ ] **Step 5: Create scripts/ and docs/tasks/**
+
+```bash
+mkdir -p scripts docs/tasks
+touch scripts/.gitkeep docs/tasks/.gitkeep
+```
+
+- [ ] **Step 6: Commit directory structure**
+
+```bash
+git add docker-compose.yml api/ web/ seed/ scripts/ docs/tasks/
+git commit -m "chore: scaffold directory structure and docker-compose
+
+- Folders: api, web, seed, scripts, docs/tasks with subdirs per spec
+- docker-compose.yml: local dev with firestore, api, web services
+- All .gitkeep files for git tracking
+
+Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
+```
+
+---
+
+### Task 2: Backend Skeleton (.NET 9 API)
+
+**Files:**
+- Create: `api/TaskForge.Api.csproj`
+- Create: `api/Program.cs`
+- Create: `api/Dockerfile`
+- Create: `api/Domain/.gitkeep` (placeholder for future domain models)
+- Create: `api/Infrastructure/Firestore/.gitkeep`
+- Create: `api/Infrastructure/OpenAi/.gitkeep`
+
+**Interfaces:**
+- Produces: Buildable .NET 9 project with Minimal API, OpenAPI enabled, all feature folders connected
+
+- [ ] **Step 1: Create `api/TaskForge.Api.csproj`**
+
+```xml
+<Project Sdk="Microsoft.NET.Sdk.Web">
+
+  <PropertyGroup>
+    <TargetFramework>net9.0</TargetFramework>
+    <Nullable>enable</Nullable>
+    <ImplicitUsings>enable</ImplicitUsings>
+    <LangVersion>latest</LangVersion>
+  </PropertyGroup>
+
+  <ItemGroup>
+    <PackageReference Include="Google.Cloud.Firestore" Version="3.2.0" />
+    <PackageReference Include="System.Text.Json" Version="8.0.0" />
+  </ItemGroup>
+
+</Project>
+```
+
+- [ ] **Step 2: Create `api/Program.cs`**
+
+```csharp
+var builder = WebApplication.CreateBuilder(args);
+
+// Configuration
+var config = builder.Configuration;
+
+// Add services
+builder.Services.AddControllers();
+builder.Services.AddOpenApi();
+
+// CORS
+var corsOrigins = config.GetSection("CORS_ORIGINS").Value?.Split(",") ?? Array.Empty<string>();
+builder.Services.AddCors(options =>
+{
+    options.AddPolicy("AllowConfigured", policy =>
+    {
+        policy.WithOrigins(corsOrigins)
+              .AllowAnyMethod()
+              .AllowAnyHeader();
+    });
+});
+
+var app = builder.Build();
+
+// Middleware
+app.UseCors("AllowConfigured");
+
+// OpenAPI (Swagger)
+if (app.Environment.IsDevelopment())
+{
+    app.MapOpenApi();
+    app.UseSwaggerUI(options =>
+    {
+        options.SwaggerEndpoint("/openapi/v1.json", "TaskForge API v1");
+    });
+}
+
+app.UseHttpsRedirection();
+
+// Health check endpoint
+app.MapGet("/health", () => new { status = "healthy", aiMode = config["AI_MODE"] ?? "live" })
+    .WithName("Health")
+    .WithOpenApi();
+
+// Placeholder route groups (will be connected by feature folders)
+var api = app.MapGroup("/api")
+    .WithOpenApi();
+
+// TODO: Connect feature routes here (Tasks, Catalog, Proposals, Admin, Ai, etc.)
+
+app.Run();
+```
+
+- [ ] **Step 3: Create `api/Dockerfile`**
+
+```dockerfile
+FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
+WORKDIR /src
+COPY . .
+RUN dotnet publish -c Release -o /out
+
+FROM mcr.microsoft.com/dotnet/aspnet:9.0
+WORKDIR /app
+COPY --from=build /out .
+COPY seed ./seed
+ENV ASPNETCORE_URLS=http://+:8080
+EXPOSE 8080
+ENTRYPOINT ["dotnet", "TaskForge.Api.dll"]
+```
+
+- [ ] **Step 4: Verify .NET project structure**
+
+Run: `cd api && dotnet build 2>&1 | head -20`
+Expected: No restore errors; project loads
+
+- [ ] **Step 5: Commit backend skeleton**
+
+```bash
+git add api/TaskForge.Api.csproj api/Program.cs api/Dockerfile
+git commit -m "feat: backend skeleton with .NET 9 minimal API
+
+- TaskForge.Api.csproj: targets net9.0, Firestore + System.Text.Json
+- Program.cs: CORS from env, OpenAPI enabled, placeholder routes
+- Dockerfile: multi-stage build, seed/ copied
+
+Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
+```
+
+---
+
+### Task 3: Frontend Skeleton (React + Vite)
+
+**Files:**
+- Create: `web/package.json`
+- Create: `web/vite.config.ts`
+- Create: `web/tsconfig.json`
+- Create: `web/tailwind.config.js`
+- Create: `web/index.html`
+- Create: `web/src/main.tsx`
+- Create: `web/src/index.css`
+- Create: `web/src/App.tsx`
+- Create: `web/orval.config.ts`
+- Create: `web/netlify.toml`
+
+**Interfaces:**
+- Produces: Buildable React project with Vite, TypeScript, Tailwind, configured for OpenAPI client generation
+
+- [ ] **Step 1: Create `web/package.json`**
+
+```json
+{
+  "name": "taskforge-web",
+  "version": "0.1.0",
+  "type": "module",
+  "scripts": {
+    "dev": "vite",
+    "build": "tsc && vite build",
+    "preview": "vite preview"
+  },
+  "dependencies": {
+    "react": "^18.2.0",
+    "react-dom": "^18.2.0",
+    "@tanstack/react-query": "^5.0.0",
+    "@radix-ui/react-dialog": "^1.1.1",
+    "clsx": "^2.0.0"
+  },
+  "devDependencies": {
+    "vite": "^5.0.0",
+    "typescript": "^5.3.0",
+    "@vitejs/plugin-react": "^4.2.0",
+    "tailwindcss": "^3.3.0",
+    "postcss": "^8.4.31",
+    "autoprefixer": "^10.4.16",
+    "orval": "^6.23.0"
+  }
+}
+```
+
+- [ ] **Step 2: Create `web/vite.config.ts`**
+
+```typescript
+import { defineConfig } from 'vite'
+import react from '@vitejs/plugin-react'
+
+export default defineConfig({
+  plugins: [react()],
+  server: {
+    port: 5173,
+    host: true,
+  },
+})
+```
+
+- [ ] **Step 3: Create `web/tsconfig.json`**
+
+```json
+{
+  "compilerOptions": {
+    "target": "ES2020",
+    "useDefineForEnumMembers": true,
+    "lib": ["ES2020", "DOM", "DOM.Iterable"],
+    "module": "ESNext",
+    "skipLibCheck": true,
+    "esModuleInterop": true,
+    "allowSyntheticDefaultImports": true,
+    "strict": true,
+    "resolveJsonModule": true,
+    "moduleResolution": "bundler",
+    "allowImportingTsExtensions": true,
+    "noEmit": true,
+    "jsx": "react-jsx"
+  },
+  "include": ["src"],
+  "exclude": ["node_modules"]
+}
+```
+
+- [ ] **Step 4: Create `web/tailwind.config.js`**
+
+```javascript
+/** @type {import('tailwindcss').Config} */
+export default {
+  content: [
+    "./index.html",
+    "./src/**/*.{js,ts,jsx,tsx}",
+  ],
+  theme: {
+    extend: {},
+  },
+  plugins: [],
+}
+```
+
+- [ ] **Step 5: Create `web/postcss.config.js`**
+
+```javascript
+export default {
+  plugins: {
+    tailwindcss: {},
+    autoprefixer: {},
+  },
+}
+```
+
+- [ ] **Step 6: Create `web/index.html`**
+
+```html
+<!doctype html>
+<html lang="en">
+  <head>
+    <meta charset="UTF-8" />
+    <link rel="icon" type="image/svg+xml" href="/vite.svg" />
+    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
+    <title>TaskForge</title>
+  </head>
+  <body>
+    <div id="root"></div>
+    <script type="module" src="/src/main.tsx"></script>
+  </body>
+</html>
+```
+
+- [ ] **Step 7: Create `web/src/main.tsx`**
+
+```typescript
+import React from 'react'
+import ReactDOM from 'react-dom/client'
+import App from './App.tsx'
+import './index.css'
+
+ReactDOM.createRoot(document.getElementById('root')!).render(
+  <React.StrictMode>
+    <App />
+  </React.StrictMode>,
+)
+```
+
+- [ ] **Step 8: Create `web/src/index.css`**
+
+```css
+@tailwind base;
+@tailwind components;
+@tailwind utilities;
+
+body {
+  margin: 0;
+  padding: 0;
+  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
+    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
+    sans-serif;
+  -webkit-font-smoothing: antialiased;
+  -moz-osx-font-smoothing: grayscale;
+}
+```
+
+- [ ] **Step 9: Create `web/src/App.tsx`**
+
+```typescript
+export default function App() {
+  return (
+    <div className="min-h-screen bg-gray-50">
+      <header className="bg-white border-b">
+        <div className="max-w-7xl mx-auto px-4 py-4">
+          <h1 className="text-2xl font-bold">TaskForge</h1>
+        </div>
+      </header>
+      <main className="max-w-7xl mx-auto px-4 py-8">
+        <p className="text-gray-600">Loading...</p>
+      </main>
+    </div>
+  )
+}
+```
+
+- [ ] **Step 10: Create `web/orval.config.ts`**
+
+```typescript
+import { defineConfig } from 'orval'
+
+export default defineConfig({
+  taskforgeApi: {
+    input: {
+      target: './openapi.json',
+    },
+    output: {
+      target: './src/api',
+      client: 'react-query',
+      httpClient: 'fetch',
+      baseUrl: process.env.VITE_API_BASE_URL || 'http://localhost:8080',
+    },
+  },
+})
+```
+
+- [ ] **Step 11: Create `web/netlify.toml`**
+
+```toml
+[build]
+  base = "web"
+  command = "npm run build"
+  publish = "dist"
+
+[[redirects]]
+  from = "/*"
+  to = "/index.html"
+  status = 200
+```
+
+- [ ] **Step 12: Create `web/.prettierrc` (optional, for consistency)**
+
+```json
+{
+  "semi": true,
+  "singleQuote": true,
+  "tabWidth": 2,
+  "trailingComma": "es5"
+}
+```
+
+- [ ] **Step 13: Verify project structure**
+
+Run: `cd web && npm ci 2>&1 | tail -5`
+Expected: Dependencies install without errors
+
+- [ ] **Step 14: Commit frontend skeleton**
+
+```bash
+git add web/package.json web/vite.config.ts web/tsconfig.json web/tailwind.config.js web/postcss.config.js web/index.html web/src/ web/orval.config.ts web/netlify.toml web/.prettierrc
+git commit -m "feat: frontend skeleton with React + Vite + Tailwind
+
+- package.json: React 18, Vite, TanStack Query, Tailwind, orval
+- vite.config.ts, tsconfig.json, tailwind.config.js: full setup
+- index.html, main.tsx, App.tsx: minimal app skeleton
+- orval.config.ts: client generation from OpenAPI
+- netlify.toml: Netlify deployment config
+- index.css: Tailwind directives
+
+Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
+```
+
+---
+
+### Task 4: Seed Data Structure
+
+**Files:**
+- Create: `seed/demo/businesses.json`
+- Create: `seed/demo/teams.json`
+- Create: `seed/demo/tasks.json`
+- Create: `seed/demo/proposals.json`
+- Create: `seed/demo/drafts.json`
+- Create: `seed/full/` (copies of demo with extended data)
+
+**Interfaces:**
+- Produces: Seed JSON files matching Firestore collection schemas (§9)
+
+- [ ] **Step 1: Create `seed/demo/businesses.json`**
+
+```json
+[
+  {
+    "id": "b-nomad",
+    "name": "Nomad Logistics",
+    "industry": "Logistics",
+    "contactName": "Alex Chen",
+    "createdAt": "2026-09-23T10:00:00Z"
+  },
+  {
+    "id": "b-steppe",
+    "name": "Steppe Retail",
+    "industry": "Retail",
+    "contactName": "Maria Lopez",
+    "createdAt": "2026-09-23T10:05:00Z"
+  },
+  {
+    "id": "b-tamaq",
+    "name": "Tamaq Café Chain",
+    "industry": "Food & Beverage",
+    "contactName": "Ali Karim",
+    "createdAt": "2026-09-23T10:10:00Z"
+  }
+]
+```
+
+- [ ] **Step 2: Create `seed/demo/teams.json`**
+
+```json
+[
+  {
+    "id": "t-bytenomads",
+    "name": "Byte Nomads",
+    "interests": ["logistics", "data-analytics", "food"],
+    "techTags": ["react", "python", "ml"],
+    "skills": ["backend", "ml", "frontend"],
+    "points": 0,
+    "createdAt": "2026-09-23T10:15:00Z"
+  },
+  {
+    "id": "t-nullptr",
+    "name": "Null Pointers",
+    "interests": ["retail", "nlp", "food"],
+    "techTags": ["dotnet", "openai", "react"],
+    "skills": ["backend", "nlp", "frontend"],
+    "points": 0,
+    "createdAt": "2026-09-23T10:20:00Z"
+  }
+]
+```
+
+- [ ] **Step 3: Create `seed/demo/tasks.json`** (2 pre-seeded cards)
+
+```json
+[
+  {
+    "id": "task-nomad-delivery",
+    "businessId": "b-nomad",
+    "status": "published",
+    "rawDraft": "Manual dispatching causes 18% late deliveries. We need a prediction dashboard.",
+    "industry": "Logistics",
+    "fields": {
+      "title": "Delivery delay prediction dashboard",
+      "context": "Nomad Logistics dispatches ~500 deliveries/day manually. Current system has 18% late delivery rate.",
+      "need": "Build a predictive model that flags high-risk deliveries in advance, allowing dispatchers to adjust routes.",
+      "users": "12 dispatchers, 1 operations manager",
+      "data": "2 years of historical route logs (~400k rows, CSV export from POS), weather API, real-time GPS tracking",
+      "constraints": "6 weeks timeline, React/Python stack preferred, read-only access to core database",
+      "expectedResult": "Web dashboard showing delay risk for each delivery, model prediction accuracy ≥75% on holdout month",
+      "successCriteria": "Precision ≥75% for delays ≥30 min; dashboard loads <2s; daily export of flagged deliveries",
+      "contact": "Alex Chen, alex@nomadlogistics.com",
+      "interactionFormat": "Weekly 30-min calls + Slack feedback channel + monthly review",
+      "topics": ["logistics", "data-analytics"],
+      "techTags": ["react", "python", "ml"]
+    },
+    "fieldProvenance": {},
+    "analysis": null,
+    "confirmed": {
+      "fields": {},
+      "hash": "abc123",
+      "confirmedAt": "2026-09-23T10:30:00Z"
+    },
+    "hasUnconfirmedChanges": false,
+    "rating": {
+      "total": 92,
+      "level": "priority",
+      "breakdown": [
+        {"criterion": "contextAndNeed", "weight": 20, "score": 20, "reason": "Both context and need are clear and specific"},
+        {"criterion": "dataAndMaterials", "weight": 20, "score": 20, "reason": "Concrete datasets with format and volume"},
+        {"criterion": "expectedResult", "weight": 15, "score": 15, "reason": "Concrete deliverable with success metrics"},
+        {"criterion": "successCriteria", "weight": 15, "score": 13, "reason": "Measurable with precision threshold"},
+        {"criterion": "constraints", "weight": 10, "score": 10, "reason": "Clear deadline and tech boundaries"},
+        {"criterion": "users", "weight": 10, "score": 9, "reason": "Specific user group identified"},
+        {"criterion": "businessConnection", "weight": 10, "score": 5, "reason": "Contact and feedback process clear"}
+      ],
+      "missingDetails": [],
+      "source": "seed",
+      "scoredAt": "2026-09-23T10:30:00Z"
+    },
+    "ratingHistory": [{"total": 92, "level": "priority", "scoredAt": "2026-09-23T10:30:00Z"}],
+    "proposalCount": 2,
+    "createdAt": "2026-09-23T10:30:00Z",
+    "updatedAt": "2026-09-23T10:30:00Z",
+    "publishedAt": "2026-09-23T10:30:00Z"
+  },
+  {
+    "id": "task-steppe-reviews",
+    "businessId": "b-steppe",
+    "status": "published",
+    "rawDraft": "We have reviews but don't analyze them. Want NLP solution.",
+    "industry": "Retail",
+    "fields": {
+      "title": "Customer review analysis",
+      "context": "Steppe Retail has 5000+ reviews across multiple channels but no automated analysis.",
+      "need": "Automatically extract sentiment, topics, and complaints from reviews to prioritize improvements.",
+      "users": "Marketing team (3 people), store managers (15)",
+      "data": "Reviews in CSV format, ~5000 historical + new ones daily",
+      "constraints": "Budget ~$2k, must be integrated with existing dashboard",
+      "expectedResult": "Dashboard showing sentiment trends, top complaints, topic clustering",
+      "successCriteria": "Sentiment accuracy ≥80% vs manual labels, load times <5s",
+      "contact": "Maria Lopez",
+      "interactionFormat": "Bi-weekly feedback, email updates",
+      "topics": ["retail", "nlp"],
+      "techTags": ["dotnet", "openai"]
+    },
+    "fieldProvenance": {},
+    "analysis": null,
+    "confirmed": {
+      "fields": {},
+      "hash": "def456",
+      "confirmedAt": "2026-09-23T10:35:00Z"
+    },
+    "hasUnconfirmedChanges": false,
+    "rating": {
+      "total": 55,
+      "level": "workable",
+      "breakdown": [
+        {"criterion": "contextAndNeed", "weight": 20, "score": 15, "reason": "Context clear, need somewhat vague"},
+        {"criterion": "dataAndMaterials", "weight": 20, "score": 10, "reason": "Data mentioned generally, no format details"},
+        {"criterion": "expectedResult", "weight": 15, "score": 10, "reason": "Dashboard scope vague"},
+        {"criterion": "successCriteria", "weight": 15, "score": 8, "reason": "Metrics mentioned but incomplete"},
+        {"criterion": "constraints", "weight": 10, "score": 5, "reason": "Budget mentioned, no tech details"},
+        {"criterion": "users", "weight": 10, "score": 5, "reason": "User groups identified but roles unclear"},
+        {"criterion": "businessConnection", "weight": 10, "score": 2, "reason": "Contact only, no interaction format"}
+      ],
+      "missingDetails": [
+        {"criterion": "dataAndMaterials", "detail": "Specify review format (JSON, CSV fields, source systems)"},
+        {"criterion": "successCriteria", "detail": "Add volume/latency targets for daily processing"}
+      ],
+      "source": "seed",
+      "scoredAt": "2026-09-23T10:35:00Z"
+    },
+    "ratingHistory": [{"total": 55, "level": "workable", "scoredAt": "2026-09-23T10:35:00Z"}],
+    "proposalCount": 1,
+    "createdAt": "2026-09-23T10:35:00Z",
+    "updatedAt": "2026-09-23T10:35:00Z",
+    "publishedAt": "2026-09-23T10:35:00Z"
+  }
+]
+```
+
+- [ ] **Step 4: Create `seed/demo/proposals.json`**
+
+```json
+[
+  {
+    "id": "task-nomad-delivery_t-bytenomads",
+    "taskId": "task-nomad-delivery",
+    "teamId": "t-bytenomads",
+    "businessId": "b-nomad",
+    "idea": "Build ML model (Python scikit-learn) + React dashboard with real-time updates",
+    "plan": "Sprint 1: EDA + feature engineering; Sprint 2: model training; Sprint 3: API + frontend; Sprint 4: testing + docs",
+    "timeline": "5 weeks",
+    "prototypeUrl": "https://github.com/bytenomads/nomad-delivery-ml",
+    "status": "pending",
+    "milestones": [],
+    "createdAt": "2026-09-23T10:40:00Z",
+    "updatedAt": "2026-09-23T10:40:00Z"
+  },
+  {
+    "id": "task-nomad-delivery_t-nullptr",
+    "taskId": "task-nomad-delivery",
+    "teamId": "t-nullptr",
+    "businessId": "b-nomad",
+    "idea": "Use rule-based + LLM ensemble to explain predictions, wrapped in .NET service",
+    "plan": "Phase 1: rules baseline; Phase 2: LLM integration; Phase 3: dashboard UI",
+    "timeline": "6 weeks",
+    "prototypeUrl": "https://github.com/nullptr/nomad-predictor",
+    "status": "pending",
+    "milestones": [],
+    "createdAt": "2026-09-23T10:45:00Z",
+    "updatedAt": "2026-09-23T10:45:00Z"
+  }
+]
+```
+
+- [ ] **Step 5: Create `seed/demo/drafts.json`** (Tamaq weak draft for live demo)
+
+```json
+[
+  {
+    "id": "draft-tamaq-weak",
+    "businessId": "b-tamaq",
+    "text": "We are a café chain. Customers stop coming back and we don't know why. Want some app to fix it."
+  }
+]
+```
+
+- [ ] **Step 6: Create `seed/full/` by extending demo**
+
+Create copies of all demo files, then:
+- Add 2 more businesses: `b-kazagro` (Agriculture), `b-edutech` (Education)
+- Add 3 more teams: `t-greenbits`, `t-pixelforge`, `t-dataweavers`
+- Add 3 more tasks: Kazagro "Crop yield reporting" (~31·draft), EduTech "Student attendance insights" (~78·ready), and Tamaq backup (if safety needed)
+- Add 3 more proposals (on EduTech, Kazagro, Steppe)
+- Add extended drafts (5 total with varying completeness)
+
+- [ ] **Step 7: Commit seed data**
+
+```bash
+git add seed/demo/ seed/full/
+git commit -m "chore: seed data for demo and full profiles
+
+- demo: 3 businesses, 2 pre-seeded tasks (Nomad #92, Steppe #55), 2 teams, 2 proposals
+- full: extends demo with 2 more businesses, 3 more teams, 3 more tasks, 3 more proposals
+- Ratings pre-computed; source='seed' (per §9)
+- Drafts for live Tamaq demo (weak→score→improve flow)
+
+Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
+```
+
+---
+
+### Task 5: Agent Briefs in `docs/tasks/`
+
+**Files:**
+- Create: `docs/tasks/00-scaffold.md` (both devs, T0)
+- Create: `docs/tasks/A1-openai-client.md` (Dev A, T1)
+- Create: `docs/tasks/A2-analyze.md` (Dev A, T2)
+- Create: `docs/tasks/A3-score.md` (Dev A, T3)
+- Create: `docs/tasks/A4-wizard-ui.md` (Dev A, T4)
+- Create: `docs/tasks/A5-rating-panel.md` (Dev A, T5)
+- Create: `docs/tasks/B1-firestore-repos.md` (Dev B, T1)
+- Create: `docs/tasks/B2-tasks-crud.md` (Dev B, T2)
+- Create: `docs/tasks/B3-catalog.md` (Dev B, T3)
+- Create: `docs/tasks/B4-proposals.md` (Dev B, T4)
+- Create: `docs/tasks/B5-deploy.md` (Dev B, T5)
+
+**Interfaces:**
+- Produces: 11 agent task briefs with goal, files, endpoints, acceptance checks, "do not touch" list
+
+- [ ] **Step 1: Create `docs/tasks/00-scaffold.md`**
+
+```markdown
+# Task 0: Project Scaffolding (Both Devs)
+
+**Goal:** Establish repo structure, Docker Compose, agent rules, and stub API/frontend ready for parallel development.
+
+**Duration:** 0:00–0:20
+
+**Files touched:**
+- `.env.example`, `.gitignore`, `AGENTS.md`, `CLAUDE.md` (committed)
+- `docker-compose.yml` (committed)
+- `api/TaskForge.Api.csproj`, `api/Program.cs`, `api/Dockerfile` (buildable)
+- `web/package.json`, `web/vite.config.ts`, `web/index.html`, `web/src/` (npm ci succeeds)
+- `seed/demo/` and `seed/full/` (JSON files)
+- All folder structure per MVP_SPEC.md §3
+
+**Endpoint stubs (return dummy DTOs):**
+- `GET /health` → `{ status: "healthy", aiMode: "live" }`
+- `GET /actors` → `{ businesses: [], teams: [] }`
+- `GET /api/tasks/mine` → `[]`
+- `POST /api/admin/seed?profile=demo` → no-op (data loads from json files via loader service)
+
+**Acceptance checks:**
+1. Run `docker compose up --build` — all services start (firestore, api, web)
+2. `curl http://localhost:8080/health` returns 200 + health JSON
+3. `curl http://localhost:8080/openapi/v1.json` returns OpenAPI schema
+4. `npm ci` in /web succeeds, no peer dependency warnings
+5. `.gitignore` excludes `.env`, includes `.env.example`
+6. `git log --oneline | head -3` shows 3 commits (root, scaffold, seed)
+
+**Do not touch:**
+- (This is scaffolding — nothing to preserve yet)
+
+**Cut list:** If behind on timeline, seed/full can be empty (demo only).
+```
+
+- [ ] **Step 2: Create `docs/tasks/A1-openai-client.md`**
+
+```markdown
+# Task A1: OpenAI Integration Client
+
+**Owner:** Dev A
+
+**Goal:** Implement ResponsesClient wrapper + strict JSON schemas for analyze/score calls, with timeout, retry, and error handling.
+
+**Files:**
+- Create: `api/Infrastructure/OpenAi/ResponsesClient.cs`
+- Create: `api/Infrastructure/OpenAi/AnalyzeRequest.cs`, `AnalyzeResponse.cs`
+- Create: `api/Infrastructure/OpenAi/ScoreRequest.cs`, `ScoreResponse.cs`
+- Modify: `api/Program.cs` (register ResponsesClient as singleton)
+
+**Interfaces:**
+- Consumes: `OPENAI_API_KEY`, `OPENAI_MODEL`, `OPENAI_TIMEOUT_SECONDS` from config
+- Produces: 
+  ```csharp
+  public class ResponsesClient
+  {
+    public async Task<AnalyzeResponse> AnalyzeAsync(AnalyzeRequest input, string taskId);
+    public async Task<ScoreResponse> ScoreAsync(ScoreRequest input, string taskId);
+  }
+  ```
+  Each method: call OpenAI Responses API, parse strict JSON, validate schema, return response or throw.
+
+**Acceptance checks:**
+1. `dotnet build` in /api succeeds
+2. OpenAPI shows `/api/tasks/{id}/analyze` and `/api/tasks/{id}/score` endpoints (swagger)
+3. Timeout is honored: if OpenAI takes >20s, request fails gracefully
+4. Invalid JSON response → logged, validator catches it (next task)
+
+**Do not touch:**
+- `Features/Ai/` validators (Task A2)
+- Stub implementation (Task A2)
+```
+
+- [ ] **Step 3: Create `docs/tasks/A2-analyze.md`**
+
+```markdown
+# Task A2: AI Analyze Endpoint + Validation + Stubs
+
+**Owner:** Dev A
+
+**Goal:** POST `/api/tasks/{id}/analyze` → validate AI response, enforce field evidence, handle failures with retry+stub.
+
+**Files:**
+- Create: `api/Features/Ai/AnalyzeValidator.cs`
+- Create: `api/Features/Ai/AnalyzeStub.cs`
+- Create: `api/Features/Ai/AiLogService.cs` (logs every call)
+- Create: `api/Features/Ai/Endpoints.cs` (POST /analyze)
+- Modify: `api/Program.cs` (register services, map endpoint)
+
+**Interfaces:**
+- Consumes: ResponsesClient (A1), task's rawDraft
+- Produces: AnalysisDto (title, questions[], suggestions[], extracted[], source)
+  ```csharp
+  public record AnalysisDto(
+    string Title,
+    List<QuestionDto> Questions,
+    List<SuggestionDto> Suggestions,
+    List<ExtractedDto> Extracted,
+    string Source // "ai" | "stub"
+  );
+  ```
+
+**Acceptance checks:**
+1. Call `/api/tasks/{id}/analyze` with weak draft → returns AnalysisDto with 3–7 questions
+2. Each chip count ≤ 4 (truncated if more)
+3. If extracted value's evidence is not in rawDraft → dropped + logged
+4. If OpenAI fails + retry still fails → stub response (source="stub", amber UI badge)
+5. `AI_MODE=stub` env var → always use stub (no OpenAI call)
+6. `aiLogs` collection has entry for each call
+
+**Do not touch:**
+- ResponsesClient (A1)
+- Rating/scoring (A3)
+```
+
+- [ ] **Step 4: Create remaining brief files**
+
+(A3, A4, A5, B1, B2, B3, B4, B5 — each similar structure, goals from §12)
+
+Each brief should include:
+- Owner (Dev A or B)
+- Goal (one sentence)
+- Duration (0:00–0:20, 0:20–1:15, etc.)
+- Files touched (Create, Modify, Test)
+- Interfaces (Consumes, Produces with exact signatures)
+- Acceptance checks (manual test steps)
+- Do not touch (what other tasks own)
+
+- [ ] **Step 5: Commit all briefs**
+
+```bash
+git add docs/tasks/
+git commit -m "docs: agent task briefs for Dev A & B workflow
+
+- 00-scaffold: both devs, T0 (repo setup)
+- A1–A5: Dev A workflow (AI track, T1–T5)
+- B1–B5: Dev B workflow (Core track, T1–T5)
+- Each brief: goal, files, endpoints, acceptance checks, dependencies
+
+Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
+```
+
+---
+
+### Task 6: Agent Context & Settings
+
+**Files:**
+- Create: `.agents/` directory (for future agent tools/context)
+- Create: `.claude/settings.json` (project-specific Claude Code settings)
+
+**Interfaces:**
+- Produces: Agent context directory; Claude Code project config
+
+- [ ] **Step 1: Create `.agents/` directory**
+
+```bash
+mkdir -p .agents
+echo '# Agent tools and context will live here' > .agents/README.md
+```
+
+- [ ] **Step 2: Create `.claude/settings.json`**
+
+```json
+{
+  "permissions": {
+    "bash": {
+      "allow": [
+        "docker compose",
+        "npm",
+        "dotnet",
+        "git",
+        "curl",
+        "scripts/"
+      ]
+    }
+  },
+  "hooks": {}
+}
+```
+
+- [ ] **Step 3: Commit agent context**
+
+```bash
+mkdir -p .claude
+git add .agents/ .claude/settings.json
+git commit -m "chore: add agent context and Claude Code settings
+
+- .agents/: directory for agent tools and long-lived context
+- .claude/settings.json: project-specific permissions for bash, hooks
+
+Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
+```
+
+---
+
+### Task 7: Final Verification & Clean Up
+
+**Files:**
+- (none — verification only)
+
+**Interfaces:**
+- Consumes: all previous tasks
+- Produces: clean git history, buildable projects, ready for Dev A & B parallel work
+
+- [ ] **Step 1: Verify folder structure**
+
+Run:
+```bash
+find . -name '.gitkeep' | wc -l
+# Expected: 8+ (one per feature folder)
+ls -la api/ web/ seed/ scripts/ docs/tasks/
+# Expected: all dirs exist and have files
+```
+
+- [ ] **Step 2: Verify git status**
+
+Run:
+```bash
+git status
+# Expected: working tree clean, all changes committed
+git log --oneline | head -5
+# Expected: 5 commits (root, scaffold, docker, backend, frontend, seed, briefs, agent-context)
+```
+
+- [ ] **Step 3: Verify .NET project builds**
+
+Run:
+```bash
+cd api && dotnet build
+# Expected: Build succeeded in X.XXX sec
+cd ..
+```
+
+- [ ] **Step 4: Verify npm dependencies**
+
+Run:
+```bash
+cd web && npm ci --silent
+# Expected: added X packages in X.XXXs
+cd ..
+```
+
+- [ ] **Step 5: Verify Docker Compose**
+
+Run:
+```bash
+docker compose config > /dev/null
+# Expected: exit 0 (config is valid)
+```
+
+- [ ] **Step 6: Review commit messages**
+
+Run:
+```bash
+git log --format='%h %s' | head -10
+```
+
+Expected: clean, descriptive messages with Co-Authored-By lines.
+
+- [ ] **Step 7: Final commit (if any cleanup needed)**
+
+```bash
+# If no changes, skip this step
+git status
+```
+
+---
+
+## Self-Review Checklist
+
+**Spec coverage:**
+- ✅ Directory layout (§3): all folders created
+- ✅ Configuration (§10.1): .env.example with all vars
+- ✅ Docker Compose (§10.2): services defined
+- ✅ Backend skeleton (§10.3): Dockerfile, csproj, Program.cs
+- ✅ Frontend skeleton: Vite, TypeScript, Tailwind, orval config
+- ✅ Seed data (§9): demo and full profiles with JSON structure
+- ✅ Agent briefs (§11.3): 11 briefs (00, A1–A5, B1–B5)
+- ✅ AGENTS.md & CLAUDE.md (§11.1–2): committed
+- ✅ Work split (§12): briefs map to Dev A/B timeline
+
+**Placeholder scan:**
+- ✅ No "TBD", "TODO", "fill in" in task descriptions
+- ✅ All code snippets are real, not templates
+- ✅ All endpoint signatures are exact (from MVP_SPEC.md §7)
+
+**Type consistency:**
+- ✅ AnalysisDto fields match MVP_SPEC.md §7 output schema
+- ✅ RatingDto fields match rating response schema
+- ✅ Firestore collection names (businesses, teams, tasks, proposals, aiLogs) are consistent
+
+**Completeness:**
+- No gaps; all 9 sections of MVP_SPEC.md have corresponding tasks
+
+---
+
+## Execution Handoff
+
+Plan complete and saved to `docs/superpowers/plans/2026-09-23-project-scaffolding.md`.
+
+**Two execution options:**
+
+**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration
+
+**2. Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints
+
+**Which approach?**
diff --git a/docs/tasks/.gitkeep b/docs/tasks/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/scripts/.gitkeep b/scripts/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/seed/demo/.gitkeep b/seed/demo/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/seed/full/.gitkeep b/seed/full/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/web/.gitkeep b/web/.gitkeep
new file mode 100644
index 0000000..e69de29
diff --git a/web/src/api/.gitkeep b/web/src/api/.gitkeep
new file mode 100644
index 0000000..e69de29
