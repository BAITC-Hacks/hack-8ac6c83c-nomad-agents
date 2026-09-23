using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Rating;

/// <summary>Deterministic, versioned readiness rules. This service accepts fields only;</summary>
/// <summary>callers are responsible for passing the confirmed snapshot for an awarded rating.</summary>
public sealed class RatingService
{
    public const string RulesVersion = "en-mvp-1";

    private static readonly CriterionRule[] Rules =
    [
        new("contextAndNeed", 20, ["context", "need"], ContextNeed),
        new("dataAndMaterials", 20, ["data"], Data),
        new("expectedResult", 15, ["expectedResult"], ExpectedResult),
        new("successCriteria", 15, ["successCriteria"], SuccessCriteria),
        new("constraints", 10, ["constraints"], Constraints),
        new("users", 10, ["users"], Users),
        new("businessConnection", 10, ["contact", "interactionFormat"], BusinessConnection)
    ];

    public TaskForge.Api.Domain.Rating Calculate(TaskFields confirmedFields, DateTimeOffset scoredAt)
    {
        ArgumentNullException.ThrowIfNull(confirmedFields);

        var breakdown = Rules.Select(rule => Evaluate(rule, confirmedFields)).ToArray();
        var total = breakdown.Sum(item => item.Score);
        var level = total switch
        {
            < 40 => ReadinessLevels.Draft,
            < 70 => ReadinessLevels.Workable,
            < 90 => ReadinessLevels.Ready,
            _ => ReadinessLevels.Priority
        };
        var missing = breakdown
            .Where(item => item.Score < item.Weight)
            .Select(item => new MissingDetail(item.Criterion, item.Reason))
            .Take(3)
            .ToArray();
        var quests = breakdown
            .Where(item => item.Score < item.Weight)
            .Select(item => MakeQuest(item, confirmedFields))
            .OrderByDescending(item => item.PotentialPoints)
            .ThenBy(item => Array.FindIndex(Rules, rule => rule.Key == item.Criterion))
            .ToArray();

        return new TaskForge.Api.Domain.Rating(
            total,
            level,
            breakdown,
            missing,
            quests,
            RatingSources.Rules,
            RulesVersion,
            MakeCacheKey(confirmedFields),
            scoredAt);
    }

    private static RatingBreakdownItem Evaluate(CriterionRule rule, TaskFields fields)
    {
        var result = rule.Check(fields);
        var score = result.Score switch
        {
            -1 => rule.Weight,
            -2 => rule.Weight / 2,
            _ => 0
        };
        return new RatingBreakdownItem(rule.Key, rule.Weight, score, result.Reason, result.Signals);
    }

    private static ImprovementQuest MakeQuest(RatingBreakdownItem item, TaskFields fields)
    {
        var key = item.Criterion switch
        {
            "contextAndNeed" => IsEmpty(fields.Context) ? "context" : "need",
            "dataAndMaterials" => "data",
            "expectedResult" => "expectedResult",
            "successCriteria" => "successCriteria",
            "constraints" => "constraints",
            "users" => "users",
            "businessConnection" => IsEmpty(fields.Contact) ? "contact" : "interactionFormat",
            _ => throw new InvalidOperationException("Unknown rating criterion.")
        };
        var action = item.Criterion switch
        {
            "contextAndNeed" => IsEmpty(fields.Context)
                ? "Describe the current situation."
                : "Describe what needs to change.",
            "dataAndMaterials" => "Name an available source and how the team can inspect it.",
            "expectedResult" => "Name the deliverable and what it must do.",
            "successCriteria" => "Define an outcome and a measurable target or acceptance check.",
            "constraints" => "Add another deadline, technology, access, legal, or budget boundary.",
            "users" => "Explain what the named user group does with the result.",
            "businessConnection" => IsEmpty(fields.Contact)
                ? "Add a business contact."
                : "Describe consultation and how feedback will be given.",
            _ => throw new InvalidOperationException("Unknown rating criterion.")
        };
        return new ImprovementQuest(item.Criterion, key, action, item.Weight - item.Score);
    }

