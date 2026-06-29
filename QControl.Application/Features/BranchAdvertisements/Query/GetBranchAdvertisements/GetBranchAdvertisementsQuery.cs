using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;

namespace Qcontrol.Application.Features.BranchAdvertisements.Query.GetBranchAdvertisements;

public sealed record GetBranchAdvertisementsQuery
    : IQuery<IReadOnlyList<BranchAdvertisementResponse>>
{
    public int BranchId { get; init; }

    public bool? IsActive { get; init; }
}
