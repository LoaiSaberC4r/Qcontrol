using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;

public sealed record PermanentDeleteBranchCommand
    : ICommand<PermanentDeleteBranchResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.BranchBranding,
        OperationalCacheTags.BranchBrandingForBranch(BranchId),
        OperationalCacheTags.BranchAdvertisements,
        OperationalCacheTags.BranchAdvertisementsForBranch(BranchId),
        OperationalCacheTags.WaitingAreas,
        OperationalCacheTags.Displays
    };
}
