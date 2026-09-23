# F3 — Catalog, Task View (Team), Recommendations

**Priority: P0 — top priority for My Tasks, shared catalog, topic + level filters, and confirmed team view. P1 adds recommendations.** Source: [MVP_SPEC.md](../../../MVP_SPEC.md) §§2, 6.3–6.4, 8.2–8.3.

Spec: MVP_SPEC.md §8.2 (B1 My Tasks), §8.3 (T1, T2), §6.3, §6.4.

## Goal
The shared catalog list and team-facing read-only task view are P0. Add the recommendations strip only after the P0 journey works.

## Files to touch
- `web/src/pages/business/MyTasks.tsx` (route `/business/tasks`) — table: title, status, score + `LevelBadge`, catalog position `#n/m`, proposals count, "Open" link; **New task** button → wizard.
- `web/src/pages/Catalog.tsx` (route `/catalog`) — P0 filters: Topic multi-select (from `GET /catalog/topics`) and Level (4 checkboxes), combinable via repeated query values in B5's contract; "Showing n of m" count; card list: unfiltered position `#n of m`, title, short confirmed-card summary, business name, `LevelBadge`, score bar (compact `RatingPanel`), topics, tech tags, proposals count, **View details** CTA; priority → gold border + ★; draft level → "Needs clarification" label. P1: "Recommended for {team}" strip on top (team role only, ≤3 cards, `GET /teams/{id}/recommendations`, show matched tags).
- `web/src/pages/team/TaskView.tsx` (route `/catalog/:id`) — read-only confirmed card with expected result/success criteria/constraints prominent; compact RatingPanel; existing proposal status shown and edits locked after a decision (proposal form itself lives in F4).

## Depends on
- F0 actor context/client, B4 task/mine reads and B5 catalog. Recommendations depend on B10's P1 slice.
- F2's `RatingPanel` (`compact` variant) and `LevelBadge`.
- P1 recommendations never filter the P0 catalog — the full catalog always renders below the strip regardless of matches.

## Acceptance checks (manual — S1, S2, S7)
- Seeded demo: Nomad (Priority) ranks above Steppe (Workable) with correct `#n of m`.
- Level filter to Workable shows only Steppe but preserves its unfiltered `#n of m` position; topic and level together narrow the visible list without renumbering positions.
- P1: recommendations strip shows only for team role, ≤3 items, with matched-tag explanation text.
- Draft-level tasks are visible in catalog (not hidden), badge reads "Needs clarification".

## Do not touch
- `web/src/api/**` (generated).
- Sorting/position/recommendation logic — all computed server-side; this task only renders `CatalogItemDto[]`.
