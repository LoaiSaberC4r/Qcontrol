using Qcontrol.Application.Features.Branches.Shared;

namespace Qcontrol.Application.Features.Branches.Command.CreateBranch;

public sealed record CreateBranchResponse
{
    public int BranchId { get; init; }

    public string? ArabicName { get; init; }

    public string? EnglishName { get; init; }

    public string IPAddress { get; init; } = string.Empty;

    public bool IsUpdatesAvailable { get; init; }

    public DateTime LastUpdated { get; init; }

    public string? License { get; init; }

    public BranchLocationResponse Location { get; init; } = new();

    public Guid CreatedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }
}