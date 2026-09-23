# F3 — Catalog, Task View (Team), Recommendations

Spec: MVP_SPEC.md §8.2 (B1 My Tasks), §8.3 (T1, T2), §6.3, §6.4.

## Goal
The shared catalog list, the team-facing read-only task view, and the recommendations strip. Never cut per §12.

## Files to touch
- `web/src/pages/business/MyTasks.tsx` (route `/business/tasks`) — table: title, status, score + `LevelBadge`, catalog position `#n/m`, proposals count, "Open" link; **New task** button → wizard.
- `web/src/pages/Catalog.tsx` (route `/catalog`) — Filters: Topic (multi-select, from `GET /catalog/topics`), Level (4 checkboxes); "Showing n of m" count; card list: position #, title, short confirmed-card summary, business name, `LevelBadge`, score bar (compact `RatingPanel`), topics, tech tags, proposals count, **View details** CTA; priority → gold border + ★; draft level → "Needs clarification" label. "Recommended for {team}" strip on top (team role only, ≤3 cards, `GET /teams/{id}/recommendations`, show matched tags).
- `web/src/pages/team/TaskView.tsx` (route `/catalog/:id`) — read-only confirmed card with expected result/success criteria/constraints prominent; compact RatingPanel; existing proposal status shown and edits locked after a decision (proposal form itself lives in F4).

## Depends on
- F2's `RatingPanel` (`compact` variant) and `LevelBadge`.
- Recommendations never filter the catalog — the full catalog always renders below the strip regardless of matches.

## Acceptance checks (manual — S1, S2, S7)
- Seeded demo: Nomad (Priority) ranks above Steppe (Workable) with correct `#n of m`.
- Level filter to Workable shows only Steppe, with position recalculated against the filtered subset per spec (`#1 of 1`) while badge position stays true to the unfiltered rank shown elsewhere — confirm exact behavior against §6.3 ("Position: 1-based index in the unfiltered sorted list") before filtering UI.
- Recommendations strip shows only for team role, ≤3 items, with matched-tag explanation text.
- Draft-level tasks are visible in catalog (not hidden), badge reads "Needs clarification".

## Do not touch
- `web/src/api/**` (generated).
- Sorting/position/recommendation logic — all computed server-side; this task only renders `CatalogItemDto[]`.
