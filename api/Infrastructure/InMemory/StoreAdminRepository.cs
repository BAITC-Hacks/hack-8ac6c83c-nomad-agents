namespace TaskForge.Api.Infrastructure.InMemory;

public interface IStoreAdminRepository
{
    void ClearAll();
}

public sealed class StoreAdminRepository(InMemoryDataStore store) : IStoreAdminRepository
{
    public void ClearAll() => store.Clear();
}
