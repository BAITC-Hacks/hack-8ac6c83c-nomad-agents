namespace TaskForge.Api.Features.Ai;

public record AnalysisDto(
    string Title,
    QuestionDto[] Questions,
    SuggestionDto[] Suggestions,
    ExtractedFieldDto[] Extracted,
    string Source
);

public record QuestionDto(
    string Id,
    string FieldKey,
    string Question,
    string[] Chips
);

public record SuggestionDto(
    string FieldKey,
    string Action
);

public record ExtractedFieldDto(
    string FieldKey,
    string Value,
    string Evidence
);

public record AiLogDto(
    string Id,
    string Kind,
    string TaskId,
    string Model,
    string SystemPrompt,
    string InputJson,
    string RawOutput,
    string Validation,
    string[] Errors,
    int LatencyMs,
    DateTime CreatedAt
);
