using Google.Cloud.Firestore;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.Firestore;

public class AiLogRepository : FirestoreRepository<AiLog>
{
    public AiLogRepository(FirestoreDb db) : base(db, "aiLogs") { }

    public async Task<List<AiLog>> GetByTaskAsync(string taskId, int limit = 20)
    {
        var docs = await _db.Collection(_collectionName)
            .WhereEqualTo("taskId", taskId)
            .OrderByDescending("createdAt")
            .Limit(limit)
            .GetSnapshotAsync();
        return docs.Documents.Select(d => d.ConvertTo<AiLog>()).ToList();
    }

    public async Task<List<AiLog>> GetRecentAsync(int limit = 20)
    {
        var docs = await _db.Collection(_collectionName)
            .OrderByDescending("createdAt")
            .Limit(limit)
            .GetSnapshotAsync();
        return docs.Documents.Select(d => d.ConvertTo<AiLog>()).ToList();
    }
}
