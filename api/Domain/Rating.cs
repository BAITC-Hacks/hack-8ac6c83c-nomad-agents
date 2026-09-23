namespace TaskForge.Api.Domain;

public record Rating(
    int Total,
    string Level,
    List<CriterionScore> Breakdown,
    List<MissingDetail> MissingDetails,
    string Source, // "ai" | "stub" | "seed" | "cache"
    string CacheKey,
    DateTime ScoredAt,
    NextLevel? NextLevel = null
);

public record CriterionScore(
    string Criterion,
    int Weight,
    int Score,
    string Reason
);

public record MissingDetail(
    string Criterion,
    string Detail
);

public record NextLevel(
    string Level,
    int PointsNeeded
);

public static class ReadinessLevel
{
    public const string Draft = "draft";
    public const string Workable = "workable";
    public const string Ready = "ready";
    public const string Priority = "priority";

    public static string GetLabel(string level) => level switch
    {
        Draft => "Needs clarification",
        Workable => "Workable",
        Ready => "Ready",
        Priority => "Priority",
        _ => "Unknown"
    };

    public static string GetLevel(int score) => score switch
    {
        >= 90 => Priority,
        >= 70 => Ready,
        >= 40 => Workable,
        _ => Draft
    };
}
