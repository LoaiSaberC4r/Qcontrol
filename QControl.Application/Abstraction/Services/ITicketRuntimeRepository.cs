using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Enums;

namespace QControl.Application.Abstraction.Services;

public interface ITicketRuntimeRepository
{
    Task<Result<TicketDetailsResponse>> CreateTicketAsync(int branchId, int serviceId,
        int segmentId, string? lookupValue, IReadOnlyCollection<CustomInputSubmission> inputs,
        bool requireLookupValueWhenNoCustomInputs, Guid performerId,
        CancellationToken cancellationToken);
    Task<Result<ReservationDetailsResponse>> CreateReservationAsync(int branchId, int serviceId,
        int segmentId, DateTime scheduledOnUtc, string? lookupValue, IReadOnlyCollection<CustomInputSubmission> inputs,
        Guid performerId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<KioskReservationSearchItemResponse>>> SearchReservationsForKioskAsync(
        int branchId, int serviceId, string? lookupValue,
        IReadOnlyCollection<CustomInputSubmission> inputs, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> CreateTicketFromReservationAsync(int branchId,
        int reservationId, Guid performerId, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> CancelTicketAsync(int branchId, int ticketId,
        string reason, Guid performerId, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> ReturnNoShowToWaitingAsync(int branchId, int ticketId,
        Guid performerId, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> StartServiceAsync(int branchId, int ticketId,
        Guid performerId, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> CompleteServiceAsync(int branchId, int ticketId,
        Guid performerId, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> ManualTransferAsync(int branchId, int ticketId,
        int targetServiceId, string reason, Guid performerId, CancellationToken cancellationToken);
    /// <summary>
    /// Persists a call attempt for a known Ticket. This does not select or prioritize a Ticket.
    /// </summary>
    Task<Result<TicketDetailsResponse>> RecordCallAttemptAsync(int branchId, int ticketId,
        int windowId, Guid performerId, CancellationToken cancellationToken);
    Task<Result<ReservationDetailsResponse>> CancelReservationAsync(int branchId, int reservationId,
        string reason, Guid performerId, CancellationToken cancellationToken);

    Task<Result<Pagination<TicketListItemResponse>>> GetTicketsAsync(int branchId, int pageNumber,
        int pageSize, TicketStatus? status, int? serviceId, int? segmentId, string? ticketNumber,
        DateOnly? businessDate, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> GetTicketAsync(int branchId, int ticketId,
        CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<TicketHistoryResponse>>> GetTicketHistoryAsync(int branchId,
        int ticketId, CancellationToken cancellationToken);
    Task<Result<Pagination<ReservationListItemResponse>>> GetReservationsAsync(int branchId,
        int pageNumber, int pageSize, ReservationStatus? status, int? serviceId, int? segmentId,
        DateOnly? businessDate, CancellationToken cancellationToken);
    Task<Result<ReservationDetailsResponse>> GetReservationAsync(int branchId, int reservationId,
        CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ReservationHistoryResponse>>> GetReservationHistoryAsync(int branchId,
        int reservationId, CancellationToken cancellationToken);
    Task<Result<Pagination<TicketListItemResponse>>> GetArchivedTicketsAsync(int branchId,
        int pageNumber, int pageSize, TicketStatus? status, int? serviceId, int? segmentId,
        string? ticketNumber, DateOnly? businessDate, CancellationToken cancellationToken);
    Task<Result<TicketDetailsResponse>> GetArchivedTicketAsync(int branchId, int ticketId,
        CancellationToken cancellationToken);
    Task<Result<Pagination<ReservationListItemResponse>>> GetArchivedReservationsAsync(int branchId,
        int pageNumber, int pageSize, ReservationStatus? status, int? serviceId, int? segmentId,
        DateOnly? businessDate, CancellationToken cancellationToken);
    Task<Result<ReservationDetailsResponse>> GetArchivedReservationAsync(int branchId,
        int reservationId, CancellationToken cancellationToken);

    Task<int> AutoCancelNoShowTicketsAsync(int batchSize, CancellationToken cancellationToken);
    Task<int> ProcessReservationEndOfDayAsync(int batchSize, CancellationToken cancellationToken);
    Task<int> ArchiveEligibleTicketsAsync(int batchSize, CancellationToken cancellationToken);
    Task<int> ArchiveEligibleReservationsAsync(int batchSize, CancellationToken cancellationToken);
}

public sealed record BranchServiceAvailability(
    bool ServiceExists,
    bool IsActive,
    bool IsAssigned,
    bool IsScheduleAvailable,
    bool IsTicketIssuable,
    bool HasReservation,
    string? RangePrefix,
    int? RangeStartNumber,
    int? RangeEndNumber)
{
    public bool IsAvailable => ServiceExists && IsActive && IsAssigned && IsScheduleAvailable;
}

public interface IBranchServiceAvailabilityChecker
{
    Task<BranchServiceAvailability> CheckAsync(int branchId, int serviceId,
        DateTime atUtc, CancellationToken cancellationToken);
}
