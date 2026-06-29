namespace Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;

public sealed record PermanentDeleteBranchResponse
{
    public int BranchId { get; init; }

    public string Message { get; init; } = string.Empty;
}
