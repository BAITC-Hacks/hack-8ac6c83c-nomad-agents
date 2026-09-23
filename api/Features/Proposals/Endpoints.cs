using TaskForge.Api.Features.Actors;

namespace TaskForge.Api.Features.Proposals;

public static class ProposalEndpoints
{
    public static void MapProposalEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/tasks/{taskId}/proposals/mine", Upsert)
            .WithTags("Proposals").Produces<ProposalDto>().ProducesValidationProblem().ProducesProblem(409);
        app.MapGet("/api/tasks/{taskId}/proposals/mine", MineForTask)
            .WithTags("Proposals").Produces<ProposalDto>().ProducesProblem(404);
        app.MapGet("/api/tasks/{taskId}/proposals", ListForTask)
            .WithTags("Proposals").Produces<IReadOnlyList<ProposalDto>>().ProducesProblem(403).ProducesProblem(404);
        app.MapGet("/api/proposals/mine", ListMine)
            .WithTags("Proposals").Produces<IReadOnlyList<ProposalDto>>();
        app.MapPost("/api/proposals/{proposalId}/decision", Decide)
            .WithTags("Proposals").Produces<ProposalDto>().ProducesValidationProblem().ProducesProblem(403).ProducesProblem(409);
    }

    private static IResult Upsert(string taskId, UpsertProposalRequest request, HttpContext context,
        ActorGuard guard, ProposalService service)
    {
        if (guard.RequireTeam(context) is { } denied) return denied;
        return ToResult(service.Upsert(taskId, guard.GetActor(context)!.ActorId, request));
    }

    private static IResult MineForTask(string taskId, HttpContext context, ActorGuard guard, ProposalService service)
    {
        if (guard.RequireTeam(context) is { } denied) return denied;
        return ToResult(service.MineForTask(taskId, guard.GetActor(context)!.ActorId));
    }

    private static IResult ListForTask(string taskId, HttpContext context, ActorGuard guard, ProposalService service)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        return ToResult(service.ListForTask(taskId, guard.GetActor(context)!.ActorId));
    }

    private static IResult ListMine(HttpContext context, ActorGuard guard, ProposalService service)
    {
        if (guard.RequireTeam(context) is { } denied) return denied;
        return Results.Ok(service.ListMine(guard.GetActor(context)!.ActorId));
    }

    private static IResult Decide(string proposalId, ProposalDecisionRequest request, HttpContext context,
        ActorGuard guard, ProposalService service)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        return ToResult(service.Decide(proposalId, guard.GetActor(context)!.ActorId, request));
    }

    private static IResult ToResult<T>(ProposalServiceResult<T> result) => result.Status switch
    {
        200 => Results.Ok(result.Value),
        400 => Results.ValidationProblem(result.Errors ?? new Dictionary<string, string[]>(),
            title: "Validation failed", statusCode: 400),
        403 => Results.Problem(statusCode: 403, title: "Forbidden", detail: result.Detail),
        404 => Results.Problem(statusCode: 404, title: "Not found", detail: result.Detail),
        409 => Results.Problem(statusCode: 409, title: "Conflict", detail: result.Detail),
        _ => Results.Problem(statusCode: result.Status, title: "Proposal operation failed", detail: result.Detail)
    };
}
