# TaskForge — MVP Specification
**AI Challenge Coach: Business Task Readiness & Open Team Selection**
4-hour hackathon build · 2 developers · .NET 9 + React + Firestore + OpenAI

---

## 0. TL;DR

A business rep types a weak task description → AI Challenge Coach identifies gaps, asks clarifying questions (with ≤4 answer chips each), and suggests improvements → rep edits and confirms a structured card → the backend computes a transparent 0–100 readiness score and actionable improvement quests → task is published to a shared catalog sorted by rating → any student team proposes → the business manually selects/rejects → optional confirmed milestones give the team points.

**One required AI call:** `analyze` (gaps + questions + chips + suggestions). The backend owns scoring, levels, quests, and catalog position. AI wording is advice; it never awards points or selects a team.

The product gamifies the quality of the business problem statement. Each editing view answers: **Where am I now? What is missing? What should I add next?** The live demo shows a confirmed before/after score, a level change when a threshold is crossed, and the resulting catalog position.

### Decisions log (from Q&A)

| # | Topic | Decision |
|---|---|---|
| 1 | Hosting | API → GCP Cloud Run; SPA → Netlify; Docker Compose for local |
| 2 | Team split | Dev A = AI track, Dev B = Core track (see §12) |
| 3 | OpenAI style | Responses API + strict JSON schema |
| 4 | Model | Mini-tier model, name in env var |
| 5 | AI scope | SoW minimum: completeness analysis + clarifying questions |
| 6 | Rating | Backend-owned, deterministic rubric; AI feedback cannot change points |
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
| 18 | Sort | Rating desc, then confirmedAt desc, then task ID |
| 19 | Filters | Topic + readiness level (level is SoW-mandatory) |
| 20 | Recommendations | Rule-based: team interest/tech tags ∩ task tags |
| 21 | Proposal fields | Idea, plan, timeline, prototype link |
| 22 | Proposals | Any team may submit; one active proposal per team per task, editable while Pending. No cap across teams. |
| 23 | Decision | Pending / Selected / Rejected, manual, reversible until milestone |
| 24 | Milestones | P2: business confirms → +10 team points → leaderboard |
| 25 | Seed | Demo: 3 businesses, 2 published cards plus live wizard demo, 2 teams; full: source-data minimums are preloaded |
| 26 | Volume | Two seed profiles: `demo` and `full` (SoW 5× minimum) |
| 27 | AI failure | Validate → 1 retry → local stub, flagged in UI |
| 28 | Secrets | Local `.env` (gitignored) + committed `.env.example` |
| 29 | Agent tooling | `AGENTS.md` + `CLAUDE.md` + per-module briefs in `/docs/tasks` |
| 30 | Testing | None automated; manual demo rehearsal + README test scenarios |

---

## 1. ⚠ Red flags (read first)

| # | Risk | Mitigation in this spec |
|---|---|---|
| R1 | **Rating must be explainable and stable (25 pts criterion).** | The backend applies versioned, field-specific 0/half/full rules to confirmed fields; AI cannot supply points or levels. Identical confirmed input and rubric version produce the same breakdown, quests, and total. |
| R2 | **"AI must not add facts."** Chips and extraction can smuggle invented facts. | Chips are options only and require user selection. Extraction is restricted to verbatim spans from the draft; backend verifies value/evidence correspondence and that evidence occurs in the raw draft. AI-generated titles require human review. |
| R3 | **"Draft" naming collision.** Readiness level "Draft" (0–39) ≠ unpublished task. | Use `status: editing \| published` and `level: draft \| workable \| ready \| priority`. UI label for level draft: "Needs clarification". |
| R4 | **Seed below SoW §6 minimum** (5 of each). | `full` contains at least 5 persisted drafts, complete cards, teams, and proposals immediately after seed; it never counts live wizard activity. |
| R5 | **SoW requires readiness filter** — not only topic. | Both filters implemented. |
| R6 | **Live demo depends on OpenAI latency/availability.** | Mini model, 20s timeout, stub fallback, `AI_MODE=stub` env override for offline. |
| R7 | **4 h not 5 h; 2 devs not 3–5.** | Hard cut list in §12; code freeze at T+3:45. |
| R8 | **Secrets.** Real `OPENAI_API_KEY` in git = key revoked + judges notice. | `.env` in `.gitignore` from minute 1; Cloud Run gets vars via `--set-env-vars`. |
| R9 | **CORS / mixed hosting.** Netlify → Cloud Run calls may fail during a hosted demo. | CORS allow-list from env; rehearse the local Compose flow first and deploy only if time remains. |
| R10 | **No automated tests.** | Manual scenario checklist §14 run twice before freeze. |

---

## 2. Scope

### Delivery priorities (4-hour MVP)

P0 is the demo gate. Start P1 only after the P0 flow runs end to end. P2 is optional. The `full` seed profile and README remain submission deliverables even when the live demo uses `demo`. P0 includes two successive confirmations within the initial wizard to show a score change; post-publish editing and preview are P1.

