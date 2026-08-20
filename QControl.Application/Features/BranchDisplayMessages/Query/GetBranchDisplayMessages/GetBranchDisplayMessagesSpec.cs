using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.BranchDisplayMessages.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Query.GetBranchDisplayMessages;

internal sealed class GetBranchDisplayMessagesSpec
    : Specification<BranchDisplayMessage, BranchDisplayMessageResponse>
{
    public GetBranchDisplayMessagesSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseNoTracking();
        AddOrderBy(x => x.DisplayOrder);
        Select(x => new BranchDisplayMessageResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            TextAr = x.TextAr,
            TextEn = x.TextEn,
            DisplayOrder = x.DisplayOrder,
            IsActive = x.IsActive,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        });
    }
}
