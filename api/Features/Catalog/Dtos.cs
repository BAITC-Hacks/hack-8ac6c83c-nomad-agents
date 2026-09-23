using TaskForge.Api.Domain;
using TaskForge.Api.Features.Rating;

namespace TaskForge.Api.Features.Catalog;

public sealed record CatalogItemDto(
    string Id,
    string BusinessId,
    string BusinessName,
    string Title,
    string ShortSummary,
    string Status,
    RatingDto Rating,
    int Score,
    string Level,
    IReadOnlyList<string> Topics,
    IReadOnlyList<string> TechTags,
    int ProposalCount,
    int Position,
    int CatalogPosition,
    int CatalogTotal,
    bool IsPriority,
    bool HasUnconfirmedChanges,
    TaskFields ConfirmedFields);

public sealed record CatalogResponse(IReadOnlyList<CatalogItemDto> Items, int Total);
