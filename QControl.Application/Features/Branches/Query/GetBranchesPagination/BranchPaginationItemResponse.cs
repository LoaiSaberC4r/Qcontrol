using Qcontrol.Application.Features.Branches.Shared;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchesPagination;

public sealed record BranchPaginationItemResponse
{
    public int BranchId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string Governorate { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public BranchAuditUserResponse CreatedBy { get; init; } = new();

    public DateTime CreatedOnUtc { get; init; }

    public BranchAuditUserResponse? LastModifiedBy { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}
