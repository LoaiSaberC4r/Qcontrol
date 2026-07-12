using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Shared;

internal sealed class DisplayNumberExistsInBranchSpec
    : Specification<Display, int>
{
    public DisplayNumberExistsInBranchSpec(
        int branchId,
        string number,
        int? excludedDisplayId = null)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.BranchId == branchId &&
            x.Number == number);

        if (excludedDisplayId.HasValue)
        {
            var displayId = excludedDisplayId.Value;
            AddCriteria(x => x.Id != displayId);
        }

        Select(x => x.Id);
    }
}
