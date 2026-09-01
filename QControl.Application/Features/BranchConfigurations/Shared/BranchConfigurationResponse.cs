namespace Qcontrol.Application.Features.BranchConfigurations.Shared;

public sealed class BranchConfigurationResponse
{
    public int BranchId { get; init; }

    public TimeSpan AllowedTime { get; init; }

    public int MaximumTicketCallAttempts { get; init; }
    public int TicketNoShowAutoCancellationMinutes { get; init; }
    public int TicketArchiveRetentionDays { get; init; }

    public bool IsConfigured { get; init; }

    public string? RowVersion { get; init; }

    public string? Message { get; init; }
}
