using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Shared;

internal static class BranchAdvertisementResponseFactory
{
    public static BranchAdvertisementResponse FromEntity(
        BranchAdvertisement advertisement)
    {
        return new BranchAdvertisementResponse
        {
            Id = advertisement.Id,
            BranchId = advertisement.BranchId,
            ImageUrl = BranchMediaUrlMapper.ToMediaUrl(
                advertisement.ImagePath) ?? string.Empty,
            DisplayOrder = advertisement.DisplayOrder,
            IsActive = advertisement.IsActive,
            RowVersion = RowVersionConverter.ToBase64(
                advertisement.RowVersion)
        };
    }

    public static BranchAdvertisementStateResponse StateFromEntity(
        BranchAdvertisement advertisement,
        string message)
    {
        return new BranchAdvertisementStateResponse
        {
            AdvertisementId = advertisement.Id,
            BranchId = advertisement.BranchId,
            IsActive = advertisement.IsActive,
            DisplayOrder = advertisement.DisplayOrder,
            RowVersion = RowVersionConverter.ToBase64(
                advertisement.RowVersion),
            Message = message
        };
    }
}
