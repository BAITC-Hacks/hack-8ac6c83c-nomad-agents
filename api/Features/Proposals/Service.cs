using TaskForge.Api.Domain;
using TaskForge.Api.Infrastructure.InMemory;

namespace TaskForge.Api.Features.Proposals;

public sealed record ProposalServiceResult<T>(T? Value, int Status = 200,
    IReadOnlyDictionary<string, string[]>? Errors = null, string? Detail = null);

public sealed class ProposalService(
    IProposalRepository proposals,
    ITaskRepository tasks,
    IActorRepository actors)
{
    public ProposalServiceResult<ProposalDto> Upsert(string taskId, string teamId, UpsertProposalRequest request)
    {
        var task = tasks.Get(taskId);
        if (task is null || task.Status != TaskStatuses.Published || task.Confirmed is null || task.Rating is null)
            return NotFound<ProposalDto>("A proposal can only be submitted to a published task.");
        var errors = Validate(request);
        if (errors.Count > 0) return Invalid<ProposalDto>(errors);
        var now = DateTimeOffset.UtcNow;
        var proposal = new Proposal($"{taskId}_{teamId}", taskId, teamId, task.BusinessId,
            request.Idea!.Trim(), request.Plan!.Trim(), request.Timeline!.Trim(), request.PrototypeUrl?.Trim() ?? "",
            ProposalStatuses.Pending, null, null, [], now, now);
        var result = proposals.UpsertPending(proposal);
        return result.Outcome switch
        {
            RepositoryOutcome.Success => new(Map(result.Value!)),
            RepositoryOutcome.NotFound => NotFound<ProposalDto>(),
            _ => Conflict<ProposalDto>("A decided proposal is locked and cannot be edited.")
        };
    }

    public ProposalServiceResult<ProposalDto?> MineForTask(string taskId, string teamId)
    {
        var task = tasks.Get(taskId);
        if (task is null || task.Status != TaskStatuses.Published || task.Confirmed is null)
            return NotFound<ProposalDto?>("Published task not found.");
        var proposal = proposals.Get($"{taskId}_{teamId}");
        return new(proposal is null ? null : Map(proposal));
    }

    public ProposalServiceResult<IReadOnlyList<ProposalDto>> ListForTask(string taskId, string businessId)
    {
        var task = tasks.Get(taskId);
        if (task is null) return NotFound<IReadOnlyList<ProposalDto>>();
        if (task.BusinessId != businessId) return Forbidden<IReadOnlyList<ProposalDto>>();
        return new(proposals.ListByTask(taskId).Select(Map).ToArray());
    }

    public IReadOnlyList<ProposalDto> ListMine(string teamId) =>
        proposals.ListByTeam(teamId).Select(Map).ToArray();

    public ProposalServiceResult<ProposalDto> Decide(string proposalId, string businessId, ProposalDecisionRequest request)
    {
        var proposal = proposals.Get(proposalId);
        if (proposal is null) return NotFound<ProposalDto>("Proposal not found.");
        if (proposal.BusinessId != businessId) return Forbidden<ProposalDto>();
        Dictionary<string, string[]> errors = [];
        var decision = request.Decision?.Trim().ToLowerInvariant() ?? "";
        if (decision is not (ProposalStatuses.Selected or ProposalStatuses.Rejected))
            errors["decision"] = ["Decision must be 'selected' or 'rejected'."];
        if (request.Reason?.Length > 1000) errors["reason"] = ["Decision reason must be at most 1000 characters."];
        if (errors.Count > 0) return Invalid<ProposalDto>(errors);
        var result = proposals.TryDecide(proposalId, decision,
            string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(), DateTimeOffset.UtcNow);
        return result.Outcome switch
        {
            RepositoryOutcome.Success => new(Map(result.Value!)),
            RepositoryOutcome.NotFound => NotFound<ProposalDto>("Proposal not found."),
            _ => Conflict<ProposalDto>("The proposal decision is locked because work has already been confirmed.")
        };
    }

    private ProposalDto Map(Proposal proposal)
    {
        var team = actors.GetTeam(proposal.TeamId);
        var task = tasks.Get(proposal.TaskId);
        var tags = team is null ? [] : team.Interests.Concat(team.TechTags).Concat(team.Skills)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new(proposal.Id, proposal.TaskId, proposal.TeamId, proposal.BusinessId,
            proposal.Idea, proposal.Plan, proposal.Timeline, proposal.PrototypeUrl, proposal.Status,
            proposal.DecisionReason, proposal.DecisionReason, proposal.DecidedAt, proposal.Milestones,
            proposal.CreatedAt, proposal.UpdatedAt, team?.Name, tags, task?.Confirmed?.Fields.Title);
    }

    private static Dictionary<string, string[]> Validate(UpsertProposalRequest request)
    {
        Dictionary<string, string[]> errors = [];
        Check("idea", request.Idea, 20, 2000);
        Check("plan", request.Plan, 20, 3000);
        Check("timeline", request.Timeline, 3, 200);
        var url = request.PrototypeUrl?.Trim();
        if (url?.Length > 0 && (!Uri.TryCreate(url, UriKind.Absolute, out var parsed) ||
            parsed.Scheme is not ("http" or "https") || string.IsNullOrWhiteSpace(parsed.Host)))
            errors["prototypeUrl"] = ["Prototype URL must be an absolute HTTP(S) URL or empty."];
        return errors;

        void Check(string key, string? value, int min, int max)
        {
            var length = value?.Trim().Length ?? 0;
            if (length < min || length > max) errors[key] = [$"{key} must be between {min} and {max} characters."];
        }
    }

    private static ProposalServiceResult<T> Invalid<T>(IReadOnlyDictionary<string, string[]> errors) => new(default, 400, errors);
    private static ProposalServiceResult<T> NotFound<T>(string detail = "Task not found.") => new(default, 404, Detail: detail);
    private static ProposalServiceResult<T> Forbidden<T>() => new(default, 403, Detail: "Only the business that owns this task may review or decide its proposals.");
    private static ProposalServiceResult<T> Conflict<T>(string detail) => new(default, 409, Detail: detail);
}
