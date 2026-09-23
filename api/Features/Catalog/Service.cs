using System.Text.RegularExpressions;
using TaskForge.Api.Domain;
using TaskForge.Api.Features.Rating;
using TaskForge.Api.Infrastructure.InMemory;

namespace TaskForge.Api.Features.Catalog;

public sealed record CatalogResult(
    CatalogResponse? Value,
    IReadOnlyDictionary<string, string[]>? Errors = null);

public sealed class CatalogService(ITaskRepository tasks, IActorRepository actors)
{
    private static readonly HashSet<string> Levels = new(StringComparer.OrdinalIgnoreCase)
    {
        ReadinessLevels.Draft, ReadinessLevels.Workable, ReadinessLevels.Ready, ReadinessLevels.Priority
    };

    public CatalogResult List(IEnumerable<string>? topics, IEnumerable<string>? levels)
    {
        var topicFilter = NormalizeFilter(topics);
        var levelFilter = NormalizeFilter(levels);
        var unknownLevels = levelFilter.Where(level => !Levels.Contains(level)).ToArray();
        if (unknownLevels.Length > 0)
            return new(null, new Dictionary<string, string[]>
            {
                ["level"] = [$"Unknown readiness level(s): {string.Join(", ", unknownLevels)}. Allowed values: draft, workable, ready, priority."]
            });

        var ranked = RankedTasks();
        var total = ranked.Count;
        var items = ranked
            .Where(item => topicFilter.Length == 0 || item.Task.Confirmed!.Fields.Topics.Any(
                topic => topicFilter.Contains(topic, StringComparer.OrdinalIgnoreCase)))
            .Where(item => levelFilter.Length == 0 || levelFilter.Contains(
                item.Task.Rating!.Level, StringComparer.OrdinalIgnoreCase))
            .Select(item => Map(item.Task, item.Position, total))
            .ToArray();
        return new(new(items, total));
    }

    public IReadOnlyList<string> Topics() => RankedTasks()
        .SelectMany(item => item.Task.Confirmed!.Fields.Topics)
        .Select(topic => topic.Trim())
        .Where(topic => topic.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private IReadOnlyList<(TaskCard Task, int Position)> RankedTasks() => tasks.ListPublished()
        .Where(task => task.Confirmed is not null && task.Rating is not null)
        .OrderByDescending(task => task.Rating!.Total)
        .ThenByDescending(task => task.Confirmed?.ConfirmedAt ?? task.PublishedAt)
        .ThenBy(task => task.Id, StringComparer.Ordinal)
        .Select((task, index) => (task, index + 1))
        .ToArray();

    private CatalogItemDto Map(TaskCard task, int position, int total)
    {
        var fields = task.Confirmed!.Fields;
        var rating = task.Rating!;
        return new(task.Id, task.BusinessId, actors.GetBusiness(task.BusinessId)?.Name ?? "Unknown business",
            fields.Title, Summary(fields), task.Status, RatingDto.FromDomain(rating), rating.Total,
            rating.Level, fields.Topics, fields.TechTags, task.ProposalCount, position, position, total,
            rating.Level == ReadinessLevels.Priority, task.HasUnconfirmedChanges, fields);
    }

    private static string Summary(TaskFields fields)
    {
        var context = Clean(fields.Context);
        var need = Clean(fields.Need);
        var summary = context.Length > 0 && need.Length > 0
            ? $"{context} {need}"
            : context.Length > 0 ? context : need;
        if (summary.Length <= 240) return summary;
        var cut = summary.LastIndexOf(' ', 237);
        return summary[..(cut > 80 ? cut : 237)].TrimEnd() + "…";
    }

    private static string Clean(string value) => Regex.Replace(value.Trim(), @"\s+", " ");

    private static string[] NormalizeFilter(IEnumerable<string>? values) => (values ?? [])
        .Select(value => value.Trim())
        .Where(value => value.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
