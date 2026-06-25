using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Windows.Command.DeleteWindow;

public sealed record DeleteWindowCommand
    : ICommand<DeleteWindowResponse>
{
    public int Id { get; init; }
}
