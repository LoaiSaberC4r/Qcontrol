namespace Qcontrol.Api.Contracts.BranchConfigurations;

public sealed class CreateBranchConfigurationRequest
{
    public TimeSpan? AllowedTime { get; init; }

    public int MaximumTicketCallAttempts { get; init; } = 3;
    public int TicketNoShowAutoCancellationMinutes { get; init; } = 30;
    public int TicketArchiveRetentionDays { get; init; } = 30;
}
