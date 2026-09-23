# Deterministic readiness (B3)

`RatingService` implements MVP_SPEC §§4.2–4.3 and 5.3. All detector vocabulary and numeric patterns are in `ReadinessRules`, version `en-mvp-1`. It has no AI dependency. Each of seven rows earns zero, integer half, or full weight; 15-point rows earn 7 at half credit. Unrecognized nonempty wording earns half credit with the English-vocabulary limitation in its reason. Matches indicate specificity, not truth.

## B4 / B7 integration

- Inject `RatingService`. Use `Score(ConfirmedTaskSnapshot)` for confirmed input, or `ScoreFields(capturedFields)` while preparing a revision-safe confirmation or seed. Neither writes a task or history entry. B7 may mark its saved copy `Source = RatingSources.Seed`.
- B4 must atomically persist the captured fields and rating against the captured task revision. Compare the prior confirmed rating's `CacheKey` and rules version to suppress unchanged confirmation history. Do not suppress a task's first history entry just because another task populated the shared cache. Keep the last 10 entries; compare the new total/level with the preceding confirmed entry for the actual delta and upward threshold crossing.
- `Preview(fields)` calculates without caching or persisting. Label it as a preview at the calling endpoint/UI.
- `RatingDto.FromDomain(rating)` maps to the spec contract and adds `nextLevel`, omitted at 90+. The next gap is threshold minus total; it never sums quests.
- One quest per incomplete criterion targets its first unmet condition. Quests sort by `weight - score`, with rubric order breaking ties. These are point ceilings; supplying only one of several missing conditions may not award that whole amount. `missingDetails` lists all unmet conditions (up to three per criterion).

Cache entries live exclusively in `InMemoryDataStore`, accessed through `IRatingCacheRepository`; admin reset clears them with other application state. The SHA-256 key includes the version and every card field, with Unicode Form KC, invariant lowercase, whitespace normalization, and sorted/deduplicated tag sets. Cache hits preserve the original scoring timestamp and return source `cache`. Original card text is never changed. A vocabulary/pattern change requires a new rules version.

No endpoint is added in B3. B4 must expose `RatingDto` in its response and regenerate the API client when wiring confirmation. This checkout has no client-generation script yet.

## Manual verification

Build passed with zero warnings/errors. An ignored temporary console inspection invoked the service directly; no automated tests were added. Actual seed JSON is absent in this checkout, so the following are representative English cards, not verification of B7's future fixtures.

| Card | Context + need | Data | Result | Success | Constraints | Users | Connection | Total / level |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| Blank | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 / draft |
| Partial | 10 | 10 | 0 | 0 | 0 | 5 | 0 | 25 / draft |
| Steppe representative | 20 | 10 | 7 | 0 | 0 | 5 | 5 | 47 / workable |
| Nomad representative | 20 | 20 | 15 | 15 | 10 | 10 | 10 | 100 / priority |

Partial: context `We dispatch manually`, data `We have reviews`, users `12 dispatchers`, other fields blank.

Steppe: context `Reviews arrive through multiple channels`, need `Understand customer complaints`, data `We have reviews`, expected result `An analysis tool`, users `Customers`, contact `Retail owner`, other fields blank.

Nomad: context `Manual dispatching causes 18% late deliveries`, need `Predict delivery delays`, users `12 dispatchers monitor delays`, data `2 years of CSV route logs, 400k rows and weather API`, constraints `6 weeks, use React and Python, read-only DB replica`, result `Web dashboard to display a model to predict delays`, success `Predict delays with at least 75% precision on holdout month`, contact `Nomad operations manager`, interaction `Weekly 30-minute call and Slack feedback`. The short seed description in the spec must be expanded into actual field text with the required action/usage evidence; group counts and artifact names alone do not earn full credit.

Also inspected: bare `export`, URL, API, CSV and `400k rows` stay at half data credit; `CSV route logs` and `logs export` get full. `accuracy 75` and `accuracy 75%` remain 7 while `accuracy at least 75%` gets 15. Two deadlines count once; `6 weeks and read-only` counts twice. A group alone or action in an unrelated sentence gets half users credit. `app to show data` and `app to generate data` stay at 7: those verbs are not in the spec's exact versioned result-function list. Cyrillic-only users text gets half with the language limitation. Data-only edits change only the data row. Normalized inputs share a cache key; live/stub modes share the result and timestamp; store reset clears the cache. Level edges 39/40, 69/70 and 89/90 return the specified levels and gaps.

`docker compose up --build -d` could not start: this machine's Docker wrapper points to a missing Podman executable. B4's confirmation endpoint is also not implemented, so the end-to-end confirm/improve flow remains to be checked during integration.
