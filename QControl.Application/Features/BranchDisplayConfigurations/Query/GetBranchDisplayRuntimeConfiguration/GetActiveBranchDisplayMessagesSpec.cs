using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;

internal sealed class GetActiveBranchDisplayMessagesSpec
    : Specification<BranchDisplayMessage, BranchDisplayRuntimeMessageResponse>
{
    public GetActiveBranchDisplayMessagesSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        AddCriteria(x => x.IsActive);
        UseNoTracking();
        AddOrderBy(x => x.DisplayOrder);
        Select(x => new BranchDisplayRuntimeMessageResponse
        {
            Id = x.Id,
            TextAr = x.TextAr,
            TextEn = x.TextEn,
            DisplayOrder = x.DisplayOrder
        });
    }
}
