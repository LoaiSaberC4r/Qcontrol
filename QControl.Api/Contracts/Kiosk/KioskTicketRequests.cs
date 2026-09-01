using QControl.Api.Contracts.Tickets;

namespace QControl.Api.Contracts.Kiosk;

public sealed record KioskReservationSearchRequest(
    int ServiceId,
    string? Field,
    IReadOnlyCollection<CustomInputRequest> CustomInputs);

public sealed record CreateKioskTicketRequest(
    int ServiceId,
    int SegmentId,
    string? Field,
    IReadOnlyCollection<CustomInputRequest> CustomInputs);
