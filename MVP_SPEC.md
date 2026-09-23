# TaskForge — MVP Specification
**Business Task Quality Rating & Open Team Selection**
4-hour hackathon build · 2 developers · .NET 10 + React + Firestore + OpenAI

---

## 0. TL;DR

A business rep types a weak task description → AI asks clarifying questions (with ≤4 answer chips each) and suggests actionable improvements → rep edits and confirms a structured card → AI scores it 0–100 against a fixed 7-criterion rubric → task is published to a shared catalog sorted by rating → any student team proposes → the business manually selects/rejects → confirmed milestones give the team points.

**One AI function family, two calls:** `analyze` (gaps + questions + chips + suggestions) and `score` (rubric scoring). Everything else is deterministic.

### Decisions log (from Q&A)

| # | Topic | Decision |
|---|---|---|
| 1 | Hosting | API → GCP Cloud Run; SPA → Netlify; Docker Compose for local |
| 2 | Team split | Dev A = AI track, Dev B = Core track (see §12) |
| 3 | OpenAI style | Responses API + strict JSON schema |
| 4 | Model | Mini-tier model, name in env var |
| 5 | AI scope | SoW minimum: completeness analysis + clarifying questions |
| 6 | Rating | AI-scored per criterion, backend-guarded (clamp, empty=0, hash cache) |
| 7 | Identity | Role switcher, no login |
| 8 | Data access | Frontend → .NET API only; Firestore closed to clients |
| 9 | Local DB | Firestore emulator in compose |
| 10 | API style | Minimal APIs, single project, feature folders |
| 11 | Frontend | Vite + TypeScript + Tailwind + shadcn/ui |
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
| 22 | Duplicates | One proposal per team per task, editable while Pending |
| 23 | Decision | Pending / Selected / Rejected, manual, reversible until milestone |
| 24 | Milestones | Minimal: business confirms → +10 team points → leaderboard |
| 25 | Seed | 3 businesses, 3 cards (full / medium / wizard-demo), 2 teams |
| 26 | Volume | Two seed profiles: `demo` and `full` (SoW 5× minimum) |
| 27 | AI failure | Validate → 1 retry → local stub, flagged in UI |
| 28 | Secrets | Local `.env` (gitignored) + committed `.env.example` |
| 29 | Agent tooling | `AGENTS.md` + `CLAUDE.md` + per-module briefs in `/docs/tasks` |
| 30 | Testing | None automated; manual demo rehearsal + README test scenarios |

---

## 1. ⚠ Red flags (read first)

| # | Risk | Mitigation in this spec |
|---|---|---|
| R1 | **AI-scored rating vs. "transparent" (25 pts criterion).** LLM scores drift between identical inputs. | Per-criterion score + reason + missing details from AI; backend clamps to weight; empty fields forced to 0; SHA-256 cache of confirmed fields → identical card = identical score; rubric anchors in prompt. |
| R2 | **"AI must not add facts."** Chips and extraction can smuggle invented facts. | Chips are *options the user picks*, never auto-inserted. Extracted draft values require an `evidence` substring that the backend verifies exists in the raw draft; failures are dropped. Suggestions are phrased as "Add X", never as content. |
| R3 | **"Draft" naming collision.** Readiness level "Draft" (0–39) ≠ unpublished task. | Use `status: editing \| published` and `level: draft \| workable \| ready \| priority`. UI label for level draft: "Needs clarification". |
| R4 | **Seed below SoW §6 minimum** (5 of each). | `full` seed profile pads to 5 drafts / 5 cards / 5 teams / 5 proposals. |
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
- Recommendations strip for teams (rule-based)
- Proposals: submit/edit (one per team per task)
- Business review: compare proposals, Select / Reject / Reset
- Milestone confirmation → team points → leaderboard
- Seed/reset endpoints (`demo`, `full`)
- AI call log viewer (prompt, input, output, validation result) — satisfies SoW §5 "show prompt, input/output format, invalid handling"

### Out of scope
Auth, passwords, complex roles, chat, notifications, calendar, file upload, ML training, vector DB, mobile layout, project tracker, automated tests, CI/CD.

---

## 3. Architecture

```
┌──────────────┐   HTTPS/JSON   ┌───────────────────────────┐    gRPC    ┌───────────┐
│ React SPA    │ ─────────────► │ .NET 10 Minimal API        │ ─────────► │ Firestore │
│ (Netlify)    │  X-Actor-*     │ (Cloud Run)                │            │ (native)  │
│ TanStack Q.  │                │  Features/ Rating/ Ai/     │ ─────────► │ OpenAI    │
└──────────────┘                └───────────────────────────┘   HTTPS    │ Responses │
                                                                          └───────────┘
Local: docker compose → web (vite) + api + firestore-emulator
```

- Frontend never touches Firestore. Firestore security rules: deny all client access.
- API is stateless; all state in Firestore.
- Actor identity via headers set by role switcher: `X-Actor-Role: business|team`, `X-Actor-Id: <id>`. API validates existence and ownership (not security, just correctness).

### Repository layout

