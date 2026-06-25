using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;

internal sealed class GetDisplayForPermanentDeleteSpec
    : Specification<Display, PermanentDeleteDisplayCandidate>
{
    public GetDisplayForPermanentDeleteSpec(int displayId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.Id == displayId);

        Select(x => new PermanentDeleteDisplayCandidate
        {
            Id = x.Id,
            IsDeleted = x.IsDeleted
        });
    }
}
