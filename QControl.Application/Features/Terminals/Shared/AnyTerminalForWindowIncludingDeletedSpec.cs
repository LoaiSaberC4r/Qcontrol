using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed class AnyTerminalForWindowIncludingDeletedSpec
    : Specification<Terminal, int>
{
    public AnyTerminalForWindowIncludingDeletedSpec(int windowId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.WindowId == windowId);

        Select(x => x.Id);
    }
}
