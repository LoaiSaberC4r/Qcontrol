using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchConfigurations.Query.GetBranchConfiguration;

public sealed record GetBranchConfigurationQuery
    : IQuery<BranchConfigurationResponse>
{
    public int BranchId { get; init; }
}
