namespace Qcontrol.Api.Contracts.BranchConfigurations;

public sealed class UpdateBranchConfigurationRequest
{
    public TimeSpan? AllowedTime { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
