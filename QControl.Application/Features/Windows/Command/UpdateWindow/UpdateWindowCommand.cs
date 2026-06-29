using BuildingBlock.Application.Abstraction;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.Windows.Command.UpdateWindow;

public sealed record UpdateWindowCommand
    : ICommand<UpdateWindowResponse>,
      ICacheInvalidator
{
    public int Id { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IEnumerable<string> Tags => new[]
    {
        OperationalCacheTags.Windows,
        OperationalCacheTags.Window(Id),
        OperationalCacheTags.DisplayWindows
    };
}
