namespace TaskForge.Api.Domain;

public static class ReadinessLevels
{
    public const string Draft = "draft";
    public const string Workable = "workable";
    public const string Ready = "ready";
    public const string Priority = "priority";

    public static bool IsValid(string value) => value is Draft or Workable or Ready or Priority;
}

public static class RatingSources
{
    public const string Rules = "rules";
    public const string Seed = "seed";
    public const string Cache = "cache";
}

public sealed record RatingBreakdownItem(
    string Criterion,
    int Weight,
    int Score,
    string Reason,
    IReadOnlyList<string> MatchedSignals);

public sealed record MissingDetail(string Criterion, string Detail);

public sealed record ImprovementQuest(
    string Criterion,
    string FieldKey,
    string Action,
    int PotentialPoints);

public sealed record Rating(
    int Total,
    string Level,
    IReadOnlyList<RatingBreakdownItem> Breakdown,
    IReadOnlyList<MissingDetail> MissingDetails,
    IReadOnlyList<ImprovementQuest> Quests,
    string Source,
    string RatingRulesVersion,
    string CacheKey,
    DateTimeOffset ScoredAt);

public sealed record RatingHistoryEntry(
    int Total,
    string Level,
    DateTimeOffset ScoredAt);
