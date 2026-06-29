using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

public sealed record PermanentDeleteTerminalCommand
    : ICommand<PermanentDeleteTerminalResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Terminals,
        OperationalCacheTags.Terminal(Id)
    };
}
