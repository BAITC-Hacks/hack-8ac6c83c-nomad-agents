using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Actors;

public sealed record BusinessActorDto(
    string Id,
    string Name,
    string Industry,
    string ContactName)
{
    public static BusinessActorDto FromDomain(Business business) =>
        new(business.Id, business.Name, business.Industry, business.ContactName);
}

public sealed record TeamActorDto(string Id, string Name)
{
    public static TeamActorDto FromDomain(Team team) => new(team.Id, team.Name);
}

public sealed record ActorsResponse(
    IReadOnlyList<BusinessActorDto> Businesses,
    IReadOnlyList<TeamActorDto> Teams);
