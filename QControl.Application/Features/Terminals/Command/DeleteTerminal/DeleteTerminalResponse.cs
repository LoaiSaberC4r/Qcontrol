namespace Qcontrol.Application.Features.Terminals.Command.DeleteTerminal;

public sealed record DeleteTerminalResponse
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
