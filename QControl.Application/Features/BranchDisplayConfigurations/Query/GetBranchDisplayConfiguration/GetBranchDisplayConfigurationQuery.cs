using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayConfiguration;

public sealed record GetBranchDisplayConfigurationQuery
    : IQuery<BranchDisplayConfigurationResponse>
{
    public int BranchId { get; init; }
}
