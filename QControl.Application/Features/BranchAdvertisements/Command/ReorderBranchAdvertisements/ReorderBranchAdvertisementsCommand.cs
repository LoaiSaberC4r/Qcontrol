using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.ReorderBranchAdvertisements;

public sealed record ReorderBranchAdvertisementsCommand
    : ICommand<IReadOnlyList<BranchAdvertisementResponse>>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public List<ReorderBranchAdvertisementItem> Items { get; init; } = new();

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.BranchAdvertisements,
        OperationalCacheTags.BranchAdvertisementsForBranch(BranchId)
    };
}
