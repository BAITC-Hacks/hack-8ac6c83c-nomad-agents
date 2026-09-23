namespace TaskForge.Api.Domain;

public static class ProposalStatuses
{
    public const string Pending = "pending";
    public const string Selected = "selected";
    public const string Rejected = "rejected";

    public static bool IsValid(string value) => value is Pending or Selected or Rejected;
}

public sealed record Milestone(
    string Id,
    string Title,
    int Points,
    DateTimeOffset ConfirmedAt);

public sealed record Proposal(
    string Id,
    string TaskId,
    string TeamId,
    string BusinessId,
    string Idea,
    string Plan,
    string Timeline,
    string PrototypeUrl,
    string Status,
    string? DecisionReason,
    DateTimeOffset? DecidedAt,
    IReadOnlyList<Milestone> Milestones,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
