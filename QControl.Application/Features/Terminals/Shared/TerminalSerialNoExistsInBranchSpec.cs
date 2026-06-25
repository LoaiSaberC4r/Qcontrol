using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Shared;

internal sealed class TerminalSerialNoExistsInBranchSpec
    : Specification<Terminal, int>
{
    public TerminalSerialNoExistsInBranchSpec(
        int branchId,
        string serialNo,
        int? excludedTerminalId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x =>
            x.Window.WaitingArea.BranchId == branchId &&
            x.SerialNo == serialNo);

        if (excludedTerminalId.HasValue)
        {
            var terminalId = excludedTerminalId.Value;
            AddCriteria(x => x.Id != terminalId);
        }

        Select(x => x.Id);
    }
}
