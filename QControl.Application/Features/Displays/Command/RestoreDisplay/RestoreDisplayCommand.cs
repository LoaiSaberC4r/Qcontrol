using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Displays.Command.RestoreDisplay;

public sealed record RestoreDisplayCommand
    : ICommand<RestoreDisplayResponse>
{
    public int Id { get; init; }
}
