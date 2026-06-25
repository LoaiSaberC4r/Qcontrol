using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Shared;

internal sealed class WindowNumberExistsSpec
    : Specification<Window, int>
{
    public WindowNumberExistsSpec(
        int waitingAreaId,
        string number,
        int? excludedWindowId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x =>
            x.WaitingAreaId == waitingAreaId &&
            x.Number == number);

        if (excludedWindowId.HasValue)
        {
            var windowId = excludedWindowId.Value;
            AddCriteria(x => x.Id != windowId);
        }

        Select(x => x.Id);
    }
}
