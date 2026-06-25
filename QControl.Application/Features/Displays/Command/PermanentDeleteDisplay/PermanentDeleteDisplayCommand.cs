using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;

public sealed record PermanentDeleteDisplayCommand
    : ICommand<PermanentDeleteDisplayResponse>
{
    public int Id { get; init; }
}
