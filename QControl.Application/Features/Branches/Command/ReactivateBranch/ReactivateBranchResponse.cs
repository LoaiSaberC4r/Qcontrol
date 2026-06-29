namespace Qcontrol.Application.Features.Branches.Command.ReactivateBranch;

public sealed record ReactivateBranchResponse
{
    public int BranchId { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
