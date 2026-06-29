using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.AddBranchAdvertisements;

public sealed record AddBranchAdvertisementsCommand
    : ICommand<IReadOnlyList<BranchAdvertisementResponse>>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public List<IFormFile> Images { get; init; } = new();

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Branches,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.BranchAdvertisements,
        OperationalCacheTags.BranchAdvertisementsForBranch(BranchId)
    };
}
