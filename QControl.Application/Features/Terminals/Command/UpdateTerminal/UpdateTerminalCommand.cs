using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;

public sealed record UpdateTerminalCommand
    : ICommand<UpdateTerminalResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Terminals,
        OperationalCacheTags.Terminal(Id)
    };
}
