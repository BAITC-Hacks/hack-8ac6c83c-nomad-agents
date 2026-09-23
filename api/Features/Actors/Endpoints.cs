using TaskForge.Api.Infrastructure.InMemory;

namespace TaskForge.Api.Features.Actors;

public static class ActorEndpoints
{
    public static void MapActorEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/actors", (IActorRepository repository) =>
                TypedResults.Ok(new ActorsResponse(
                    repository.ListBusinesses().Select(BusinessActorDto.FromDomain).ToArray(),
                    repository.ListTeams().Select(TeamActorDto.FromDomain).ToArray())))
            .WithName("GetActors")
            .WithSummary("Lists the demo business and team identities available to the role switcher.");
    }
}
