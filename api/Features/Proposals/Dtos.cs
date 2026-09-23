using System.Text.Json.Serialization;
using TaskForge.Api.Domain;

namespace TaskForge.Api.Features.Proposals;

public sealed record UpsertProposalRequest(
    string? Idea,
    string? Plan,
    string? Timeline,
    string? PrototypeUrl);

public sealed record ProposalDecisionRequest(string? Decision, string? Reason);

public sealed record ProposalDto(
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
    string? Reason,
    DateTimeOffset? DecidedAt,
    IReadOnlyList<Milestone> Milestones,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? TeamName,
    IReadOnlyList<string> Tags,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? TaskTitle);