#### P0 — end-to-end demo
- Role switcher (3 businesses, N teams)
- Task wizard: draft → AI analyze → answer questions → editable card → confirm → deterministic rating → publish
- AI Challenge Coach panel: current score/progress, level, next threshold, improvement quests with point ceilings and direct links to fields, confirmed before/after comparison in the initial wizard and level-up event
- Rating panel: score, level, breakdown, missing details, delta vs. previous, next-level hint
- Shared catalog: all published tasks, sort by rating, filters topic + level, priority highlighted, catalog position `#n of m`
- Proposals: submit/edit (one active proposal per team per task; unlimited teams may propose)
- Business review: compare proposals, Select / Reject
- Seed profiles (`demo`, `full`), local seed/reset, README and manual demo rehearsal

#### P1 — after P0 is stable
- Recommendations strip for teams (rule-based on interests, skills, and technology tags)
- Post-publish card edits, rating preview and before/after history
- AI call log viewer (prompt, input, output, validation result); document prompt/schema and invalid-response handling in README even if viewer is cut
- Decision Reset before any milestone

#### P2 — if time remains
- Milestone confirmation → team points → leaderboard
- Rating history sparkline, Cloud Run + Netlify deployment

Task readiness and catalog position are the primary gamification. Milestone points recognize confirmed team progress required by `TECHTASK.pdf`; they do not feed task ratings, student XP, or automatic assignment.

### Out of scope
Auth, passwords, complex roles, chat, notifications, calendar, file upload, ML training, vector DB, mobile layout, project tracker, automated tests, CI/CD.

---

## 3. Architecture

```
┌──────────────┐   HTTPS/JSON   ┌───────────────────────────┐    gRPC    ┌───────────┐
│ React SPA    │ ─────────────► │ .NET 9 Minimal API        │ ─────────► │ Firestore │
│ (Netlify)    │  X-Actor-*     │ (Cloud Run)                │            │ (native)  │
│ TanStack Q.  │                │  Features/ Rating/ Ai/     │ ─────────► │ OpenAI    │
└──────────────┘                └───────────────────────────┘   HTTPS    │ Responses │
                                                                          └───────────┘
Local: docker compose → web (vite) + api + firestore-emulator
```

- Frontend never touches Firestore. Firestore security rules: deny all client access.
- API is stateless; all state in Firestore.
- Demo actor identity via role-switcher headers `X-Actor-Role: business|team`, `X-Actor-Id: <id>`. Validate existence and ownership for correctness; this is not authentication. Public destructive admin operations are disabled or require a server-side secret.

### Repository layout

```
/
├─ AGENTS.md                  # shared agent rules (Codex + Claude)
├─ CLAUDE.md                  # "Read AGENTS.md" + Claude-specific notes
├─ README.md                  # SoW deliverable: architecture, formula, catalog rules, test scenarios
├─ MVP_SPEC.md                # source of truth for this build
├─ .env.example
├─ .gitignore                 # includes .env
├─ docker-compose.yml
├─ docs/
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
│     ├─ Ai/                  # analyze, stub, validator, ai-logs
│     ├─ Rating/              # deterministic rules, quests, levels, cache, history
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

The AI Challenge Coach reference calls 40–69 “Working”; this MVP uses “Workable” because that is the label in `TECHTASK.pdf`. The score boundaries are identical.

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
  answersApplied: bool                     # false until one successful Apply in Step 2
  appliedAnswersHash: string | null         # canonical submitted payload, for retry idempotency
  analysis: {                              # last AI analyze result (validated)
    missingFields: string[],
    questions: [{ id, fieldKey, question, chips: string[] }],
    suggestions: [{ fieldKey, action }],
    extracted: [{ fieldKey, value, evidence }],
    source: "ai" | "stub", createdAt
  }
  revision: int                             # incremented on every editable-field/answer mutation
  confirmed: {                             # snapshot at last confirm
    fields: {...}, hash: string, revision: int, confirmedAt
  } | null
  hasUnconfirmedChanges: bool
  rating: {
    total: int, level: string,
    breakdown: [{ criterion, weight, score, reason, matchedSignals: string[] }],
    missingDetails: [{ criterion, detail }],
    quests: [{ criterion, fieldKey, action, potentialPoints }],
    source: "rules" | "seed" | "cache",
    ratingRulesVersion: string, cacheKey: string, scoredAt
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
  kind: "analyze", taskId, model,
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
3. "missingFields": list only the provided field keys whose details are absent or too weak.
4. "questions": 3 to 7 questions, one per missing or weak field, most important first
   (priority: need, context, data, expectedResult, successCriteria, users, constraints,
   interactionFormat, contact). Each question is short, concrete, answerable in 1-3 sentences.
5. "chips": 0 to 4 short answer OPTIONS per question that the user may pick. They are generic
   typical options for this kind of business, phrased as choices (e.g. "CSV export from POS system"),
   never as claims about this company. Use [] when options would require guessing specifics
   (e.g. contact).
6. "suggestions": 2 to 6 actionable improvement items, each starting with a verb
   ("Add...", "Specify...", "Describe..."), telling WHAT to add, never containing invented content.
7. "fieldKey" must be one of the provided field keys.
8. Language: same language as the draft. English demo fixtures are required for the MVP scoring rules; other languages receive best-effort scoring.
Return JSON matching the schema only.
```

