namespace Qcontrol.Application.Features.BranchConfigurations.Shared;

public sealed class BranchConfigurationResponse
{
    public int BranchId { get; init; }

    public TimeSpan AllowedTime { get; init; }

    public bool IsConfigured { get; init; }

    public string? RowVersion { get; init; }

    public string? Message { get; init; }
}
