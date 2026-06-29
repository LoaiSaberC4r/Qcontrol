using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Displays.Command.DeactivateDisplay;

public sealed record DeactivateDisplayCommand
    : ICommand<DeactivateDisplayResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Displays,
        OperationalCacheTags.Display(Id),
        OperationalCacheTags.DisplayWindows
    };
}
