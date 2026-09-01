namespace Qcontrol.Api.Contracts.BranchConfigurations;

public sealed class UpdateBranchConfigurationRequest
{
    public TimeSpan? AllowedTime { get; init; }

    public int? MaximumTicketCallAttempts { get; init; }
    public int? TicketNoShowAutoCancellationMinutes { get; init; }
    public int? TicketArchiveRetentionDays { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
