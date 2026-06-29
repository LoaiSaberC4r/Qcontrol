using Qcontrol.Application.Features.Branches.Shared;

namespace Qcontrol.Application.Features.Branches.Command.CreateBranch;

public sealed record CreateBranchResponse
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

    public Guid CreatedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }
}
