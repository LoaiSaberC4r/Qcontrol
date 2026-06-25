using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;

public sealed record UpdateTerminalCommand
    : ICommand<UpdateTerminalResponse>
{
    public int Id { get; init; }

    public int RequestId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;
}
