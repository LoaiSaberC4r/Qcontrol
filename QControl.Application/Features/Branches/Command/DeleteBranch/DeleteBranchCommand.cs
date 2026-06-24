using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Branches.Command.DeleteBranch;

public sealed record DeleteBranchCommand
    : ICommand<DeleteBranchResponse>
{
    public int BranchId { get; init; }
}