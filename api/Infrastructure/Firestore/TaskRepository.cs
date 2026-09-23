using Google.Cloud.Firestore;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.Firestore;

public class TaskRepository : FirestoreRepository<TaskCard>
{
    public TaskRepository(FirestoreDb db) : base(db, "tasks") { }

    public async Task<List<TaskCard>> GetByBusinessAsync(string businessId)
    {
        var docs = await _db.Collection(_collectionName)
            .WhereEqualTo("businessId", businessId)
            .GetSnapshotAsync();
        return docs.Documents.Select(d => d.ConvertTo<TaskCard>()).ToList();
    }

    public async Task<List<TaskCard>> GetPublishedAsync()
    {
        var docs = await _db.Collection(_collectionName)
            .WhereEqualTo("status", "Published")
            .GetSnapshotAsync();
        return docs.Documents.Select(d => d.ConvertTo<TaskCard>()).ToList();
    }
}
