# B0 — API contract and actor guard

**Priority: P0 — after [B00](B00-runnable-api-and-swagger.md).** Source: [MVP_SPEC.md](../../MVP_SPEC.md) §§2, 7.

## Goal

Turn the scaffold's role headers into one reusable backend rule and freeze the mandatory demo API contract described in [README](README.md). All later agents use this contract. This is a demo identity mechanism, not login.

## Starting point and files

`api/Program.cs` maps the endpoint groups but has no actor validation. `api/Domain/Actor.cs` has only an `ActorContext` record. Add a focused resolver/filter in `api/Features/Actors/` or `api/Infrastructure/`; update `Program.cs` only for shared registration and middleware. `api/Features/Actors/Endpoints.cs` already has `/api/actors` and can remain public.

## Implement

- Resolve `X-Actor-Role: business|team` and `X-Actor-Id` once per request. Load the named actor via the existing actor repositories; reject missing, unknown, or mismatched actor headers. Exempt `/api/health`, `GET /api/actors`, and guarded admin operations.
- Expose helpers/policies that feature endpoints can call for business owner, team member, and task/proposal ownership. A team must receive only a task's confirmed view; business draft/edit operations require the owning business.
- Use `Results.ValidationProblem` for invalid request fields, `404` for missing resources, `403` for a known actor without permission, and `409` for stale revision or illegal state transition. Do not throw for normal control flow.
- Publish one concise route/DTO decision in this brief or a small `docs/backend/contract.md`: align existing task and proposal routes with spec §7 and document repeated `topic`/`level` catalog query values for the spec §8.3 multi-select UI; keep `GET /tasks/{id}/rating-preview` for P1 and omit standalone AI scoring. Do not expose fake optional endpoint responses. `GET /actors` must provide the 3 business and team identities needed by the P0 role switcher.

## Acceptance

`dotnet build api/TaskForge.Api.csproj` passes. Once B7 supplies seeded actors and B4 supplies an owner route, manually call that route with valid, missing, unknown, and wrong-role headers and observe the intended status codes. `/api/health` and `/api/actors` remain available without headers.

## Boundary

Do not implement task lifecycle, scoring, AI calls, persistence mapping, proposals, or frontend code. Other agents own their feature DTOs/endpoints and call this shared actor guard.
