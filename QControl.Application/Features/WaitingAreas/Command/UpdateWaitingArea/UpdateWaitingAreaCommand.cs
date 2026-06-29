using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;

public sealed record UpdateWaitingAreaCommand
    : ICommand<UpdateWaitingAreaResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public int Number { get; init; }

    public string? AudioDevice { get; init; }

    public string? ControlDevice { get; init; }

    public string? DescriptiveName { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.WaitingAreas,
        OperationalCacheTags.WaitingArea(Id),
        OperationalCacheTags.Windows
    };
}
