namespace Qcontrol.Api.Contracts.Windows;

public sealed class UpdateWindowRequest
{
    public string Number { get; init; } = string.Empty;

    public string? DescriptiveName { get; init; }

    public string? IPAddress { get; init; }

    public bool EnableTicketBooking { get; init; }

    public bool EnableDirectCall { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}
