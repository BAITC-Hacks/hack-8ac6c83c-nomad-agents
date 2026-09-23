namespace TaskForge.Api.Infrastructure.OpenAi;

// Analyze schemas
public record AnalyzeInput(
    string Draft,
    string Industry,
    Dictionary<string, string> CurrentFields,
    FieldCatalogItem[] FieldCatalog
);

public record FieldCatalogItem(
    string Key,
    string Meaning
);

public record AnalyzeOutput(
    string Title,
    ExtractedField[] Extracted,
    QuestionOutput[] Questions,
    SuggestionOutput[] Suggestions
);

public record ExtractedField(
    string FieldKey,
    string Value,
    string Evidence
);

public record QuestionOutput(
    string FieldKey,
    string Question,
    string[] Chips
);

public record SuggestionOutput(
    string FieldKey,
    string Action
);

// Score schemas
public record ScoreInput(
    RubricCriterion[] Rubric,
    Dictionary<string, string> Card
);

public record RubricCriterion(
    string Criterion,
    int Weight,
    string[] Fields,
    string Full,
    string Half,
    string Zero
);

public record ScoreOutput(
    CriterionScore[] Criteria
);

public record CriterionScore(
    string Criterion,
    int Score,
    string Reason,
    string[] MissingDetails
);

// OpenAI API request/response
public record OpenAiRequest(
    string Model,
    MessageInput[] Input,
    TextFormat Text
);

public record MessageInput(
    string Role,
    string Content
);

public record TextFormat(
    JsonFormat Format
);

public record JsonFormat(
    string Type,
    string Name,
    bool Strict,
    object Schema
);

public record OpenAiResponse(
    string Id,
    string Object,
    long Created,
    string Model,
    Output[] Output,
    Usage Usage
);

public record Output(
    string Type,
    Content[] Content
);

public record Content(
    string Type,
    string Text
);

public record Usage(
    int InputTokens,
    int OutputTokens
);
