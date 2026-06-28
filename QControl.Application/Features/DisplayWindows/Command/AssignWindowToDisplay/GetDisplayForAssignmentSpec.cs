using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

internal sealed class GetDisplayForAssignmentSpec
    : Specification<Display, DisplayAssignmentCandidate>
{
    public GetDisplayForAssignmentSpec(int displayId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.Id == displayId);

        Select(x => new DisplayAssignmentCandidate
        {
            Id = x.Id,
            BranchId = x.BranchId,
            IsDeleted = x.IsDeleted
        });
    }
}
