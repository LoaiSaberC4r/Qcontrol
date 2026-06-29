using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.WaitingAreas.Command.PermanentDeleteWaitingArea;

public sealed record PermanentDeleteWaitingAreaCommand
    : ICommand<PermanentDeleteWaitingAreaResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.WaitingAreas,
        OperationalCacheTags.WaitingArea(Id),
        OperationalCacheTags.Windows
    };
}
