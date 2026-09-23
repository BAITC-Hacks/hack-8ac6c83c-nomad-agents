# F4 — Proposal Form, Business Review, My Proposals

**Priority: P0 — top priority for proposal Submit / Update and business Select / Reject. P1 adds Reset; P2 adds milestones and points.** Source: [MVP_SPEC.md](../../../MVP_SPEC.md) §§2, 6.5–6.6, 8.2–8.3.

Spec: MVP_SPEC.md §8.2 (B3 Proposals tab), §8.3 (T2 form, T3), §6.5, §6.6.

## Goal
P0 covers team proposal submission and business comparison/manual decision. Decision Reset is P1; milestone confirmation is P2.

## Files to touch
- `web/src/components/ProposalForm.tsx` — fields: idea (20–2000), plan (20–3000), timeline (3–200), prototypeUrl (valid http(s) or empty); **Submit / Update proposal** → `PUT /tasks/{id}/proposals/mine`; disabled/read-only once a decision has been made (status ≠ pending); mounted inside `TaskView` (F3) on `/catalog/:id`.
- `web/src/pages/business/TaskDetail.tsx` — P0 **Proposals** tab (extends F2's task detail): compare table (team name, tags, idea, plan, timeline, link, status); row actions **Select** / **Reject** (optional reason). P1 adds **Reset** before any milestone. P2 adds **Confirm milestone** (title required) on a selected row → `POST /proposals/{id}/milestones`.
- `web/src/pages/team/MyProposals.tsx` (route `/team/proposals`) — P0 list with status badges, using `GET /proposals/mine`; P2 adds milestone history.

## Depends on
- F2 must own the Task Detail tab shell before this task adds the Proposals tab to it — coordinate or stub the shell first.
- F3's `TaskView` page must exist as the mount point for `ProposalForm`.

## Acceptance checks (manual — P0: S7–S9; P2: S10)
- Invalid prototype URL shows a validation error and saves nothing.
- Selecting one proposal doesn't block others from also being selected (multiple `selected` allowed).
- P0: owning business can compare and manually Select / Reject; no team is auto-selected. P1: Reset is available before a milestone. P2: confirming a milestone adds exactly +10 team points, locks the decision, and double-clicking does not award twice.
- Team's own proposal editing is disabled once status leaves `pending`.

## Do not touch
- `web/src/api/**` (generated).
- Decision/points logic — server-side only; this task renders `ProposalDto[]` and calls the decision/milestone endpoints.
