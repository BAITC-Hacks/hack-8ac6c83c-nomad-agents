namespace TaskForge.Api.Features.Tasks;

public record CreateTaskRequest(
    string RawDraft,
    string Industry
);

public record AnswersRequest(
    AnswerItem[] Answers
);

public record AnswerItem(
    string QuestionId,
    string FieldKey,
    string Text
);

public record UpdateFieldsRequest(
    Dictionary<string, string> Fields
);

public record TaskDto(
    string Id,
    string BusinessId,
    string Status,
    TaskFieldsDto Fields,
    int ProposalCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt,
    int CatalogPosition = 0
);

public record TaskFieldsDto(
    string Title = "",
    string Context = "",
    string Need = "",
    string Users = "",
    string Data = "",
    string Constraints = "",
    string ExpectedResult = "",
    string SuccessCriteria = "",
    string Contact = "",
    string InteractionFormat = "",
    string[] Topics = default!,
    string[] TechTags = default!
);

public record TaskSummaryDto(
    string Id,
    string Title,
    string Status,
    int? RatingTotal,
    string? RatingLevel,
    int CatalogPosition,
    int ProposalCount
);
