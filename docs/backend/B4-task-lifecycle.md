# B4 — Task creation, answers, confirmation, publication

**Priority: P0 — top priority.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 6.1–6.2, 7.

## Goal

Replace the placeholder task endpoints with the complete business task journey: draft → analyze → editable card → confirmed rating → publish. Use B0 actor checks, B1 repositories, B2 analysis, and B3 rating.

## Starting point and files

`api/Features/Tasks/Endpoints.cs` currently returns invented IDs/empty objects and stores nothing. `Dtos.cs` does not expose the rating, analysis, confirmed view, revision, or unconfirmed-change state needed by the UI. Own these two files and a `TaskService` in this feature folder. Coordinate `TaskCard` additions with B1.

## Implement

- `POST /api/tasks`: business only; validate raw draft length 20–4000 and industry; persist an editing task with server-generated ID/owner/revision. `GET /api/tasks/mine` returns only that business's tasks; `GET /api/tasks/{id}` returns owner editing state or the last confirmed published view to a team.
- `POST /api/tasks/{id}/analyze`: owner only; call B2, persist validated analysis, return `AnalysisDto`. Never auto-copy AI title/extracts into confirmed fields.
- `PUT /api/tasks/{id}/answers`: accept one atomic `{ answers:[{questionId,fieldKey,text}] }` submission, validate IDs and field keys against the stored analysis, join nonempty answers in question order per field, and append once to existing editable text. In the same write set `answersApplied=true`, store a canonical payload hash, and increment revision. An identical retry returns the saved task without changing fields/revision; a different later submission returns `409` and directs editing to `PUT /fields`. `PUT /fields` accepts typed partial edits (including `topics[]` and `techTags[]`; the current `Dictionary<string,string>` request cannot represent them), enforces §4.1 limits, and allows explicit acceptance of grounded draft extracts. Increment revision and set `hasUnconfirmedChanges` on each effective edit.
- `POST /api/tasks/{id}/confirm`: snapshot one revision, calculate B3 score, and commit fields, hash, rating, and history atomically only if the revision still matches. Return task, rating, delta, and position. Return `409` for stale revisions. Reconfirming unchanged fields returns cached rating without adding history.
- `POST /api/tasks/{id}/publish`: require owner, nonempty confirmed title, rating, and no unconfirmed changes. Low-score tasks remain publishable. Editing a published task leaves the old confirmed card/rating visible until the next confirm.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. With seeded in-memory actors, manually create a weak draft, analyze, apply answers, retry the identical payload, and submit a different payload. Verify the retry leaves fields/revision unchanged and the changed payload returns `409`. Edit the card, confirm, improve it in the initial wizard, confirm again, and inspect previous/current score, delta, breakdown, quests, and level-up only if a threshold crossed. Publish, then check owner access and the catalog-safe confirmed view. A concurrent edit must cause stale confirm to return `409`.

## Boundary

Do not implement AI logic, rating rules, catalog sorting, proposals, or frontend UI here. B4 owns task HTTP behavior and task state transitions. Post-publish editing and rating preview are P1; preserve the model and route contract for them without delaying P0.
