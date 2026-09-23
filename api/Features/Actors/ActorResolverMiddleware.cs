using TaskForge.Api.Domain;
using TaskForge.Api.Infrastructure.InMemory;

namespace TaskForge.Api.Features.Actors;

public sealed class ActorResolverMiddleware(RequestDelegate next)
{
    public const string ContextItemKey = "TaskForge.Actor";

    public async Task InvokeAsync(HttpContext context, IActorRepository actors)
    {
        if (IsPublicRequest(context.Request))
        {
            await next(context);
            return;
        }

        var roleHeader = context.Request.Headers["X-Actor-Role"].ToString().Trim();
        var idHeader = context.Request.Headers["X-Actor-Id"].ToString().Trim();

        var errors = new Dictionary<string, string[]>();
        if (roleHeader.Length == 0)
        {
            errors["X-Actor-Role"] = ["The X-Actor-Role header is required."];
        }
        else if (!TryParseRole(roleHeader, out _))
        {
            errors["X-Actor-Role"] = ["The X-Actor-Role header must be 'business' or 'team'."];
        }

        if (idHeader.Length == 0)
        {
            errors["X-Actor-Id"] = ["The X-Actor-Id header is required."];
        }

        if (errors.Count > 0)
        {
            await Results.ValidationProblem(
                errors,
                title: "Invalid actor headers",
                statusCode: StatusCodes.Status400BadRequest).ExecuteAsync(context);
            return;
        }

        TryParseRole(roleHeader, out var role);
        var business = actors.GetBusiness(idHeader);
        var team = actors.GetTeam(idHeader);
        var actorExists = business is not null || team is not null;

        if (!actorExists)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Actor not found",
                detail: $"No actor exists with id '{idHeader}'.").ExecuteAsync(context);
            return;
        }

        var roleMatches = role == ActorRole.Business ? business is not null : team is not null;
        if (!roleMatches)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Actor role mismatch",
                detail: $"Actor '{idHeader}' does not have role '{roleHeader}'.").ExecuteAsync(context);
            return;
        }

        context.Items[ContextItemKey] = new ActorContext(role, idHeader);
        await next(context);
    }

    private static bool TryParseRole(string value, out ActorRole role)
    {
        if (value.Equals("business", StringComparison.OrdinalIgnoreCase))
        {
            role = ActorRole.Business;
            return true;
        }

        if (value.Equals("team", StringComparison.OrdinalIgnoreCase))
        {
            role = ActorRole.Team;
            return true;
        }

        role = default;
        return false;
    }

    private static bool IsPublicRequest(HttpRequest request)
    {
        if (HttpMethods.IsOptions(request.Method)) return true;

        var path = request.Path;
        return path.Equals("/api/health", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/actors", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/api/admin")
            || path.StartsWithSegments("/openapi")
            || path.StartsWithSegments("/swagger");
    }
}
