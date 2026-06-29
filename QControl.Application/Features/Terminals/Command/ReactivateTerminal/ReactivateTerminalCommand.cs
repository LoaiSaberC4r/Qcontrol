using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Terminals.Command.ReactivateTerminal;

public sealed record ReactivateTerminalCommand
    : ICommand<ReactivateTerminalResponse>,
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
