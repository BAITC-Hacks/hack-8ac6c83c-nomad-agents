namespace TaskForge.Api.Features.Health;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () => TypedResults.Ok(new HealthResponse("ok", "not_checked")))
            .WithName("GetHealth")
            .WithSummary("Checks that the API host is running; dependency reachability is not checked.");
    }
}
