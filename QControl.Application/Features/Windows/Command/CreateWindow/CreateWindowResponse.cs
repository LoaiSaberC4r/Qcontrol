namespace Qcontrol.Application.Features.Windows.Command.CreateWindow;

public sealed record CreateWindowResponse
{
    public int Id { get; init; }

    public int WaitingAreaId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public Guid CreatedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public string Message { get; init; } = string.Empty;
}
