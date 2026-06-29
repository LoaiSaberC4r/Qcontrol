using BuildingBlock.Domain.Specification;

namespace Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;

internal sealed class GetBranchBrandingForPermanentDeleteSpec
    : Specification<QControl.Domain.Entities.BranchBranding>
{
    public GetBranchBrandingForPermanentDeleteSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseTracking();
    }
}
