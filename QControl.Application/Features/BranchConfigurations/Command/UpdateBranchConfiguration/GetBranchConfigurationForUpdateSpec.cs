using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;

internal sealed class GetBranchConfigurationForUpdateSpec
    : Specification<BranchConfiguration>
{
    public GetBranchConfigurationForUpdateSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseTracking();
    }
}
