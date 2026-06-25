using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Terminals.Command.DeleteTerminal;

public sealed record DeleteTerminalCommand
    : ICommand<DeleteTerminalResponse>
{
    public int Id { get; init; }
}
