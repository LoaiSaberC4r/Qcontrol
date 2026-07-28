using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceSubtree;

public sealed record CreateBranchServiceSubtreeCommand
    : ICommand<CreateBranchServiceSubtreeResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public int ParentServiceId { get; init; }

    public CreateBranchServiceTreeNodeCommand? Root { get; init; }

    public IEnumerable<string> Tags =>
        new[]
        {
            OperationalCacheTags.Services,
            OperationalCacheTags.ServiceCentral,
            OperationalCacheTags.Service(ParentServiceId),
            OperationalCacheTags.BranchServices,
            OperationalCacheTags.BranchServiceSegments,
            OperationalCacheTags.BranchServicesForBranch(BranchId),
            OperationalCacheTags.ServiceGlobalizationRequests,
            OperationalCacheTags.ServiceGlobalizationRequestsForBranch(BranchId)
        };
}
