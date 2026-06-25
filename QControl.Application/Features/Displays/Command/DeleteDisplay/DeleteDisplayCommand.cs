using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Displays.Command.DeleteDisplay;

public sealed record DeleteDisplayCommand
    : ICommand<DeleteDisplayResponse>
{
    public int Id { get; init; }
}
