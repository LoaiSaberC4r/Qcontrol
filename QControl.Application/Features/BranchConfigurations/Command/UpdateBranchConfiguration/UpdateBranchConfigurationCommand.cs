using BuildingBlock.Application.Abstraction;
using Qcontrol.Application.Features.BranchConfigurations.Shared;

namespace Qcontrol.Application.Features.BranchConfigurations.Command.UpdateBranchConfiguration;

public sealed record UpdateBranchConfigurationCommand
    : ICommand<BranchConfigurationResponse>
{
    public int BranchId { get; init; }

    public TimeSpan? AllowedTime { get; init; }

    public int? MaximumTicketCallAttempts { get; init; }
    public int? TicketNoShowAutoCancellationMinutes { get; init; }
    public int? TicketArchiveRetentionDays { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
