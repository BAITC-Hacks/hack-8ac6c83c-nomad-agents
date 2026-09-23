namespace TaskForge.Api.Domain;

public static class TaskStatuses
{
    public const string Editing = "editing";
    public const string Published = "published";

    public static bool IsValid(string value) => value is Editing or Published;
}

public static class AnalysisSources
{
    public const string Ai = "ai";
    public const string Stub = "stub";
}

public static class FieldProvenanceValues
{
    public const string User = "user";
    public const string DraftExtract = "draft-extract";
    public const string Chip = "chip";
}

public sealed record TaskFields(
    string Title,
    string Context,
    string Need,
    string Users,
    string Data,
    string Constraints,
    string ExpectedResult,
    string SuccessCriteria,
    string Contact,
    string InteractionFormat,
    IReadOnlyList<string> Topics,
    IReadOnlyList<string> TechTags)
{
    public static TaskFields Empty { get; } = new(
        "", "", "", "", "", "", "", "", "", "", [], []);
}

public sealed record AnalysisQuestion(
    string Id,
    string FieldKey,
    string Question,
    IReadOnlyList<string> Chips);

public sealed record AnalysisSuggestion(string FieldKey, string Action);

public sealed record DraftExtraction(string FieldKey, string Value, string Evidence);

public sealed record TaskAnalysis(
    string Title,
    IReadOnlyList<string> MissingFields,
    IReadOnlyList<AnalysisQuestion> Questions,
    IReadOnlyList<AnalysisSuggestion> Suggestions,
    IReadOnlyList<DraftExtraction> Extracted,
    string Source,
    DateTimeOffset CreatedAt);

public sealed record ConfirmedTaskSnapshot(
    TaskFields Fields,
    string Hash,
    int Revision,
    DateTimeOffset ConfirmedAt);

public sealed record TaskCard(
    string Id,
    string BusinessId,
    string Status,
    string RawDraft,
    string Industry,
    TaskFields Fields,
    IReadOnlyDictionary<string, string> FieldProvenance,
    bool AnswersApplied,
    string? AppliedAnswersHash,
    TaskAnalysis? Analysis,
    int Revision,
    ConfirmedTaskSnapshot? Confirmed,
    bool HasUnconfirmedChanges,
    Rating? Rating,
    IReadOnlyList<RatingHistoryEntry> RatingHistory,
    int ProposalCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? PublishedAt);
