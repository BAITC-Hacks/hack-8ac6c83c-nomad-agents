namespace TaskForge.Api.Features.Proposals;

public static class ProposalsEndpoints
{
    public static void MapProposalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api");

        group.MapPut("tasks/{taskId}/proposals/mine", UpsertProposal)
            .WithName("UpsertProposal")
            .WithOpenApi();

        group.MapGet("tasks/{taskId}/proposals", GetProposals)
            .WithName("GetProposals")
            .WithOpenApi();

        group.MapGet("proposals/mine", GetMyProposals)
            .WithName("GetMyProposals")
            .WithOpenApi();

        group.MapPost("proposals/{proposalId}/decision", DecideProposal)
            .WithName("DecideProposal")
            .WithOpenApi();

        group.MapPost("proposals/{proposalId}/milestones", ConfirmMilestone)
            .WithName("ConfirmMilestone")
            .WithOpenApi();

        group.MapGet("leaderboard", GetLeaderboard)
            .WithName("GetLeaderboard")
            .WithOpenApi();
    }

    private static async Task<ProposalDto> UpsertProposal(string taskId, UpsertProposalRequest request) => new(
        Id: "",
        TaskId: taskId,
        TeamId: "",
        TeamName: "",
        Idea: request.Idea,
        Plan: request.Plan,
        Timeline: request.Timeline,
        PrototypeUrl: request.PrototypeUrl,
        Status: "pending",
        DecisionReason: null,
        DecidedAt: null,
        Milestones: Array.Empty<MilestoneDto>(),
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow
    );

    private static async Task<ProposalDto[]> GetProposals(string taskId) => Array.Empty<ProposalDto>();

    private static async Task<ProposalDto[]> GetMyProposals() => Array.Empty<ProposalDto>();

    private static async Task<ProposalDto> DecideProposal(string proposalId, DecisionRequest request) => new(
        Id: proposalId,
        TaskId: "",
        TeamId: "",
        TeamName: "",
        Idea: "",
        Plan: "",
        Timeline: "",
        PrototypeUrl: null,
        Status: request.Decision,
        DecisionReason: request.Reason,
        DecidedAt: DateTime.UtcNow,
        Milestones: Array.Empty<MilestoneDto>(),
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow
    );

    private static async Task<ProposalDto> ConfirmMilestone(string proposalId, MilestoneRequest request) => new(
        Id: proposalId,
        TaskId: "",
        TeamId: "",
        TeamName: "",
        Idea: "",
        Plan: "",
        Timeline: "",
        PrototypeUrl: null,
        Status: "selected",
        DecisionReason: null,
        DecidedAt: null,
        Milestones: Array.Empty<MilestoneDto>(),
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow
    );

    private static async Task<LeaderboardEntryDto[]> GetLeaderboard() => Array.Empty<LeaderboardEntryDto>();
}
