namespace TaskForge.Api.Features.Catalog;

public record CatalogItemDto(
    string Id,
    string Title,
    string BusinessName,
    int RatingTotal,
    string RatingLevel,
    string[] Topics,
    string[] TechTags,
    int ProposalCount,
    int CatalogPosition,
    bool IsPriority
);

public record RecommendationDto(
    CatalogItemDto Task,
    string[] MatchedTags
);
