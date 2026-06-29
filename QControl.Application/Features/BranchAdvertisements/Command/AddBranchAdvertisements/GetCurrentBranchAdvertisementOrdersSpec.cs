using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.AddBranchAdvertisements;

internal sealed class GetCurrentBranchAdvertisementOrdersSpec
    : Specification<BranchAdvertisement, int>
{
    public GetCurrentBranchAdvertisementOrdersSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseNoTracking();
        AddOrderBy(x => x.DisplayOrder);
        Select(x => x.DisplayOrder);
    }
}
