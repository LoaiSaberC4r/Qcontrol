using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;

public sealed record CreateBranchConfigurationCommand
    : ICommand<BranchConfigurationResponse>
{
    public int BranchId { get; init; }

    public TimeSpan? AllowedTime { get; init; }
}
