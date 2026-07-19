using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Command.PermanentDeleteService;

internal sealed class GetOwnedBranchServiceForPermanentDeleteSpec
    : Specification<BranchService>
{
    public GetOwnedBranchServiceForPermanentDeleteSpec(
        int branchId,
        int serviceId)
    {
        AddCriteria(assignment =>
            assignment.BranchId == branchId &&
            assignment.ServiceId == serviceId);
        UseTracking();
    }
}

internal sealed class GetServiceSchedulesForPermanentDeleteSpec
    : Specification<ServiceSchedule>
{
    public GetServiceSchedulesForPermanentDeleteSpec(int serviceId)
    {
        AddCriteria(schedule => schedule.ServiceId == serviceId);
        UseTracking();
    }
}

internal sealed class GetServiceImagesForPermanentDeleteSpec
    : Specification<ServiceImage>
{
    public GetServiceImagesForPermanentDeleteSpec(int serviceId)
    {
        IgnoreGlobalFilters();
        AddCriteria(image => image.ServiceId == serviceId);
        UseTracking();
    }
}
