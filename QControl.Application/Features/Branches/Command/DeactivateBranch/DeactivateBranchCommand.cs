using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Branches.Command.DeactivateBranch;

public sealed record DeactivateBranchCommand
    : ICommand<DeactivateBranchResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.WaitingAreas,
        OperationalCacheTags.Windows,
        OperationalCacheTags.Terminals,
        OperationalCacheTags.Displays,
        OperationalCacheTags.DisplayWindows
    };
}
