using QControl.Api.Contracts.Tickets;

namespace QControl.Api.Contracts.Reservations;

public sealed record CreateReservationRequest(
    int ServiceId,
    int SegmentId,
    DateTime ScheduledOnUtc,
    IReadOnlyCollection<CustomInputRequest> CustomInputs);
