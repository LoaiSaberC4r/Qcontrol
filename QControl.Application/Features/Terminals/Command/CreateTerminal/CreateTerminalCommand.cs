using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Terminals.Command.CreateTerminal;

public sealed record CreateTerminalCommand
    : ICommand<CreateTerminalResponse>
{
    public int WindowId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;
}
