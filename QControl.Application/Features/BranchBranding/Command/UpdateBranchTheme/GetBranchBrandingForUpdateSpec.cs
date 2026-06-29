using BuildingBlock.Domain.Specification;

namespace Qcontrol.Application.Features.BranchBranding.Command.UpdateBranchTheme;

internal sealed class GetBranchBrandingForUpdateSpec
    : Specification<QControl.Domain.Entities.BranchBranding>
{
    public GetBranchBrandingForUpdateSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseTracking();
    }
}
