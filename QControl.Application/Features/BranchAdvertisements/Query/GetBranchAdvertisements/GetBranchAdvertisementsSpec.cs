using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.BranchAdvertisements.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Query.GetBranchAdvertisements;

internal sealed class GetBranchAdvertisementsSpec
    : Specification<BranchAdvertisement, BranchAdvertisementResponse>
{
    public GetBranchAdvertisementsSpec(
        int branchId,
        bool? isActive)
    {
        AddCriteria(x => x.BranchId == branchId);

        if (isActive.HasValue)
        {
            AddCriteria(x => x.IsActive == isActive.Value);
        }

        UseNoTracking();
        AddOrderBy(x => x.DisplayOrder);

        Select(x => new BranchAdvertisementResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            ImageUrl = "/Media/" + x.ImagePath,
            DisplayOrder = x.DisplayOrder,
            IsActive = x.IsActive,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        });
    }
}
