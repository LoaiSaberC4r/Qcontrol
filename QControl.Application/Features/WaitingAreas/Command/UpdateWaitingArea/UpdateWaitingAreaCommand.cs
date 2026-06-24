using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;

public sealed record UpdateWaitingAreaCommand
    : ICommand<UpdateWaitingAreaResponse>
{
    public int Id { get; init; }

    public int RequestId { get; init; }

    public int BranchId { get; init; }

    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }
}
