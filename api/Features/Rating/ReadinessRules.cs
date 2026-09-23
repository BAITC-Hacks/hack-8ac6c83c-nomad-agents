using System.Text;
using System.Text.RegularExpressions;

namespace TaskForge.Api.Features.Rating;

/// <summary>Finite English evidence detectors from MVP_SPEC §5.3. Vocabulary changes require a new version.</summary>
public static class ReadinessRules
{
    public const string Version = "en-mvp-1";
    public const string LanguageLimitation = "Evidence detectors support the en-mvp-1 English vocabulary only; unsupported or ambiguous wording receives at most half credit.";

    private const string Number = @"\b\d+(?:[.,]\d+)?";
    private const string Source = @"\b(?:logs?|transactions?|purchase histor(?:y|ies)|reviews?|orders?|records?|datasets?|databases?|files?|documents?|surveys?|route data)\b";
    private const string Format = @"\b(?:csv|xlsx|excel|json|pdf|sql|apis?)\b";
    private const string Quantity = Number + @"\s*[km]?\s+(?:rows?|records?|files?|months?|years?|gb|mb)\b";
    private const string DataAccess = @"\b(?:read-only|exports?|shared folders?|api access|replicas?|sample provided)\b";
    private const string Artifact = @"\b(?:dashboards?|apps?|websites?|reports?|models?|prototypes?|apis?|tools?|services?)\b";
    private const string Function = @"\b(?:predicts?|displays?|tracks?|classif(?:y|ies)|summariz(?:e|es)|alerts?|search(?:es)?|recommends?|analyz(?:e|es))\b";
    private const string Outcome = @"\b(?:predicts? delays?|accuracy|precision|recall|conversion|response time|error rate|completion rate)\b";
    private const string Acceptance = @"(?:\b(?:at least|no more than|under|over)\s+|>=\s*|<=\s*|≥\s*|≤\s*)\d+(?:[.,]\d+)?\s*(?:%|\b(?:seconds?|minutes?|days?|users?|records?)\b)";
    private const string Group = @"\b(?:dispatchers?|customers?|students?|teachers?|analysts?|managers?|operators?|support agents?)\b";
    private const string Usage = @"\b(?:uses? to|reviews?|monitors?|enters?|decides?|approves?|receives? alerts)\b";
    private const string Consultation = @"\b(?:calls?|meetings?|consultations?|office hours|weekly syncs?)\b";
    private const string Feedback = @"\b(?:feedback|reviews?|comments?|approves?|acceptance sessions?)\b";
    private const string Obligation = @"\b(?:must|use|only|required)\b";
    private const string LegalObligation = @"\b(?:must|required|only)\b";
    private const string Technology = @"(?:\b(?:react|python|java|sql)\b|(?:^|\s)\.net\b)";
    private const string Legal = @"\b(?:nda|gdpr|consent|anonymized)\b";
    private const string Deadline = Number + @"\s+(?:days?|weeks?|months?)\b|\b\d{4}-\d{2}-\d{2}\b|\b\d{1,2}[/]\d{1,2}[/]\d{4}\b|\b(?:january|february|march|april|may|june|july|august|september|october|november|december)\s+\d{1,2},?\s+\d{4}\b";
    private const string ConstraintAccess = @"\b(?:read-only|no production access|offline)\b";
    private const string BudgetWords = @"\b(?:budget|cap|maximum)\b";
    private const string Money = @"(?:[$€£₸]\s*\d+(?:[.,]\d+)?|\b\d+(?:[.,]\d+)?\s*(?:usd|eur|gbp|kzt|dollars?|euros?|pounds?|tenge)\b)";
    private const string ExplicitCheck = @"\b(?:accepted if|passes)\s+(.+)";

