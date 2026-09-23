# B1 — Firestore persistence and domain mapping

**Priority: P0 — top priority.** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 4, 6.

## Goal

Make the existing repositories read and write real domain objects against both the local emulator and production Firestore. This task establishes data shape and transaction helpers needed by B2–B7.

## Starting point and files

`api/Program.cs` currently calls `FirestoreDb.Create`, while the spec uses `FirestoreDbBuilder` with emulator detection. `api/Infrastructure/Firestore/*.cs` call `ConvertTo<T>()` on positional records without a demonstrated serializer mapping. `TaskRepository.GetPublishedAsync()` compares status to `"Published"`, while the API/spec use lowercase `published`. Update `api/Domain/*.cs`, `api/Infrastructure/Firestore/*.cs`, and Firestore registration in `Program.cs`.

## Implement

- Configure `FirestoreDbBuilder` for `GCP_PROJECT_ID` and emulator detection. Persist timestamps in UTC and choose one documented Firestore serialization strategy for the records (attributes/converters or explicit dictionary mapping). Prove round-trip of nested fields, enum values, arrays, rating history, and nullable confirmation.
- Make document IDs and stored IDs consistent. Persist task statuses as `editing|published` and proposal statuses as `pending|selected|rejected`; fix repository queries accordingly.
- Align domain shape with the P0 contract: task revision, confirmed field snapshot, `hasUnconfirmedChanges`, `answersApplied` and canonical `appliedAnswersHash` for one-time Apply and identical-request retry, validated analysis, rating breakdown/quests/rules version, and proposal decision metadata. Preserve seeded actor profiles and support both `demo` and `full` profiles.
- Add repository methods for conditional task update/confirmation and proposal upsert/decision using Firestore transactions where needed. Keep Firestore access inside repositories. Query the small demo catalog in memory; avoid composite-index requirements.
- Keep existing names where practical so dependent agents can reuse the scaffold. Document any changed property names for B2–B7.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. With the emulator running, manually write then read a business, team, draft task, confirmed task, rating, proposal, and AI log; compare their key fields. Verify the published query returns only `published` tasks and a transaction rejects a stale task revision.

## Boundary

Do not implement HTTP feature behavior, scoring logic, seed contents, or frontend code. B1 owns domain serialization and repository contracts; later briefs own service logic.
