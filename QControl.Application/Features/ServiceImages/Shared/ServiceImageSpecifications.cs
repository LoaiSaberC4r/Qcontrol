using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceImages.Shared;

internal sealed class GetServiceImagesSpec
    : Specification<ServiceImage>
{
    public GetServiceImagesSpec(
        int serviceId,
        ServiceImageType? imageType = null,
        bool useTracking = false)
    {
        AddCriteria(x => x.ServiceId == serviceId);
        AddCriteria(x => x.IsActive);

        if (imageType.HasValue)
        {
            var requestedImageType = imageType.Value;
            AddCriteria(x => x.ImageType == requestedImageType);
        }

        if (useTracking)
        {
            UseTracking();
        }
        else
        {
            UseNoTracking();
        }

        AddOrderBy(x => x.ImageType);
        AddOrderBy(x => x.DisplayOrder);
        AddOrderBy(x => x.Id);
    }
}

internal sealed class GetServiceImagesForServicesSpec
    : Specification<ServiceImage>
{
    public GetServiceImagesForServicesSpec(
        IEnumerable<int> serviceIds,
        bool includeAds)
    {
        var ids = serviceIds.ToArray();

        AddCriteria(x => ids.Contains(x.ServiceId));
        AddCriteria(x => x.IsActive);

        if (!includeAds)
        {
            AddCriteria(x => x.ImageType != ServiceImageType.Ads);
        }

        UseNoTracking();
        AddOrderBy(x => x.ServiceId);
        AddOrderBy(x => x.ImageType);
        AddOrderBy(x => x.DisplayOrder);
        AddOrderBy(x => x.Id);
    }
}

internal sealed class GetServiceImageForMutationSpec
    : Specification<ServiceImage>
{
    public GetServiceImageForMutationSpec(
        int serviceId,
        int imageId)
    {
        AddCriteria(x => x.ServiceId == serviceId);
        AddCriteria(x => x.Id == imageId);
        AddCriteria(x => x.IsActive);
        UseTracking();
    }
}
