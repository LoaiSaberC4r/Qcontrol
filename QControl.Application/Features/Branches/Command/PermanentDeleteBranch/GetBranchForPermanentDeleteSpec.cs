using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.PermanentDeleteBranch;

internal sealed class GetBranchForDeleteSpec
    : Specification<Branch>
{
    public GetBranchForDeleteSpec(int branchId)
    {
        AddCriteria(x => x.Id == branchId);
        AddInclude(query => query.Include(x => x.Location));
        UseTracking();
    }
}
