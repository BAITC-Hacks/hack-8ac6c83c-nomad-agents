using TaskForge.Api.Domain;

namespace TaskForge.Api.Infrastructure.InMemory;

public interface IProposalRepository
{
    Proposal? Get(string id);
    IReadOnlyList<Proposal> List();
    IReadOnlyList<Proposal> ListByTask(string taskId);
    IReadOnlyList<Proposal> ListByTeam(string teamId);
    bool Add(Proposal proposal);
    void Upsert(Proposal proposal);
    RepositoryResult<Proposal> UpsertPending(Proposal proposal);
    RepositoryResult<Proposal> TryUpdate(string id, Func<Proposal, bool> canUpdate, Func<Proposal, Proposal> update);
    RepositoryResult<Proposal> TryDecide(string id, string decision, string? reason, DateTimeOffset decidedAt);
    RepositoryResult<Proposal> TryAddMilestoneAndAwardPoints(string id, Milestone milestone);
}

public sealed class ProposalRepository(InMemoryDataStore store) : IProposalRepository
{
    public Proposal? Get(string id) => store.Collection<Proposal>().GetValueOrDefault(id);

    public IReadOnlyList<Proposal> List() =>
        store.Collection<Proposal>().Values.OrderBy(proposal => proposal.Id).ToArray();

    public IReadOnlyList<Proposal> ListByTask(string taskId) =>
        store.Collection<Proposal>().Values
            .Where(proposal => proposal.TaskId.Equals(taskId, StringComparison.Ordinal))
            .OrderByDescending(proposal => proposal.UpdatedAt)
            .ThenBy(proposal => proposal.Id)
            .ToArray();

    public IReadOnlyList<Proposal> ListByTeam(string teamId) =>
        store.Collection<Proposal>().Values
            .Where(proposal => proposal.TeamId.Equals(teamId, StringComparison.Ordinal))
            .OrderByDescending(proposal => proposal.UpdatedAt)
            .ThenBy(proposal => proposal.Id)
            .ToArray();

    public bool Add(Proposal proposal)
    {
        Validate(proposal);
        return store.Collection<Proposal>().TryAdd(proposal.Id, Normalize(proposal));
    }

    public void Upsert(Proposal proposal)
    {
        Validate(proposal);
        store.Collection<Proposal>()[proposal.Id] = Normalize(proposal);
    }

    public RepositoryResult<Proposal> UpsertPending(Proposal proposal)
    {
        Validate(proposal);
        lock (store.SyncRoot)
        {
            var proposals = store.Collection<Proposal>();
            if (proposals.TryGetValue(proposal.Id, out var current))
            {
                if (current.Status != ProposalStatuses.Pending
                    || !HasSameIdentity(current, proposal))
                    return RepositoryResult<Proposal>.Conflict(current);

                var updated = Normalize(proposal with
                {
                    CreatedAt = current.CreatedAt,
                    Status = current.Status,
                    DecisionReason = current.DecisionReason,
                    DecidedAt = current.DecidedAt,
                    Milestones = current.Milestones
                });
                proposals[proposal.Id] = updated;
                return RepositoryResult<Proposal>.Success(updated);
            }

            var created = Normalize(proposal);
            proposals[proposal.Id] = created;
            return RepositoryResult<Proposal>.Success(created);
        }
    }

    public RepositoryResult<Proposal> TryUpdate(
        string id,
        Func<Proposal, bool> canUpdate,
        Func<Proposal, Proposal> update)
    {
        lock (store.SyncRoot)
        {
            var proposals = store.Collection<Proposal>();
            if (!proposals.TryGetValue(id, out var current)) return RepositoryResult<Proposal>.NotFound();
            if (!canUpdate(current)) return RepositoryResult<Proposal>.Conflict(current);

            var updated = update(current);
            if (!HasSameIdentity(current, updated))
                return RepositoryResult<Proposal>.Conflict(current);

            updated = updated with { CreatedAt = current.CreatedAt };
            Validate(updated);
            updated = Normalize(updated);
            proposals[id] = updated;
            return RepositoryResult<Proposal>.Success(updated);
        }
    }

    public RepositoryResult<Proposal> TryDecide(
        string id,
        string decision,
        string? reason,
        DateTimeOffset decidedAt)
    {
        if (!ProposalStatuses.IsValid(decision))
            throw new ArgumentException("Unknown proposal decision.", nameof(decision));

        return TryUpdate(
            id,
            proposal => proposal.Milestones.Count == 0,
            proposal => proposal with
            {
                Status = decision,
                DecisionReason = reason,
                DecidedAt = Utc(decidedAt),
                UpdatedAt = Utc(decidedAt)
            });
    }

    public RepositoryResult<Proposal> TryAddMilestoneAndAwardPoints(string id, Milestone milestone)
    {
        lock (store.SyncRoot)
        {
            var proposals = store.Collection<Proposal>();
            if (!proposals.TryGetValue(id, out var proposal)) return RepositoryResult<Proposal>.NotFound();
            if (proposal.Status != ProposalStatuses.Selected
                || proposal.Milestones.Any(item => item.Id.Equals(milestone.Id, StringComparison.Ordinal)))
                return RepositoryResult<Proposal>.Conflict(proposal);

            var teams = store.Collection<Team>();
            if (!teams.TryGetValue(proposal.TeamId, out var team))
                return RepositoryResult<Proposal>.Conflict(proposal);

            var newPoints = (long)team.Points + milestone.Points;
            if (milestone.Points <= 0 || newPoints > int.MaxValue)
                return RepositoryResult<Proposal>.Conflict(proposal);

            var normalizedMilestone = milestone with { ConfirmedAt = Utc(milestone.ConfirmedAt) };
            var updated = proposal with
            {
                Milestones = [.. proposal.Milestones, normalizedMilestone],
                UpdatedAt = normalizedMilestone.ConfirmedAt
            };
            proposals[id] = updated;
            teams[team.Id] = team with { Points = (int)newPoints };
            return RepositoryResult<Proposal>.Success(updated);
        }
    }

    private static void Validate(Proposal proposal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(proposal.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(proposal.TaskId);
        ArgumentException.ThrowIfNullOrWhiteSpace(proposal.TeamId);
        ArgumentException.ThrowIfNullOrWhiteSpace(proposal.BusinessId);
        if (!ProposalStatuses.IsValid(proposal.Status))
            throw new ArgumentException("Proposal status must be 'pending', 'selected', or 'rejected'.", nameof(proposal));
        if (!proposal.Id.Equals($"{proposal.TaskId}_{proposal.TeamId}", StringComparison.Ordinal))
            throw new ArgumentException("Proposal id must be '<taskId>_<teamId>'.", nameof(proposal));
    }

    private static bool HasSameIdentity(Proposal current, Proposal updated) =>
        updated.Id.Equals(current.Id, StringComparison.Ordinal)
        && updated.TaskId.Equals(current.TaskId, StringComparison.Ordinal)
        && updated.TeamId.Equals(current.TeamId, StringComparison.Ordinal)
        && updated.BusinessId.Equals(current.BusinessId, StringComparison.Ordinal);

    private static Proposal Normalize(Proposal proposal) => proposal with
    {
        CreatedAt = Utc(proposal.CreatedAt),
        UpdatedAt = Utc(proposal.UpdatedAt),
        DecidedAt = proposal.DecidedAt is { } decidedAt ? Utc(decidedAt) : null,
        Milestones = proposal.Milestones
            .Select(item => item with { ConfirmedAt = Utc(item.ConfirmedAt) })
            .ToArray()
    };

    private static DateTimeOffset Utc(DateTimeOffset value) => value.ToUniversalTime();
}
