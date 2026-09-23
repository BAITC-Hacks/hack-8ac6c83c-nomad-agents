using Google.Cloud.Firestore;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.Firestore;

public class BusinessRepository : FirestoreRepository<Business>
{
    public BusinessRepository(FirestoreDb db) : base(db, "businesses") { }
}

public class TeamRepository : FirestoreRepository<Team>
{
    public TeamRepository(FirestoreDb db) : base(db, "teams") { }
}
