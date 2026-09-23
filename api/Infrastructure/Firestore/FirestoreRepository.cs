using Google.Cloud.Firestore;

namespace TaskForge.Api.Infrastructure.Firestore;

public abstract class FirestoreRepository<T> where T : class
{
    protected readonly FirestoreDb _db;
    protected readonly string _collectionName;

    protected FirestoreRepository(FirestoreDb db, string collectionName)
    {
        _db = db;
        _collectionName = collectionName;
    }

    public async Task<T?> GetAsync(string id)
    {
        var doc = await _db.Collection(_collectionName).Document(id).GetSnapshotAsync();
        return doc.Exists ? doc.ConvertTo<T>() : null;
    }

    public async Task<List<T>> ListAsync()
    {
        var docs = await _db.Collection(_collectionName).GetSnapshotAsync();
        return docs.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }

    public async Task SetAsync(string id, T data)
    {
        await _db.Collection(_collectionName).Document(id).SetAsync(data);
    }

    public async Task UpdateAsync(string id, Dictionary<string, object> updates)
    {
        await _db.Collection(_collectionName).Document(id).UpdateAsync(updates);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Collection(_collectionName).Document(id).DeleteAsync();
    }

    public async Task BatchWriteAsync(WriteBatch batch)
    {
        await batch.CommitAsync();
    }
}
