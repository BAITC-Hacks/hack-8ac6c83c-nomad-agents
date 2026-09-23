# Task breakdown review — 2026-09-23

Sources: [TECHTASK.pdf](../TECHTASK.pdf) and [MVP_SPEC.md](../MVP_SPEC.md). Working queues: [backend](backend/README.md), [frontend](tasks/frontend/README.md). This review checks requirements and ownership; it does not verify implementation readiness.

## Findings

The main P0 journey is covered by B00–B8 and F0–F4. Before this review, some P1/P2 backend features, final SPA integration, README delivery, and rehearsals had no explicit owners. Added [B10](backend/B10-optional-features.md) and [I0](tasks/I0-integration-and-delivery.md). Clarified B3 rules and frontend dependencies.

## Requirements coverage

| TECHTASK requirement | Backend | Frontend / delivery | MVP priority |
|---|---|---|---|
| Draft, at least 3 questions, editable card, confirmation (§§2–3) | B2, B4 | F1 | P0 |
| All card fields (§3) | B1, B4 | F1, F2, F3 | P0 |
| Score 0–100, explanation, missing details, recalculation (§4) | B3, B4 | F1, F2 | P0; post-publication editing P1 |
| Open catalog, sorting, topic and level filters; low scores do not block proposals (§§3–4) | B5, B6 | F3, F4 | P0 |
| Proposals and manual selection of zero, one, or several teams (§§1–3) | B6 | F4 | P0 |
| AI without invented facts, prompt/schema and invalid-response handling (§5) | B2 | F1, I0; F6 viewer | P0; viewer P1 |
| 5 drafts, cards, teams, and proposals (§6) | B7 | I0 | P0 |
| Team points for a confirmed milestone (§2, step 8) | B10 | F4, F5 | P2 — coverage conflict with TECHTASK |
| Launch instructions, README, demo within 5 minutes (§§10–11) | B00, B8 | I0 | P0 |
| Recommendations that do not restrict the catalog (§5) | B10 | F3 | P1, may be deferred |

## Discrepancies and decisions

1. **Milestone points.** TECHTASK §2 includes a result with points, but its mandatory demo in §11 ends with the business decision. MVP §§2, 12 explicitly allows cutting milestones/leaderboard. P2 is preserved; P0 must not be described as complete coverage of TECHTASK. Full coverage requires the B10/F4 milestone feature; the PDF does not require a separate leaderboard.
2. **B3 scoring.** Corrected the erroneous full-credit rule for user count without a usage scenario; clarified that `export` does not replace a data source. MVP §5.3 defines the exact detectors. S18 is included in manual acceptance. Seed examples must be checked against their actual fields rather than adjusted to force expected scores.
3. **Unassigned work.** B5 excluded recommendations, B6 excluded milestones/leaderboard, and B8 excluded all P1/P2 work, while F3/F5/F6 expected the corresponding APIs. B10 now assigns this work, including the AI-log read endpoint, preview, and Reset.
4. **Final integration.** B8 checked only the API and excluded frontend work. I0 owns client generation, combined UI/API verification, README, full seed checks, and two rehearsals; Netlify is optional.
5. **Identifiers and paths.** MVP §§11–12 describes the older A1–A5/B1–B5 breakdown; current B identifiers refer to backend work and F identifiers to frontend work. The mapping is below. AGENTS.md references a nonexistent `docs/MVP_SPEC.md`; the actual source is at the repository root. The historical scaffold plan is not the current queue.
6. **MVP ambiguities for B0.** GET preview reads persisted editable fields, so the UI must first call PUT /fields; that GET does not accept purely local unsaved fields. A milestone body containing only `{title}` cannot distinguish a retry from a new milestone: agree on a stable ID/idempotency key, update the contract, and generate the client before implementation. Recommendations in §6.4 use interests/techTags; skills are stored in profiles but have no separate formula weight. Do not invent one from the general description in §2.

## Mapping the old plan to current tasks

| MVP §§11–12 | Current queue |
|---|---|
| 00 scaffold | B00, B0, F0 |
| A1/A2 OpenAI/analyze | B2 |
| A3 score | B3, B4 (confirm/history), B10 (preview) |
| A4 wizard | F1 |
| A5 rating/logs | F2, F6, B10 (logs endpoint) |
| B1 repos/seed | B1, B7 |
| B2 CRUD | B4 |
| B3 catalog | B5, F3, B10 (recommendations) |
| B4 proposals | B6, F4, B10 (milestones), F5 |
| B5 deploy | B9, I0 (Netlify) |

For two developers, preserve the split from MVP §12: Dev A owns B2/B3/F1/F2, then F6; Dev B owns B00/B0/B1/B4/B5/B6/B7/F0/F3/F4; B8/I0 is joint integration work. These are ownership boundaries, not a requirement to complete every task sequentially. Start the B0 contract before working B1 repositories; verify the actor guard after B1/B7. The B2 stub and pure B3 rules can be prepared against agreed DTOs, with persistence connected later. Prepare B7 early for frontend work. Generate the client when the contract changes rather than waiting for final B8 integration. P1/P2 must not delay P0.
