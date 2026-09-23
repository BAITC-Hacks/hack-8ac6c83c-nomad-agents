using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TaskForge.Api.Domain;
using TaskForge.Api.Infrastructure.InMemory;
using RatingModel = TaskForge.Api.Domain.Rating;

namespace TaskForge.Api.Features.Rating;

public sealed class RatingService(IRatingCacheRepository cache)
{
    public RatingModel Score(ConfirmedTaskSnapshot confirmed) => ScoreFields(confirmed.Fields);

    // B4 passes its captured fields, then persists the result with that revision atomically.
    // B7 can use the same method; neither method awards points or writes task/history state.
    public RatingModel ScoreFields(TaskFields fields)
    {
        var key = CacheKey(fields);
        var existing = cache.Get(key);
        if (existing is not null) return existing with { Source = RatingSources.Cache };
        var calculated = Calculate(fields, key);
        var saved = cache.GetOrAdd(calculated);
        return ReferenceEquals(saved, calculated) ? saved : saved with { Source = RatingSources.Cache };
    }

    // Pure preview: no cache insertion, task changes, or history entry.
    public RatingModel Preview(TaskFields fields) => Calculate(fields, CacheKey(fields));

    public static string Level(int total) => total switch
    {
        < 40 => ReadinessLevels.Draft,
        < 70 => ReadinessLevels.Workable,
        < 90 => ReadinessLevels.Ready,
        _ => ReadinessLevels.Priority
    };

    public static NextLevelDto? NextLevel(int total) => total switch
    {
        < 40 => new(ReadinessLevels.Workable, 40 - total),
        < 70 => new(ReadinessLevels.Ready, 70 - total),
        < 90 => new(ReadinessLevels.Priority, 90 - total),
        _ => null
    };

