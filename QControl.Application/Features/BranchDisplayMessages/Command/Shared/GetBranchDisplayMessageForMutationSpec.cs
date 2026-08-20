using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayMessages.Command.Shared;

internal sealed class GetBranchDisplayMessageForMutationSpec
    : Specification<BranchDisplayMessage>
{
    public GetBranchDisplayMessageForMutationSpec(int messageId)
    {
        AddCriteria(x => x.Id == messageId);
        UseTracking();
    }
}
