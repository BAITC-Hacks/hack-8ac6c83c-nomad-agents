namespace TaskForge.Api.Features.Proposals;

public record UpsertProposalRequest(
    string Idea,
    string Plan,
    string Timeline,
    string? PrototypeUrl
);

public record DecisionRequest(
    string Decision, // "selected" | "rejected" | "pending"
    string? Reason
);

public record MilestoneRequest(
    string Title
);

public record ProposalDto(
    string Id,
    string TaskId,
    string TeamId,
    string TeamName,
    string Idea,
    string Plan,
    string Timeline,
    string? PrototypeUrl,
    string Status,
    string? DecisionReason,
    DateTime? DecidedAt,
    MilestoneDto[] Milestones,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record MilestoneDto(
    string Id,
    string Title,
    int Points,
    DateTime? ConfirmedAt
);

public record LeaderboardEntryDto(
    string TeamId,
    string Name,
    int Points
);
