using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.InMemory;

public interface IActorRepository
{
    Business? GetBusiness(string id);
    Team? GetTeam(string id);
    IReadOnlyList<Business> ListBusinesses();
    IReadOnlyList<Team> ListTeams();
    void Upsert(Business business);
    void Upsert(Team team);
}

public sealed class ActorRepository : IActorRepository
{
    private readonly InMemoryDataStore store;

    public ActorRepository(InMemoryDataStore store)
    {
        this.store = store;
        EnsureDemoActors();
    }

    public Business? GetBusiness(string id) =>
        store.Collection<Business>().GetValueOrDefault(id);

    public Team? GetTeam(string id) =>
        store.Collection<Team>().GetValueOrDefault(id);

    public IReadOnlyList<Business> ListBusinesses() =>
        store.Collection<Business>().Values.OrderBy(actor => actor.Name).ToArray();

    public IReadOnlyList<Team> ListTeams() =>
        store.Collection<Team>().Values.OrderBy(actor => actor.Name).ToArray();

    public void Upsert(Business business) =>
        store.Collection<Business>()[business.Id] = business with
        {
            CreatedAt = NormalizeCreatedAt(business.CreatedAt)
        };

    public void Upsert(Team team) =>
        store.Collection<Team>()[team.Id] = team with
        {
            CreatedAt = NormalizeCreatedAt(team.CreatedAt)
        };

    private static DateTimeOffset NormalizeCreatedAt(DateTimeOffset value) =>
        value == default ? DateTimeOffset.UtcNow : value.ToUniversalTime();

    private void EnsureDemoActors()
    {
        lock (store.SyncRoot)
        {
            var businesses = store.Collection<Business>();
            var createdAt = DateTimeOffset.UtcNow;
            businesses.TryAdd("b-nomad", new Business("b-nomad", "Nomad Logistics", "Logistics", "Aigerim", createdAt));
            businesses.TryAdd("b-steppe", new Business("b-steppe", "Steppe Retail", "Retail", "Dana", createdAt));
            businesses.TryAdd("b-tamaq", new Business("b-tamaq", "Tamaq Café Chain", "Food & Beverage", "Mira", createdAt));

            var teams = store.Collection<Team>();
            teams.TryAdd("t-bytenomads", new Team(
                "t-bytenomads",
                "Byte Nomads",
                ["logistics", "data-analytics", "food"],
                ["react", "python", "ml"],
                ["frontend", "data science", "machine learning"],
                CreatedAt: createdAt));
            teams.TryAdd("t-nullptr", new Team(
                "t-nullptr",
                "Null Pointers",
                ["retail", "nlp", "food"],
                ["dotnet", "openai", "react"],
                ["backend", "prompt engineering", "frontend"],
                CreatedAt: createdAt));
        }
    }
}
