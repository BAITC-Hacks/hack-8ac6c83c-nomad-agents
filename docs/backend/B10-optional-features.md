# B10 — Explicit backend ownership for P1/P2

Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 6–8. Begin only after the P0 integration gate. This brief closes ownership gaps; it does not promote optional scope into P0. See [audit](../README.md) for the milestone requirement conflict.

## P1 slices

- **Recommendations (B5 extension):** own `Features/Catalog` service/DTOs and the team recommendation route. Depends on B1/B5/B6; consumed by F3. Implement §6.4 exactly: published, score ≥40, no proposal from this team, case-insensitive interest/technology hits, top 3 with matched tags. No sensitive attributes or catalog restriction. Skills remain profile data; do not invent a new weight. Manually check no-match, existing proposal, low score, and another team's identity.
- **Published editing / preview (B4 extension):** depends on B3/B4/B5; consumed by F2. Preserve the last confirmed catalog view until reconfirmation, show history (last 10) and actual delta. GET preview scores current persisted editable fields after PUT /fields, without writing awarded rating/history or changing rank. Check S12/S15 and verify preview alone has no catalog effect.
- **Decision Reset (B6 extension):** depends on B6; consumed by F4. Add `pending` reset and enforce the spec transitions, blocking all decision changes after any milestone. Check selected→pending, rejected→pending and locked decision.
- **AI logs read endpoint (B2 extension):** depends on B0/B1/B2; consumed by F6. Own `Features/Ai/Endpoints.cs` and log DTOs. GET `/api/ai-logs?taskId=` returns last 20 entries with prompt, input, raw output, validation/errors and latency, through repositories and actor guard. No secrets. Check S13/S14; P0 logging itself remains B2.

## P2 slice

**Milestones and leaderboard (B6 extension):** depends on B1/B6; consumed by F4/F5. Own proposal service/endpoints and repository transaction extension. Only the owning business can confirm a titled milestone on a selected proposal. Use a stable milestone ID/idempotency key agreed in B0; atomically create once and add exactly 10 team points. Lock decisions after confirmation. Expose GET `/api/leaderboard`, ordered by points descending. Manually retry the same milestone concurrently, verify a single award, reject wrong actor/non-selected proposal, and check team totals. Do not feed these points into task rating or automatically select teams.

## Contract and boundaries

Each slice updates OpenAPI and triggers client regeneration through I0 before the corresponding UI is enabled. Build the API and run the slice's manual checks in one local API process, including restart behavior where relevant. No automated tests. Coordinate domain/repository changes with B1, and do not independently rewrite B2–B6 or frontend files.
