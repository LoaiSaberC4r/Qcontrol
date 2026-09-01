using BuildingBlock.Application.Abstraction;
using QControl.Application.Features.TicketRuntime.Shared;

namespace QControl.Application.Features.TicketRuntime;

public sealed record SearchKioskReservationsQuery
    : IQuery<IReadOnlyList<KioskReservationSearchItemResponse>>
{
    public int BranchId { get; init; }
    public int ServiceId { get; init; }
    public string? Field { get; init; }
    public IReadOnlyCollection<CustomInputSubmission> CustomInputs { get; init; }
        = Array.Empty<CustomInputSubmission>();
}

public sealed record CreateKioskTicketCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int ServiceId { get; init; }
    public int SegmentId { get; init; }
    public string? Field { get; init; }
    public IReadOnlyCollection<CustomInputSubmission> CustomInputs { get; init; }
        = Array.Empty<CustomInputSubmission>();
}

public sealed record CreateKioskTicketFromReservationCommand
    : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int ReservationId { get; init; }
}
