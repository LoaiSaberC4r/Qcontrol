using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.CreateBranchConfiguration;

public sealed record CreateBranchConfigurationCommand
    : ICommand<BranchConfigurationResponse>
{
    public int BranchId { get; init; }

    public TimeSpan? AllowedTime { get; init; }

    public int MaximumTicketCallAttempts { get; init; } = 3;
    public int TicketNoShowAutoCancellationMinutes { get; init; } = 30;
    public int TicketArchiveRetentionDays { get; init; } = 30;
}
