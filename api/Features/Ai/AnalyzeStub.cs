using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Ai;

public static class AnalyzeStub
{
    public static AnalysisDto Create(TaskFields fields)
    {
        // Conservative clarification heuristic, not a readiness score. Do not infer facts from prose.
        var ranked = FieldCatalog.All.OrderBy(f => f.Read(fields).Trim().Length).ToArray();
        var gaps = FieldCatalog.All.Where(f => IsWeak(f.Read(fields))).ToArray();
        var selected = gaps.Concat(ranked).DistinctBy(f => f.Key).Take(Math.Clamp(gaps.Length, 3, 5));
        return new("", gaps.Select(f => f.Key).ToArray(),
            selected.Select((f, i) => new AnalysisQuestion($"q{i + 1}", f.Key,
                gaps.Contains(f) ? f.Question : $"Please confirm or make more specific: {f.Question}", [])).ToArray(),
            gaps.Select(f => new AnalysisSuggestion(f.Key, $"Add {f.Meaning.ToLowerInvariant()}")).ToArray(),
            [], AnalysisSources.Stub);
    }

    private static bool IsWeak(string value) => value.Trim().Length < 15 ||
        new[] { "unknown", "not sure", "to be decided", "to be confirmed", "not applicable" }
            .Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}