    private static CheckResult ContextNeed(TaskFields f)
    {
        var context = Present(f.Context);
        var need = Present(f.Need);
        if (!context && !need) return Zero("Both the current situation and needed change are missing.");
        if (!context) return Half("The current situation is missing.", "need.present");
        if (!need) return Half("What needs to change is missing.", "context.present");
        return Full("Current situation and needed change are present.", "context.present", "need.present");
    }

    private static CheckResult Data(TaskFields f)
    {
        var text = Normalize(f.Data);
        if (IsEmpty(text)) return Zero("Name a data source and an inspection detail.");
        var source = HasPhrase(text, "logs", "transactions", "purchase history", "reviews", "orders", "records", "dataset", "database", "files", "documents", "survey", "api", "route data");
        var format = HasPhrase(text, "csv", "xlsx", "excel", "json", "pdf", "sql", "api");
        var quantity = HasRegex(text, @"\b\d+(?:[.,]\d+)?\s*(?:k|m|b)?\s*(?:rows?|records?|files?|months?|years?|gb|mb)\b");
        var access = HasPhrase(text, "read-only", "export", "shared folder", "api access", "replica", "sample provided");
        var signals = new List<string>();
        if (source) signals.Add("data.source");
        if (format) signals.Add("data.format");
        if (quantity) signals.Add("data.quantity");
        if (access) signals.Add("data.access");
        if (source && (format || quantity || access)) return Full("A data source and an inspection detail are identified.", signals.ToArray());
        var absent = !source ? "a recognizable data source" : "a format, quantity, or access detail";
        return Half($"Nonempty data is present, but the detector could not establish {absent}.", signals.ToArray());
    }

    private static CheckResult ExpectedResult(TaskFields f)
    {
        var text = Normalize(f.ExpectedResult);
        if (IsEmpty(text)) return Zero("Name a deliverable and what it must do.");
        var artifact = HasPhrase(text, "dashboard", "app", "website", "report", "model", "prototype", "api", "tool", "service");
        var function = HasPhrase(text, "predict", "display", "track", "classify", "summarize", "alert", "search", "recommend", "analyze");
        var signals = Signals((artifact, "result.artifact"), (function, "result.function"));
        if (artifact && function) return Full("A deliverable and its function are identified.", signals);
        return Half($"Nonempty result is present, but the detector could not establish {(artifact ? "what the deliverable must do" : "a recognizable deliverable") }.", signals);
    }

    private static CheckResult SuccessCriteria(TaskFields f)
    {
        var text = Normalize(f.SuccessCriteria);
        if (IsEmpty(text)) return Zero("Define an outcome and a measurable target or acceptance check.");
        var outcome = HasPhrase(text, "predict delays", "accuracy", "precision", "recall", "conversion", "response time", "error rate", "completion rate");
        var measured = HasRegex(text, @"\b(?:at least|no more than|under|over|>=|<=|≥|≤)\s*\d+(?:[.,]\d+)?\s*(?:%|seconds?|minutes?|days?|users?|records?)(?:\b|(?=\s|$|[,.;]))");
        var accepted = HasRegex(text, @"\b(?:accepted if|passes)\b.{1,100}\b(?:test|deliverable|check|scenario|case)\b");
        var acceptance = measured || accepted;
        var signals = Signals((outcome, "success.outcome"), (measured, "success.acceptance.measure"), (accepted, "success.acceptance.check"));
        if (outcome && acceptance) return Full("An outcome and measurable acceptance condition are present.", signals);
        var absent = !outcome ? "a recognizable outcome" : "a measurable target or concrete acceptance check";
        return Half($"Nonempty criteria are present, but the detector could not establish {absent}.", signals);
    }

