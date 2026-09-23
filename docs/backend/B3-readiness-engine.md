# B3 — Deterministic readiness engine and quests

**Priority: P0 — top priority.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 4.2–4.3, 5.3.

## Goal

Implement MVP spec §§4.2–4.3 and 5.3 as backend code. The score must depend only on confirmed card fields and a versioned ruleset, never on an AI response. B4 calls this service during confirmation; B7 uses it while seeding.

## Starting point and files

`api/Domain/Rating.cs` has levels but stale `ai|stub` score sources and no `matchedSignals`/quests/rules version. `api/Features/Rating/` contains DTOs and an empty endpoint mapper. Add `RatingService` and rule helpers there; coordinate any domain shape changes with B1. No standalone score endpoint is needed.

## Version `en-mvp-1` rules

Normalize Unicode whitespace/case; fewer than three non-space characters counts as empty. Award each criterion `0`, `floor(weight/2)`, or full weight. Use conservative, explicit signal detectors and record their IDs in `matchedSignals`. Initial detector vocabulary only needs to cover the prepared English demo; unsupported wording remains half credit with a clear reason, not an AI guess. Keep terms/patterns centralized and version them when changed.

| Criterion | 0 | Half | Full evidence (all stated signals required) |
|---|---|---|---|
| Context + need / 20 | Both empty | One field present | Both `context` and `need` present |
| Data / 20 | Empty | Nonempty | Named source/example (`logs`, `database`, `files`, `API`, `export`, etc.) **and** format/quantity/access marker (`CSV`, `PDF`, count + unit, URL, read-only access, etc.) |
| Expected result / 15 | Empty | Nonempty | Deliverable term (`dashboard`, `report`, `model`, `prototype`, `API`, `app`, etc.) **and** function/action (`predict`, `classify`, `show`, `generate`, `track`, etc.) |
| Success criteria / 15 | Empty | Nonempty | Outcome/measure term **and** numeric target with unit/comparison or explicit pass/fail acceptance condition; a bare number is insufficient |
| Constraints / 10 | Empty | One recognized boundary | Two distinct categories among time, technology, access, legal, budget; e.g. `6 weeks` plus `read-only` |
| Users / 10 | Empty | User group named | Group plus count, role, or usage situation; e.g. `12 dispatchers` |
| Business connection / 10 | Both empty | Contact or interaction present | Contact plus consultation channel/cadence **and** feedback procedure |

Map each missing signal to a plain-language `reason` and quest. Return seven ordered breakdown rows, `total`, level (0–39 draft, 40–69 workable, 70–89 ready, 90–100 priority), next threshold, and quests ordered by `potentialPoints = weight - score`. Quest points are ceilings, not guarantees. Cache by canonical confirmed fields plus `ratingRulesVersion = en-mvp-1`; identical inputs yield identical results in live and stub AI modes. A repeated confirmation may reuse the result without another history entry. The P0 wizard must support a second confirm and show actual score/level delta; a level-up notice appears only when a threshold is crossed.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. Manually calculate a blank card (0), a partially filled card, and the Nomad/Steppe seed cards; compare each of seven rows and the sum with the service output. Verify a changed field changes only relevant criteria, score never exceeds 100, a bare digit/long filler does not earn full credit, and identical fields score identically in both AI modes.

## Boundary

Do not call OpenAI, write task snapshots, expose a score endpoint, or edit frontend code. B3 owns rules, score DTO mapping, and quest generation; B4 owns confirmation/history persistence.
