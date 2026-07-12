using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Shared;

internal sealed class DisplaySerialNoExistsInBranchSpec
    : Specification<Display, int>
{
    public DisplaySerialNoExistsInBranchSpec(
        int branchId,
        string serialNo,
        int? excludedDisplayId = null)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.BranchId == branchId &&
            x.SerialNo == serialNo);

        if (excludedDisplayId.HasValue)
        {
            var displayId = excludedDisplayId.Value;
            AddCriteria(x => x.Id != displayId);
        }

        Select(x => x.Id);
    }
}
