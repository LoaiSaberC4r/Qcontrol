using BuildingBlock.Application.Abstraction;

namespace Qcontrol.Application.Features.Windows.Command.UpdateWindow;

public sealed record UpdateWindowCommand
    : ICommand<UpdateWindowResponse>
{
    public int Id { get; init; }

    public int RequestId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }
}