```
/
├─ AGENTS.md                  # shared agent rules (Codex + Claude)
├─ CLAUDE.md                  # "Read AGENTS.md" + Claude-specific notes
├─ README.md                  # SoW deliverable: architecture, formula, catalog rules, test scenarios
├─ .env.example
├─ .gitignore                 # includes .env
├─ docker-compose.yml
├─ docs/
│  ├─ MVP_SPEC.md             # this file
│  └─ tasks/                  # per-module agent briefs (§11)
├─ api/
│  ├─ Dockerfile
│  ├─ TaskForge.Api.csproj
│  ├─ Program.cs
│  ├─ Domain/                 # records: TaskCard, Rating, Proposal, Team, Business, enums
│  ├─ Infrastructure/
│  │  ├─ Firestore/           # FirestoreDb factory, repositories
│  │  └─ OpenAi/              # ResponsesClient (HttpClient), schemas, prompts
│  └─ Features/
│     ├─ Actors/              # businesses, teams, role list
│     ├─ Tasks/               # create draft, answers, card edit, confirm, publish
│     ├─ Ai/                  # analyze, score, stub, validator, ai-logs
│     ├─ Rating/              # guard, levels, cache, history
│     ├─ Catalog/             # list, filters, position, recommendations
│     ├─ Proposals/           # upsert, list, decision, milestones
│     └─ Admin/               # seed, reset, health
├─ web/
│  ├─ netlify.toml
│  ├─ orval.config.ts
│  └─ src/
│     ├─ api/                 # GENERATED — do not edit
│     ├─ components/          # RoleSwitcher, RatingPanel, LevelBadge, ChipAnswer, ...
│     ├─ pages/               # business/*, team/*, AiLogs
│     └─ lib/                 # actor context, fetch mutator adding X-Actor headers
├─ seed/
│  ├─ demo/*.json
│  └─ full/*.json
└─ scripts/
   ├─ gen-client.sh
   ├─ deploy-api.sh
   └─ deploy-web.sh
```

---

## 4. Domain model

### 4.1 Card fields (SoW §3)

| Key | Label | Type | Max len | Rating criterion |
|---|---|---|---|---|
| `title` | Title | string | 120 | — (required to publish) |
| `context` | Context (current situation) | text | 2000 | contextAndNeed |
| `need` | Need (what must change) | text | 2000 | contextAndNeed |
| `users` | Users | text | 1000 | users |
| `data` | Data & materials | text | 2000 | dataAndMaterials |
| `constraints` | Constraints | text | 1500 | constraints |
| `expectedResult` | Expected result | text | 1500 | expectedResult |
| `successCriteria` | Success criteria | text | 1500 | successCriteria |
| `contact` | Contact | string | 200 | businessConnection |
| `interactionFormat` | Interaction format (consultations, feedback) | text | 1000 | businessConnection |
| `topics` | Topics / industry | string[] | ≤5 tags | — (filter, recommendations) |
| `techTags` | Technologies | string[] | ≤8 tags | — (recommendations) |

### 4.2 Rating rubric (SoW §4)

| Criterion key | Weight | Fields used | Empty rule |
|---|---|---|---|
| `contextAndNeed` | 20 | context, need | both empty → 0; one empty → max 10 |
| `dataAndMaterials` | 20 | data | empty → 0 |
| `expectedResult` | 15 | expectedResult | empty → 0 |
| `successCriteria` | 15 | successCriteria | empty → 0 |
| `constraints` | 10 | constraints | empty → 0 |
| `users` | 10 | users | empty → 0 |
| `businessConnection` | 10 | contact, interactionFormat | both empty → 0; one empty → max 5 |
| **Total** | **100** | | |

"Empty" = null, whitespace, or < 3 non-space chars.

### 4.3 Readiness levels

| Score | `level` | UI label | Catalog behavior |
|---|---|---|---|
| 0–39 | `draft` | Needs clarification | Visible, grey badge, proposals allowed, not recommended |
| 40–69 | `workable` | Workable | Visible, blue badge, eligible for recommendations |
| 70–89 | `ready` | Ready | Visible, green badge, higher position (by sort) |
| 90–100 | `priority` | Priority | Visible, gold badge + highlighted card border + ★ |

### 4.4 Firestore collections

```
businesses/{businessId}
  name, industry, contactName, createdAt

teams/{teamId}
  name, interests: string[], techTags: string[], skills: string[], points: int, createdAt

tasks/{taskId}
  businessId
  status: "editing" | "published"
  rawDraft: string
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
  confirmed: {                             # snapshot at last confirm
    fields: {...}, hash: string, confirmedAt
  } | null
  hasUnconfirmedChanges: bool
  rating: {
    total: int, level: string,
    breakdown: [{ criterion, weight, score, reason }],
    missingDetails: [{ criterion, detail }],
    source: "ai" | "stub" | "seed" | "cache",
    scoredAt
  } | null
  ratingHistory: [{ total, level, scoredAt }]   # last 10, for delta display
  proposalCount: int
  createdAt, updatedAt, publishedAt

proposals/{proposalId}          # id = `${taskId}_${teamId}` → enforces 1 per team per task
  taskId, teamId, businessId
  idea, plan, timeline, prototypeUrl
  status: "pending" | "selected" | "rejected"
  decisionReason?: string
  decidedAt?
  milestones: [{ id, title, points, confirmedAt }]
  createdAt, updatedAt

aiLogs/{logId}
  kind: "analyze" | "score", taskId, model,
  systemPrompt, input (json string), rawOutput, validation: "ok" | "retry-ok" | "fallback-stub",
  errors: string[], latencyMs, createdAt
```

