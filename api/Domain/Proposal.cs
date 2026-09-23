namespace TaskForge.Api.Domain;

public record Proposal(
    string Id,
    string TaskId,
    string TeamId,
    string BusinessId,
    string Idea,
    string Plan,
    string Timeline,
    string? PrototypeUrl,
    ProposalStatus Status,
    string? DecisionReason,
    DateTime? DecidedAt,
    List<Milestone> Milestones,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record Milestone(
    string Id,
    string Title,
    int Points,
    DateTime? ConfirmedAt
);

public enum ProposalStatus { Pending, Selected, Rejected }
