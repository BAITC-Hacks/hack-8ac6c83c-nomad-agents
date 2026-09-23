# Analysis integration (B2 → B4)

Inject `AnalysisService` and call `AnalyzeAsync(task, cancellationToken)` with the captured task revision. It returns `AnalysisDto`; expose that response type on B4's analyze endpoint for OpenAPI. `ToDomain()` creates the `TaskAnalysis` for revision-safe repository persistence. Analysis does not mutate tasks or apply answers. Titles, chips, and improvement actions require business review; extracted values alone are copied from actual draft substrings.

Set `AI_MODE=stub` for deterministic offline operation (Compose defaults to stub). For live operation set `AI_MODE=live`, `OPENAI_MODEL`, and `OPENAI_API_KEY` in the process environment or local Compose `.env`. Missing model/key configuration also falls back safely. No model is selected implicitly.

The total live analysis budget is 20 seconds, including at most one retry. Every completed attempt records the prompt, input, raw response, validation outcome and latency through `IAiLogRepository`; fallback gets a separate `fallback-stub` entry. Invalid attempts use `invalid`, successful retries use `retry-ok`. Soft validation fixes appear in the log errors even when the result is accepted. Configured API key text is redacted from persisted logs.

Fallback treats empty/short fields and explicit uncertainty as weak; this is a clarification heuristic, not a readiness score. It does not infer structured facts from the raw draft. Nearly complete cards still get three specificity/confirmation questions.

Manual checks once B4 is available: analyze a weak draft; analyze a populated card; check that invented/paraphrased extractions are dropped; check malformed provider output retries then falls back; verify stub mode makes no network requests. Review per-attempt logs for these flows. No automated tests are added under the repository's hackathon rules.

This checkout does not yet contain `scripts/gen-client.sh` or the B4 endpoint. B4 must regenerate the client after exposing the typed endpoint; B2 introduces no HTTP route or frontend changes.

Responses request format reference: https://developers.openai.com/api/docs/guides/structured-outputs