**Output schema**
```json
{
  "type": "object",
  "additionalProperties": false,
  "required": ["title", "missingFields", "extracted", "questions", "suggestions"],
  "properties": {
    "title": { "type": "string", "description": "Short neutral title from the draft wording; empty if unclear" },
    "missingFields": { "type": "array", "items": { "type": "string", "enum": ["context","need","users","data","constraints","expectedResult","successCriteria","contact","interactionFormat"] } },
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
- `missingFields`: trim, deduplicate, reject unknown field keys; show as gaps to review, not as business facts
- each `chips.Count` ≤ 4 → truncate to 4 (soft fix), trim, dedupe, drop empty/ > 80 chars
- `extracted[i].evidence` must be a case-insensitive substring of `rawDraft`, and `value` must be copied from the evidenced source text (normalization only; no paraphrases or added details) → else drop item (log)
- `title` is an untrusted suggestion: require business review/edit before confirm and publish
- Prompt rule: title may only use words/facts supported by the draft; if unclear return empty. The wizard labels it as an AI suggestion and does not silently treat it as confirmed user text.
- `suggestions` truncate to 6, drop empty
- `title` > 120 chars → truncate
- Assign `questions[i].id = "q{i+1}"`

### 5.3 Backend readiness engine (no LLM score)

The `RatingService` calculates the seven criterion scores from card fields. For an awarded rating, it uses the **confirmed snapshot**; for an explicitly labeled preview, it uses the current editable fields without awarding points. Each criterion receives `0`, `floor(weight / 2)`, or its full weight. The total is their sum; level thresholds remain in §4.3. A nonempty field alone earns at most half credit. Full credit requires the field-specific evidence shown below. The implementation must keep these checks in a versioned, inspectable ruleset and return the rule that fired in each breakdown reason. Text length or an isolated digit never qualifies for full credit.

| Criterion | Half-credit condition | Full-credit evidence check | Quest when not full |
|---|---|---|---|
| `contextAndNeed` (20) | Exactly one of `context`, `need` has content | Both have content | Describe the missing current situation or required change |
| `dataAndMaterials` (20) | `data` has nonempty content | Source/example plus a concrete format, quantity, or access detail | Name an available source and how the team can inspect it |
| `expectedResult` (15) | `expectedResult` has nonempty content | A deliverable/artifact and what it must do | Name the deliverable and its function |
| `successCriteria` (15) | `successCriteria` has nonempty content | An outcome plus a measurable target or explicit acceptance check | Define a measure and acceptance threshold |
| `constraints` (10) | One boundary is stated | Two distinct boundaries, such as deadline plus technology or access | Add another deadline, technology, access, legal, or budget boundary |
| `users` (10) | A user group is named | User group plus its role or usage situation | Explain what that group does with the result |
| `businessConnection` (10) | Contact or interaction format is present | Contact plus consultation format and feedback procedure | Add the missing contact, consultation, or feedback detail |

#### Versioned detector rules (`ratingRulesVersion = en-mvp-1`)

These are finite, inspectable English-language MVP rules. Check the field named in the rubric only (except the two-field criteria). Lowercase invariantly, normalize Unicode and whitespace, retain word boundaries, and recognize the phrases below as whole words/phrases; allow common plurals. Match numeric patterns only with a unit or comparator. Emit signal IDs in `matchedSignals` and the missing signal in `reason`. Keep the field text intact. Signal matches indicate that a description is specific enough for readiness points; they do not verify the truth of a business claim.

| Criterion | Required signals for full credit | Initial English detector vocabulary / pattern |
|---|---|---|
| `contextAndNeed` | `context.present` AND `need.present` | Both respective fields meet the nonempty rule of §4.2. |
| `dataAndMaterials` | `data.source` AND at least one of `data.format`, `data.quantity`, `data.access` | Source: `logs, transactions, purchase history, reviews, orders, records, dataset, database, files, documents, survey, API, route data`; format: `CSV, XLSX, Excel, JSON, PDF, SQL, API`; quantity: number + `rows, records, files, months, years, GB, MB` (including `400k rows`); access: `read-only, export, shared folder, API access, replica, sample provided`. A format word alone cannot satisfy `data.source` unless it names an actual source such as `CSV route logs`. |
| `expectedResult` | `result.artifact` AND `result.function` | Artifact: `dashboard, app, website, report, model, prototype, API, tool, service`; function: an action verb tied to that artifact, e.g. `predict, display, track, classify, summarize, alert, search, recommend, analyze`. A lone `app` is half credit. |
| `successCriteria` | `success.outcome` AND `success.acceptance` | Outcome: named action/metric such as `predict delays, accuracy, precision, recall, conversion, response time, error rate, completion rate`; acceptance: comparator (`at least, no more than, under, over, >=, <=, ≥, ≤`) with number and unit/metric (`%, seconds, minutes, days, users, records`), OR `accepted if / passes` followed by a concrete test or deliverable check. A bare number or bare percentage cannot earn full credit. |
| `constraints` | At least two distinct categories | Deadline: number + `days/weeks/months` or an explicit date; technology: `React, Python, .NET, Java, SQL` with `must, use, only, required`; access: `read-only, no production access, offline`; budget: currency/amount with `budget, cap, maximum`; legal: `NDA, GDPR, consent, anonymized` with `must, required, only`. Two phrases in one category count once. |
| `users` | `users.group` AND `users.usage` | Group: `dispatchers, customers, students, teachers, analysts, managers, operators, support agents` (optionally count); usage: role/action phrase such as `use to, review, monitor, enter, decide, approve, receive alerts` linked to that group. A group name alone is half credit. |
| `businessConnection` | `contact.present` AND `interaction.consultation` AND `interaction.feedback` | Contact meets §4.2 nonempty rule; consultation: `call, meeting, consultation, office hours, weekly sync`; feedback: `feedback, review, comments, approve, acceptance session`. Both must be in `interactionFormat`; a channel alone is insufficient. |

Implement phrase matching with small per-field tables and explicit numeric regexes, not a generic token counter. When an English phrasing is outside the table or a match is ambiguous, award at most half credit for the nonempty field and state what the detector could not establish. Multiple full-credit conditions must be independently present. If a field is empty, apply the zero/half rules in §4.2 and the table above; do not infer evidence from another field. Non-English input may be clarified by AI in its own language, but these evidence detectors are optimized and manually validated only for English. For non-English or mixed-language text, detect supported terms where possible, otherwise use half credit for nonempty fields and display the limitation. No AI response can modify `score`, `level`, or `matchedSignals`.

For each criterion return `{ criterion, weight, score, reason, matchedSignals }` and up to three `missingDetails`. A quest is generated from the highest-value missing condition with `potentialPoints = weight - score`, a short action, and `fieldKey` for the **Add details** button. `potentialPoints` is a ceiling if the missing evidence is supplied, not a promised score increase. Sort quests by potential points descending. The next-level hint uses `threshold - total`, never a sum of potential points.

Use `ratingRulesVersion` in the cache key: SHA-256 of canonical normalized confirmed fields plus ruleset version. Identical confirmed fields under the same ruleset yield the same score regardless of AI availability or model. Append confirmed results to `ratingHistory` (last 10); compute delta against the preceding confirmed result. A repeated confirm with no edits returns the cached result and adds no history entry.

### 5.4 Failure handling (analyze call)

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
- `AnalyzeStub`: list missing/weak keys in `missingFields` and ask about them in priority order, up to 5 and never fewer than 3 questions. If fewer than 3 gaps exist, ask relevant specificity/confirmation questions about the weakest populated fields. Chips `[]`; suggestions = "Add {label}" for each gap; extracted `[]`.
- The readiness engine in §5.3 runs unchanged when AI is unavailable. The user can continue editing and confirming the card; only AI questions, chips, and wording suggestions switch to basic mode.

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

Every analyze attempt (success, retry, or stub fallback) writes an `aiLogs` document. `AI_MODE=stub` skips OpenAI entirely, while rating remains deterministic.

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
- Points are awarded only for confirmed fields (SoW §4): the authoritative rating is committed only on `confirm`, from that revision's confirmed snapshot.
- While editing, the Coach may request a deterministic preview from the unsaved fields. Label it **Preview — confirm to update your rating**; previews are never written to `ratingHistory`, used for catalog sorting, or treated as awarded points.
- Catalog always displays `confirmed.fields` + `rating`, never unconfirmed edits.
- Publishing is allowed at any score (low rating doesn't hide the task — SoW §4).
- Publish requires: `title` non-empty, `rating != null`, no unconfirmed changes.

### 6.2 Answers → card merge (one-time Apply)
The clarification round is applied **once** per task in wizard Step 2. `PUT /answers` accepts `[{ questionId, fieldKey, text }]` as one atomic submission and checks the IDs and mapped field keys against the current validated analysis. For each field, join nonempty answers in question order; append once to existing editable user text with a newline, or set the joined answers if the field is empty. Set `answersApplied=true` in the same write as the field update and revision increment. The card fields are then authoritative user-editable content; later changes go through `PUT /fields`, including post-publish edits. No original-value reconstruction or repeated answer merge.
- On a retry, the server returns the saved task without changing fields or revision if the payload is identical to the stored one-time application; a different subsequent payload returns 409 and directs the user to edit the card.
- Store the applied payload or a canonical payload hash beside `answersApplied` to distinguish a retry from a different submission.
- `fieldProvenance` is UI-only: `chip` if the submitted text exactly matches a chip, otherwise `user`; extracted draft values apply only when the user clicks **Use** (`draft-extract`).
- Returning to Step 2 after Apply shows the applied answers read-only and a link to the card editor; **Apply answers** is disabled.

### 6.3 Catalog
- Source: `tasks where status == published`, loaded in memory.
- Sort: `rating.total desc`, then `confirmedAt desc` (or `publishedAt` before first confirmation), then stable task ID. Unconfirmed edits do not change catalog order.
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
- Id `${taskId}_${teamId}` → one active proposal per team per task, upsert. No cap across teams. Editable only while `pending`.
- Validation: idea 20–2000, plan 20–3000, timeline 3–200, prototypeUrl valid `http(s)` URL or empty.
- Allowed on any published task, any level.
- Only the owning business can decide. Transitions: `pending ↔ selected`, `pending ↔ rejected`, `selected ↔ rejected`. Once a milestone is confirmed, decision is locked.
- Multiple `selected` allowed; zero is fine.
- No automatic selection anywhere (SoW §3, §5).

### 6.6 Milestones & team points
- Business on a `selected` proposal → "Confirm milestone" (title required) → `+10` to `teams.points` and entry in `proposal.milestones`. Use a stable milestone ID and atomically create it only if absent while incrementing points, so retries/double clicks cannot award twice.
- Leaderboard: teams sorted by `points desc`.

---

## 7. API specification

All routes under `/api`. JSON. Errors = RFC 7807 `ProblemDetails` with `errors` map for validation. Headers `X-Actor-Role`, `X-Actor-Id` required except `/health`, `/admin/*`, `GET /actors`. Admin seed/reset mutations are local-only or protected by a server-side admin secret; never expose unauthenticated destructive admin routes publicly.

| Method | Route | Actor | Body → Response |
|---|---|---|---|
| GET | `/health` | — | `{ status, aiMode, firestore: "ok" }` |
| GET | `/actors` | — | `{ businesses[], teams[] }` for role switcher |
| POST | `/tasks` | business | `{ rawDraft, industry }` → `TaskDto` (status editing) |
| GET | `/tasks/mine` | business | `TaskSummaryDto[]` (incl. position, proposalCount) |
| GET | `/tasks/{id}` | any | `TaskDto` (team sees confirmed view only) |
| POST | `/tasks/{id}/analyze` | business owner | → `AnalysisDto` |
| PUT | `/tasks/{id}/answers` | business owner | one-time `{ answers:[{questionId, fieldKey, text}] }` → `TaskDto`; identical retry returns saved result, changed retry → 409 |
| PUT | `/tasks/{id}/fields` | business owner | `{ fields:{...partial} }` → `TaskDto` |
| GET | `/tasks/{id}/rating-preview` | business owner | deterministic preview from current editable fields; no persistence or catalog effect |
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
| POST | `/admin/seed?profile=demo\|full` | local/admin | wipes + loads seed |
| POST | `/admin/reset` | local/admin | wipe all collections |

**DTO sketches**
```ts
RatingDto { total:number; level:'draft'|'workable'|'ready'|'priority';
  breakdown:{criterion:string; weight:number; score:number; reason:string; matchedSignals:string[]}[];
  missingDetails:{criterion:string; detail:string}[];
  quests:{criterion:string; fieldKey:string; action:string; potentialPoints:number}[];
  source:'rules'|'seed'|'cache'; ratingRulesVersion:string; scoredAt:string;
  nextLevel?:{ level:string; pointsNeeded:number } }

AnalysisDto { title:string;
  missingFields:string[];
  questions:{id:string; fieldKey:string; question:string; chips:string[]}[];
  suggestions:{fieldKey:string; action:string}[];
  extracted:{fieldKey:string; value:string; evidence:string}[];
  source:'ai'|'stub' }
```

Confirm/scoring is revision-safe: capture the edited task revision when scoring starts, then atomically save confirmed fields, canonical hash, rating, and `confirmedAt` only if the revision has not changed. If it changed, return a conflict and ask the business to confirm again. Catalog reads use only this committed snapshot.

---

## 8. Frontend specification

### 8.1 Global
- **Top bar:** TaskForge name + “AI Challenge Coach” product label · RoleSwitcher (grouped select: Businesses / Teams) · nav links for the current role · "AI logs" link.
- Actor stored in React context + `localStorage`; orval custom mutator injects `X-Actor-*` headers.
- Toasts for errors (ProblemDetails `title` + first field error).
- After each mutation, invalidate related TanStack queries (task, catalog, mine; leaderboard only if P2 is built).

### 8.2 Business pages

**B1. My Tasks** (`/business/tasks`)
Table: title · status · score + LevelBadge · catalog position `#n/m` · proposals count · "Open". Button **New task**.

**B2. New Task Wizard** (`/business/tasks/new` → `/business/tasks/:id/wizard`)
- Stepper: `1 Draft → 2 Clarify → 3 Card → 4 Rating → 5 Publish`.
- **Step 1 Draft:** textarea (20–4000), industry select, **Analyze** → creates task + calls analyze. Loading state "Analyzing your description…".
- **Step 2 Clarify:** “AI Challenge Coach” panel with 3–7 relevant question cards. Each has question text, field label, up to 4 chips (selecting inserts editable text), and a free-text answer. Show suggested actions separately. If `source=stub`, show “AI unavailable — basic questions; you can continue editing.” **Apply answers** maps answers once to editable card fields; after Apply, show answers read-only and direct further changes to the card editor.
- **Step 3 Card:** two-column layout: editable form on the left, Coach/RatingPanel on the right. Include all §4.1 fields and tag inputs. Draft extracts show evidence and a **Use** action; never auto-insert them. Show confirmed score/progress and an optional clearly marked unsaved preview. Each quest has a point ceiling and **Add details** action that focuses the named field. **Confirm & score** commits the card.
- **Step 4 Rating:** show previous and current confirmed scores, delta, before/after level, and a level-up notice only when a threshold is crossed. RatingPanel and quests show the next action. Buttons: **Improve** (back to step 3) · **Publish**.
- **Step 5 Published:** confetti-lite message, catalog position, link to task.

**B3. Task detail (owner)** (`/business/tasks/:id`)
P0: confirmed **Card** and **Rating** (breakdown), plus **Proposals** (compare; Select / Reject). P1 adds card edit/re-score, unconfirmed banner, before/after history, and Reset. P2 adds history sparkline and **Confirm milestone**.

**RatingPanel component**
- Big score `72/100` + LevelBadge.
- Progress bar with markers at 40 / 70 / 90.
- Delta chip after confirm: `+23 ▲` (green) / `−5 ▼` (red).
- "Next level: Priority — 18 points needed".
- Breakdown table: criterion · score/weight mini-bar · reason.
- "Improve Your Challenge": quests sorted by potential points, each with a short action, point ceiling, and **Add details** field shortcut. Explain that the full point gain requires the listed evidence.
- Source tag: deterministic rules / seeded / cached. The AI/basic-mode badge belongs to clarification only.
- Show “before → after” and a level-up notice after confirmation; never animate a preview as awarded points.
- Catalog position "#2 of 5".

### 8.3 Team pages

**T1. Catalog** (`/catalog`)
- Top strip **Recommended for {team}** (≤3 cards with matched tags) — only for team role.
- Filters: Topic (multi-select), Level (4 checkboxes). Count "Showing n of m".
- Card list: position #, title, short confirmed-card summary, business name, LevelBadge, score bar, topics, tech tags, proposals count, **View details** CTA; priority → gold border + ★; draft → "Needs clarification".

**T2. Task view** (`/catalog/:id`)
Confirmed card (read-only) with expected result, success criteria, and constraints prominent; compact RatingPanel; proposal form (solution idea, implementation plan, estimated duration, prototype URL) → **Submit / Update proposal**. Team name comes from the role switcher. Show existing status and lock edits after a decision.

**T3. My proposals** (`/team/proposals`) — list with status badges; milestones if P2 is built.

**T4. Leaderboard** (`/leaderboard`, P2) — teams by points.

### 8.4 AI Logs (`/ai-logs`)
Table: time, kind, task, model, validation result, latency. Expand row → system prompt, input JSON, raw output. Used in demo to satisfy SoW §5.

---

## 9. Seed data

### 9.1 `demo` profile (`/seed/demo/`)

`demo` is the short rehearsal profile and is not intended to meet source-data volume minimums. Use `full` for evaluation/submission; it satisfies all counts immediately after seeding.

**businesses.json**
| id | name | industry | Role in demo |
|---|---|---|---|
| `b-nomad` | Nomad Logistics | Logistics | Fully filled card (Priority under §5.3 rules) |
| `b-steppe` | Steppe Retail | Retail | Medium card (Workable under §5.3 rules) |
| `b-tamaq` | Tamaq Café Chain | Food & Beverage | Live AI wizard demo (weak draft) |

**tasks.json** (2 published cards; derive seeded ratings using the §5.3 rules and store `ratingRulesVersion`)
1. **Nomad Logistics — "Delivery delay prediction dashboard"** — published, all fields rich: context (manual dispatching, 18% late deliveries), need, users (12 dispatchers), data (2 years of CSV route logs, ~400k rows, weather API), constraints (6 weeks, React/Python, read-only DB replica), expected result (web dashboard + delay model), success criteria (predict delays ≥30 min with ≥75% precision on holdout month), contact + weekly 30-min call + Slack feedback. Level **Priority** under the ruleset. topics: `logistics, data-analytics`; tech: `react, python, ml`.
2. **Steppe Retail — "Customer review analysis"** — published, medium: context + need clear, data vague ("we have reviews"), expected result vague, no success criteria, users generic, contact only. Level **Workable** under the ruleset. topics: `retail, nlp`; tech: `dotnet, openai`.
3. **Tamaq Café Chain** — its weak draft text lives in `drafts.json` and is typed/pasted live:
   > "We are a café chain. Customers stop coming back and we don't know why. Want some app to fix it."
Expected wizard outcome: low initial score, then a visible increase after confirmed additions. The same card fields always produce the same score under one ruleset, regardless of AI mode; use a prepared, manually verified fixture for any scripted numeric claims.

The `demo` profile may include a hidden editing backup card for rehearsal convenience; live creation does not count toward the `full` minimum.

**teams.json**
| id | name | interests | techTags |
|---|---|---|---|
| `t-bytenomads` | Byte Nomads | logistics, data-analytics, food | react, python, ml |
| `t-nullptr` | Null Pointers | retail, nlp, food | dotnet, openai, react |

Each team fixture also has a non-empty `skills` array, as required by the source-data profile definition.

**proposals.json** (on Nomad task, to demo compare/select/reject)
- `t-bytenomads` — pending — idea: ML model + dashboard, plan 4 sprints, timeline 5 weeks, link.
- `t-nullptr` — pending — idea: rules + LLM explanations, plan 3 phases, timeline 6 weeks, link.

**drafts.json** — Tamaq weak draft (+ Steppe raw draft for reference).

### 9.2 `full` profile (`/seed/full/`) — SoW §6 minimum
Includes `demo` content plus enough persisted records to contain at least 5 drafts, 5 complete task cards, 5 team profiles, and 5 proposals immediately after seed. Counts never depend on the live demo.
- businesses: `b-kazagro` (Agriculture), `b-edutech` (Education)
- tasks: Kazagro "Crop yield reporting" (**Draft** under the ruleset); EduTech "Student attendance insights" (**Ready**); plus `t-tamaq-backup` as a complete, confirmed editing card → at least 5 card documents with every rating field. This backup remains hidden from the catalog.
- teams: `t-greenbits`, `t-pixelforge`, `t-dataweavers` → at least 5 teams, each with name, interests, skills, and technologies
- proposals: +3 (EduTech ← greenbits, Kazagro ← dataweavers, Steppe ← pixelforge) → at least 5 proposals, each with team, idea, plan, timeline, and link field
- drafts.json: at least 5 distinct drafts of varying completeness (very weak / weak / medium / good / complete)

The demo fixtures and scoring rules are validated in English; other-language readiness detection is best-effort (§5.3). The seed loader calculates each rating with the same versioned backend rules as confirmation; no AI call runs on seed. A later edit and confirmation recalculates it with those rules. Do not hardcode score totals that disagree with the fields.

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
    build:
      context: .
      dockerfile: api/Dockerfile
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
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
COPY seed ./seed
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskForge.Api.dll"]
```
Docker Compose builds from repository root using `api/Dockerfile`, so `COPY seed ./seed` resolves to the checked-in root fixture directory. Cloud Run source deployment uses `api/` as its source root, so `scripts/deploy-api.sh` first stages fixtures into `api/seed/`; treat that directory as generated and gitignore it. The script updates the staging directory without deleting it first.

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
ADMIN_MUTATIONS_ENABLED="false"    # public deployment must not expose seed/reset
ENV_FILE=".env"                  # OPENAI_API_KEY read from here
# ================

OPENAI_API_KEY="$(grep -E '^OPENAI_API_KEY=' "$ENV_FILE" | cut -d= -f2-)"
[[ -z "$OPENAI_API_KEY" || "$OPENAI_API_KEY" == sk-REPLACE_ME ]] && { echo "Missing OPENAI_API_KEY in $ENV_FILE"; exit 1; }

mkdir -p api/seed
cp -r seed/. api/seed/

gcloud config set project "$PROJECT_ID"
gcloud run deploy "$SERVICE" \
  --source ./api \
  --region "$REGION" \
  --allow-unauthenticated \
  --set-env-vars "GCP_PROJECT_ID=$PROJECT_ID,OPENAI_MODEL=$OPENAI_MODEL,AI_MODE=live,CORS_ORIGINS=$CORS_ORIGINS,ADMIN_MUTATIONS_ENABLED=$ADMIN_MUTATIONS_ENABLED,OPENAI_API_KEY=$OPENAI_API_KEY"

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
If code generation blocks integration for >10 minutes, follow §12's typed fetch fallback.

orval config: input `./openapi.json`, output `src/api/`, client `react-query`, custom mutator `src/lib/http.ts` (adds base URL + actor headers).

---

## 11. Agent tooling (Claude + Codex)

### 11.1 `AGENTS.md` (committed, both agents read it)
```markdown
# AGENTS.md — TaskForge
Spec: MVP_SPEC.md (source of truth). Briefs: docs/tasks/*.md.

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
| `A1-openai-client.md` | A | ResponsesClient, analyze schema and prompt §5.2 |
| `A2-analyze.md` | A | Analyze endpoint, validator, evidence check, stub, aiLogs |
| `A3-score.md` | A | Backend readiness rules §5.3, quests, cache, history, levels, preview |
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

**Dev A — Coach and rating track:** stub analyze first, deterministic rating/quests, wizard UI, RatingPanel, then live OpenAI and AI logs.
**Dev B — Core track:** scaffold, Firestore, seed, CRUD, catalog, recommendations, proposals, decisions, milestones, leaderboard, deploy.

| Time | Dev A | Dev B | Sync point |
|---|---|---|---|
| 0:00–0:20 | Agree spec; implement `AI_MODE=stub` analyze contract and sample response | Scaffold repo, API contract, AGENTS.md and briefs; prepare fixtures for `full` | **0:20** stub callable, contract frozen |
| 0:20–1:15 | A3 versioned rating rules and quests; A4 wizard skeleton using stub | B1 Firestore repos, seed/reset (`full` and `demo`), `/actors`, role switcher and shell | 1:15 merge; stub → card → deterministic score callable |
| 1:15–2:15 | Finish Wizard steps 1–5 and RatingPanel; wire stub questions to one-time Apply and confirm | B2 tasks CRUD (one-time answers, fields, confirm → rating service, publish), My Tasks; B3 catalog API/UI, filters and position | **2:15** P0 draft → publish visible in catalog |
| 2:15–3:00 | A1/A2 live OpenAI Responses integration, validator and retry/fallback; keep stub selectable | B4 proposals (team form, business compare table, Select/Reject); then P1 recommendations | **3:00** P0 flow locally with stub and live call checked |
| 3:00–3:20 | Rehearse P0 in stub mode; P1 preview, history and AI logs if stable | Rehearse P0; P1 recommendations/edit, then P2 milestones/leaderboard if stable | **3:20** local rehearsal complete |
| 3:20–3:45 | Bugfix from rehearsal #1 | Bugfix from rehearsal #1; check `full` seed counts and scored levels | **3:45 CODE FREEZE** |
| 3:45–4:00 | README (architecture, formula, catalog rules, scenarios) | Rehearsal #2 on the running demo environment; keep local compose ready | Demo |

### Cut strategy
Finish P0 before P1, and P1 before P2. Drop P2 hosting, sparkline and milestone/leaderboard first; then defer P1 AI logs page (document prompt/schema, one real response and invalid handling in README), recommendations, post-publish edit/preview/history. Preserve a confirmed before/after demonstration with two edits and confirms in the initial wizard. **Never cut:** AI clarification (live integration plus reliable stub), editable card, deterministic score/breakdown, Coach progress/quests, confirmed increase/level-up, catalog sort/filters, proposal, manual decision.

If OpenAPI/orval or its custom mutator blocks frontend integration for more than 10 minutes, implement a minimal hand-written typed fetch wrapper for P0 routes with the same base URL and actor headers. Keep the endpoint DTOs as the contract and return to generation only after P0 works.

---

## 13. README.md outline (SoW deliverable)
1. What it is (3 lines) + screenshot
2. Architecture diagram (§3) + stack
3. Launch: `cp .env.example .env` → set key → `docker compose up --build` → `curl -X POST localhost:8080/api/admin/seed?profile=demo` → open `localhost:5173`
4. Rating formula: rubric table §4.2, deterministic evidence checks §5.3, levels §4.3, preview/confirmation rule
5. AI: prompts, input/output schemas, invalid-response handling §5.4, "no invented facts" measures
6. Catalog rules §6.3, recommendations §6.4, decision rules §6.5
7. Data model §4.4, seed profiles §9
8. Manual test scenarios §14
9. Known limitations / future work

---

## 14. Manual test scenarios (run twice before freeze)

| # | Scenario | Expected |
|---|---|---|
| S1 | Seed demo, open catalog as Byte Nomads | Nomad (Priority) above Steppe (Workable); displayed totals match §5.3 rules ; if P1 recommendations are built, matching tags are explained |
| S2 | Filter level = Workable | Only Steppe; position still `#2 of 2` |
| S3 | As Tamaq: paste weak draft → Analyze | 3–7 questions, ≤4 chips each, suggestions list, AI badge |
| S4 | Answer via chips + free text → Apply → Confirm | Editable card is filled from user answers; confirmed score, progress, breakdown, and highest-value quests are visible |
| S5 | In the initial wizard add missing data and success criteria, then Confirm again | Confirmed score/delta rises and before/after appears; P1 preview, if built, is marked unawarded |
| S6 | Confirm again without changes | Same score, source "cached" |
| S7 | Publish, switch to Null Pointers → catalog | Tamaq visible at correct position; submit proposal |
| S8 | Submit proposal with invalid URL | Validation error shown, nothing saved |
| S9 | As Tamaq → Proposals → Select Null Pointers | Status Selected; team sees it in My proposals |
| S10 (P2) | As Nomad → reject Null Pointers, select Byte Nomads, confirm milestone | Statuses updated; Byte Nomads +10 on leaderboard; decision locked |
| S11 | Set `AI_MODE=stub`, restart, run S3–S4 | Template questions and amber clarification badge; same confirmed fields receive the same backend score |
| S12 (P1) | Edit published task field, don't confirm | Catalog still shows old version; banner "Unconfirmed changes" |
| S13 (P1) | AI logs page | analyze entries with prompt, input, raw output, validation status |
| S14 | Force malformed AI output, then exhaust retry | One retry occurs, deterministic question stub is used, and attempt/fallback logs explain the failure |
| S15 (P1) | Confirm a revision, edit and confirm another revision, then restore the original fields | Original cache key/result is reused; score is independent of AI mode; repeated confirm adds no history entry |
| S16 | Submit the same answers twice, then change one answer and submit again | Identical retry leaves fields/revision unchanged; changed retry returns 409 with card edit path |
| S18 | Score `three years of customer transactions in Excel`, `customer purchase history`, and a Russian-language data description | English source + quantity/format yields full; English source alone half; unsupported Russian evidence remains at most half with an explanation |
| S17 | Seed `full` and count records | At least 5 drafts, 5 complete cards, 5 teams with skills, and 5 proposals exist before demo interaction |

---


## 15. Five-minute demo script (SoW §11)

| Time | Actor | Action | Show |
|---|---|---|---|
| 0:00 | Team | Open ranked catalog | Priority task appears above Workable; low-rated tasks remain visible. |
| 0:25 | Business | Enter weak request, select **Analyze with AI** | Coach identifies gaps and asks at least three relevant questions. |
| 1:05 | Business | Answer questions, inspect editable card | User-provided facts are distinct from AI wording; no fact is silently invented. |
| 1:40 | Business | Confirm card | Readiness progress, criterion breakdown, next threshold, and high-value quests appear. |
| 2:15 | Business | Follow **Add details** for data and success criteria, then confirm again | Before/after score and actual delta appear; show the level-up notice if the prepared fixture crosses 70. Preview is shown only if P1 is built. |
| 3:00 | Business | Publish | Confirmed card enters the shared catalog at its score-based position. |
| 3:25 | Team | Open task and submit idea, plan, duration, and prototype link | Proposal appears for the business; recommendations never restrict access. |
| 4:05 | Business | Compare proposals and manually select or reject | Decision and team-visible status update; no automatic assignment. |
| 4:35 | Presenter | Show prompt/schema and one real response in README; open AI log if P1 is built | Explain malformed-output retry and deterministic fallback. |

Rehearse with a prepared fixture whose confirmed fields cross a threshold under the current `ratingRulesVersion`. Read actual scores from the running app; example values in the AI Challenge Coach reference are illustrative. Keep local Compose with `AI_MODE=stub` available as a fallback.

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
