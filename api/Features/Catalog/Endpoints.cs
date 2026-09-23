namespace TaskForge.Api.Features.Catalog;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/catalog");

        group.MapGet("", GetCatalog)
            .WithName("GetCatalog")
            .WithOpenApi();

        group.MapGet("topics", GetTopics)
            .WithName("GetTopics")
            .WithOpenApi();

        group.MapGet("teams/{teamId}/recommendations", GetRecommendations)
            .WithName("GetRecommendations")
            .WithOpenApi();
    }

    private static async Task<CatalogItemDto[]> GetCatalog(string? topic = null, string? level = null) => Array.Empty<CatalogItemDto>();

    private static async Task<string[]> GetTopics() => Array.Empty<string>();

    private static async Task<RecommendationDto[]> GetRecommendations(string teamId) => Array.Empty<RecommendationDto>();
}
