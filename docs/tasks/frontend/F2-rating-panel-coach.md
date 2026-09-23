# F2 — RatingPanel / Coach Component + Task Detail (Owner)

**Priority: P0 — top priority for RatingPanel, quests, breakdown, level-up, and confirmed owner detail. P1 adds post-publish editing/history.** Source: [MVP_SPEC.md](../../../MVP_SPEC.md) §§2, 5.3, 8.2.

Spec: MVP_SPEC.md §8.2 (RatingPanel spec, B3 task detail), §5.3, §6.1.

## Goal
The shared, reusable RatingPanel component (used by the wizard, task detail, and catalog/team view in compact form) plus the business owner's Task Detail page.

## Files to touch
- `web/src/components/RatingPanel.tsx` — big score `x/100` + `LevelBadge`; progress bar with markers at 40/70/90; delta chip (`+23 ▲` green / `−5 ▼` red) shown only after a confirm; "Next level: X — n points needed"; breakdown table (criterion, score/weight mini-bar, reason); "Improve Your Challenge" quest list sorted by potential points desc, each with action + point ceiling + **Add details** field-shortcut callback prop; source tag (rules/seeded/cached); catalog position `#n of m`. Accept a `variant` prop: `full` | `compact` (compact drops breakdown table for the catalog/team card use).
- `web/src/components/LevelBadge.tsx` — 4 levels per §4.3 (grey/blue/green/gold badges + label text).
- `web/src/pages/business/TaskDetail.tsx` (route `/business/tasks/:id`) — P0 tabs: confirmed **Card**, full **Rating** with breakdown, and a **Proposals** tab provided by F4. P1 adds inline post-publish edits, Confirm & re-score, "Unconfirmed changes — catalog shows last confirmed version" banner, and before/after history. P2 adds the history sparkline.

## Depends on
None structurally, but F1 and F4 both consume `RatingPanel`/`LevelBadge` — build these first or coordinate interfaces early.

## Acceptance checks (manual — S4, S5; P1: S12)
- Progress bar markers align to 40/70/90 regardless of score.
- Level-up notice appears only when confirmed score crosses a threshold vs. the previous confirmed score, never on a preview.
- P0: two initial-wizard confirmations show the actual before/after score, delta, breakdown, quests, and a level-up notice only on a crossed threshold.
- P1: editing a published task's field without confirming shows the "Unconfirmed changes" banner and catalog position/rating stay the last-confirmed values (verify against catalog data returned by API).
- Quests are sorted by potential points descending; clicking **Add details** scrolls/focuses the named field in the Card tab.

## Do not touch
- `web/src/api/**` (generated).
- Any score/level computation — display only what the API returns.
