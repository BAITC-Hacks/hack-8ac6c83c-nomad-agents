# I0 — Full-stack integration, README and demo handoff

**Priority: P0.** Sources: [MVP_SPEC.md](../../MVP_SPEC.md) §§10, 13–15; [TECHTASK.pdf](../../TECHTASK.pdf) §§10–11. Joint Dev A/Dev B integration task. Begin client wiring with B0/F0; final acceptance depends on B8 and F0–F4.

## Ownership

- Own `scripts/gen-client.sh`, `web/orval.config.ts`, generated-client workflow, shared frontend query invalidation, root README, and final Compose launch documentation. B8 owns API wiring; F0 owns the actor context/HTTP mutator. Coordinate those interfaces rather than implementing another client.
- Generate `web/src/api/` from running OpenAPI after contract changes; never hand-edit generated files. If generation blocks >10 minutes, the MVP §12 typed fetch fallback lives outside that directory and uses the same DTOs/actor headers.
- Invalidate task, mine, catalog, and proposal queries after relevant mutations. On actor change, partition actor-dependent query keys or clear their cache so one actor never sees another actor's cached owner view. Manually verify switches business→team→another business.
- Document architecture, rating weights/detectors/English limitation, catalog rules, identities, both seed profiles, exact launch commands, AI prompt/input/output and invalid-response handling, known P1/P2 omissions. Include a real AI example if available; label a stub truthfully and demonstrate prompt/schema/fallback when the external API is unavailable.

## Manual acceptance and deliverables

- Build API and web; run `docker compose up --build` and click the full P0 path, rather than accepting API-only checks. Use S1–S9, S11, S14, S16–S18 from MVP §14; check S18's language limitation is visible. Run optional scenarios only for delivered optional slices.
- Seed `full` and verify at least five persisted drafts/cards/teams/proposals before interaction; then restore `demo` locally for rehearsal. Check low-score visibility/proposals and manual selection of multiple or zero teams.
- Rehearse the ≤5-minute script twice, with a prepared English card whose actual confirmed score rises across a threshold, and preserve a reliable stub-mode fallback. Record actual results and remaining limitations in README; do not claim unexecuted checks passed.
- Confirm repository/archive delivery instructions, runnable local environment, README and prepared demonstration are present. No automated tests, keys or `.env` commits.

## Optional hosting (P2)

After P0/P1 are stable, own `web/netlify.toml`, `scripts/deploy-web.sh` and frontend build/base-URL instructions. Coordinate with B9's Cloud Run URL and CORS. If deployment is requested, check SPA deep links and the hosted full flow. Local Compose remains sufficient for the required runnable deliverable.
