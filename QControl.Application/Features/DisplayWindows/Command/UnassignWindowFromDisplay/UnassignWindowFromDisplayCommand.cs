using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;

public sealed record UnassignWindowFromDisplayCommand
    : ICommand<UnassignWindowFromDisplayResponse>
{
    public int DisplayId { get; init; }

    public int WindowId { get; init; }
}
