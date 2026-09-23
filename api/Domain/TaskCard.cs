namespace TaskForge.Api.Domain;

public record TaskCard(
    string Id,
    string BusinessId,
    TaskStatus Status,
    string RawDraft,
    string Industry,
    TaskFields Fields,
    FieldProvenance FieldProvenance,
    TaskAnalysis? Analysis,
    int Revision,
    ConfirmedSnapshot? Confirmed,
    bool HasUnconfirmedChanges,
    Rating? Rating,
    List<RatingSnapshot> RatingHistory,
    int ProposalCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt
);

public record TaskFields(
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

public record FieldProvenance(
    Dictionary<string, string> Provenance // fieldKey -> "user" | "draft-extract" | "chip"
);

public record TaskAnalysis(
    List<Question> Questions,
    List<Suggestion> Suggestions,
    List<ExtractedField> Extracted,
    string Source, // "ai" | "stub"
    DateTime CreatedAt
);

public record Question(
    string Id,
    string FieldKey,
    string QuestionText,
    string[] Chips
);

public record Suggestion(
    string FieldKey,
    string Action
);

public record ExtractedField(
    string FieldKey,
    string Value,
    string Evidence
);

public record ConfirmedSnapshot(
    TaskFields Fields,
    string Hash,
    int Revision,
    DateTime ConfirmedAt
);

public record RatingSnapshot(
    int Total,
    string Level,
    DateTime ScoredAt
);

public enum TaskStatus { Editing, Published }
