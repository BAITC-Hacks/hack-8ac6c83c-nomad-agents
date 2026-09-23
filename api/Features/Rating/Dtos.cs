namespace TaskForge.Api.Features.Rating;

public record RatingDto(
    int Total,
    string Level,
    CriterionScoreDto[] Breakdown,
    MissingDetailDto[] MissingDetails,
    string Source,
    DateTime ScoredAt,
    NextLevelDto? NextLevel = null,
    int? Delta = null,
    int CatalogPosition = 0
);

public record CriterionScoreDto(
    string Criterion,
    int Weight,
    int Score,
    string Reason
);

public record MissingDetailDto(
    string Criterion,
    string Detail
);

public record NextLevelDto(
    string Level,
    int PointsNeeded
);
