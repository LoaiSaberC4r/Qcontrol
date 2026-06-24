using Qcontrol.Application.Features.Branches.Shared;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchDetails;

public sealed record GetBranchDetailsResponse
{
    public int BranchId { get; init; }

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public string IPAddress { get; init; } = string.Empty;

    public bool IsUpdatesAvailable { get; init; }

    public DateTime LastUpdated { get; init; }

    public string? License { get; init; }

    public BranchLocationResponse Location { get; init; } = new();

    public BranchAuditUserResponse CreatedBy { get; init; } = new();

    public DateTime CreatedOnUtc { get; init; }

    public BranchAuditUserResponse? LastModifiedBy { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}