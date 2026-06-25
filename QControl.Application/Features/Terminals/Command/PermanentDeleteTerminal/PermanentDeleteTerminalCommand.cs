using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

public sealed record PermanentDeleteTerminalCommand
    : ICommand<PermanentDeleteTerminalResponse>
{
    public int Id { get; init; }
}
