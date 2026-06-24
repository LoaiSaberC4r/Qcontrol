using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Command.UpdateBranch;

internal sealed class GetBranchForUpdateSpec
    : Specification<Branch>
{
    public GetBranchForUpdateSpec(int branchId)
    {
        AddCriteria(x => x.Id == branchId);
        AddInclude(query => query.Include(x => x.Location));
        UseTracking();
    }
}