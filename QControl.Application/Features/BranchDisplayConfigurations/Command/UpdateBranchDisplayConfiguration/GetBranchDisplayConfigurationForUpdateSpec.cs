using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Command.UpdateBranchDisplayConfiguration;

internal sealed class GetBranchDisplayConfigurationForUpdateSpec
    : Specification<BranchDisplayConfiguration>
{
    public GetBranchDisplayConfigurationForUpdateSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseTracking();
    }
}
