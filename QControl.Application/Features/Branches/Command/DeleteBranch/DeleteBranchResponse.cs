namespace Qcontrol.Application.Features.Branches.Command.DeleteBranch;

public sealed record DeleteBranchResponse
{
    public int BranchId { get; init; }
}