    public static string CacheKey(TaskFields fields)
    {
        // Fixed property order, normalized scalar text, and order-independent tag sets.
        var canonical = JsonSerializer.Serialize(new
        {
            version = ReadinessRules.Version,
            title = ReadinessRules.Normalize(fields.Title),
            context = ReadinessRules.Normalize(fields.Context), need = ReadinessRules.Normalize(fields.Need),
            users = ReadinessRules.Normalize(fields.Users), data = ReadinessRules.Normalize(fields.Data),
            constraints = ReadinessRules.Normalize(fields.Constraints), expectedResult = ReadinessRules.Normalize(fields.ExpectedResult),
            successCriteria = ReadinessRules.Normalize(fields.SuccessCriteria), contact = ReadinessRules.Normalize(fields.Contact),
            interactionFormat = ReadinessRules.Normalize(fields.InteractionFormat),
            topics = Tags(fields.Topics), techTags = Tags(fields.TechTags)
        });
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));

        static string[] Tags(IReadOnlyList<string> tags) => tags.Select(ReadinessRules.Normalize)
            .Where(t => t.Length > 0).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }

    private static RatingModel Calculate(TaskFields fields, string key)
    {
        List<RatingBreakdownItem> rows = [];
        List<MissingDetail> details = [];
        List<ImprovementQuest> quests = [];
        var context = ReadinessRules.Normalize(fields.Context);
        var need = ReadinessRules.Normalize(fields.Need);
        var contextPresent = ReadinessRules.Present(context);
        var needPresent = ReadinessRules.Present(need);
        Add("contextAndNeed", 20, contextPresent || needPresent, contextPresent && needPresent,
            Array.AsReadOnly(new[] { contextPresent ? "context.present" : "", needPresent ? "need.present" : "" }.Where(s => s != "").ToArray()),
            [new("context.present", "context", "Describe the current situation", contextPresent),
             new("need.present", "need", "Describe what needs to change", needPresent)], false);

        var data = ReadinessRules.Normalize(fields.Data);
        var signals = ReadinessRules.DataSignals(data);
        var source = signals.Contains("data.source");
        var detail = signals.Any(s => s is "data.format" or "data.quantity" or "data.access");
        Add("dataAndMaterials", 20, ReadinessRules.Present(data), source && detail, signals,
            [new("data.source", "data", "Name an available data source or example", source),
             new("data.format / data.quantity / data.access", "data", "Specify a data format, quantity, or access method", detail)]);

        var result = ReadinessRules.Normalize(fields.ExpectedResult);
        signals = ReadinessRules.ResultSignals(result);
        Add("expectedResult", 15, ReadinessRules.Present(result), signals.Count == 2, signals,
            [new("result.artifact", "expectedResult", "Name the deliverable", signals.Contains("result.artifact")),
             new("result.function", "expectedResult", "Describe what that deliverable must do", signals.Contains("result.function"))]);

        var success = ReadinessRules.Normalize(fields.SuccessCriteria);
        signals = ReadinessRules.SuccessSignals(success);
        Add("successCriteria", 15, ReadinessRules.Present(success), signals.Count == 2, signals,
            [new("success.outcome", "successCriteria", "Name the outcome or measure", signals.Contains("success.outcome")),
             new("success.acceptance", "successCriteria", "Define a target with a comparator and unit, or a concrete acceptance check", signals.Contains("success.acceptance"))]);

        var constraints = ReadinessRules.Normalize(fields.Constraints);
        signals = ReadinessRules.ConstraintSignals(constraints);
        Add("constraints", 10, ReadinessRules.Present(constraints), signals.Count >= 2, signals,
            [new("constraints.distinctCategories", "constraints", signals.Count == 0
                ? "State two boundaries from time, technology, access, legal, or budget"
                : "Add a boundary from a different category: time, technology, access, legal, or budget", signals.Count >= 2)]);

        var users = ReadinessRules.Normalize(fields.Users);
        signals = ReadinessRules.UserSignals(users);
        Add("users", 10, ReadinessRules.Present(users), signals.Count == 2, signals,
            [new("users.group", "users", "Name the user group", signals.Contains("users.group")),
             new("users.usage", "users", "Explain what that group does with the result", signals.Contains("users.usage"))]);

        var contact = ReadinessRules.Normalize(fields.Contact);
        var interaction = ReadinessRules.Normalize(fields.InteractionFormat);
        signals = ReadinessRules.ConnectionSignals(contact, interaction);
        Add("businessConnection", 10, ReadinessRules.Present(contact) || ReadinessRules.Present(interaction), signals.Count == 3, signals,
            [new("contact.present", "contact", "Add a contact person and channel", signals.Contains("contact.present")),
             new("interaction.consultation", "interactionFormat", "Specify the consultation format or cadence", signals.Contains("interaction.consultation")),
             new("interaction.feedback", "interactionFormat", "Describe the feedback procedure", signals.Contains("interaction.feedback"))]);

        var total = rows.Sum(row => row.Score);
        return new(total, Level(total), rows.AsReadOnly(), details.AsReadOnly(),
            Array.AsReadOnly(quests.OrderByDescending(q => q.PotentialPoints).ToArray()),
            RatingSources.Rules, ReadinessRules.Version, key, DateTimeOffset.UtcNow);

        void Add(string criterion, int weight, bool present, bool full, IReadOnlyList<string> matched,
            Condition[] conditions, bool englishEvidence = true)
        {
            var score = !present ? 0 : full ? weight : weight / 2;
            var missing = conditions.Where(c => !c.Matched).Take(3).ToArray();
            var reason = full ? $"Full credit: {string.Join(", ", matched)}."
                : (!present ? "Empty (fewer than three non-space characters). " : "Half credit for supplied content. ") +
                  "Missing evidence: " + string.Join("; ", missing.Select(c => $"{c.Signal}: {c.Action}")) + ".";
            if (!full && present && englishEvidence) reason += " " + ReadinessRules.LanguageLimitation;
            rows.Add(new(criterion, weight, score, reason, matched));
            foreach (var condition in missing) details.Add(new(criterion, condition.Action));
            if (score < weight && missing.Length > 0)
                quests.Add(new(criterion, missing[0].Field, missing[0].Action, weight - score));
        }
    }

    private sealed record Condition(string Signal, string Field, string Action, bool Matched);
}
