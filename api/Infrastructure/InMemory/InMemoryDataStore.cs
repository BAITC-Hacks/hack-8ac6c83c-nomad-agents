using System.Collections;
using System.Collections.Concurrent;

namespace TaskForge.Api.Infrastructure.InMemory;

/// <summary>
/// Process-local storage shared by repositories for the lifetime of the API process.
/// Data is intentionally discarded when the process restarts.
/// </summary>
public sealed class InMemoryDataStore
{
    private readonly ConcurrentDictionary<Type, object> collections = new();

    public object SyncRoot { get; } = new();

    public ConcurrentDictionary<string, T> Collection<T>() where T : class =>
        (ConcurrentDictionary<string, T>)collections.GetOrAdd(
            typeof(T),
            static _ => new ConcurrentDictionary<string, T>(StringComparer.Ordinal));

    public void Clear()
    {
        lock (SyncRoot)
        {
            foreach (var collection in collections.Values)
            {
                ((IDictionary)collection).Clear();
            }
        }
    }
}
