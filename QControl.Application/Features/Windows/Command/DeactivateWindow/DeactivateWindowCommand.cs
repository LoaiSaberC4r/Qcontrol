using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Windows.Command.DeactivateWindow;

public sealed record DeactivateWindowCommand
    : ICommand<DeactivateWindowResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Windows,
        OperationalCacheTags.Window(Id),
        OperationalCacheTags.Terminals,
        OperationalCacheTags.DisplayWindows
    };
}
