using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchVideos.Command.Shared;

internal sealed class GetBranchVideoForMutationSpec
    : Specification<BranchVideo>
{
    public GetBranchVideoForMutationSpec(int videoId)
    {
        AddCriteria(x => x.Id == videoId);
        UseTracking();
    }
}
