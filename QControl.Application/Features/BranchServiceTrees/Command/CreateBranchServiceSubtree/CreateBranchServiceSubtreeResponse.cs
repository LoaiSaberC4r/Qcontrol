using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceSubtree;

public sealed class CreateBranchServiceSubtreeResponse
{
    public int BranchId { get; init; }

    public int ParentServiceId { get; init; }

    public ServiceScope Scope { get; init; }

    public int OwnerBranchId { get; init; }

    public int CreatedServicesCount { get; init; }

    public int CreatedAssignmentsCount { get; init; }

    public CreatedBranchServiceTreeNodeResponse Root { get; init; } = null!;

    public ServiceGlobalizationRequestSummaryResponse? GlobalizationRequest
    {
        get;
        init;
    }

    public string Message { get; init; } = string.Empty;
}
