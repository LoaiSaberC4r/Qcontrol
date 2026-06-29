using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.WaitingAreas.Command.ReactivateWaitingArea;

public sealed record ReactivateWaitingAreaCommand
    : ICommand<ReactivateWaitingAreaResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.WaitingAreas,
        OperationalCacheTags.WaitingArea(Id),
        OperationalCacheTags.Windows,
        OperationalCacheTags.Terminals,
        OperationalCacheTags.DisplayWindows
    };
}
