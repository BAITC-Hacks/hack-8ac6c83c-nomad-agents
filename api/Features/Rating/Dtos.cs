using System.Text.Json.Serialization;
using TaskForge.Api.Domain;
using RatingModel = TaskForge.Api.Domain.Rating;

namespace TaskForge.Api.Features.Rating;

public sealed record NextLevelDto(string Level, int PointsNeeded);

public sealed record RatingDto(
    int Total, string Level, IReadOnlyList<RatingBreakdownItem> Breakdown,
    IReadOnlyList<MissingDetail> MissingDetails, IReadOnlyList<ImprovementQuest> Quests,
    string Source, string RatingRulesVersion, DateTimeOffset ScoredAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] NextLevelDto? NextLevel)
{
    public static RatingDto FromDomain(RatingModel rating) => new(rating.Total, rating.Level,
        rating.Breakdown, rating.MissingDetails, rating.Quests, rating.Source, rating.RatingRulesVersion,
        rating.ScoredAt, BuildNextLevel(rating.Total));

    private static NextLevelDto? BuildNextLevel(int total) => total switch
    {
        < 40 => new(ReadinessLevels.Workable, 40 - total),
        < 70 => new(ReadinessLevels.Ready, 70 - total),
        < 90 => new(ReadinessLevels.Priority, 90 - total),
        _ => null
    };
}
