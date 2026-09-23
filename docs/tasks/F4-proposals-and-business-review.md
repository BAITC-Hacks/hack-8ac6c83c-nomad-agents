# F4 — Proposal Form, Business Review, My Proposals

Spec: MVP_SPEC.md §8.2 (B3 Proposals tab), §8.3 (T2 form, T3), §6.5, §6.6.

## Goal
Team-side proposal submission and business-side compare/decide/milestone flow. Never cut per §12 (proposal + manual decision).

## Files to touch
- `web/src/components/ProposalForm.tsx` — fields: idea (20–2000), plan (20–3000), timeline (3–200), prototypeUrl (valid http(s) or empty); **Submit / Update proposal** → `PUT /tasks/{id}/proposals/mine`; disabled/read-only once a decision has been made (status ≠ pending); mounted inside `TaskView` (F3) on `/catalog/:id`.
- `web/src/pages/business/TaskDetail.tsx` — **Proposals** tab (extends F2's task detail): compare table (team name, tags, idea, plan, timeline, link, status); row actions **Select** / **Reject** (optional reason) / **Reset**; on a `selected` row show **Confirm milestone** (title required) → `POST /proposals/{id}/milestones`.
- `web/src/pages/team/MyProposals.tsx` (route `/team/proposals`) — list with status badges + milestone history, using `GET /proposals/mine`.

## Depends on
- F2 must own the Task Detail tab shell before this task adds the Proposals tab to it — coordinate or stub the shell first.
- F3's `TaskView` page must exist as the mount point for `ProposalForm`.

## Acceptance checks (manual — S8, S9, S10, S16)
- Invalid prototype URL shows a validation error and saves nothing.
- Selecting one proposal doesn't block others from also being selected (multiple `selected` allowed).
- Rejecting then re-selecting, then confirming a milestone: team points update by exactly +10 on the leaderboard, decision becomes locked (no further transitions), and double-clicking "Confirm milestone" does not award twice.
- Team's own proposal editing is disabled once status leaves `pending`.

## Do not touch
- `web/src/api/**` (generated).
- Decision/points logic — server-side only; this task renders `ProposalDto[]` and calls the decision/milestone endpoints.
