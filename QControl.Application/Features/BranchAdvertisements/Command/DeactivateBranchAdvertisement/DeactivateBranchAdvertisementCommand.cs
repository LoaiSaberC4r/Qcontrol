using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.DeactivateBranchAdvertisement;

public sealed record DeactivateBranchAdvertisementCommand
    : ICommand<BranchAdvertisementStateResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int AdvertisementId { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.BranchAdvertisements,
        OperationalCacheTags.BranchAdvertisementsForBranch(BranchId)
    };
}
