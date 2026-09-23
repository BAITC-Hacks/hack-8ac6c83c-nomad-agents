namespace TaskForge.Api.Features.Ai;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ai-logs");

        group.MapGet("", GetAiLogs)
            .WithName("GetAiLogs")
            .WithOpenApi();
    }

    private static async Task<AiLogDto[]> GetAiLogs(string? taskId = null, int limit = 20) => Array.Empty<AiLogDto>();
}