No composite indexes needed if catalog sorting/filtering is done in memory (≤ 100 tasks). **Do it in memory** — avoids index-creation delays on Firestore.

---

## 5. AI design

### 5.1 OpenAI integration

- Endpoint: `POST https://api.openai.com/v1/responses`
- Client: plain `HttpClient` + typed DTOs (avoids SDK surface churn). Timeout 20 s.
- Request shape:

```json
{
  "model": "${OPENAI_MODEL}",
  "input": [
    { "role": "system", "content": "<system prompt>" },
    { "role": "user",   "content": "<JSON input>" }
  ],
  "text": { "format": { "type": "json_schema", "name": "task_analysis", "strict": true, "schema": { } } }
}
```

- Read `output_text` (or walk `output[].content[].text`), `JsonSerializer.Deserialize`.
- Strict schemas: every property listed in `required`, `additionalProperties: false`. **Counts (≥3 questions, ≤4 chips) are enforced in backend validation**, since strict mode may ignore some array keywords.
- If the chosen model rejects `temperature`, omit it.

### 5.2 Call 1 — `analyze` (completeness + questions + chips + suggestions)

**Input**
```json
{
  "draft": "We lose customers and want an app to fix it. We are a cafe chain.",
  "industry": "Food & Beverage",
  "currentFields": { "title": "", "context": "", "need": "", "...": "" },
  "fieldCatalog": [
    { "key": "context", "meaning": "What is happening now" },
    { "key": "need", "meaning": "What must change" },
    { "key": "users", "meaning": "Who the solution is for" },
    { "key": "data", "meaning": "Available data, examples, sources" },
    { "key": "constraints", "meaning": "Deadlines, tech, access limits" },
    { "key": "expectedResult", "meaning": "Concrete deliverable of the student team" },
    { "key": "successCriteria", "meaning": "Measurable acceptance signs" },
    { "key": "contact", "meaning": "Contact person/channel" },
    { "key": "interactionFormat", "meaning": "Consultation format, feedback procedure" }
  ]
}
```

**System prompt**
```
You help a business representative turn a rough task description into a complete task card
for student teams. You analyze completeness only.

RULES
1. Never invent facts. Only the user's draft and current fields are facts.
2. "extracted": copy values ONLY if they are explicitly stated in the draft. For each, "evidence"
   must be an exact substring of the draft. If unsure, do not extract.
3. "questions": 3 to 7 questions, one per missing or weak field, most important first
   (priority: need, context, data, expectedResult, successCriteria, users, constraints,
   interactionFormat, contact). Each question is short, concrete, answerable in 1-3 sentences.
4. "chips": 0 to 4 short answer OPTIONS per question that the user may pick. They are generic
   typical options for this kind of business, phrased as choices (e.g. "CSV export from POS system"),
   never as claims about this company. Use [] when options would require guessing specifics
   (e.g. contact).
5. "suggestions": 2 to 6 actionable improvement items, each starting with a verb
   ("Add...", "Specify...", "Describe..."), telling WHAT to add, never containing invented content.
6. "fieldKey" must be one of the provided field keys.
7. Language: same language as the draft.
Return JSON matching the schema only.
```

**Output schema**
```json
{
  "type": "object",
  "additionalProperties": false,
  "required": ["title", "extracted", "questions", "suggestions"],
  "properties": {
    "title": { "type": "string", "description": "Short neutral title from the draft wording; empty if unclear" },
    "extracted": {
      "type": "array",
      "items": {
        "type": "object", "additionalProperties": false,
        "required": ["fieldKey", "value", "evidence"],
        "properties": {
          "fieldKey": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] },
          "value":    { "type": "string" },
          "evidence": { "type": "string" }
        }
      }
    },
    "questions": {
      "type": "array",
      "items": {
        "type": "object", "additionalProperties": false,
        "required": ["fieldKey", "question", "chips"],
        "properties": {
          "fieldKey": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] },
          "question": { "type": "string" },
          "chips":    { "type": "array", "items": { "type": "string" } }
        }
      }
    },
    "suggestions": {
      "type": "array",
      "items": {
        "type": "object", "additionalProperties": false,
        "required": ["fieldKey", "action"],
        "properties": {
          "fieldKey": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] },
          "action":   { "type": "string" }
        }
      }
    }
  }
}
```

**Backend validation (`AnalyzeValidator`)**
- `questions.Count` in [3, 7] → else invalid
- each `chips.Count` ≤ 4 → truncate to 4 (soft fix), trim, dedupe, drop empty/ > 80 chars
- `extracted[i].evidence` must be a case-insensitive substring of `rawDraft` → else drop item (log)
- `suggestions` truncate to 6, drop empty
- `title` > 120 chars → truncate
- Assign `questions[i].id = "q{i+1}"`

### 5.3 Call 2 — `score` (rubric scoring)

