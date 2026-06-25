using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed class GetActiveTerminalsForWindowSpec
    : Specification<Terminal>
{
    public GetActiveTerminalsForWindowSpec(int windowId)
    {
        AddCriteria(x => x.WindowId == windowId);
        AddOrderBy(x => x.Id);
    }
}
