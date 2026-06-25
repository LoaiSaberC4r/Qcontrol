using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed class GetTerminalIncludingDeletedSpec
    : Specification<Terminal>
{
    public GetTerminalIncludingDeletedSpec(
        int terminalId,
        bool useNoTracking = false)
    {
        IgnoreGlobalFilters();
        AddCriteria(x => x.Id == terminalId);

        if (useNoTracking)
        {
            UseNoTracking();
        }
    }
}
