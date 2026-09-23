# F6 — AI Logs Page

**Priority: P1 — after the P0 flow.** Source: [MVP_SPEC.md](../../../MVP_SPEC.md) §§2, 5.4, 8.4.

Spec: MVP_SPEC.md §8.4, §5.4. Cut-list item #2 (keep the raw response/README fallback if cut).

## Goal
Provides a viewer for the prompt, input/output format, and invalid handling. P0 persists logs and documents a prompt/schema, one real response, and invalid-response handling in the README even when this viewer is deferred.

## Files to touch
- `web/src/pages/AiLogs.tsx` (route `/ai-logs`) — table: time, kind, task, model, validation result, latency, from `GET /ai-logs?taskId=`; expandable row reveals system prompt, input JSON, raw output.

## Depends on
- B2's persisted attempt/fallback logs, B10's P1 log read endpoint and generated contract, and F0's actor context/navigation. I0 owns client generation. Do not treat an empty placeholder endpoint as a working viewer.

## Acceptance checks (manual — S13, S14)
- A successful analyze call and a forced-invalid-then-retry-then-stub call both appear with correct `validation` values (`ok` / `retry-ok` / `fallback-stub`) and visible errors array.
- Expanding a row shows readable (pretty-printed) prompt/input/output, not raw escaped JSON.

## Do not touch
- `web/src/api/**` (generated).
