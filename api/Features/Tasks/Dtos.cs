using System.Text.Json.Serialization;
using TaskForge.Api.Domain;
using TaskForge.Api.Features.Ai;
using TaskForge.Api.Features.Rating;

namespace TaskForge.Api.Features.Tasks;

public sealed record CreateTaskRequest(string? RawDraft, string? Industry);
public sealed record AnswerDto(string? QuestionId, string? FieldKey, string? Text);
public sealed record ApplyAnswersRequest(IReadOnlyList<AnswerDto>? Answers);

public sealed record TaskFieldsPatch(
    string? Title = null, string? Context = null, string? Need = null, string? Users = null,
    string? Data = null, string? Constraints = null, string? ExpectedResult = null,
    string? SuccessCriteria = null, string? Contact = null, string? InteractionFormat = null,
    IReadOnlyList<string>? Topics = null, IReadOnlyList<string>? TechTags = null);

public sealed record AcceptedExtractionDto(string? FieldKey, string? Value, string? Evidence);
public sealed record UpdateFieldsRequest(
    TaskFieldsPatch? Fields,
    IReadOnlyList<AcceptedExtractionDto>? AcceptedExtracts = null);

public sealed record ConfirmedTaskDto(TaskFields Fields, string Hash, int Revision, DateTimeOffset ConfirmedAt);
public sealed record CatalogPositionDto(int Rank, int Total);

public sealed record TaskDto(
    string Id, string BusinessId, string Status, string RawDraft, string Industry,
    TaskFields Fields, IReadOnlyDictionary<string, string> FieldProvenance,
    bool AnswersApplied, AnalysisDto? Analysis, int Revision,
    ConfirmedTaskDto? Confirmed, bool HasUnconfirmedChanges, RatingDto? Rating,
    IReadOnlyList<RatingHistoryEntry> RatingHistory, int ProposalCount,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, DateTimeOffset? PublishedAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] CatalogPositionDto? Position)
{
    public static TaskDto Owner(TaskCard task, CatalogPositionDto? position = null) => new(
        task.Id, task.BusinessId, task.Status, task.RawDraft, task.Industry, task.Fields,
        task.FieldProvenance, task.AnswersApplied, task.Analysis is null ? null : Map(task.Analysis),
        task.Revision, task.Confirmed is null ? null : new(task.Confirmed.Fields, task.Confirmed.Hash,
            task.Confirmed.Revision, task.Confirmed.ConfirmedAt), task.HasUnconfirmedChanges,
        task.Rating is null ? null : RatingDto.FromDomain(task.Rating), task.RatingHistory,
        task.ProposalCount, task.CreatedAt, task.UpdatedAt, task.PublishedAt, position);

    public static TaskDto ConfirmedView(TaskCard task, CatalogPositionDto? position) => new(
        task.Id, task.BusinessId, task.Status, "", task.Industry, task.Confirmed!.Fields,
        new Dictionary<string, string>(), false, null, task.Confirmed.Revision,
        new(task.Confirmed.Fields, task.Confirmed.Hash, task.Confirmed.Revision, task.Confirmed.ConfirmedAt),
        task.HasUnconfirmedChanges, task.Rating is null ? null : RatingDto.FromDomain(task.Rating),
        [], task.ProposalCount, task.CreatedAt, task.UpdatedAt, task.PublishedAt, position);

    private static AnalysisDto Map(TaskAnalysis value) => new(value.Title, value.MissingFields,
        value.Questions, value.Suggestions, value.Extracted, value.Source);
}

public sealed record TaskSummaryDto(
    string Id, string Title, string Status, RatingDto? Rating, bool HasUnconfirmedChanges,
    int ProposalCount, CatalogPositionDto? Position, DateTimeOffset UpdatedAt);

public sealed record ConfirmTaskResponse(
    TaskDto Task, RatingDto Rating, int Delta, CatalogPositionDto? Position);
