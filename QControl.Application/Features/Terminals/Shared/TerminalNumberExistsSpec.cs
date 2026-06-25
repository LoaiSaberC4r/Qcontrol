using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed class TerminalNumberExistsSpec
    : Specification<Terminal, int>
{
    public TerminalNumberExistsSpec(
        int windowId,
        string number,
        int? excludedTerminalId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x =>
            x.WindowId == windowId &&
            x.Number == number);

        if (excludedTerminalId.HasValue)
        {
            var terminalId = excludedTerminalId.Value;
            AddCriteria(x => x.Id != terminalId);
        }

        Select(x => x.Id);
    }
}
