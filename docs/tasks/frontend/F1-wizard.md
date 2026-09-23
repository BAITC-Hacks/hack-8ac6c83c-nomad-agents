# F1 — New Task Wizard (Steps 1–5)

Spec: MVP_SPEC.md §8.2 (B2), §5.2, §6.1, §6.2.

## Goal
The business-side draft → analyze → clarify → card → confirm → publish flow. This is the primary demo path — never cut per §12 cut list.

## Files to touch
- `web/src/pages/business/NewTaskWizard.tsx` (route `/business/tasks/new` → `/business/tasks/:id/wizard`)
- `web/src/components/wizard/Stepper.tsx` — 5-step header (Draft → Clarify → Card → Rating → Publish)
- `web/src/components/wizard/DraftStep.tsx` — textarea (20–4000 chars), industry select, **Analyze** button → `POST /tasks` then `POST /tasks/{id}/analyze`; loading state "Analyzing your description…"
- `web/src/components/wizard/ClarifyStep.tsx` — Coach panel: 3–7 question cards, each with question text, field label, up to 4 chips (click inserts editable text) + free-text answer box; separate suggestions list; stub banner "AI unavailable — basic questions; you can continue editing." when `source==='stub'`; **Apply answers** → `PUT /tasks/{id}/answers`
- `web/src/components/wizard/CardStep.tsx` — two-column: editable form (all §4.1 fields incl. tag inputs for `topics`/`techTags`) left, Coach/RatingPanel preview right; draft extracts shown with evidence + explicit **Use** button (never auto-inserted, `PUT /tasks/{id}/fields` on use); quest **Add details** focuses the named field; **Confirm & score** → `POST /tasks/{id}/confirm`
- `web/src/components/wizard/RatingStep.tsx` — previous vs current confirmed score, delta, before/after level, level-up notice only on threshold cross; **Improve** (back to Card step) / **Publish** (`POST /tasks/{id}/publish`)
- `web/src/components/wizard/PublishedStep.tsx` — confirmation message, catalog position, link to task detail

## Depends on
- F2 (RatingPanel component) — reuse it inside CardStep/RatingStep rather than duplicating.
- Generated client for `/tasks`, `/tasks/{id}/analyze`, `/tasks/{id}/answers`, `/tasks/{id}/fields`, `/tasks/{id}/confirm`, `/tasks/{id}/publish`.

## Acceptance checks (manual — S3, S4, S5, S7)
- Weak draft → Analyze produces 3–7 questions, ≤4 chips each.
- Selecting a chip inserts editable text into the answer box (not read-only).
- Apply answers fills the correct card fields; user-written text vs AI-derived text is visually distinguishable (provenance).
- Confirm shows score/breakdown/quests; Publish only enabled once title is non-empty and rating exists.
- Preview (unconfirmed) is clearly labeled "Preview — confirm to update your rating" and never shown as awarded.

## Do not touch
- `web/src/api/**` (generated).
- Rating math — all scoring is server-side; this task only renders what the API returns.
