namespace TaskForge.Api.Features.Actors;

public record ActorsDto(
    BusinessDto[] Businesses,
    TeamDto[] Teams
);

public record BusinessDto(
    string Id,
    string Name,
    string Industry,
    string ContactName
);

public record TeamDto(
    string Id,
    string Name,
    string[] Interests,
    string[] TechTags,
    string[] Skills,
    int Points
);
