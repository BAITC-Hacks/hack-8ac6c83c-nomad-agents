# B1 — In-memory storage and domain mapping

**Priority: P0 — top priority.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 4, 6.

## Goal

Implement repositories over the singleton process-local `InMemoryDataStore`. This task establishes the domain shape and atomic update helpers needed by B2–B7. No database, emulator, ORM, or persistence provider is used.

## Starting point and files

`api/Program.cs` registers `InMemoryDataStore` as a singleton and `api/Infrastructure/InMemory/InMemoryDataStore.cs` provides typed concurrent collections plus a shared lock. Add domain records under `api/Domain/` and repositories under `api/Infrastructure/InMemory/`.

## Implement

- Keep one typed collection per domain type, keyed by stable string ID. Store UTC timestamps and preserve nested fields, arrays, rating history, and nullable confirmation directly as domain records.
- Use lowercase task statuses (`editing|published`) and proposal statuses (`pending|selected|rejected`).
- Align domain shape with the P0 contract: task revision, confirmed field snapshot, `hasUnconfirmedChanges`, `answersApplied`, canonical `appliedAnswersHash`, validated analysis, rating breakdown/quests/rules version, and proposal decision metadata.
- Add repository methods for conditional task update/confirmation and proposal upsert/decision. Use `InMemoryDataStore.SyncRoot` for multi-collection or compare-and-update operations so a stale revision cannot commit. Do not claim durability or cross-instance consistency.
- Keep all application data inside the singleton store. `demo`, `full`, and reset operations clear and repopulate these collections. A process restart discards tasks, proposals, ratings, and logs. Until B7 owns full seeding, B0 bootstraps only the five role-switcher actors when the actor repository is first resolved.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. In one API process, manually write and read a business, team, draft task, confirmed task, rating, proposal, and AI log; compare key fields. Verify the published query returns only published tasks, concurrent stale revision is rejected, and reset empties every collection. Restarting the API clears feature state; the B0 actor bootstrap is recreated on first actor access.

## Boundary

Do not implement HTTP feature behavior, scoring logic, seed contents, durable files, or frontend code. B1 owns domain records, in-memory repository contracts, and atomic process-local updates.

## Implemented result

Domain records cover actors, editable and confirmed tasks, analysis, ratings/history, proposals/milestones, and AI logs. Repositories provide stable-ID reads, seed upserts, revision-checked task changes, atomic confirmation, pending proposal upsert, guarded decisions, atomic milestone/team-point updates, recent AI logs, and full reset. Conditional updates preserve ownership identity and UTC-normalize stored timestamps.
