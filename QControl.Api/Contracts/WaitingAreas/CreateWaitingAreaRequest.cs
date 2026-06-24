namespace Qcontrol.Api.Contracts.WaitingAreas;

public sealed class CreateWaitingAreaRequest
{
    public int BranchId { get; init; }

    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }
}
