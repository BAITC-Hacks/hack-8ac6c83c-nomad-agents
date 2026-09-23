using System.Text.Json;
using System.Text.Json.Serialization;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Ai;

public sealed record AnalysisValidation(AnalysisDto? Analysis, IReadOnlyList<string> Errors);

public static class AnalyzeValidator
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true
    };

    public static AnalysisValidation Validate(string output, string rawDraft)
    {
        AnalysisOutput? result;
        try { result = JsonSerializer.Deserialize<AnalysisOutput>(output, Json); }
        catch (JsonException) { return new(null, ["Malformed analysis JSON or schema mismatch."]); }
        if (result is null) return new(null, ["Analysis is null."]);
        List<string> errors = [];
        if (result.Questions.Length is < 3 or > 7) errors.Add("Expected 3–7 questions.");
        if (result.MissingFields.Any(k => !FieldCatalog.Known(k?.Trim()))) errors.Add("Unknown missing field key.");
        if (result.Questions.Any(q => q is null || !FieldCatalog.Known(q.FieldKey) ||
                string.IsNullOrWhiteSpace(q.Question) || q.Question.Length > 500 || q.Chips is null))
            errors.Add("Invalid question field, text, or chips.");
        if (result.Questions.Where(q => q is not null).Select(q => q.FieldKey).Distinct().Count() != result.Questions.Length)
            errors.Add("Questions must refer to distinct fields.");
        if (result.Suggestions.Any(s => s is null || !FieldCatalog.Known(s.FieldKey))) errors.Add("Unknown suggestion field key.");
        if (result.Extracted.Any(e => e is null || !FieldCatalog.Known(e.FieldKey))) errors.Add("Unknown extraction field key.");
        if (errors.Count > 0) return new(null, errors);

        List<DraftExtraction> extracted = [];
        foreach (var e in result.Extracted)
        {
            var evidence = e.Evidence.Trim();
            var value = e.Value.Trim();
            var index = rawDraft.IndexOf(evidence, StringComparison.OrdinalIgnoreCase);
            var offset = evidence.IndexOf(value, StringComparison.OrdinalIgnoreCase);
            if (evidence.Length == 0 || value.Length == 0 || index < 0 || offset < 0)
            {
                errors.Add($"Dropped unsupported extraction for {e.FieldKey}.");
                continue;
            }
            // Return the actual source characters, not even the model's casing.
            extracted.Add(new(e.FieldKey, rawDraft.Substring(index + offset, value.Length),
                rawDraft.Substring(index, evidence.Length)));
        }
        var questions = result.Questions.Select((q, i) => new AnalysisQuestion($"q{i + 1}", q.FieldKey,
            q.Question.Trim(), q.Chips.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim())
                .Where(c => c.Length <= 80).Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToArray())).ToArray();
        if (result.Questions.Where((q, i) => !q.Chips.SequenceEqual(questions[i].Chips)).Any())
            errors.Add("Normalized or discarded invalid/duplicate/excess chips.");
        var suggestions = result.Suggestions.Where(s => !string.IsNullOrWhiteSpace(s.Action) && s.Action.Trim().Length <= 240)
            .Take(6).Select(s => new AnalysisSuggestion(s.FieldKey, s.Action.Trim())).ToArray();
        if (suggestions.Length < 2) return new(null, [.. errors, "Expected at least two concise improvement actions."]);
        if (suggestions.Length != result.Suggestions.Length) errors.Add("Dropped empty, long, or excess suggestions.");
        var title = result.Title.Trim();
        return new(new(title[..Math.Min(title.Length, 120)], result.MissingFields.Select(k => k.Trim()).Distinct().ToArray(),
            questions, suggestions, extracted, AnalysisSources.Ai), errors);
    }
}
