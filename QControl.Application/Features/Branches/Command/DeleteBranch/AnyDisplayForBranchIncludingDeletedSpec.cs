using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.DeleteBranch;

internal sealed class AnyDisplayForBranchIncludingDeletedSpec
    : Specification<Display, int>
{
    public AnyDisplayForBranchIncludingDeletedSpec(int branchId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.BranchId == branchId);

        Select(x => x.Id);
    }
}
