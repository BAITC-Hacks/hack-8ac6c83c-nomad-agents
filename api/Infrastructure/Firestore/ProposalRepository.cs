using Google.Cloud.Firestore;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.Firestore;

public class ProposalRepository : FirestoreRepository<Proposal>
{
    public ProposalRepository(FirestoreDb db) : base(db, "proposals") { }

    public async Task<Proposal?> GetByTaskAndTeamAsync(string taskId, string teamId)
    {
        var proposalId = $"{taskId}_{teamId}";
        return await GetAsync(proposalId);
    }

    public async Task<List<Proposal>> GetByTaskAsync(string taskId)
    {
        var docs = await _db.Collection(_collectionName)
            .WhereEqualTo("taskId", taskId)
            .GetSnapshotAsync();
        return docs.Documents.Select(d => d.ConvertTo<Proposal>()).ToList();
    }

    public async Task<List<Proposal>> GetByTeamAsync(string teamId)
    {
        var docs = await _db.Collection(_collectionName)
            .WhereEqualTo("teamId", teamId)
            .GetSnapshotAsync();
        return docs.Documents.Select(d => d.ConvertTo<Proposal>()).ToList();
    }
}
