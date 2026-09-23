# B5 — Ranked open catalog

**Priority: P0 — top priority.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 6.3, 7.

## Goal

Make every **published** task visible to every team, sorted by confirmed readiness score, with topic and readiness filters. The catalog must never expose unconfirmed business edits.

## Starting point and files

`api/Features/Catalog/Endpoints.cs` returns empty arrays; `Dtos.cs` has a summary item but no short description. `TaskRepository.GetPublishedAsync()` is corrected by B1. Own `api/Features/Catalog/` and a small catalog service; use B1 repositories and B3 rating fields.

## Implement

- `GET /api/catalog?topic=&level=` loads published tasks, uses `confirmed.fields` and confirmed rating only, and sorts `rating.total` descending, then `confirmedAt` descending, then task ID. Assign the 1-based catalog position **before** filtering; include total published count so `#n of m` remains stable across filters.
- Accept repeated `topic` and `level` query values for the §8.3 multi-select UI (for example `?topic=retail&topic=logistics&level=workable`). Match any selected topic case-insensitively and any selected level in `draft|workable|ready|priority`; combine the topic and level groups with AND. Return validation errors for unknown levels. `GET /api/catalog/topics` lists distinct confirmed topics. Apply filters after ranking and position assignment. Freeze this query shape in B0's OpenAPI contract. Keep the in-memory approach for the small demo dataset.
- Return title, short summary from confirmed context/need, business name, score, level, topics/tech tags, proposal count, position, and priority flag. Draft-level tasks are still visible and may receive proposals.
- Preserve a read route for the full confirmed card (`GET /api/tasks/{id}` from B4); catalog items link to it. Recommendations are P1: unmap the unfinished route or return an explicit unavailable response until implemented, never an empty success that implies functionality.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. With demo seed, inspect the unfiltered catalog and both filters. Verify Priority precedes Workable, a low-score published card remains visible, positions are computed before filtering, ties remain stable, and a published task with pending edits shows its old confirmed text/score until reconfirmed.

## Boundary

Do not implement recommendation ranking, task mutations, proposal decisions, or frontend catalog UI. B5 owns only catalog read behavior and DTOs.
