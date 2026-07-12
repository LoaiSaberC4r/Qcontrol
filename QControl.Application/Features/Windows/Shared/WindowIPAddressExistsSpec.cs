using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Shared;

internal sealed class WindowIPAddressExistsSpec
    : Specification<Window, int>
{
    public WindowIPAddressExistsSpec(
        int waitingAreaId,
        string ipAddress,
        int? excludedWindowId = null)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.WaitingAreaId == waitingAreaId &&
            x.IPAddress == ipAddress);

        if (excludedWindowId.HasValue)
        {
            var windowId = excludedWindowId.Value;
            AddCriteria(x => x.Id != windowId);
        }

        Select(x => x.Id);
    }
}
