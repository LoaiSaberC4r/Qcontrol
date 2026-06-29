namespace Qcontrol.Api.Contracts.WaitingAreas;

public sealed class UpdateWaitingAreaRequest
{
    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
