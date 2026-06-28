using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

internal sealed class GetWindowForAssignmentSpec
    : Specification<Window, WindowAssignmentCandidate>
{
    public GetWindowForAssignmentSpec(int windowId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.Id == windowId);

        Select(x => new WindowAssignmentCandidate
        {
            Id = x.Id,
            BranchId = x.WaitingArea.BranchId,
            IsDeleted = x.IsDeleted
        });
    }
}
