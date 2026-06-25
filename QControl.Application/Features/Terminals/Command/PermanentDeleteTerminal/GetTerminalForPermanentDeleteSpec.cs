using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

internal sealed class GetTerminalForPermanentDeleteSpec
    : Specification<Terminal, PermanentDeleteTerminalCandidate>
{
    public GetTerminalForPermanentDeleteSpec(int terminalId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.Id == terminalId);

        Select(x => new PermanentDeleteTerminalCandidate
        {
            Id = x.Id,
            IsDeleted = x.IsDeleted
        });
    }
}
