using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;

public sealed record GetBranchDisplayRuntimeConfigurationQuery
    : IQuery<BranchDisplayRuntimeConfigurationResponse>
{
    public int BranchId { get; init; }
}
