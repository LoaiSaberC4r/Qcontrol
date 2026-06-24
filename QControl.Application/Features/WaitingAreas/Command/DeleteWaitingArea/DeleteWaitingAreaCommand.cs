using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.WaitingAreas.Command.DeleteWaitingArea;

public sealed record DeleteWaitingAreaCommand
    : ICommand<DeleteWaitingAreaResponse>
{
    public int Id { get; init; }
}
