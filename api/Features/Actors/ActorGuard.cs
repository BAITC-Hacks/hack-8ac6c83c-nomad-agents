using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Actors;

public sealed class ActorGuard
{
    public ActorContext? GetActor(HttpContext context) =>
        context.Items.TryGetValue(ActorResolverMiddleware.ContextItemKey, out var value)
            && value is ActorContext actor
                ? actor
                : null;

    public IResult? RequireBusiness(HttpContext context)
    {
        var actor = GetActor(context);
        if (actor is null) return MissingActorContext();
        return actor.Role == ActorRole.Business
            ? null
            : Forbidden("A business actor is required for this operation.");
    }

    public IResult? RequireTeam(HttpContext context)
    {
        var actor = GetActor(context);
        if (actor is null) return MissingActorContext();
        return actor.Role == ActorRole.Team
            ? null
            : Forbidden("A team actor is required for this operation.");
    }

    public IResult? RequireTaskOwner(HttpContext context, string businessId) =>
        RequireBusinessOwner(context, businessId, "Only the business that owns this task may modify it.");

    public IResult? RequireProposalOwner(HttpContext context, string businessId) =>
        RequireBusinessOwner(context, businessId, "Only the business that owns this proposal's task may decide it.");

    public bool MustUseConfirmedTaskView(HttpContext context, string businessId)
    {
        var actor = GetActor(context);
        if (actor is null) return true;
        return actor.Role == ActorRole.Team || !actor.ActorId.Equals(businessId, StringComparison.Ordinal);
    }

    private IResult? RequireBusinessOwner(HttpContext context, string businessId, string detail)
    {
        var actor = GetActor(context);
        if (actor is null) return MissingActorContext();
        return actor.Role == ActorRole.Business
            && actor.ActorId.Equals(businessId, StringComparison.Ordinal)
                ? null
                : Forbidden(detail);
    }

    private static IResult Forbidden(string detail) => Results.Problem(
        statusCode: StatusCodes.Status403Forbidden,
        title: "Forbidden",
        detail: detail);

    private static IResult MissingActorContext() => Results.Problem(
        statusCode: StatusCodes.Status500InternalServerError,
        title: "Actor context unavailable",
        detail: "The actor resolver must run before protected endpoints.");
}
