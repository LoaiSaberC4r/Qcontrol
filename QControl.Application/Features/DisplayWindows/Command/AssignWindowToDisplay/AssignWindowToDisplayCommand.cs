using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

public sealed record AssignWindowToDisplayCommand
    : ICommand<AssignWindowToDisplayResponse>
{
    public int DisplayId { get; init; }

    public int WindowId { get; init; }
}
