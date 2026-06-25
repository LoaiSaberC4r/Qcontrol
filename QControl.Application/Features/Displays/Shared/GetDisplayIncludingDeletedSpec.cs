using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Shared;

internal sealed class GetDisplayIncludingDeletedSpec
    : Specification<Display>
{
    public GetDisplayIncludingDeletedSpec(int displayId)
    {
        IgnoreGlobalFilters();
        UseTracking();

        AddCriteria(x => x.Id == displayId);
    }
}
