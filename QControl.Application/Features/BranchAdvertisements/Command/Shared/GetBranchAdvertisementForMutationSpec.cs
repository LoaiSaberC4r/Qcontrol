using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchAdvertisements.Command.Shared;

internal sealed class GetBranchAdvertisementForMutationSpec
    : Specification<BranchAdvertisement>
{
    public GetBranchAdvertisementForMutationSpec(int advertisementId)
    {
        AddCriteria(x => x.Id == advertisementId);
        UseTracking();
    }
}
