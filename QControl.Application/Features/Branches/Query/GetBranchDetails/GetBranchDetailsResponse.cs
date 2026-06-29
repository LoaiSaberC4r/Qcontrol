using Qcontrol.Application.Features.Branches.Shared;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchDetails;

public sealed record GetBranchDetailsResponse
{
    public int BranchId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string? License { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public BranchLocationResponse Location { get; init; } = new();

    public BranchAuditUserResponse CreatedBy { get; init; } = new();

    public DateTime CreatedOnUtc { get; init; }

    public BranchAuditUserResponse? LastModifiedBy { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}
