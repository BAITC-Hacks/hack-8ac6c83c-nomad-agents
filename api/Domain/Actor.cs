namespace TaskForge.Api.Domain;

public record Business(
    string Id,
    string Name,
    string Industry,
    string ContactName,
    DateTime CreatedAt
);

public record Team(
    string Id,
    string Name,
    string[] Interests,
    string[] TechTags,
    string[] Skills,
    int Points,
    DateTime CreatedAt
);

public record ActorContext(
    string Role, // "business" | "team"
    string Id
);
