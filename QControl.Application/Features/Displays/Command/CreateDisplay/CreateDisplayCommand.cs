using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Displays.Command.CreateDisplay;

public sealed record CreateDisplayCommand
    : ICommand<CreateDisplayResponse>,
      ICacheInvalidator
{
    public int BranchId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string IPAddress { get; init; } = string.Empty;

    public string SerialNo { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Displays,
        OperationalCacheTags.Branch(BranchId),
        OperationalCacheTags.DisplayWindows
    };
}
