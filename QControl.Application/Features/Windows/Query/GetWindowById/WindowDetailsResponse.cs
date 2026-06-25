namespace Qcontrol.Application.Features.Windows.Query.GetWindowById;

public sealed record WindowDetailsResponse
{
    public int Id { get; init; }

    public int WaitingAreaId { get; init; }

    public int WaitingAreaNumber { get; init; }

    public string? WaitingAreaDescriptiveName { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}
