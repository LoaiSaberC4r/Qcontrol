using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Shared;

internal sealed class DisplayIPAddressExistsInBranchSpec
    : Specification<Display, int>
{
    public DisplayIPAddressExistsInBranchSpec(
        int branchId,
        string ipAddress,
        int? excludedDisplayId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x =>
            x.BranchId == branchId &&
            x.IPAddress == ipAddress);

        if (excludedDisplayId.HasValue)
        {
            var displayId = excludedDisplayId.Value;
            AddCriteria(x => x.Id != displayId);
        }

        Select(x => x.Id);
    }
}
