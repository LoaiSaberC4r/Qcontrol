using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;

public sealed record CreateWaitingAreaCommand
    : ICommand<CreateWaitingAreaResponse>
{
    public int BranchId { get; init; }

    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }
}
