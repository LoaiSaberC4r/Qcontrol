using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;

internal sealed class GetDisplayForAvailableWindowsSpec
    : Specification<Display, DisplayAvailableWindowsDisplayCandidate>
{
    public GetDisplayForAvailableWindowsSpec(int displayId)
    {
        UseNoTracking();

        AddCriteria(x => x.Id == displayId);

        Select(x => new DisplayAvailableWindowsDisplayCandidate
        {
            Id = x.Id,
            BranchId = x.BranchId
        });
    }
}
