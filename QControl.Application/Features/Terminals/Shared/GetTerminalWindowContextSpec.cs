using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed class GetTerminalWindowContextSpec
    : Specification<Window, TerminalWindowContext>
{
    public GetTerminalWindowContextSpec(int windowId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.Id == windowId);

        Select(x => new TerminalWindowContext
        {
            WindowId = x.Id,
            WaitingAreaId = x.WaitingAreaId,
            BranchId = x.WaitingArea.BranchId,
            WindowNumber = x.Number,
            WaitingAreaNumber = x.WaitingArea.Number,
            WindowIsDeleted = x.IsDeleted
        });
    }
}
