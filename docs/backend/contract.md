# API contract decisions

All application routes use the `/api` prefix and JSON. `GET /api/health`, `GET /api/actors`, development OpenAPI/Swagger routes, CORS preflight, and separately guarded `/api/admin/*` operations are public to the actor resolver. Every other route requires `X-Actor-Role: business|team` and `X-Actor-Id`.

Invalid or missing actor headers return `400` ValidationProblem. An unknown actor ID returns `404`. A known actor paired with the wrong role, or a known actor without ownership, returns `403`. Stale revisions and illegal state transitions return `409`. Normal validation and access outcomes do not use exceptions.

The mandatory route shapes follow `MVP_SPEC.md` §7:

- Tasks: create, mine, read, analyze, apply answers, update fields, confirm, and publish.
- Catalog: `GET /api/catalog` and `GET /api/catalog/topics`. Multi-select filters use repeated values, for example `?topic=retail&topic=logistics&level=workable&level=ready`. Topic values within the group are ORed, level values within the group are ORed, and the two groups are combined with AND.
- Proposals: team upsert at `/api/tasks/{taskId}/proposals/mine`, owning-business list, team mine list, and manual decision.
- `GET /api/tasks/{id}/rating-preview` remains P1. There is no standalone AI score route.
- Optional routes are mapped only when implemented; they never return placeholder success responses.

Teams always receive the last confirmed task view. Business draft/edit operations require the owning business. Actor identity is a demo correctness mechanism, not authentication.
