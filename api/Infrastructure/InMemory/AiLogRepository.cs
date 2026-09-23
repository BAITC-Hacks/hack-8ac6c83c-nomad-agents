using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.InMemory;

public interface IAiLogRepository
{
    bool Add(AiLog log);
    AiLog? Get(string id);
    IReadOnlyList<AiLog> ListRecent(int limit = 20, string? taskId = null);
}

public sealed class AiLogRepository(InMemoryDataStore store) : IAiLogRepository
{
    public bool Add(AiLog log)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(log.Id);
        return store.Collection<AiLog>().TryAdd(
            log.Id,
            log with { CreatedAt = log.CreatedAt.ToUniversalTime() });
    }

    public AiLog? Get(string id) => store.Collection<AiLog>().GetValueOrDefault(id);

    public IReadOnlyList<AiLog> ListRecent(int limit = 20, string? taskId = null)
    {
        if (limit <= 0) return [];
        return store.Collection<AiLog>().Values
            .Where(log => taskId is null || log.TaskId.Equals(taskId, StringComparison.Ordinal))
            .OrderByDescending(log => log.CreatedAt)
            .ThenBy(log => log.Id)
            .Take(limit)
            .ToArray();
    }
}
