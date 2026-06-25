using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Windows.Command.CreateWindow;

public sealed record CreateWindowCommand
    : ICommand<CreateWindowResponse>
{
    public int WaitingAreaId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }
}
