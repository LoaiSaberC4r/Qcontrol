using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;

public sealed record PermanentDeleteWindowCommand
    : ICommand<PermanentDeleteWindowResponse>
{
    public int Id { get; init; }
}
