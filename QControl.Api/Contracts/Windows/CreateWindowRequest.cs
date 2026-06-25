namespace Qcontrol.Api.Contracts.Windows;

public sealed class CreateWindowRequest
{
    public int WaitingAreaId { get; init; }

    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }
}