    private static CheckResult Constraints(TaskFields f)
    {
        var text = Normalize(f.Constraints);
        if (IsEmpty(text)) return Zero("State at least one project boundary.");
        var deadline = HasRegex(text, @"\b\d+\s*(?:days?|weeks?|months?)\b") || HasRegex(text, @"\b(?:by|before|until)\s+(?:20\d{2}-\d{2}-\d{2}|\w+\s+\d{1,2}(?:,?\s+20\d{2})?)\b");
        var tech = HasRegex(text, @"\b(?:react|python|\.net|java|sql)\b") && HasPhrase(text, "must", "use", "only", "required");
        var access = HasPhrase(text, "read-only", "no production access", "offline");
        var budget = HasRegex(text, @"(?:\$|€|£)\s*\d+(?:[.,]\d+)?") && HasPhrase(text, "budget", "cap", "maximum");
        var legal = HasPhrase(text, "nda", "gdpr", "consent", "anonymized") && HasPhrase(text, "must", "required", "only");
        var categories = Signals((deadline, "constraint.deadline"), (tech, "constraint.technology"), (access, "constraint.access"), (budget, "constraint.budget"), (legal, "constraint.legal"));
        if (categories.Length >= 2) return Full("Two distinct boundary categories are present.", categories);
        return Half(categories.Length == 1
            ? "One boundary is present; the detector needs a second distinct category for full credit."
            : "Nonempty constraints are present, but no listed boundary category was established.", categories);
    }

    private static CheckResult Users(TaskFields f)
    {
        var text = Normalize(f.Users);
        if (IsEmpty(text)) return Zero("Name a user group and its role or usage situation.");
        var group = HasPhrase(text, "dispatchers", "customers", "students", "teachers", "analysts", "managers", "operators", "support agents");
        var usage = HasPhrase(text, "use to", "review", "monitor", "enter", "decide", "approve", "receive alerts");
        var signals = Signals((group, "users.group"), (usage, "users.usage"));
        if (group && usage) return Full("A user group and its role or usage are present.", signals);
        var absent = !group ? "a listed user group" : "what that group does with the result";
        return Half($"Nonempty user details are present, but the detector could not establish {absent}.", signals);
    }

    private static CheckResult BusinessConnection(TaskFields f)
    {
        var contact = Present(f.Contact);
        var text = Normalize(f.InteractionFormat);
        var consultation = HasPhrase(text, "call", "meeting", "consultation", "office hours", "weekly sync");
        var feedback = HasPhrase(text, "feedback", "review", "comments", "approve", "acceptance session");
        if (!contact && IsEmpty(text)) return Zero("Add a contact and consultation with a feedback procedure.");
        var signals = Signals((contact, "contact.present"), (consultation, "interaction.consultation"), (feedback, "interaction.feedback"));
        if (contact && consultation && feedback) return Full("Contact, consultation, and feedback procedure are present.", signals);
        var absent = !contact ? "a contact" : !consultation ? "a consultation format" : "a feedback procedure";
        return Half($"Some business connection details are present, but the detector could not establish {absent}.", signals);
    }

    private static bool Present(string? value) => !IsEmpty(value);

    private static bool IsEmpty(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return value.EnumerateRunes().Count(rune => !Rune.IsWhiteSpace(rune)) < 3;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var normalized = value.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        return Regex.Replace(normalized, @"\s+", " ", RegexOptions.CultureInvariant).Trim();
    }

    private static bool HasPhrase(string text, params string[] phrases) => phrases.Any(phrase =>
        HasRegex(text, $@"\b{Regex.Escape(phrase)}(?:s|es)?\b"));

    private static bool HasRegex(string text, string pattern) =>
        Regex.IsMatch(text, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static string[] Signals(params (bool Match, string Id)[] signals) =>
        signals.Where(signal => signal.Match).Select(signal => signal.Id).ToArray();

    private static CheckResult Zero(string reason) => new(0, reason, []);
    private static CheckResult Half(string reason, params string[] signals) => new(-2, reason, signals);
    private static CheckResult Full(string reason, params string[] signals) => new(-1, reason, signals);

    private static string MakeCacheKey(TaskFields fields)
    {
        var canonical = string.Join("\n", RulesVersion,
            Normalize(fields.Title), Normalize(fields.Context), Normalize(fields.Need), Normalize(fields.Users),
            Normalize(fields.Data), Normalize(fields.Constraints), Normalize(fields.ExpectedResult),
            Normalize(fields.SuccessCriteria), Normalize(fields.Contact), Normalize(fields.InteractionFormat),
            string.Join("\u001f", fields.Topics.Select(Normalize)),
            string.Join("\u001f", fields.TechTags.Select(Normalize)));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    private sealed record CriterionRule(string Key, int Weight, string[] Fields, Func<TaskFields, CheckResult> Check);
    private sealed record CheckResult(int Score, string Reason, string[] Signals);
}
