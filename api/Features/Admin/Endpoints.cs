namespace TaskForge.Api.Features.Admin;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin");

        group.MapPost("seed", Seed)
            .WithName("Seed")
            .WithOpenApi();

        group.MapPost("reset", Reset)
            .WithName("Reset")
            .WithOpenApi();
    }

    private static async Task<object> Seed(string profile = "demo") => new { status = "ok", profile };

    private static async Task<object> Reset() => new { status = "ok" };
}
