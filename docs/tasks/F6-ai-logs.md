# F6 — AI Logs Page

Spec: MVP_SPEC.md §8.4, §5.4. Cut-list item #2 (keep the raw response/README fallback if cut).

## Goal
Satisfies SoW §5 "show prompt, input/output format, invalid handling" requirement for the demo.

## Files to touch
- `web/src/pages/AiLogs.tsx` (route `/ai-logs`) — table: time, kind, task, model, validation result, latency, from `GET /ai-logs?taskId=`; expandable row reveals system prompt, input JSON, raw output.

## Depends on
None.

## Acceptance checks (manual — S13, S14)
- A successful analyze call and a forced-invalid-then-retry-then-stub call both appear with correct `validation` values (`ok` / `retry-ok` / `fallback-stub`) and visible errors array.
- Expanding a row shows readable (pretty-printed) prompt/input/output, not raw escaped JSON.

## Do not touch
- `web/src/api/**` (generated).
