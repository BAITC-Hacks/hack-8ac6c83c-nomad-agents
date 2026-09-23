namespace TaskForge.Api.Domain;

public enum ActorRole
{
    Business,
    Team
}

public sealed record ActorContext(ActorRole Role, string ActorId);

public sealed record Business(
    string Id,
    string Name,
    string Industry,
    string ContactName,
    DateTimeOffset CreatedAt = default);

public sealed record Team(
    string Id,
    string Name,
    IReadOnlyList<string> Interests,
    IReadOnlyList<string> TechTags,
    IReadOnlyList<string> Skills,
    int Points = 0,
    DateTimeOffset CreatedAt = default);
