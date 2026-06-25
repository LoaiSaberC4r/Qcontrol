using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed class TerminalIPAddressExistsInBranchSpec
    : Specification<Terminal, int>
{
    public TerminalIPAddressExistsInBranchSpec(
        int branchId,
        string ipAddress,
        int? excludedTerminalId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x =>
            x.Window.WaitingArea.BranchId == branchId &&
            x.IPAddress == ipAddress);

        if (excludedTerminalId.HasValue)
        {
            var terminalId = excludedTerminalId.Value;
            AddCriteria(x => x.Id != terminalId);
        }

        Select(x => x.Id);
    }
}