    private static bool Has(string text, string pattern) => Regex.IsMatch(text, pattern,
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    public static string Normalize(string? value) => Regex.Replace(
        (value ?? "").Normalize(NormalizationForm.FormKC).ToLowerInvariant(), @"\s+", " ").Trim();

    public static bool Present(string text) => text.Count(c => !char.IsWhiteSpace(c)) >= 3;

    public static IReadOnlyList<string> DataSignals(string text)
    {
        List<string> signals = [];
        if (Has(text, Source) || NamedApi(text)) signals.Add("data.source");
        if (Has(text, Format)) signals.Add("data.format");
        if (Has(text, Quantity)) signals.Add("data.quantity");
        if (Has(text, DataAccess)) signals.Add("data.access");
        return signals.AsReadOnly();
    }

    // API is both a source and format term in the spec, but a format token alone is insufficient.
    // Require a named API (e.g. weather API), not merely "API", "JSON API", or "via API".
    private static bool NamedApi(string text) => Regex.Matches(text, @"\b([a-z][a-z0-9-]*) apis?\b")
        .Any(m => !new[] { "a", "an", "the", "via", "with", "from", "and", "or", "csv", "xlsx", "excel", "json", "pdf", "sql", "use", "using", "format" }
            .Concat(["no", "not", "without", "only", "required"])
            .Contains(m.Groups[1].Value, StringComparer.Ordinal));

    public static IReadOnlyList<string> ResultSignals(string text) => Signals(
        ("result.artifact", Has(text, Artifact)), ("result.function", Linked(text, Artifact, Function)));

    public static IReadOnlyList<string> UserSignals(string text) => Signals(
        ("users.group", Has(text, Group)), ("users.usage", Linked(text, Group, Usage)));

    public static IReadOnlyList<string> SuccessSignals(string text) => Signals(
        ("success.outcome", Has(text, Outcome)),
        ("success.acceptance", Has(text, Acceptance) || ConcreteCheck(text)));

    public static IReadOnlyList<string> ConstraintSignals(string text) => Signals(
        ("constraints.time", Has(text, Deadline)),
        ("constraints.technology", SameClause(text, Technology, Obligation)),
        ("constraints.access", Has(text, ConstraintAccess)),
        ("constraints.legal", SameClause(text, Legal, LegalObligation)),
        ("constraints.budget", SameClause(text, Money, BudgetWords)));

    public static IReadOnlyList<string> ConnectionSignals(string contact, string interaction) => Signals(
        ("contact.present", Present(contact)),
        ("interaction.consultation", Has(interaction, Consultation)),
        ("interaction.feedback", Has(interaction, Feedback)));

    private static IReadOnlyList<string> Signals(params (string Id, bool Matched)[] signals) =>
        Array.AsReadOnly(signals.Where(s => s.Matched).Select(s => s.Id).ToArray());

    private static string[] Clauses(string text) => Regex.Split(text, @"[;!?]|\.(?:\s|$)");

    private static bool SameClause(string text, string first, string second) =>
        Clauses(text).Any(c => Has(c, first) && Has(c, second));

    // An action must follow its subject in the same short clause. Unrelated sentences do not qualify.
    private static bool Linked(string text, string subject, string action) => Clauses(text).Any(clause =>
        Regex.Matches(clause, subject).Any(s => Regex.Matches(clause, action).Any(a =>
            a.Index >= s.Index + s.Length && a.Index - s.Index - s.Length <= 80 &&
            !Has(clause.Substring(s.Index + s.Length, a.Index - s.Index - s.Length), @"\b(?:not|never|cannot|can't|won't)\b"))));

    private static bool ConcreteCheck(string text) => Clauses(text).Any(clause =>
    {
        var match = Regex.Match(clause, ExplicitCheck);
        if (!match.Success) return false;
        var check = match.Groups[1].Value;
        // Require a deliverable + function, or a named metric with an explicit threshold.
        // "passes"/"accepted if good" and bare numbers are not concrete checks.
        return Linked(check, Artifact, Function) || (Has(check, Outcome) && Has(check, Acceptance)) ||
            Has(check, @"\b(?:tests?|checks?)\b") && Has(check, Artifact) && Has(check, Function);
    });
}
