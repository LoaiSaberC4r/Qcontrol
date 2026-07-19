using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServices.Shared;

internal sealed record BranchServiceBranchState(
    int Id,
    bool IsActive);

internal sealed class GetBranchForBranchServiceOperationSpec
    : Specification<Branch, BranchServiceBranchState>
{
    public GetBranchForBranchServiceOperationSpec(int branchId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(branch => branch.Id == branchId);
        Select(branch => new BranchServiceBranchState(
            branch.Id,
            branch.IsActive));
    }
}

internal sealed class GetBranchServiceIdsSpec
    : Specification<BranchService, int>
{
    public GetBranchServiceIdsSpec(
        int branchId,
        IReadOnlyCollection<int> serviceIds)
    {
        var ids = serviceIds.ToArray();

        UseNoTracking();
        AddCriteria(assignment =>
            assignment.BranchId == branchId &&
            ids.Contains(assignment.ServiceId));
        Select(assignment => assignment.ServiceId);
    }
}

internal sealed class GetBranchServicesForUnassignmentSpec
    : Specification<BranchService>
{
    public GetBranchServicesForUnassignmentSpec(int branchId)
    {
        UseTracking();
        AddCriteria(assignment => assignment.BranchId == branchId);
    }
}
