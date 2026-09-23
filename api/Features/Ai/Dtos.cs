using System.Text.Json.Serialization;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Ai;

// Title and chips remain suggestions; this DTO never changes the editable card.
public sealed record AnalysisDto(
    string Title, IReadOnlyList<string> MissingFields,
    IReadOnlyList<AnalysisQuestion> Questions, IReadOnlyList<AnalysisSuggestion> Suggestions,
    IReadOnlyList<DraftExtraction> Extracted, string Source)
{
    public TaskAnalysis ToDomain() => new(Title, MissingFields, Questions, Suggestions,
        Extracted, Source, DateTimeOffset.UtcNow);
}

internal sealed record AnalysisOutput(
    [property: JsonRequired] string Title,
    [property: JsonRequired] string[] MissingFields,
    [property: JsonRequired] QuestionOutput[] Questions,
    [property: JsonRequired] SuggestionOutput[] Suggestions,
    [property: JsonRequired] ExtractionOutput[] Extracted);
internal sealed record QuestionOutput(
    [property: JsonRequired] string FieldKey,
    [property: JsonRequired] string Question,
    [property: JsonRequired] string[] Chips);
internal sealed record SuggestionOutput(
    [property: JsonRequired] string FieldKey, [property: JsonRequired] string Action);
internal sealed record ExtractionOutput(
    [property: JsonRequired] string FieldKey, [property: JsonRequired] string Value,
    [property: JsonRequired] string Evidence);
