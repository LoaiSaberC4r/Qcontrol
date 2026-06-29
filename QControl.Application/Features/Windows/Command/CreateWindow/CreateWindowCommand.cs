using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Windows.Command.CreateWindow;

public sealed record CreateWindowCommand
    : ICommand<CreateWindowResponse>,
      ICacheInvalidator
{
    public int WaitingAreaId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Windows,
        OperationalCacheTags.WaitingArea(WaitingAreaId),
        OperationalCacheTags.DisplayWindows
    };
}
