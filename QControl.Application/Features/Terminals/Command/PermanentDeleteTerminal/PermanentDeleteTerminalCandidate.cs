namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

internal sealed record PermanentDeleteTerminalCandidate
{
    public int Id { get; init; }

    public bool IsDeleted { get; init; }
}
