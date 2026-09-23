# F2 — RatingPanel / Coach Component + Task Detail (Owner)

Spec: MVP_SPEC.md §8.2 (RatingPanel spec, B3 task detail), §5.3, §6.1.

## Goal
The shared, reusable RatingPanel component (used by the wizard, task detail, and catalog/team view in compact form) plus the business owner's Task Detail page.

## Files to touch
- `web/src/components/RatingPanel.tsx` — big score `x/100` + `LevelBadge`; progress bar with markers at 40/70/90; delta chip (`+23 ▲` green / `−5 ▼` red) shown only after a confirm; "Next level: X — n points needed"; breakdown table (criterion, score/weight mini-bar, reason); "Improve Your Challenge" quest list sorted by potential points desc, each with action + point ceiling + **Add details** field-shortcut callback prop; source tag (rules/seeded/cached); catalog position `#n of m`. Accept a `variant` prop: `full` | `compact` (compact drops breakdown table for the catalog/team card use).
- `web/src/components/LevelBadge.tsx` — 4 levels per §4.3 (grey/blue/green/gold badges + label text).
- `web/src/pages/business/TaskDetail.tsx` (route `/business/tasks/:id`) — tabs: **Card** (inline edit + Coach panel + Confirm & re-score + "Unconfirmed changes — catalog shows last confirmed version" banner), **Rating** (RatingPanel full + before/after comparison, optional history sparkline), **Proposals** tab is out of scope here (see F4).

## Depends on
None structurally, but F1 and F4 both consume `RatingPanel`/`LevelBadge` — build these first or coordinate interfaces early.

## Acceptance checks (manual — S12)
- Progress bar markers align to 40/70/90 regardless of score.
- Level-up notice appears only when confirmed score crosses a threshold vs. the previous confirmed score, never on a preview.
- Editing a published task's field without confirming shows the "Unconfirmed changes" banner and catalog position/rating stay the last-confirmed values (verify against catalog data returned by API, don't recompute in the frontend).
- Quests are sorted by potential points descending; clicking **Add details** scrolls/focuses the named field in the Card tab.

## Do not touch
- `web/src/api/**` (generated).
- Any score/level computation — display only what the API returns.
