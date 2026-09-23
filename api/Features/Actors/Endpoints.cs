using TaskForge.Api.Infrastructure.Firestore;

namespace TaskForge.Api.Features.Actors;

public static class ActorsEndpoints
{
    public static void MapActorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/actors");

        group.MapGet("", GetActors)
            .WithName("GetActors")
            .WithOpenApi();
    }

    private static async Task<ActorsDto> GetActors(
        BusinessRepository businessRepo,
        TeamRepository teamRepo)
    {
        var businesses = await businessRepo.ListAsync();
        var teams = await teamRepo.ListAsync();

        return new ActorsDto(
            Businesses: businesses.Select(b => new BusinessDto(b.Id, b.Name, b.Industry, b.ContactName)).ToArray(),
            Teams: teams.Select(t => new TeamDto(t.Id, t.Name, t.Interests, t.TechTags, t.Skills, t.Points)).ToArray()
        );
    }
}
