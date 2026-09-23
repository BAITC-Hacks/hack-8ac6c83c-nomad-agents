# TaskForge MVP TODO

Source of truth: [`MVP_SPEC.md`](../MVP_SPEC.md). This is the single implementation checklist for the gaps found in the Playwright review of `http://localhost:5173/` on 2026-09-23. A visible control in the browser-only **Sample workspace** is not a completed MVP feature.

## Current state

- The .NET API maps only `/api/health`; `/api/actors` returns 404, so the frontend switches to `web/src/lib/demo.ts`.
- The sample workspace provides clickable wizard, catalog, proposal, review, logs, and leaderboard screens, but stores changes only in the browser tab. Reloading clears its actor and changes.
- Sample analysis uses fixed questions. Sample scoring uses `ratingRulesVersion: "sample-only"` and weights that differ from §4.2 of the spec.
- `seed/demo/` and `seed/full/` contain no fixtures. The generated client and generation script are absent.

## P0 — mandatory MVP

### 1. API contract, storage, and identity

- [ ] Map the P0 routes in `MVP_SPEC.md` §7: actors; tasks and wizard mutations; catalog and topics; proposals and decisions; admin seed/reset. Keep DTOs and OpenAPI names consistent with the frontend.
- [ ] Implement repositories over the singleton `InMemoryDataStore` for businesses, teams, tasks, proposals, ratings, and AI logs. All authoritative application state must remain in that process-local store.
- [ ] Return the businesses and teams from `GET /api/actors`; validate `X-Actor-Role` and `X-Actor-Id`, actor existence, ownership, and team-only/business-only operations on protected routes.
- [ ] Return validation failures as `Results.ValidationProblem` and conflicts as appropriate ProblemDetails responses. Protect or disable destructive admin routes outside local development.
- [ ] Connect the role switcher to real actors and remove the automatic browser-sample fallback from the MVP path once the API is ready. Keep any sample mode explicitly separate and labelled.

### 2. AI Challenge Coach and clarification

- [ ] Implement `POST /api/tasks/{id}/analyze` through the OpenAI Responses API with the strict schema, prompt, and field catalog in §5.2. Only the business owner may call it.
- [ ] Validate field keys, 3–7 questions, at most four chips per question, suggestions, and extracted values against exact draft evidence. Never put AI suggestions into card fields without a user action.
- [ ] On an invalid response or call failure, retry once, then return the local question stub. Record each attempt and fallback in AI logs; show the correct AI/basic badge in the wizard.
- [ ] Implement one atomic `PUT /answers` per task. Match question IDs and field keys; merge submitted answers into editable fields once; treat an identical retry as a no-op and a changed retry as a conflict directing the user to the card editor.
- [ ] Wire the existing wizard to these endpoints. Verify chips, free text, suggestions, extract evidence with **Use**, read-only applied answers, loading, and errors against live API responses.

### 3. Card lifecycle and readiness engine

- [ ] Implement draft creation, field updates, confirm, and publish with the lifecycle rules in §6.1. Publishing requires a title, a confirmed rating, and no unconfirmed changes; low scores must still be publishable.
- [ ] Implement the versioned, deterministic backend rubric in §§4.2 and 5.3: context/need 20; data/materials 20; expected result 15; success criteria 15; constraints 10; users 10; business connection 10. Score only confirmed fields; AI must never award points.
- [ ] Return criterion scores, weights, evidence/reasons, missing details, ordered quests with point ceilings, level, next threshold, rules version, and cache/source metadata. Identical confirmed fields under one rules version must yield the same result.
- [ ] Make confirmation revision-safe and cache repeated confirmations; do not append rating history for an unchanged confirmation. Expose confirmed score changes and previous/current levels to the wizard.
- [ ] Wire the RatingPanel and Coach to backend ratings, including progress markers, breakdown, missing details, next level, quest **Add details** field focus, confirmed delta, and genuine level-up notice. Remove sample scores from the authoritative path.
- [ ] Prepare an English wizard fixture whose first and second confirmations actually produce a score increase across a level threshold. Playwright's sample run stayed at **70 → 70** after adding data and success criteria, so this required demo result is not yet verified.

### 4. Shared catalog and business pages

- [ ] Implement `GET /api/catalog` and topics from published **confirmed** snapshots. Sort by rating descending, confirmed time descending, then task ID; compute `#n of m` before filters.
- [ ] Support topic and multi-level filters, including visible draft-level cards and proposals at every level. Return score, level, position, business, tags, and proposal count; retain priority highlighting.
- [ ] Wire My Tasks, owner detail, team task view, and catalog to the real API. Show confirmed card content to teams and keep unconfirmed owner edits out of the catalog.
- [ ] Fix internal links that currently trigger a full page reload. In Playwright, opening a catalog task through its ordinary link cleared the selected actor and browser-sample task; in-app navigation must preserve the active session and route.

### 5. Proposals and manual decisions

- [ ] Implement one proposal per team per published task, with idea, plan, timeline, and optional valid HTTP(S) prototype URL. Allow edits only while Pending and allow any number of teams to propose at every readiness level.
- [ ] Implement owner-only proposal comparison and manual Select/Reject decisions. Permit zero or multiple selected teams; never select automatically.
- [ ] Wire proposal submission, editing, My Proposals status, and owner review controls to the API. Show field validation errors and refresh affected task, catalog, and proposal views after mutations.

### 6. Seed, client integration, and delivery

- [ ] Add `demo` and `full` seed fixtures and local admin seed/reset. `full` must load at least five drafts, five complete cards, five teams with skills, and five proposals before any user interaction. Seed ratings must be computed by the real rubric.
- [ ] Generate the typed client and hooks from the API OpenAPI document via `scripts/gen-client.sh`; configure orval and the actor-header mutator. Never hand-edit `web/src/api/`. Wire TanStack Query invalidation and separate actor-dependent cached data.
- [ ] Add the specified Tailwind and shadcn/ui frontend setup, or record an explicit approved deviation from the required stack. The current frontend uses React, Vite, and TypeScript with hand-written styles and fetch calls.
- [ ] Update README with the actual architecture, launch/seed commands, exact rubric and catalog rules, prompt/schema and invalid-response handling, both seed profiles, manual scenarios, and remaining limitations. Do not describe sample scoring as the MVP rating.
- [ ] Run `docker compose up --build`, seed `demo`, and manually verify spec scenarios S1–S9, S11, S14, and S16–S18. Seed `full` and verify its counts. Rehearse the five-minute end-to-end flow twice, including an observed confirmed score increase and level-up, proposal, and business decision.

## After P0 — optional scope from the spec

- [ ] **P1:** Rule-based team recommendations using interest/technology tag matches, with matched-tag explanations and no catalog restriction.
- [ ] **P1:** Post-publish card edit/re-score, unconfirmed banner, rating preview clearly marked unawarded, before/after history, and decision Reset before milestones.
- [ ] **P1:** Real AI log read endpoint and expandable viewer showing prompt, input, output, validation, errors, and latency. Document these even if the viewer is deferred.
- [ ] **P2:** Confirmed milestones with idempotent +10 team points, decision lock, leaderboard, and optional rating-history sparkline.
- [ ] **P2:** Cloud Run and Netlify deployment only after the local P0 flow works.

## Completion gate

The P0 checklist is complete only when the running frontend uses the .NET API for the full business → catalog → team proposal → business decision journey, with a real validated AI call or logged retry/fallback, a backend-owned spec-compliant rating, both seed profiles, and a rehearsed score increase. The browser-only Sample workspace does not satisfy this gate.
