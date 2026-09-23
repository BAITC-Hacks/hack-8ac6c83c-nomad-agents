using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TaskForge.Api.Domain;
using TaskForge.Api.Features.Ai;
using TaskForge.Api.Features.Rating;
using TaskForge.Api.Infrastructure.InMemory;

namespace TaskForge.Api.Features.Tasks;

public sealed record ServiceResult<T>(T? Value, int Status = 200,
    IReadOnlyDictionary<string, string[]>? Errors = null, string? Detail = null)
{
    public bool Ok => Status is >= 200 and < 300;
}

public sealed class TaskService(ITaskRepository tasks, AnalysisService analysis, RatingService rating,
    IRatingCacheRepository ratingCache)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly string[] FieldKeys =
        ["title", "context", "need", "users", "data", "constraints", "expectedResult", "successCriteria", "contact", "interactionFormat"];

    public ServiceResult<TaskDto> Create(string businessId, CreateTaskRequest request)
    {
        var errors = ValidateCreate(request);
        if (errors.Count > 0) return Invalid<TaskDto>(errors);
        var now = DateTimeOffset.UtcNow;
        var task = new TaskCard($"task-{Guid.NewGuid():N}", businessId, TaskStatuses.Editing,
            request.RawDraft!.Trim(), request.Industry!.Trim(), TaskFields.Empty,
            new Dictionary<string, string>(), false, null, null, 0, null, true, null, [], 0,
            now, now, null);
        return tasks.Add(task) ? new(TaskDto.Owner(task), 201) :
            new(null, 409, Detail: "Could not allocate a unique task ID.");
    }

    public async Task<ServiceResult<AnalysisDto>> AnalyzeAsync(string id, string businessId, CancellationToken token)
    {
        var task = tasks.Get(id);
        if (task is null) return NotFound<AnalysisDto>();
        if (task.BusinessId != businessId) return Forbidden<AnalysisDto>();
        var analyzed = await analysis.AnalyzeAsync(task, token);
        var updated = tasks.TryUpdate(id, task.Revision, current => current with
        {
            Analysis = analyzed.ToDomain(),
            Revision = current.Revision + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        return updated.Outcome switch
        {
            RepositoryOutcome.Success => new(analyzed),
            RepositoryOutcome.NotFound => NotFound<AnalysisDto>(),
            _ => Conflict<AnalysisDto>("The task changed while analysis was running. Analyze it again.")
        };
    }

    public ServiceResult<TaskDto> ApplyAnswers(string id, string businessId, ApplyAnswersRequest request)
    {
        var task = tasks.Get(id);
        if (task is null) return NotFound<TaskDto>();
        if (task.BusinessId != businessId) return Forbidden<TaskDto>();
        if (task.Analysis is null) return Invalid<TaskDto>(new Dictionary<string, string[]> { ["analysis"] = ["Analyze the task before applying answers."] });
        var errors = ValidateAnswers(request, task.Analysis);
        if (errors.Count > 0) return Invalid<TaskDto>(errors);
        var submitted = request.Answers!.ToDictionary(a => a.QuestionId!.Trim(), StringComparer.Ordinal);
        var canonical = task.Analysis.Questions.Select(q =>
        {
            var answer = submitted[q.Id];
            return new { questionId = q.Id, fieldKey = q.FieldKey, text = answer.Text!.Trim() };
        }).ToArray();
        var hash = Hash(JsonSerializer.Serialize(canonical, Json));
        if (task.AnswersApplied)
            return task.AppliedAnswersHash == hash ? new(TaskDto.Owner(task, Position(task))) :
                Conflict<TaskDto>("Answers were already applied with different content. Edit the card fields instead.");

        var byQuestion = canonical.ToDictionary(a => a.questionId, StringComparer.Ordinal);
        var fields = task.Fields;
        var provenance = new Dictionary<string, string>(task.FieldProvenance, StringComparer.Ordinal);
        foreach (var group in task.Analysis.Questions.Select(q => (Question: q, Answer: byQuestion[q.Id]))
                     .Where(x => x.Answer.text.Length > 0).GroupBy(x => x.Question.FieldKey))
        {
            var added = string.Join("\n", group.Select(x => x.Answer.text));
            fields = Set(fields, group.Key, Append(Get(fields, group.Key), added));
            provenance[group.Key] = group.All(x => x.Question.Chips.Contains(x.Answer.text, StringComparer.Ordinal))
                ? FieldProvenanceValues.Chip : FieldProvenanceValues.User;
        }
        errors = ValidateFields(fields);
        if (errors.Count > 0) return Invalid<TaskDto>(errors);
        var result = tasks.TryUpdate(id, task.Revision, current => current with
        {
            Fields = fields, FieldProvenance = provenance, AnswersApplied = true,
            AppliedAnswersHash = hash, Revision = current.Revision + 1,
            HasUnconfirmedChanges = true, UpdatedAt = DateTimeOffset.UtcNow
        });
        return Repository(result);
    }

    public ServiceResult<TaskDto> UpdateFields(string id, string businessId, UpdateFieldsRequest request)
    {
        var task = tasks.Get(id);
        if (task is null) return NotFound<TaskDto>();
        if (task.BusinessId != businessId) return Forbidden<TaskDto>();
        if (request.Fields is null && (request.AcceptedExtracts is null || request.AcceptedExtracts.Count == 0))
            return Invalid<TaskDto>(new Dictionary<string, string[]> { ["fields"] = ["Provide at least one field or accepted extract."] });
        var fields = ApplyPatch(task.Fields, request.Fields);
        var provenance = new Dictionary<string, string>(task.FieldProvenance, StringComparer.Ordinal);
        if (request.Fields is not null)
            foreach (var key in PatchedKeys(request.Fields).Where(key => !FieldValueEqual(task.Fields, fields, key)))
                provenance[key] = FieldProvenanceValues.User;
        var extractErrors = ApplyExtracts(task, request.AcceptedExtracts, ref fields, provenance);
        var errors = ValidateFields(fields);
        foreach (var pair in extractErrors) errors[pair.Key] = pair.Value;
        if (errors.Count > 0) return Invalid<TaskDto>(errors);
        if (FieldsEqual(fields, task.Fields) && DictionaryEqual(provenance, task.FieldProvenance))
            return new(TaskDto.Owner(task, Position(task)));
        var result = tasks.TryUpdate(id, task.Revision, current => current with
        {
            Fields = fields, FieldProvenance = provenance, Revision = current.Revision + 1,
            HasUnconfirmedChanges = true, UpdatedAt = DateTimeOffset.UtcNow
        });
        return Repository(result);
    }

    public ServiceResult<ConfirmTaskResponse> Confirm(string id, string businessId)
    {
        var captured = tasks.Get(id);
        if (captured is null) return NotFound<ConfirmTaskResponse>();
        if (captured.BusinessId != businessId) return Forbidden<ConfirmTaskResponse>();
        var fieldErrors = ValidateFields(captured.Fields);
        if (fieldErrors.Count > 0) return Invalid<ConfirmTaskResponse>(fieldErrors);
        var calculated = rating.Calculate(captured.Fields, DateTimeOffset.UtcNow);
        var cacheKey = calculated.CacheKey;
        var unchanged = captured.Confirmed?.Hash == cacheKey && captured.Rating is not null;
        if (unchanged)
        {
            var unchangedRating = captured.Rating! with { Source = RatingSources.Cache };
            return new(new(TaskDto.Owner(captured, Position(captured)), RatingDto.FromDomain(unchangedRating), 0, Position(captured)));
        }
        var cached = ratingCache.Get(cacheKey);
        var scored = cached is not null
            ? cached with { Source = RatingSources.Cache }
            : ratingCache.GetOrAdd(calculated);
        var prior = captured.Rating;
        var history = captured.RatingHistory.Append(new RatingHistoryEntry(scored.Total, scored.Level, scored.ScoredAt)).TakeLast(10).ToArray();
        var snapshot = new ConfirmedTaskSnapshot(captured.Fields, cacheKey, captured.Revision, DateTimeOffset.UtcNow);
        var result = tasks.TryConfirm(id, captured.Revision, snapshot, scored, history, DateTimeOffset.UtcNow);
        if (!result.IsSuccess) return result.Outcome == RepositoryOutcome.NotFound
            ? NotFound<ConfirmTaskResponse>() : Conflict<ConfirmTaskResponse>("The task changed while it was being scored. Confirm again.");
        var saved = result.Value!;
        var position = Position(saved);
        return new(new(TaskDto.Owner(saved, position), RatingDto.FromDomain(scored),
            prior is null ? 0 : scored.Total - prior.Total, position));
    }

    public ServiceResult<TaskDto> Publish(string id, string businessId)
    {
        var task = tasks.Get(id);
        if (task is null) return NotFound<TaskDto>();
        if (task.BusinessId != businessId) return Forbidden<TaskDto>();
        Dictionary<string, string[]> errors = [];
        if (task.Confirmed is null || task.Rating is null) errors["rating"] = ["Confirm and score the task before publishing."];
        if (task.Confirmed is not null && string.IsNullOrWhiteSpace(task.Confirmed.Fields.Title)) errors["title"] = ["A confirmed title is required to publish."];
        if (task.HasUnconfirmedChanges) errors["revision"] = ["Confirm the latest changes before publishing."];
        if (errors.Count > 0) return Invalid<TaskDto>(errors);
        if (task.Status == TaskStatuses.Published) return new(TaskDto.Owner(task, Position(task)));
        var result = tasks.TryUpdate(id, task.Revision, current => current with
        {
            Status = TaskStatuses.Published, Revision = current.Revision + 1,
            PublishedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        });
        return Repository(result);
    }

    public CatalogPositionDto? Position(TaskCard task)
    {
        if (task.Status != TaskStatuses.Published || task.Rating is null || task.Confirmed is null) return null;
        var ordered = tasks.ListPublished().Where(t => t.Rating is not null && t.Confirmed is not null)
            .OrderByDescending(t => t.Rating!.Total)
            .ThenByDescending(t => t.Confirmed?.ConfirmedAt ?? t.PublishedAt)
            .ThenBy(t => t.Id, StringComparer.Ordinal).ToArray();
        var index = Array.FindIndex(ordered, t => t.Id == task.Id);
        return index < 0 ? null : new(index + 1, ordered.Length);
    }

    private ServiceResult<TaskDto> Repository(RepositoryResult<TaskCard> result) => result.Outcome switch
    {
        RepositoryOutcome.Success => new(TaskDto.Owner(result.Value!, Position(result.Value!))),
        RepositoryOutcome.NotFound => NotFound<TaskDto>(),
        _ => Conflict<TaskDto>("The task changed. Reload it and try again.")
    };

    private static Dictionary<string, string[]> ValidateCreate(CreateTaskRequest request)
    {
        Dictionary<string, string[]> errors = [];
        var length = request.RawDraft?.Trim().Length ?? 0;
        if (length is < 20 or > 4000) errors["rawDraft"] = ["Raw draft must be between 20 and 4000 characters."];
        if (string.IsNullOrWhiteSpace(request.Industry) || request.Industry.Trim().Length > 120)
            errors["industry"] = ["Industry is required and must be at most 120 characters."];
        return errors;
    }

    private static Dictionary<string, string[]> ValidateAnswers(ApplyAnswersRequest request, TaskAnalysis analysis)
    {
        Dictionary<string, string[]> errors = [];
        if (request.Answers is null || request.Answers.Count == 0) { errors["answers"] = ["Answers are required."]; return errors; }
        if (request.Answers.Count != analysis.Questions.Count) errors["answers"] = ["Submit exactly one answer for every analysis question."];
        var expected = analysis.Questions.ToDictionary(q => q.Id, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var answer in request.Answers)
        {
            var id = answer.QuestionId?.Trim() ?? "";
            var key = answer.FieldKey?.Trim() ?? "";
            if (!seen.Add(id)) { errors["answers"] = ["Question IDs must be unique."]; continue; }
            if (!expected.TryGetValue(id, out var question) || question.FieldKey != key)
                errors["answers"] = ["Each question ID and field key must match the current analysis."];
            if (answer.Text is null || answer.Text.Length > 2000) errors["answers"] = ["Each answer must be at most 2000 characters."];
        }
        return errors;
    }

    private static Dictionary<string, string[]> ValidateFields(TaskFields f)
    {
        Dictionary<string, string[]> errors = [];
        Check("title", f.Title, 120); Check("context", f.Context, 2000); Check("need", f.Need, 2000);
        Check("users", f.Users, 1000); Check("data", f.Data, 2000); Check("constraints", f.Constraints, 1500);
        Check("expectedResult", f.ExpectedResult, 1500); Check("successCriteria", f.SuccessCriteria, 1500);
        Check("contact", f.Contact, 200); Check("interactionFormat", f.InteractionFormat, 1000);
        Tags("topics", f.Topics, 5); Tags("techTags", f.TechTags, 8);
        return errors;
        void Check(string key, string? value, int max) { if (value is null || value.Length > max) errors[key] = [$"{key} must be at most {max} characters."]; }
        void Tags(string key, IReadOnlyList<string>? values, int max)
        {
            if (values is null || values.Count > max || values.Any(v => string.IsNullOrWhiteSpace(v) || v.Trim().Length > 50))
                errors[key] = [$"{key} must contain at most {max} nonempty tags of at most 50 characters."];
        }
    }

    private static Dictionary<string, string[]> ApplyExtracts(TaskCard task, IReadOnlyList<AcceptedExtractionDto>? accepted,
        ref TaskFields fields, Dictionary<string, string> provenance)
    {
        Dictionary<string, string[]> errors = [];
        if (accepted is null) return errors;
        foreach (var item in accepted)
        {
            var match = task.Analysis?.Extracted.FirstOrDefault(e => e.FieldKey == item.FieldKey && e.Value == item.Value && e.Evidence == item.Evidence);
            if (match is null || !FieldKeys.Contains(item.FieldKey)) { errors["acceptedExtracts"] = ["Accepted extracts must exactly match the current grounded analysis."]; continue; }
            fields = Set(fields, match.FieldKey, Append(Get(fields, match.FieldKey), match.Value));
            provenance[match.FieldKey] = FieldProvenanceValues.DraftExtract;
        }
        return errors;
    }

    private static TaskFields ApplyPatch(TaskFields f, TaskFieldsPatch? p) => p is null ? f : f with
    {
        Title = p.Title ?? f.Title, Context = p.Context ?? f.Context, Need = p.Need ?? f.Need,
        Users = p.Users ?? f.Users, Data = p.Data ?? f.Data, Constraints = p.Constraints ?? f.Constraints,
        ExpectedResult = p.ExpectedResult ?? f.ExpectedResult, SuccessCriteria = p.SuccessCriteria ?? f.SuccessCriteria,
        Contact = p.Contact ?? f.Contact, InteractionFormat = p.InteractionFormat ?? f.InteractionFormat,
        Topics = p.Topics is null ? f.Topics : NormalizeTags(p.Topics),
        TechTags = p.TechTags is null ? f.TechTags : NormalizeTags(p.TechTags)
    };

    private static IEnumerable<string> PatchedKeys(TaskFieldsPatch p)
    {
        if (p.Title is not null) yield return "title"; if (p.Context is not null) yield return "context";
        if (p.Need is not null) yield return "need"; if (p.Users is not null) yield return "users";
        if (p.Data is not null) yield return "data"; if (p.Constraints is not null) yield return "constraints";
        if (p.ExpectedResult is not null) yield return "expectedResult"; if (p.SuccessCriteria is not null) yield return "successCriteria";
        if (p.Contact is not null) yield return "contact"; if (p.InteractionFormat is not null) yield return "interactionFormat";
        if (p.Topics is not null) yield return "topics"; if (p.TechTags is not null) yield return "techTags";
    }

    private static string[] NormalizeTags(IReadOnlyList<string> tags) => tags.Select(t => t.Trim()).Where(t => t.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    private static string Append(string current, string added) => string.IsNullOrWhiteSpace(current) ? added : $"{current}\n{added}";
    private static string Get(TaskFields f, string key) => key switch { "title" => f.Title, "context" => f.Context, "need" => f.Need, "users" => f.Users, "data" => f.Data, "constraints" => f.Constraints, "expectedResult" => f.ExpectedResult, "successCriteria" => f.SuccessCriteria, "contact" => f.Contact, "interactionFormat" => f.InteractionFormat, _ => "" };
    private static TaskFields Set(TaskFields f, string key, string value) => key switch { "title" => f with { Title = value }, "context" => f with { Context = value }, "need" => f with { Need = value }, "users" => f with { Users = value }, "data" => f with { Data = value }, "constraints" => f with { Constraints = value }, "expectedResult" => f with { ExpectedResult = value }, "successCriteria" => f with { SuccessCriteria = value }, "contact" => f with { Contact = value }, "interactionFormat" => f with { InteractionFormat = value }, _ => f };
    private static bool DictionaryEqual(IReadOnlyDictionary<string, string> a, IReadOnlyDictionary<string, string> b) => a.Count == b.Count && a.All(x => b.TryGetValue(x.Key, out var v) && v == x.Value);
    private static bool FieldValueEqual(TaskFields a, TaskFields b, string key) => key switch
    {
        "topics" => a.Topics.SequenceEqual(b.Topics),
        "techTags" => a.TechTags.SequenceEqual(b.TechTags),
        _ => Get(a, key) == Get(b, key)
    };
    private static bool FieldsEqual(TaskFields a, TaskFields b) =>
        a.Title == b.Title && a.Context == b.Context && a.Need == b.Need && a.Users == b.Users &&
        a.Data == b.Data && a.Constraints == b.Constraints && a.ExpectedResult == b.ExpectedResult &&
        a.SuccessCriteria == b.SuccessCriteria && a.Contact == b.Contact && a.InteractionFormat == b.InteractionFormat &&
        a.Topics.SequenceEqual(b.Topics) && a.TechTags.SequenceEqual(b.TechTags);
    private static string Hash(string input) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    private static ServiceResult<T> Invalid<T>(IReadOnlyDictionary<string, string[]> errors) => new(default, 400, errors);
    private static ServiceResult<T> NotFound<T>() => new(default, 404, Detail: "Task not found.");
    private static ServiceResult<T> Forbidden<T>() => new(default, 403, Detail: "Only the business that owns this task may modify it.");
    private static ServiceResult<T> Conflict<T>(string detail) => new(default, 409, Detail: detail);
}
