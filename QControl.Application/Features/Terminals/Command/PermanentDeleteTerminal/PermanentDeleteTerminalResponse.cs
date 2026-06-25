namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

public sealed record PermanentDeleteTerminalResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
