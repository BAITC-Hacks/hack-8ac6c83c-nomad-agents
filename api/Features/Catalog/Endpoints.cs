using Microsoft.AspNetCore.Mvc;

namespace TaskForge.Api.Features.Catalog;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog").WithTags("Catalog");
        group.MapGet("/", List)
            .Produces<CatalogResponse>()
            .ProducesValidationProblem();
        group.MapGet("/topics", (CatalogService service) => Results.Ok(service.Topics()))
            .Produces<IReadOnlyList<string>>();
    }

    private static IResult List(
        CatalogService service,
        [FromQuery(Name = "topic")] string[]? topics,
        [FromQuery(Name = "level")] string[]? levels)
    {
        var result = service.List(topics, levels);
        return result.Errors is null
            ? Results.Ok(result.Value)
            : Results.ValidationProblem(result.Errors, title: "Invalid catalog filters", statusCode: 400);
    }
}