**Input**
```json
{
  "rubric": [
    { "criterion": "contextAndNeed", "weight": 20, "fields": ["context","need"],
      "full": "Current situation and required change are both clear and specific",
      "half": "One is clear, the other vague", "zero": "Missing" },
    { "criterion": "dataAndMaterials", "weight": 20, "fields": ["data"],
      "full": "Concrete datasets/examples/sources with format, volume or access path",
      "half": "Data mentioned generally", "zero": "Missing" },
    { "criterion": "expectedResult", "weight": 15, "fields": ["expectedResult"],
      "full": "Concrete deliverable (what artifact, what it does)", "half": "Vague goal", "zero": "Missing" },
    { "criterion": "successCriteria", "weight": 15, "fields": ["successCriteria"],
      "full": "Measurable signs (numbers, thresholds, acceptance check)", "half": "Qualitative only", "zero": "Missing" },
    { "criterion": "constraints", "weight": 10, "fields": ["constraints"],
      "full": "Deadline + tech or access boundaries", "half": "One kind of boundary", "zero": "Missing" },
    { "criterion": "users", "weight": 10, "fields": ["users"],
      "full": "Specific user group and their situation", "half": "Generic users", "zero": "Missing" },
    { "criterion": "businessConnection", "weight": 10, "fields": ["contact","interactionFormat"],
      "full": "Contact + consultation format + feedback procedure", "half": "Contact only or format only", "zero": "Missing" }
  ],
  "card": { "context": "...", "need": "...", "...": "..." }
}
```

**System prompt**
```
You score how ready a business task card is for a student team, using ONLY the rubric.
For each criterion give an integer score from 0 to weight, using the anchors
(full = weight, half ≈ weight/2, zero = 0; intermediate values allowed).
Judge only what is written in the listed fields. Do not reward length alone.
"reason": one sentence explaining the score, citing what is present or missing.
"missingDetails": 0-3 concrete things the business could add to raise this criterion; empty if full.
Do not invent facts about the business. Same language as the card.
Return JSON matching the schema only.
```

**Output schema**
```json
{
  "type": "object", "additionalProperties": false,
  "required": ["criteria"],
  "properties": {
    "criteria": {
      "type": "array",
      "items": {
        "type": "object", "additionalProperties": false,
        "required": ["criterion", "score", "reason", "missingDetails"],
        "properties": {
          "criterion": { "type": "string", "enum": ["contextAndNeed","dataAndMaterials","expectedResult","successCriteria","constraints","users","businessConnection"] },
          "score": { "type": "integer" },
          "reason": { "type": "string" },
          "missingDetails": { "type": "array", "items": { "type": "string" } }
        }
      }
    }
  }
}
```

**Backend guard (`RatingGuard`) — authoritative**
1. Exactly the 7 criteria, no duplicates → else invalid.
2. `score = clamp(score, 0, weight)`.
3. Apply empty rules from §4.2 (empty → 0; partial caps 10 / 5). Replace reason with "Field is empty" when forced.
4. `total = Σ score`; `level` from §4.3.
5. Cache: `hash = SHA256(normalized confirmed fields JSON)`. If `hash == task.confirmed.hash` and a rating exists → return cached (`source: "cache"`), no AI call.
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
- `AnalyzeStub`: for each empty field in priority order take a template question (table below), up to 5, min 3 (if fewer than 3 empty → ask to *specify* weakest by length); chips `[]`; suggestions = "Add {label}" for each empty field; extracted `[]`.
- `ScoreStub`: per criterion, `score = weight × min(1, len(fieldsText)/threshold)` with threshold 200 chars (context/need, data), 120 (others); +20% if it contains a digit (successCriteria, constraints); clamp; then guard rules.

| fieldKey | Stub question |
|---|---|
| need | What exactly should change after the students' work? |
| context | What is happening now and why is it a problem? |
| data | What data, examples or sources can you give the team? |
| expectedResult | What concrete result do you expect from the team? |
| successCriteria | How will you measure that the solution is accepted? |
| users | Who will use the solution? |
| constraints | Are there deadlines, required technologies or access limits? |
| interactionFormat | How and how often can the team consult with you and get feedback? |
| contact | Who is the contact person and how to reach them? |

Every call (success, retry, stub) writes an `aiLogs` document. `AI_MODE=stub` env skips OpenAI entirely.

---

## 6. Business logic rules

