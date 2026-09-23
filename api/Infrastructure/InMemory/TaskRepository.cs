using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.InMemory;

public interface ITaskRepository
{
    TaskCard? Get(string id);
    IReadOnlyList<TaskCard> List();
    IReadOnlyList<TaskCard> ListByBusiness(string businessId);
    IReadOnlyList<TaskCard> ListPublished();
    bool Add(TaskCard task);
    void Upsert(TaskCard task);
    RepositoryResult<TaskCard> TryUpdate(string id, int expectedRevision, Func<TaskCard, TaskCard> update);
    RepositoryResult<TaskCard> TryConfirm(
        string id,
        int expectedRevision,
        ConfirmedTaskSnapshot confirmed,
        Rating rating,
        IReadOnlyList<RatingHistoryEntry> ratingHistory,
        DateTimeOffset updatedAt);
}

public sealed class TaskRepository(InMemoryDataStore store) : ITaskRepository
{
    public TaskCard? Get(string id) => store.Collection<TaskCard>().GetValueOrDefault(id);

    public IReadOnlyList<TaskCard> List() =>
        store.Collection<TaskCard>().Values.OrderBy(task => task.Id).ToArray();

    public IReadOnlyList<TaskCard> ListByBusiness(string businessId) =>
        store.Collection<TaskCard>().Values
            .Where(task => task.BusinessId.Equals(businessId, StringComparison.Ordinal))
            .OrderByDescending(task => task.UpdatedAt)
            .ThenBy(task => task.Id)
            .ToArray();

    public IReadOnlyList<TaskCard> ListPublished() =>
        store.Collection<TaskCard>().Values
            .Where(task => task.Status == TaskStatuses.Published)
            .OrderBy(task => task.Id)
            .ToArray();

    public bool Add(TaskCard task)
    {
        Validate(task);
        return store.Collection<TaskCard>().TryAdd(task.Id, Normalize(task));
    }

    public void Upsert(TaskCard task)
    {
        Validate(task);
        store.Collection<TaskCard>()[task.Id] = Normalize(task);
    }

    public RepositoryResult<TaskCard> TryUpdate(
        string id,
        int expectedRevision,
        Func<TaskCard, TaskCard> update)
    {
        lock (store.SyncRoot)
        {
            var tasks = store.Collection<TaskCard>();
            if (!tasks.TryGetValue(id, out var current)) return RepositoryResult<TaskCard>.NotFound();
            if (current.Revision != expectedRevision) return RepositoryResult<TaskCard>.Conflict(current);

            var updated = update(current);
            if (!updated.Id.Equals(current.Id, StringComparison.Ordinal)
                || !updated.BusinessId.Equals(current.BusinessId, StringComparison.Ordinal))
                return RepositoryResult<TaskCard>.Conflict(current);

            updated = updated with { CreatedAt = current.CreatedAt };

            Validate(updated);
            updated = Normalize(updated);
            tasks[id] = updated;
            return RepositoryResult<TaskCard>.Success(updated);
        }
    }

    public RepositoryResult<TaskCard> TryConfirm(
        string id,
        int expectedRevision,
        ConfirmedTaskSnapshot confirmed,
        Rating rating,
        IReadOnlyList<RatingHistoryEntry> ratingHistory,
        DateTimeOffset updatedAt)
    {
        if (confirmed.Revision != expectedRevision)
        {
            var current = Get(id);
            return current is null
                ? RepositoryResult<TaskCard>.NotFound()
                : RepositoryResult<TaskCard>.Conflict(current);
        }

        return TryUpdate(id, expectedRevision, current => current with
        {
            Confirmed = confirmed with { ConfirmedAt = Utc(confirmed.ConfirmedAt) },
            Rating = Normalize(rating),
            RatingHistory = ratingHistory
                .TakeLast(10)
                .Select(item => item with { ScoredAt = Utc(item.ScoredAt) })
                .ToArray(),
            Revision = current.Revision + 1,
            HasUnconfirmedChanges = false,
            UpdatedAt = Utc(updatedAt)
        });
    }

    private static void Validate(TaskCard task)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(task.BusinessId);
        if (!TaskStatuses.IsValid(task.Status))
            throw new ArgumentException("Task status must be 'editing' or 'published'.", nameof(task));
        if (task.Revision < 0) throw new ArgumentOutOfRangeException(nameof(task), "Revision cannot be negative.");
    }

    private static TaskCard Normalize(TaskCard task) => task with
    {
        CreatedAt = Utc(task.CreatedAt),
        UpdatedAt = Utc(task.UpdatedAt),
        PublishedAt = task.PublishedAt is { } publishedAt ? Utc(publishedAt) : null,
        Analysis = task.Analysis is { } analysis
            ? analysis with { CreatedAt = Utc(analysis.CreatedAt) }
            : null,
        Confirmed = task.Confirmed is { } confirmed
            ? confirmed with { ConfirmedAt = Utc(confirmed.ConfirmedAt) }
            : null,
        Rating = task.Rating is { } rating ? Normalize(rating) : null,
        RatingHistory = task.RatingHistory
            .TakeLast(10)
            .Select(item => item with { ScoredAt = Utc(item.ScoredAt) })
            .ToArray()
    };

    private static Rating Normalize(Rating rating) =>
        rating with { ScoredAt = Utc(rating.ScoredAt) };

    private static DateTimeOffset Utc(DateTimeOffset value) => value.ToUniversalTime();
}
