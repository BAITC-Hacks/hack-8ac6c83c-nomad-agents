namespace TaskForge.Api.Features.Health;

using TaskForge.Api.Infrastructure.InMemory;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", (InMemoryDataStore _) =>
                TypedResults.Ok(new HealthResponse("ok", "in_memory")))
            .WithName("GetHealth")
            .WithSummary("Checks that the API host and its process-local in-memory store are available.");
    }
}