### 6.1 Task lifecycle
```
[create draft] → editing ──analyze──► editing (questions shown)
     ──answers + card edits──► editing (hasUnconfirmedChanges=true)
     ──confirm──► rating computed, confirmed snapshot saved
     ──publish (requires title + rating)──► published
published ──edit──► hasUnconfirmedChanges=true (catalog still shows LAST CONFIRMED card + rating)
          ──confirm──► re-scored, catalog updates position
```
- Points are awarded only for confirmed fields (SoW §4): rating is computed only on `confirm`, from the `confirmed` snapshot.
- Catalog always displays `confirmed.fields` + `rating`, never unconfirmed edits.
- Publishing is allowed at any score (low rating doesn't hide the task — SoW §4).
- Publish requires: `title` non-empty, `rating != null`, no unconfirmed changes.

### 6.2 Answers → card merge
`PUT /answers` with `[{ questionId, fieldKey, text }]`:
- field empty → set to `text`
- field non-empty → append `"\n" + text`
- provenance: `chip` if text equals a chip exactly, else `user`
- extracted draft values are applied only when the user clicks "Use" next to each (provenance `draft-extract`)

### 6.3 Catalog
- Source: `tasks where status == published`, loaded in memory.
- Sort: `rating.total desc`, then `updatedAt desc`.
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
- Id `${taskId}_${teamId}` → upsert. Editable only while `pending`.
- Validation: idea 20–2000, plan 20–3000, timeline 3–200, prototypeUrl valid `http(s)` URL or empty.
- Allowed on any published task, any level.
- Only the owning business can decide. Transitions: `pending ↔ selected`, `pending ↔ rejected`, `selected ↔ rejected`. Once a milestone is confirmed, decision is locked.
- Multiple `selected` allowed; zero is fine.
- No automatic selection anywhere (SoW §3, §5).

### 6.6 Milestones & team points
- Business on a `selected` proposal → "Confirm milestone" (title required) → `+10` to `teams.points` (Firestore transaction) and entry in `proposal.milestones`.
- Leaderboard: teams sorted by `points desc`.

---

## 7. API specification

All routes under `/api`. JSON. Errors = RFC 7807 `ProblemDetails` with `errors` map for validation. Headers `X-Actor-Role`, `X-Actor-Id` required except `/health`, `/admin/*`, `GET /actors`.

| Method | Route | Actor | Body → Response |
|---|---|---|---|
| GET | `/health` | — | `{ status, aiMode, firestore: "ok" }` |
| GET | `/actors` | — | `{ businesses[], teams[] }` for role switcher |
| POST | `/tasks` | business | `{ rawDraft, industry }` → `TaskDto` (status editing) |
| GET | `/tasks/mine` | business | `TaskSummaryDto[]` (incl. position, proposalCount) |
| GET | `/tasks/{id}` | any | `TaskDto` (team sees confirmed view only) |
| POST | `/tasks/{id}/analyze` | business owner | → `AnalysisDto` |
| PUT | `/tasks/{id}/answers` | business owner | `{ answers:[{questionId, fieldKey, text}] }` → `TaskDto` |
| PUT | `/tasks/{id}/fields` | business owner | `{ fields:{...partial} }` → `TaskDto` |
| POST | `/tasks/{id}/confirm` | business owner | → `{ task: TaskDto, rating: RatingDto, delta: int, position }` |
| POST | `/tasks/{id}/publish` | business owner | → `TaskDto` |
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
| POST | `/admin/seed?profile=demo\|full` | — | wipes + loads seed |
| POST | `/admin/reset` | — | wipe all collections |

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

---

## 8. Frontend specification

### 8.1 Global
- **Top bar:** app name · RoleSwitcher (grouped select: Businesses / Teams) · nav links for the current role · "AI logs" link.
- Actor stored in React context + `localStorage`; orval custom mutator injects `X-Actor-*` headers.
- Toasts for errors (ProblemDetails `title` + first field error).
- After each mutation, invalidate related TanStack queries (task, catalog, mine, leaderboard).

### 8.2 Business pages

**B1. My Tasks** (`/business/tasks`)
Table: title · status · score + LevelBadge · catalog position `#n/m` · proposals count · "Open". Button **New task**.

**B2. New Task Wizard** (`/business/tasks/new` → `/business/tasks/:id/wizard`)
- Stepper: `1 Draft → 2 Clarify → 3 Card → 4 Rating → 5 Publish`.
- **Step 1 Draft:** textarea (20–4000), industry select, **Analyze** → creates task + calls analyze. Loading state "Analyzing your description…".
- **Step 2 Clarify:** list of 3–7 question cards. Each: question text, field label tag, up to 4 **chips** (toggle-select, multi allowed; selecting inserts chip text into the textarea), free-text textarea (editable after chip insert). Side panel **Suggested actions** (checklist, informational). If `source=stub` → amber badge "AI unavailable — basic questions". **Apply answers** → PUT answers → step 3.
- **Step 3 Card:** form with all fields from §4.1, tag inputs for topics/techTags. Fields with draft-extracted values show a "From your draft: '…evidence…' [Use]" hint. Empty fields highlighted with the related suggestion text as placeholder. Provenance dot per field (user / chip / draft). **Confirm & score**.
- **Step 4 Rating:** RatingPanel (below). Buttons: **Improve** (back to step 3) · **Publish**.
- **Step 5 Published:** confetti-lite message, catalog position, link to task.

**B3. Task detail (owner)** (`/business/tasks/:id`)
Tabs: **Card** (edit inline → Confirm & re-score; banner "Unconfirmed changes — catalog shows last confirmed version") · **Rating** (RatingPanel + history sparkline) · **Proposals** (compare table: team, tags, idea, plan, timeline, link, status; actions Select / Reject (optional reason) / Reset; on Selected: **Confirm milestone**).

**RatingPanel component**
- Big score `72/100` + LevelBadge.
- Progress bar with markers at 40 / 70 / 90.
- Delta chip after confirm: `+23 ▲` (green) / `−5 ▼` (red).
- "Next level: Priority — 18 points needed".
- Breakdown table: criterion · score/weight mini-bar · reason.
- "How to raise your rating": missingDetails grouped by criterion, sorted by (weight − score) desc.
- Source tag: AI / basic mode / cached.
- Catalog position "#2 of 5".

### 8.3 Team pages

**T1. Catalog** (`/catalog`)
- Top strip **Recommended for {team}** (≤3 cards with matched tags) — only for team role.
- Filters: Topic (multi-select), Level (4 checkboxes). Count "Showing n of m".
- Card list: position #, title, business name, LevelBadge, score bar, topics, tech tags, proposals count; priority → gold border + ★; draft → "Needs clarification".

**T2. Task view** (`/catalog/:id`)
Confirmed card (read-only), RatingPanel (compact), proposal form (idea, plan, timeline, prototype URL) → **Submit / Update proposal**; shows current status if exists (locked if decided).

**T3. My proposals** (`/team/proposals`) — list with status badges + milestones.

**T4. Leaderboard** (`/leaderboard`) — teams by points.

### 8.4 AI Logs (`/ai-logs`)
Table: time, kind, task, model, validation result, latency. Expand row → system prompt, input JSON, raw output. Used in demo to satisfy SoW §5.

---

## 9. Seed data

### 9.1 `demo` profile (`/seed/demo/`)

**businesses.json**
| id | name | industry | Role in demo |
|---|---|---|---|
| `b-nomad` | Nomad Logistics | Logistics | Fully filled card (Priority ~92) |
| `b-steppe` | Steppe Retail | Retail | Medium card (Workable ~55) |
| `b-tamaq` | Tamaq Café Chain | Food & Beverage | Live AI wizard demo (weak draft) |

**tasks.json** (3 cards; ratings precomputed, `source:"seed"`)
1. **Nomad Logistics — "Delivery delay prediction dashboard"** — published, all fields rich: context (manual dispatching, 18% late deliveries), need, users (12 dispatchers), data (2 years of CSV route logs, ~400k rows, weather API), constraints (6 weeks, React/Python, read-only DB replica), expected result (web dashboard + delay model), success criteria (predict delays ≥30 min with ≥75% precision on holdout month), contact + weekly 30-min call + Slack feedback. Score **92 · priority**. topics: `logistics, data-analytics`; tech: `react, python, ml`.
2. **Steppe Retail — "Customer review analysis"** — published, medium: context + need clear, data vague ("we have reviews"), expected result vague, no success criteria, users generic, contact only. Score **55 · workable**. topics: `retail, nlp`; tech: `dotnet, openai`.
3. **Tamaq Café Chain** — **not pre-seeded as a card.** Its weak draft text lives in `drafts.json` and is typed/pasted live:
   > "We are a café chain. Customers stop coming back and we don't know why. Want some app to fix it."
   Expected wizard outcome: score ~20 → after answers ~60 → after one improvement round ~80+.

   *(3rd card for the "3 cards" requirement = the Tamaq card created live; if a pre-built 3rd card is wanted for safety, seed `t-tamaq-backup` as `editing`, hidden from catalog.)*

**teams.json**
| id | name | interests | techTags |
|---|---|---|---|
| `t-bytenomads` | Byte Nomads | logistics, data-analytics, food | react, python, ml |
| `t-nullptr` | Null Pointers | retail, nlp, food | dotnet, openai, react |

**proposals.json** (on Nomad task, to demo compare/select/reject)
- `t-bytenomads` — pending — idea: ML model + dashboard, plan 4 sprints, timeline 5 weeks, link.
- `t-nullptr` — pending — idea: rules + LLM explanations, plan 3 phases, timeline 6 weeks, link.

**drafts.json** — Tamaq weak draft (+ Steppe raw draft for reference).

### 9.2 `full` profile (`/seed/full/`) — SoW §6 minimum
Everything in `demo`, plus:
- businesses: `b-kazagro` (Agriculture), `b-edutech` (Education)
- tasks: Kazagro "Crop yield reporting" **~31 · draft**; EduTech "Student attendance insights" **~78 · ready** → 5 cards total (with Tamaq backup card counted when seeded as published, or 4 + Tamaq live = 5)
- teams: `t-greenbits`, `t-pixelforge`, `t-dataweavers` → 5 teams
- proposals: +3 (EduTech ← greenbits, Kazagro ← dataweavers, Steppe ← pixelforge) → 5
- drafts.json: 5 drafts of varying completeness (very weak / weak / medium / good / complete)

Seeded ratings are stored as-is (no AI call on seed). First edit+confirm triggers real scoring.

---

## 10. Local dev & deployment

### 10.1 `.env.example`
```bash
# --- API ---
OPENAI_API_KEY=sk-REPLACE_ME
OPENAI_MODEL=REPLACE_WITH_CURRENT_MINI_MODEL
OPENAI_TIMEOUT_SECONDS=20
AI_MODE=live                    # live | stub
GCP_PROJECT_ID=taskforge-local
FIRESTORE_EMULATOR_HOST=firestore:8080   # local only; unset in Cloud Run
CORS_ORIGINS=http://localhost:5173
ASPNETCORE_URLS=http://+:8080
# --- WEB ---
VITE_API_BASE_URL=http://localhost:8080
```

### 10.2 `docker-compose.yml`
```yaml
services:
  firestore:
    image: gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators
    command: gcloud emulators firestore start --host-port=0.0.0.0:8080 --project=taskforge-local
    ports: ["8081:8080"]

  api:
    build: ./api
    env_file: .env
    environment:
      FIRESTORE_EMULATOR_HOST: firestore:8080
      GCP_PROJECT_ID: taskforge-local
    ports: ["8080:8080"]
    depends_on: [firestore]
    volumes: ["./seed:/app/seed:ro"]

  web:
    image: node:22-alpine
    working_dir: /app
    command: sh -c "npm ci && npm run dev -- --host 0.0.0.0"
    environment:
      VITE_API_BASE_URL: http://localhost:8080
    ports: ["5173:5173"]
    volumes: ["./web:/app", "/app/node_modules"]
    depends_on: [api]
```

Firestore client: `new FirestoreDbBuilder { ProjectId = cfg.GCP_PROJECT_ID, EmulatorDetection = EmulatorDetection.EmulatorOrProduction }.Build()`.

### 10.3 `api/Dockerfile`
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
COPY seed ./seed
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskForge.Api.dll"]
```
*(Build context note: seed lives at repo root — either copy `seed/` into `api/` at build time via the deploy script, or set build context to repo root with `-f api/Dockerfile`.)*

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
ENV_FILE=".env"                  # OPENAI_API_KEY read from here
# ================

OPENAI_API_KEY="$(grep -E '^OPENAI_API_KEY=' "$ENV_FILE" | cut -d= -f2-)"
[[ -z "$OPENAI_API_KEY" || "$OPENAI_API_KEY" == sk-REPLACE_ME ]] && { echo "Missing OPENAI_API_KEY in $ENV_FILE"; exit 1; }

rm -rf api/seed && cp -r seed api/seed

gcloud config set project "$PROJECT_ID"
gcloud run deploy "$SERVICE" \
  --source ./api \
  --region "$REGION" \
  --allow-unauthenticated \
  --set-env-vars "GCP_PROJECT_ID=$PROJECT_ID,OPENAI_MODEL=$OPENAI_MODEL,AI_MODE=live,CORS_ORIGINS=$CORS_ORIGINS,OPENAI_API_KEY=$OPENAI_API_KEY"

gcloud run services describe "$SERVICE" --region "$REGION" --format='value(status.url)'
```
One-time GCP prep: enable Run, Cloud Build, Artifact Registry, Firestore APIs; create Firestore database (Native mode); the default Cloud Run service account needs `roles/datastore.user`.

### 10.5 Netlify
`web/netlify.toml`
```toml
[build]
  base = "web"
  command = "npm run build"
  publish = "dist"
[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```
Set `VITE_API_BASE_URL=https://<cloud-run-url>` in Netlify site env. Deploy via Git integration or `netlify deploy --prod --dir=web/dist`.

### 10.6 `scripts/gen-client.sh`
```bash
#!/usr/bin/env bash
set -euo pipefail
# ==== CONFIG ====
API_URL="http://localhost:8080"
SPEC_PATH="/openapi/v1.json"
OUT_SPEC="web/openapi.json"
# ================
curl -fsS "$API_URL$SPEC_PATH" -o "$OUT_SPEC"
(cd web && npx orval)
```
orval config: input `./openapi.json`, output `src/api/`, client `react-query`, custom mutator `src/lib/http.ts` (adds base URL + actor headers).

---

## 11. Agent tooling (Claude + Codex)

### 11.1 `AGENTS.md` (committed, both agents read it)
```markdown
# AGENTS.md — TaskForge
Spec: docs/MVP_SPEC.md (source of truth). Briefs: docs/tasks/*.md.

## Stack
.NET 10 Minimal API (api/), React+Vite+TS+Tailwind+shadcn (web/), Firestore (emulator locally), OpenAI Responses API.

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
```

### 11.2 `CLAUDE.md`
```markdown
Read AGENTS.md first — it is authoritative. Work only within the brief you are given.
Before finishing: build (`dotnet build` / `npm run build`) and report changed files.
```

### 11.3 Briefs in `docs/tasks/` (one per agent session)
| File | Owner | Content |
|---|---|---|
| `00-scaffold.md` | A+B | Repo tree, compose, empty endpoints returning stub DTOs, OpenAPI on |
| `A1-openai-client.md` | A | ResponsesClient, schemas, prompts §5.2/5.3 verbatim |
| `A2-analyze.md` | A | Analyze endpoint, validator, evidence check, stub, aiLogs |
| `A3-score.md` | A | Score call, RatingGuard, hash cache, history, levels, stub |
| `A4-wizard-ui.md` | A | Wizard steps 1–5, chips, suggestions, provenance |
| `A5-rating-panel.md` | A | RatingPanel, delta, next level, AI logs page |
| `B1-firestore-repos.md` | B | Repos, models, seed/reset loader, actors endpoint |
| `B2-tasks-crud.md` | B | create/answers/fields/confirm/publish wiring, mine list |
| `B3-catalog.md` | B | Catalog sort/position/filters, recommendations, catalog UI |
| `B4-proposals.md` | B | Proposal upsert, list, decision, milestones, leaderboard, UIs |
| `B5-deploy.md` | B | Cloud Run + Netlify + CORS |

Each brief: goal · files to touch · DTOs/endpoints · acceptance checks (manual) · "do not touch" list.

---

## 12. Work split — 2 developers, 240 min

**Dev A — AI track:** OpenAI client, analyze, score, rating guard, wizard UI, RatingPanel, AI logs.
**Dev B — Core track:** scaffold, Firestore, seed, CRUD, catalog, recommendations, proposals, decisions, milestones, leaderboard, deploy.

| Time | Dev A | Dev B | Sync point |
|---|---|---|---|
| 0:00–0:20 | Agree spec, write AGENTS.md, briefs A1–A5 | Scaffold repo, compose, emulator, API skeleton with all endpoints returning stub DTOs, OpenAPI | **0:20** contract frozen, client generated |
| 0:20–1:15 | A1 OpenAI client + A2 analyze (+stub, validator, logs) | B1 Firestore repos, seed/reset (demo profile), `/actors`, role switcher UI, app shell | 1:15 merge; analyze callable via Swagger |
| 1:15–2:15 | A4 Wizard UI steps 1–3 (draft, clarify w/ chips, card editor); A3 score endpoint + guard | B2 tasks CRUD (answers merge, fields, confirm → calls A3 service, publish), My Tasks page; B3 catalog API + UI + filters + position | **2:15** end-to-end: draft → publish visible in catalog |
| 2:15–3:00 | A5 RatingPanel (delta, next level, missing details), wizard steps 4–5, post-publish edit flow | B3 recommendations; B4 proposals (team form, business compare table, decisions, milestones, leaderboard) | 3:00 merge; full flow locally |
| 3:00–3:20 | Stub mode check (`AI_MODE=stub`), AI logs page, error toasts | B5 deploy API (Cloud Run) + web (Netlify), CORS, seed prod | **3:20** deployed |
| 3:20–3:45 | Bugfix from rehearsal #1 | Bugfix from rehearsal #1, `full` seed profile | **3:45 CODE FREEZE** |
| 3:45–4:00 | README (architecture, formula, catalog rules, scenarios) | Rehearsal #2 on prod, backup local compose ready | Demo |

### Cut list (in order, if behind)
1. AI logs page → show `aiLogs` in Firestore console instead
2. Rating history sparkline
3. Leaderboard page (keep points on team badge)
4. Recommendations strip
5. Milestones (SoW step 8, not in mandatory demo)
6. Netlify/Cloud Run → demo from local compose
**Never cut:** wizard, rating recalc + delta, catalog sort/filters, proposal, manual decision.

---

## 13. README.md outline (SoW deliverable)
1. What it is (3 lines) + screenshot
2. Architecture diagram (§3) + stack
3. Launch: `cp .env.example .env` → set key → `docker compose up --build` → `curl -X POST localhost:8080/api/admin/seed?profile=demo` → open `localhost:5173`
4. Rating formula: rubric table §4.2, levels §4.3, guard rules §5.3, recalculation rule
5. AI: prompts, input/output schemas, invalid-response handling §5.4, "no invented facts" measures
6. Catalog rules §6.3, recommendations §6.4, decision rules §6.5
7. Data model §4.4, seed profiles §9
8. Manual test scenarios §14
9. Known limitations / future work

---

## 14. Manual test scenarios (run twice before freeze)

| # | Scenario | Expected |
|---|---|---|
| S1 | Seed demo, open catalog as Byte Nomads | Nomad (92, ★ Priority) above Steppe (55, Workable); recommendations show Nomad with matched tags |
| S2 | Filter level = Workable | Only Steppe; position still `#2 of 2` |
| S3 | As Tamaq: paste weak draft → Analyze | 3–7 questions, ≤4 chips each, suggestions list, AI badge |
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

---

## 15. 5-minute demo script (SoW §11)

| Time | Actor | Action | Say |
|---|---|---|---|
| 0:00 | — | Catalog as Byte Nomads | "Rating = task readiness, not company fame. Priority on top, drafts visible but flagged." |
| 0:30 | Tamaq | New task → paste weak draft → Analyze | "AI finds gaps and asks questions — it never invents facts." |
| 1:15 | Tamaq | Pick chips + type 2 answers → Apply → Confirm | "Score 38 → Needs clarification. Breakdown shows exactly why." |
| 2:00 | Tamaq | Improve: add data + success criteria → Confirm | "+30, now Ready. Rating recalculates on every confirmed change." |
| 2:40 | Tamaq | Publish → catalog `#2 of 3` | "Higher rating = higher catalog position." |
| 3:05 | Null Pointers | Recommended strip → open Tamaq → submit proposal | "Any team can propose; AI only recommends, never restricts." |
| 3:45 | Tamaq | Proposals → Select Null Pointers | "The business decides. No automatic assignment." |
| 4:05 | Nomad | Reject one, select other, confirm milestone → leaderboard | "Teams earn points for confirmed progress." |
| 4:35 | — | AI logs row expanded | "Prompt, structured input/output, validation and fallback." |
| 5:00 | — | End | |

Backup: local `docker compose` with `AI_MODE=stub` ready in a second browser tab.

---

## 16. Evaluation mapping (SoW §9)

| Criterion | Pts | Covered by |
|---|---|---|
| End-to-end scenario | 20 | Wizard → publish → proposal → decision (§6, §15) |
| Card quality | 15 | Field-mapped questions, chips, suggestions, editable card (§5.2, §8.2) |
| Business gamification | 25 | Rubric breakdown, reasons, missing details, delta, next level, position, guard + cache (§4, §5.3, §8.2) |
| Catalog & proposals | 15 | All tasks visible, sort, topic+level filters, manual decisions (§6.3–6.5) |
| AI function | 10 | Structured schemas, evidence check, no invented facts, fallback (§5) |
| Technical quality | 10 | Compose launch, validation, ProblemDetails, feature structure, README (§10, §13) |
| Demo | 5 | Scripted 5-min flow + backup (§15) |
