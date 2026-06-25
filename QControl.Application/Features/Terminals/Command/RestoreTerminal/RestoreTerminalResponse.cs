namespace Qcontrol.Application.Features.Terminals.Command.RestoreTerminal;

public sealed record RestoreTerminalResponse
{
    public int Id { get; init; }

    public int WindowId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public DateTime? RestoredOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
