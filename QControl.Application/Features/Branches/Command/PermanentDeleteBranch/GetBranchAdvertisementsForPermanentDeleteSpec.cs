using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;

internal sealed class GetBranchAdvertisementsForPermanentDeleteSpec
    : Specification<BranchAdvertisement>
{
    public GetBranchAdvertisementsForPermanentDeleteSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseTracking();
    }
}
