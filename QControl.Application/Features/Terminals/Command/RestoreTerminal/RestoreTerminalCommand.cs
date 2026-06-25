using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Terminals.Command.RestoreTerminal;

public sealed record RestoreTerminalCommand
    : ICommand<RestoreTerminalResponse>
{
    public int Id { get; init; }
}
