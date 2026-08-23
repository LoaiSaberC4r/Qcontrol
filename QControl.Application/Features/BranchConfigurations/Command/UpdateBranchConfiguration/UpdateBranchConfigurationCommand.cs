using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;

public sealed record UpdateBranchConfigurationCommand
    : ICommand<BranchConfigurationResponse>
{
    public int BranchId { get; init; }

    public TimeSpan? AllowedTime { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
