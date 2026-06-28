using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;

internal sealed class GetDisplayForLinkedWindowsSpec
    : Specification<Display, DisplayLinkedWindowsDisplayCandidate>
{
    public GetDisplayForLinkedWindowsSpec(int displayId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.Id == displayId);

        Select(x => new DisplayLinkedWindowsDisplayCandidate
        {
            Id = x.Id
        });
    }
}
