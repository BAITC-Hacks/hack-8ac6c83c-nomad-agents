using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.InMemory;

public interface IRatingCacheRepository
{
    Rating? Get(string key);
    Rating GetOrAdd(Rating rating);
}

public sealed class RatingCacheRepository(InMemoryDataStore store) : IRatingCacheRepository
{
    public Rating? Get(string key) => store.Collection<Rating>().GetValueOrDefault(key);

    public Rating GetOrAdd(Rating rating)
    {
        lock (store.SyncRoot)
            return store.Collection<Rating>().GetOrAdd(rating.CacheKey, rating);
    }
}
