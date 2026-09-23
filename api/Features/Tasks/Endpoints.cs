using TaskForge.Api.Domain;
using TaskForge.Api.Features.Actors;
using TaskForge.Api.Features.Ai;
using TaskForge.Api.Infrastructure.InMemory;

namespace TaskForge.Api.Features.Tasks;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapPost("/", Create).Produces<TaskDto>(201).ProducesValidationProblem();
        group.MapGet("/mine", Mine).Produces<IReadOnlyList<TaskSummaryDto>>();
        group.MapGet("/{id}", Get).Produces<TaskDto>().ProducesProblem(403).ProducesProblem(404);
        group.MapPost("/{id}/analyze", Analyze).Produces<AnalysisDto>().ProducesValidationProblem().ProducesProblem(409);
        group.MapPut("/{id}/answers", ApplyAnswers).Produces<TaskDto>().ProducesValidationProblem().ProducesProblem(409);
        group.MapPut("/{id}/fields", UpdateFields).Produces<TaskDto>().ProducesValidationProblem().ProducesProblem(409);
        group.MapPost("/{id}/confirm", Confirm).Produces<ConfirmTaskResponse>().ProducesValidationProblem().ProducesProblem(409);
        group.MapPost("/{id}/publish", Publish).Produces<TaskDto>().ProducesValidationProblem().ProducesProblem(409);
    }

    private static IResult Create(HttpContext context, ActorGuard guard, TaskService service, CreateTaskRequest request)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        var actor = guard.GetActor(context)!;
        return ToResult(service.Create(actor.ActorId, request), value => Results.Json(value, statusCode: 201));
    }

    private static IResult Mine(HttpContext context, ActorGuard guard, ITaskRepository repository, TaskService service)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        var actor = guard.GetActor(context)!;
        return Results.Ok(repository.ListByBusiness(actor.ActorId).Select(task => new TaskSummaryDto(
            task.Id, task.Fields.Title.Length > 0 ? task.Fields.Title : "Untitled task", task.Status,
            task.Rating is null ? null : Features.Rating.RatingDto.FromDomain(task.Rating),
            task.HasUnconfirmedChanges, task.ProposalCount, service.Position(task), task.UpdatedAt)).ToArray());
    }

    private static IResult Get(string id, HttpContext context, ActorGuard guard, ITaskRepository repository, TaskService service)
    {
        var task = repository.Get(id);
        if (task is null) return Problem(404, "Task not found", "Task not found.");
        var actor = guard.GetActor(context)!;
        if (actor.Role == ActorRole.Business)
            return actor.ActorId == task.BusinessId ? Results.Ok(TaskDto.Owner(task, service.Position(task))) :
                Problem(403, "Forbidden", "Only the business that owns this task may view its editing state.");
        if (task.Status != TaskStatuses.Published || task.Confirmed is null || task.Rating is null)
            return Problem(404, "Task not found", "No published task is available with this ID.");
        return Results.Ok(TaskDto.ConfirmedView(task, service.Position(task)));
    }

    private static async Task<IResult> Analyze(string id, HttpContext context, ActorGuard guard,
        TaskService service, CancellationToken token)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        return ToResult(await service.AnalyzeAsync(id, guard.GetActor(context)!.ActorId, token));
    }

    private static IResult ApplyAnswers(string id, HttpContext context, ActorGuard guard,
        TaskService service, ApplyAnswersRequest request)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        return ToResult(service.ApplyAnswers(id, guard.GetActor(context)!.ActorId, request));
    }

    private static IResult UpdateFields(string id, HttpContext context, ActorGuard guard,
        TaskService service, UpdateFieldsRequest request)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        return ToResult(service.UpdateFields(id, guard.GetActor(context)!.ActorId, request));
    }

    private static IResult Confirm(string id, HttpContext context, ActorGuard guard, TaskService service)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        return ToResult(service.Confirm(id, guard.GetActor(context)!.ActorId));
    }

    private static IResult Publish(string id, HttpContext context, ActorGuard guard, TaskService service)
    {
        if (guard.RequireBusiness(context) is { } denied) return denied;
        return ToResult(service.Publish(id, guard.GetActor(context)!.ActorId));
    }

    private static IResult ToResult<T>(ServiceResult<T> result, Func<T, IResult>? success = null) => result.Status switch
    {
        >= 200 and < 300 when result.Value is not null => success?.Invoke(result.Value) ?? Results.Ok(result.Value),
        400 => Results.ValidationProblem(result.Errors ?? new Dictionary<string, string[]>(),
            title: "Validation failed", statusCode: 400),
        403 => Problem(403, "Forbidden", result.Detail),
        404 => Problem(404, "Task not found", result.Detail),
        409 => Problem(409, "Conflict", result.Detail),
        _ => Problem(result.Status, "Task operation failed", result.Detail)
    };

    private static IResult Problem(int status, string title, string? detail) => Results.Problem(
        statusCode: status, title: title, detail: detail);
}
