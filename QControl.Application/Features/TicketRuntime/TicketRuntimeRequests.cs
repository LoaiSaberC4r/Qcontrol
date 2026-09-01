using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Enums;

namespace QControl.Application.Features.TicketRuntime;

public sealed record CreateTicketCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int ServiceId { get; init; }
    public int SegmentId { get; init; }
    public string? Field { get; init; }
    public IReadOnlyCollection<CustomInputSubmission> CustomInputs { get; init; } = Array.Empty<CustomInputSubmission>();
}

public sealed record CreateReservationCommand : ICommand<ReservationDetailsResponse>
{
    public int BranchId { get; init; }
    public int ServiceId { get; init; }
    public int SegmentId { get; init; }
    public DateTime ScheduledOnUtc { get; init; }
    public string? Field { get; init; }
    public IReadOnlyCollection<CustomInputSubmission> CustomInputs { get; init; } = Array.Empty<CustomInputSubmission>();
}

public sealed record CreateTicketFromReservationCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int ReservationId { get; init; }
}

public sealed record CancelTicketCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int TicketId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed record ReturnNoShowTicketToWaitingCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int TicketId { get; init; }
}

public sealed record StartTicketServiceCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int TicketId { get; init; }
}

public sealed record CompleteTicketServiceCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int TicketId { get; init; }
}

public sealed record ManualTransferTicketCommand : ICommand<TicketDetailsResponse>
{
    public int BranchId { get; init; }
    public int TicketId { get; init; }
    public int TargetServiceId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed record CancelReservationCommand : ICommand<ReservationDetailsResponse>
{
    public int BranchId { get; init; }
    public int ReservationId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed record GetTicketsQuery : IQuery<Pagination<TicketListItemResponse>>
{
    public int BranchId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public TicketStatus? Status { get; init; }
    public int? ServiceId { get; init; }
    public int? SegmentId { get; init; }
    public string? TicketNumber { get; init; }
    public DateOnly? BusinessDate { get; init; }
}

public sealed record GetTicketByIdQuery(int BranchId, int TicketId) : IQuery<TicketDetailsResponse>;
public sealed record GetTicketHistoryQuery(int BranchId, int TicketId) : IQuery<IReadOnlyList<TicketHistoryResponse>>;
public sealed record GetArchivedTicketByIdQuery(int BranchId, int TicketId) : IQuery<TicketDetailsResponse>;
public sealed record GetArchivedTicketsQuery : IQuery<Pagination<TicketListItemResponse>>
{
    public int BranchId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public TicketStatus? Status { get; init; }
    public int? ServiceId { get; init; }
    public int? SegmentId { get; init; }
    public string? TicketNumber { get; init; }
    public DateOnly? BusinessDate { get; init; }
}

public sealed record GetReservationsQuery : IQuery<Pagination<ReservationListItemResponse>>
{
    public int BranchId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ReservationStatus? Status { get; init; }
    public int? ServiceId { get; init; }
    public int? SegmentId { get; init; }
    public DateOnly? BusinessDate { get; init; }
}

public sealed record GetReservationByIdQuery(int BranchId, int ReservationId) : IQuery<ReservationDetailsResponse>;
public sealed record GetReservationHistoryQuery(int BranchId, int ReservationId) : IQuery<IReadOnlyList<ReservationHistoryResponse>>;
public sealed record GetArchivedReservationByIdQuery(int BranchId, int ReservationId) : IQuery<ReservationDetailsResponse>;
public sealed record GetArchivedReservationsQuery : IQuery<Pagination<ReservationListItemResponse>>
{
    public int BranchId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ReservationStatus? Status { get; init; }
    public int? ServiceId { get; init; }
    public int? SegmentId { get; init; }
    public DateOnly? BusinessDate { get; init; }
}
