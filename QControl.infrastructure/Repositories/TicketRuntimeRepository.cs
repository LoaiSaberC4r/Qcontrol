using System.Data;
using System.Globalization;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QControl.Application.Abstraction.Services;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed partial class TicketRuntimeRepository : ITicketRuntimeRepository
{
    private readonly PlatformWriteDbContext _db;
    private readonly IBranchServiceAvailabilityChecker _availability;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<TicketRuntimeRepository> _logger;
    private readonly ITicketPrintModelBuilder _printModelBuilder;

    public TicketRuntimeRepository(PlatformWriteDbContext db,
        IBranchServiceAvailabilityChecker availability, IDateTimeProvider clock,
        ILogger<TicketRuntimeRepository> logger,
        ITicketPrintModelBuilder printModelBuilder)
    {
        _db = db;
        _availability = availability;
        _clock = clock;
        _logger = logger;
        _printModelBuilder = printModelBuilder;
    }

    internal TicketRuntimeRepository(
        PlatformWriteDbContext db,
        IBranchServiceAvailabilityChecker availability,
        IDateTimeProvider clock,
        ILogger<TicketRuntimeRepository> logger)
        : this(db, availability, clock, logger, NullTicketPrintModelBuilder.Instance)
    {
    }

    public async Task<Result<TicketDetailsResponse>> CreateTicketAsync(int branchId,
        int serviceId, int segmentId, string? lookupValue,
        IReadOnlyCollection<CustomInputSubmission> inputs,
        bool requireLookupValueWhenNoCustomInputs,
        Guid performerId, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var availability = await _availability.CheckAsync(branchId, serviceId, now, cancellationToken);
        if (!availability.IsAvailable || !availability.IsTicketIssuable)
            return FailTicket("Tickets.Create.ServiceUnavailable", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain);

        var validatedInputs = await ValidateCreateInputsAsync(serviceId, lookupValue, inputs,
            "Tickets.Create", requireLookupValueWhenNoCustomInputs, cancellationToken);
        if (validatedInputs.IsFailure)
            return Result<TicketDetailsResponse>.Fail(validatedInputs.Errors);

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var relationship = await GetSegmentRelationshipAsync(branchId, serviceId, segmentId, cancellationToken);
            if (relationship is null)
                return await RollbackTicketAsync(transaction, "Tickets.Create.SegmentInvalid", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain, cancellationToken);

            var businessDate = DateOnly.FromDateTime(now);
            if (!await TryConsumeQuotaAsync(relationship, businessDate, now, cancellationToken))
                return await RollbackTicketAsync(transaction, "Tickets.Create.QuotaExceeded", TicketRuntimeMessages.QuotaExceeded, ErrorType.Conflict, cancellationToken);

            var number = await AllocateTicketNumberAsync(branchId, serviceId, businessDate,
                availability, now, cancellationToken);
            if (number.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<TicketDetailsResponse>.Fail(number.Errors);
            }

            var ticket = Ticket.Create(branchId, serviceId, segmentId, null,
                number.Value, businessDate, now, performerId, validatedInputs.Value.LookupValue);
            foreach (var snapshot in validatedInputs.Value.Snapshots)
                ticket.AddCustomInput(snapshot.ToTicket(now));
            await BindDefaultWorkflowAsync(ticket, branchId, serviceId, cancellationToken);
            _db.Add(ticket);
            await _db.SaveChangesAsync(cancellationToken);
            var print = await _printModelBuilder.BuildAsync(ticket.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<TicketDetailsResponse>.Ok(Map(ticket, print));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Ticket creation concurrency conflict for branch {BranchId}", branchId);
            return ConcurrencyTicket("Tickets.Create.ConcurrencyConflict");
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Ticket creation database conflict for branch {BranchId}", branchId);
            return ConcurrencyTicket("Tickets.Create.ConcurrencyConflict");
        }
    }

    public async Task<Result<ReservationDetailsResponse>> CreateReservationAsync(int branchId,
        int serviceId, int segmentId, DateTime scheduledOnUtc, string? lookupValue,
        IReadOnlyCollection<CustomInputSubmission> inputs, Guid performerId,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        if (scheduledOnUtc <= now)
            return FailReservation("Reservations.Create.TimeInvalid", TicketRuntimeMessages.ReservationTimeInvalid, ErrorType.Validation);
        var availability = await _availability.CheckAsync(branchId, serviceId, scheduledOnUtc, cancellationToken);
        if (!availability.IsAvailable || !availability.HasReservation)
            return FailReservation("Reservations.Create.ServiceUnavailable", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain);
        var validatedInputs = await ValidateCreateInputsAsync(serviceId, lookupValue, inputs,
            "Reservations.Create", requireLookupValueWhenNoCustomInputs: true,
            cancellationToken);
        if (validatedInputs.IsFailure)
            return Result<ReservationDetailsResponse>.Fail(validatedInputs.Errors);

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var relationship = await GetSegmentRelationshipAsync(branchId, serviceId, segmentId, cancellationToken);
            if (relationship is null)
                return await RollbackReservationAsync(transaction, "Reservations.Create.SegmentInvalid", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain, cancellationToken);
            var businessDate = DateOnly.FromDateTime(scheduledOnUtc);
            if (!await TryConsumeQuotaAsync(relationship, businessDate, now, cancellationToken))
                return await RollbackReservationAsync(transaction, "Reservations.Create.QuotaExceeded", TicketRuntimeMessages.QuotaExceeded, ErrorType.Conflict, cancellationToken);

            var reservation = Reservation.Create(branchId, serviceId, segmentId,
                relationship.Id, scheduledOnUtc, businessDate, now, performerId,
                validatedInputs.Value.LookupValue);
            foreach (var snapshot in validatedInputs.Value.Snapshots)
                reservation.AddCustomInput(snapshot.ToReservation(now));
            _db.Add(reservation);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<ReservationDetailsResponse>.Ok(Map(reservation));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Reservation creation concurrency conflict for branch {BranchId}", branchId);
            return ConcurrencyReservation("Reservations.Create.ConcurrencyConflict");
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Reservation creation database conflict for branch {BranchId}", branchId);
            return ConcurrencyReservation("Reservations.Create.ConcurrencyConflict");
        }
    }

    public async Task<Result<TicketDetailsResponse>> CreateTicketFromReservationAsync(
        int branchId, int reservationId, Guid performerId, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var reservation = await _db.Set<Reservation>()
                .Include(x => x.CustomInputValues)
                .SingleOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
            if (reservation is null) return await RollbackTicketAsync(transaction, "Reservations.Convert.NotFound", TicketRuntimeMessages.ReservationNotFound, ErrorType.NotFound, cancellationToken);
            if (reservation.BranchId != branchId) return await RollbackTicketAsync(transaction, "Reservations.Convert.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound, cancellationToken);
            if (await _db.Set<Ticket>().AnyAsync(x => x.ReservationId == reservationId, cancellationToken) || reservation.Status == ReservationStatus.ConvertedToTicket)
                return await RollbackTicketAsync(transaction, "Reservations.Convert.AlreadyConverted", TicketRuntimeMessages.AlreadyConverted, ErrorType.Conflict, cancellationToken);
            if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Expired)
                return await RollbackTicketAsync(transaction, "Reservations.Convert.Terminal", TicketRuntimeMessages.ReservationTerminal, ErrorType.Domain, cancellationToken);
            if (reservation.Status == ReservationStatus.NoShow && reservation.BusinessDate != DateOnly.FromDateTime(now))
                return await RollbackTicketAsync(transaction, "Reservations.Convert.DifferentDay", TicketRuntimeMessages.DifferentReservationDay, ErrorType.Domain, cancellationToken);

            var availability = await _availability.CheckAsync(branchId, reservation.ServiceId, now, cancellationToken);
            if (!availability.IsAvailable || !availability.IsTicketIssuable)
                return await RollbackTicketAsync(transaction, "Reservations.Convert.ServiceUnavailable", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain, cancellationToken);
            var relationshipValid = await _db.Set<BranchServiceSegment>().AsNoTracking()
                .AnyAsync(x => x.Id == reservation.BranchServiceSegmentId &&
                    x.BranchService.BranchId == branchId && x.BranchService.ServiceId == reservation.ServiceId &&
                    x.SegmentId == reservation.SegmentId, cancellationToken);
            if (!relationshipValid)
                return await RollbackTicketAsync(transaction, "Reservations.Convert.SegmentInvalid", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain, cancellationToken);

            var businessDate = DateOnly.FromDateTime(now);
            var number = await AllocateTicketNumberAsync(branchId, reservation.ServiceId,
                businessDate, availability, now, cancellationToken);
            if (number.IsFailure) { await transaction.RollbackAsync(cancellationToken); return Result<TicketDetailsResponse>.Fail(number.Errors); }

            var ticket = Ticket.Create(branchId, reservation.ServiceId, reservation.SegmentId,
                reservation.Id, number.Value, businessDate, now, performerId,
                reservation.LookupValue);
            foreach (var input in reservation.CustomInputValues)
                ticket.AddCustomInput(TicketCustomInputValue.Create(input.ServiceCustomInputId,
                    input.NameSnapshot, input.LabelEnSnapshot, input.LabelArSnapshot,
                    input.TypeSnapshot, input.Value, now, input.OrderSnapshot));
            await BindDefaultWorkflowAsync(ticket, branchId, reservation.ServiceId, cancellationToken);
            if (!reservation.TryConvert(now))
                return await RollbackTicketAsync(transaction, "Reservations.Convert.InvalidState", TicketRuntimeMessages.ReservationTerminal, ErrorType.Domain, cancellationToken);
            _db.Add(ticket);
            await _db.SaveChangesAsync(cancellationToken);
            var print = await _printModelBuilder.BuildAsync(ticket.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<TicketDetailsResponse>.Ok(Map(ticket, print));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken); _logger.LogWarning(ex, "Reservation {ReservationId} conversion conflict", reservationId);
            return ConcurrencyTicket("Reservations.Convert.ConcurrencyConflict");
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken); _logger.LogWarning(ex, "Reservation {ReservationId} duplicate conversion conflict", reservationId);
            return ConcurrencyTicket("Reservations.Convert.ConcurrencyConflict");
        }
    }

    public async Task<Result<IReadOnlyList<KioskReservationSearchItemResponse>>>
        SearchReservationsForKioskAsync(int branchId, int serviceId, string? lookupValue,
            IReadOnlyCollection<CustomInputSubmission> inputs,
            CancellationToken cancellationToken)
    {
        var availability = await _availability.CheckAsync(branchId, serviceId, _clock.UtcNow,
            cancellationToken);
        if (!availability.IsAvailable || !availability.IsTicketIssuable)
            return FailKioskSearch("Kiosk.Reservations.Search.ServiceUnavailable",
                TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain);

        var definitions = await _db.Set<ServiceCustomInput>().AsNoTracking()
            .Where(x => x.ServiceId == serviceId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        IQueryable<Reservation> query = _db.Set<Reservation>().AsNoTracking()
            .Where(x => x.BranchId == branchId && x.ServiceId == serviceId);

        if (definitions.Count == 0)
        {
            if (inputs.Count != 0)
                return FailKioskSearch("Kiosk.Reservations.Search.CustomInputsNotAllowed",
                    TicketRuntimeMessages.CustomInputsNotAllowed, ErrorType.Validation);
            if (string.IsNullOrWhiteSpace(lookupValue))
                return FailKioskSearch("Kiosk.Reservations.Search.FieldRequired",
                    TicketRuntimeMessages.FieldRequired, ErrorType.Validation);
            if (lookupValue.Trim().Length > 3000)
                return FailKioskSearch("Kiosk.Reservations.Search.FieldInvalid",
                    TicketRuntimeMessages.FieldInvalid, ErrorType.Validation);

            var normalizedLookupValue = lookupValue.Trim();
            query = query.Where(x => x.LookupValue == normalizedLookupValue);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(lookupValue))
                return FailKioskSearch("Kiosk.Reservations.Search.FieldNotAllowed",
                    TicketRuntimeMessages.FieldNotAllowed, ErrorType.Validation);
            if (inputs.Count == 0)
                return FailKioskSearch("Kiosk.Reservations.Search.CustomInputsRequired",
                    TicketRuntimeMessages.CustomInputsRequiredForSearch, ErrorType.Validation);
            if (inputs.Any(x => string.IsNullOrWhiteSpace(x.Value)))
                return FailKioskSearch("Kiosk.Reservations.Search.CustomInputsInvalid",
                    TicketRuntimeMessages.CustomInputsInvalid, ErrorType.Validation);

            var validated = ValidateInputs(definitions, inputs, requireAllRequired: false);
            if (validated.IsFailure)
                return Result<IReadOnlyList<KioskReservationSearchItemResponse>>.Fail(
                    validated.Errors);

            foreach (var input in validated.Value)
            {
                var inputId = input.Id;
                var value = input.Value;
                query = query.Where(reservation => reservation.CustomInputValues.Any(
                    stored => stored.ServiceCustomInputId == inputId && stored.Value == value));
            }
        }

        var reservations = await query
            .OrderBy(x => x.ScheduledOnUtc)
            .ThenBy(x => x.Id)
            .Select(x => new KioskReservationSearchItemResponse(
                x.Id, x.ServiceId, x.SegmentId, x.ScheduledOnUtc,
                x.BusinessDate, x.Status))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<KioskReservationSearchItemResponse>>.Ok(reservations);
    }

    public async Task<Result<TicketDetailsResponse>> CancelTicketAsync(int branchId,
        int ticketId, string reason, Guid performerId, CancellationToken cancellationToken)
    {
        var ticket = await LoadTicketForMutationAsync(ticketId, cancellationToken);
        if (ticket is null) return FailTicket("Tickets.Cancel.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound);
        if (ticket.BranchId != branchId) return FailTicket("Tickets.Cancel.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound);
        if (!ticket.TryCancel(reason, performerId, _clock.UtcNow))
            return FailTicket("Tickets.Cancel.InvalidTransition", ticket.Status == TicketStatus.InProgress ? TicketRuntimeMessages.TicketCannotCancelAfterStart : TicketRuntimeMessages.InvalidTransition, ErrorType.Domain);
        return await SaveTicketMutationAsync(ticket, "Tickets.Cancel", cancellationToken);
    }

    public async Task<Result<TicketDetailsResponse>> ReturnNoShowToWaitingAsync(int branchId,
        int ticketId, Guid performerId, CancellationToken cancellationToken)
    {
        var ticket = await LoadTicketForMutationAsync(ticketId, cancellationToken);
        if (ticket is null) return FailTicket("Tickets.Return.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound);
        if (ticket.BranchId != branchId) return FailTicket("Tickets.Return.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound);
        if (!ticket.TryReturnNoShowToWaiting(performerId, _clock.UtcNow))
            return FailTicket("Tickets.Return.InvalidStatus", TicketRuntimeMessages.TicketMustBeNoShow, ErrorType.Domain);
        return await SaveTicketMutationAsync(ticket, "Tickets.Return", cancellationToken);
    }

    public async Task<Result<TicketDetailsResponse>> StartServiceAsync(int branchId,
        int ticketId, Guid performerId, CancellationToken cancellationToken)
    {
        var ticket = await LoadTicketForMutationAsync(ticketId, cancellationToken);
        if (ticket is null) return FailTicket("Tickets.Start.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound);
        if (ticket.BranchId != branchId) return FailTicket("Tickets.Start.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound);
        if (ticket.CurrentAssignedApplicationUserId.HasValue && ticket.CurrentAssignedApplicationUserId != performerId)
            return FailTicket("Tickets.Start.CallOwnership", TicketRuntimeMessages.ConcurrencyConflict, ErrorType.Conflict);
        if (!ticket.TryStartService(performerId, _clock.UtcNow))
            return FailTicket("Tickets.Start.InvalidStatus", TicketRuntimeMessages.TicketMustBeCalled, ErrorType.Domain);
        return await SaveTicketMutationAsync(ticket, "Tickets.Start", cancellationToken);
    }

    public async Task<Result<TicketDetailsResponse>> CompleteServiceAsync(int branchId,
        int ticketId, Guid performerId, CancellationToken cancellationToken)
    {
        var ticket = await LoadTicketForMutationAsync(ticketId, cancellationToken);
        if (ticket is null) return FailTicket("Tickets.Complete.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound);
        if (ticket.BranchId != branchId) return FailTicket("Tickets.Complete.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound);
        if (ticket.Status != TicketStatus.InProgress)
            return FailTicket("Tickets.Complete.InvalidStatus", TicketRuntimeMessages.TicketMustBeInProgress, ErrorType.Domain);

        int? nextServiceId = null; int? nextStep = null;
        if (ticket.BoundWorkflowId.HasValue)
        {
            var next = ticket.WorkflowSteps.Where(x => x.StepOrder > (ticket.CurrentWorkflowStepOrder ?? 0)).OrderBy(x => x.StepOrder).FirstOrDefault();
            if (next is not null)
            {
                var available = await _availability.CheckAsync(branchId, next.ServiceId, _clock.UtcNow, cancellationToken);
                if (!available.IsAvailable)
                    return FailTicket("Tickets.Complete.NextUnavailable", TicketRuntimeMessages.WorkflowServiceUnavailable, ErrorType.Domain);
                nextServiceId = next.ServiceId; nextStep = next.StepOrder;
            }
        }
        ticket.TryCompleteCurrentService(nextServiceId, nextStep, performerId, _clock.UtcNow);
        return await SaveTicketMutationAsync(ticket, "Tickets.Complete", cancellationToken);
    }

    public async Task<Result<TicketDetailsResponse>> ManualTransferAsync(int branchId,
        int ticketId, int targetServiceId, string reason, Guid performerId,
        CancellationToken cancellationToken)
    {
        var ticket = await LoadTicketForMutationAsync(ticketId, cancellationToken);
        if (ticket is null) return FailTicket("Tickets.Transfer.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound);
        if (ticket.BranchId != branchId) return FailTicket("Tickets.Transfer.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound);
        if (ticket.Status != TicketStatus.InProgress) return FailTicket("Tickets.Transfer.InvalidStatus", TicketRuntimeMessages.TicketMustBeInProgress, ErrorType.Domain);
        if (ticket.BoundWorkflowId.HasValue) return FailTicket("Tickets.Transfer.WorkflowBound", TicketRuntimeMessages.WorkflowTransferForbidden, ErrorType.Domain);
        var available = await _availability.CheckAsync(branchId, targetServiceId, _clock.UtcNow, cancellationToken);
        if (!available.IsAvailable) return FailTicket("Tickets.Transfer.TargetUnavailable", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain);
        ticket.TryManualTransfer(targetServiceId, reason, performerId, _clock.UtcNow);
        await BindDefaultWorkflowAsync(ticket, branchId, targetServiceId, cancellationToken);
        return await SaveTicketMutationAsync(ticket, "Tickets.Transfer", cancellationToken);
    }

    public async Task<Result<TicketDetailsResponse>> RecordCallAttemptAsync(int branchId,
        int ticketId, int windowId, Guid performerId, CancellationToken cancellationToken)
    {
        var ticket = await LoadTicketForMutationAsync(ticketId, cancellationToken);
        if (ticket is null) return FailTicket("Tickets.Call.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound);
        if (ticket.BranchId != branchId) return FailTicket("Tickets.Call.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound);
        var windowValid = await _db.Set<Window>().AsNoTracking()
            .AnyAsync(x => x.Id == windowId && x.BranchId == branchId && x.IsActive, cancellationToken);
        if (!windowValid) return FailTicket("Tickets.Call.WindowUnavailable", TicketRuntimeMessages.ServiceUnavailable, ErrorType.Domain);
        var maximumAttempts = await _db.Set<BranchConfiguration>().AsNoTracking()
            .Where(x => x.BranchId == branchId).Select(x => (int?)x.MaximumTicketCallAttempts)
            .SingleOrDefaultAsync(cancellationToken) ?? 3;
        if (!ticket.TryRecordCallAttempt(windowId, performerId, maximumAttempts, _clock.UtcNow))
            return FailTicket("Tickets.Call.ConcurrencyConflict", TicketRuntimeMessages.ConcurrencyConflict, ErrorType.Conflict);
        return await SaveTicketMutationAsync(ticket, "Tickets.Call", cancellationToken);
    }

    public async Task<Result<ReservationDetailsResponse>> CancelReservationAsync(int branchId,
        int reservationId, string reason, Guid performerId, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var reservation = await _db.Set<Reservation>().Include(x => x.CustomInputValues)
                .SingleOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
            if (reservation is null) return await RollbackReservationAsync(transaction, "Reservations.Cancel.NotFound", TicketRuntimeMessages.ReservationNotFound, ErrorType.NotFound, cancellationToken);
            if (reservation.BranchId != branchId) return await RollbackReservationAsync(transaction, "Reservations.Cancel.WrongBranch", TicketRuntimeMessages.WrongBranch, ErrorType.NotFound, cancellationToken);
            if (!reservation.TryCancel(reason, performerId, _clock.UtcNow))
                return await RollbackReservationAsync(transaction, "Reservations.Cancel.Terminal", TicketRuntimeMessages.ReservationTerminal, ErrorType.Domain, cancellationToken);
            await ReleaseQuotaAsync(reservation.BranchServiceSegmentId, reservation.BusinessDate, _clock.UtcNow, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<ReservationDetailsResponse>.Ok(Map(reservation));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken); _logger.LogWarning(ex, "Reservation {ReservationId} cancellation conflict", reservationId);
            return ConcurrencyReservation("Reservations.Cancel.ConcurrencyConflict");
        }
    }

    public async Task<Result<Pagination<TicketListItemResponse>>> GetTicketsAsync(int branchId,
        int pageNumber, int pageSize, TicketStatus? status, int? serviceId, int? segmentId,
        string? ticketNumber, DateOnly? businessDate, CancellationToken cancellationToken)
    {
        var query = _db.Set<Ticket>().AsNoTracking().Where(x => x.BranchId == branchId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (serviceId.HasValue) query = query.Where(x => x.CurrentServiceId == serviceId);
        if (segmentId.HasValue) query = query.Where(x => x.SegmentId == segmentId);
        if (!string.IsNullOrWhiteSpace(ticketNumber)) query = query.Where(x => x.TicketNumber.Contains(ticketNumber.Trim()));
        if (businessDate.HasValue) query = query.Where(x => x.BusinessDate == businessDate);
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderByDescending(x => x.CreatedOnUtc).ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new TicketListItemResponse(x.Id, x.TicketNumber, x.BusinessDate,
                x.Status, x.IssuingServiceId, x.CurrentServiceId, x.SegmentId,
                x.CurrentQueueEnteredOnUtc)).ToListAsync(cancellationToken);
        return Result<Pagination<TicketListItemResponse>>.Ok(new(pageNumber, pageSize, total, data));
    }

    public async Task<Result<TicketDetailsResponse>> GetTicketAsync(int branchId, int ticketId,
        CancellationToken cancellationToken)
    {
        var ticket = await _db.Set<Ticket>().AsNoTracking().AsSplitQuery()
            .Include(x => x.CustomInputValues).Include(x => x.ServiceJourneys)
            .SingleOrDefaultAsync(x => x.Id == ticketId, cancellationToken);
        return ticket is null || ticket.BranchId != branchId
            ? FailTicket("Tickets.View.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound)
            : Result<TicketDetailsResponse>.Ok(Map(ticket));
    }

    public async Task<Result<IReadOnlyList<TicketHistoryResponse>>> GetTicketHistoryAsync(
        int branchId, int ticketId, CancellationToken cancellationToken)
    {
        if (!await _db.Set<Ticket>().AsNoTracking().AnyAsync(x => x.Id == ticketId && x.BranchId == branchId, cancellationToken))
            return Result<IReadOnlyList<TicketHistoryResponse>>.Fail(new Error("Tickets.History.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound));
        var data = await _db.Set<TicketHistory>().AsNoTracking().Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.OccurredOnUtc).ThenBy(x => x.Id)
            .Select(x => new TicketHistoryResponse(x.Id, x.EventType, x.FromStatus, x.ToStatus,
                x.ServiceId, x.FromServiceId, x.ToServiceId, x.WindowId,
                x.PerformerApplicationUserId, x.Reason, x.OccurredOnUtc))
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<TicketHistoryResponse>>.Ok(data);
    }

    public async Task<Result<Pagination<ReservationListItemResponse>>> GetReservationsAsync(
        int branchId, int pageNumber, int pageSize, ReservationStatus? status, int? serviceId,
        int? segmentId, DateOnly? businessDate, CancellationToken cancellationToken)
    {
        var query = _db.Set<Reservation>().AsNoTracking().Where(x => x.BranchId == branchId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (serviceId.HasValue) query = query.Where(x => x.ServiceId == serviceId);
        if (segmentId.HasValue) query = query.Where(x => x.SegmentId == segmentId);
        if (businessDate.HasValue) query = query.Where(x => x.BusinessDate == businessDate);
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderBy(x => x.ScheduledOnUtc).ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new ReservationListItemResponse(x.Id, x.ScheduledOnUtc, x.Status,
                x.ServiceId, x.SegmentId)).ToListAsync(cancellationToken);
        return Result<Pagination<ReservationListItemResponse>>.Ok(new(pageNumber, pageSize, total, data));
    }

    public async Task<Result<ReservationDetailsResponse>> GetReservationAsync(int branchId,
        int reservationId, CancellationToken cancellationToken)
    {
        var reservation = await _db.Set<Reservation>().AsNoTracking().Include(x => x.CustomInputValues)
            .SingleOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
        return reservation is null || reservation.BranchId != branchId
            ? FailReservation("Reservations.View.NotFound", TicketRuntimeMessages.ReservationNotFound, ErrorType.NotFound)
            : Result<ReservationDetailsResponse>.Ok(Map(reservation));
    }

    public async Task<Result<IReadOnlyList<ReservationHistoryResponse>>> GetReservationHistoryAsync(
        int branchId, int reservationId, CancellationToken cancellationToken)
    {
        if (!await _db.Set<Reservation>().AsNoTracking().AnyAsync(x => x.Id == reservationId && x.BranchId == branchId, cancellationToken))
            return Result<IReadOnlyList<ReservationHistoryResponse>>.Fail(new Error("Reservations.History.NotFound", TicketRuntimeMessages.ReservationNotFound, ErrorType.NotFound));
        var data = await _db.Set<ReservationHistory>().AsNoTracking().Where(x => x.ReservationId == reservationId)
            .OrderBy(x => x.OccurredOnUtc).ThenBy(x => x.Id)
            .Select(x => new ReservationHistoryResponse(x.Id, x.EventType, x.FromStatus,
                x.ToStatus, x.PerformerApplicationUserId, x.Reason, x.OccurredOnUtc))
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<ReservationHistoryResponse>>.Ok(data);
    }

    private async Task<Ticket?> LoadTicketForMutationAsync(int ticketId, CancellationToken ct) =>
        await _db.Set<Ticket>().AsSplitQuery().Include(x => x.CustomInputValues)
            .Include(x => x.ServiceJourneys).Include(x => x.WorkflowSteps)
            .SingleOrDefaultAsync(x => x.Id == ticketId, ct);

    private async Task<Result<TicketDetailsResponse>> SaveTicketMutationAsync(Ticket ticket,
        string code, CancellationToken ct)
    {
        try { await _db.SaveChangesAsync(ct); return Result<TicketDetailsResponse>.Ok(Map(ticket)); }
        catch (DbUpdateConcurrencyException ex) { _logger.LogWarning(ex, "Ticket {TicketId} concurrency conflict", ticket.Id); return ConcurrencyTicket($"{code}.ConcurrencyConflict"); }
    }

    private async Task<BranchServiceSegment?> GetSegmentRelationshipAsync(int branchId,
        int serviceId, int segmentId, CancellationToken ct) =>
        await _db.Set<BranchServiceSegment>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.BranchService.BranchId == branchId &&
                x.BranchService.ServiceId == serviceId && x.SegmentId == segmentId, ct);

    private async Task<bool> TryConsumeQuotaAsync(BranchServiceSegment relationship,
        DateOnly businessDate, DateTime now, CancellationToken ct)
    {
        var usage = await _db.Set<BranchServiceSegmentDailyUsage>()
            .SingleOrDefaultAsync(x => x.BranchServiceSegmentId == relationship.Id &&
                x.BusinessDate == businessDate, ct);
        if (usage is null)
        {
            if (relationship.Quota <= 0) return false;
            _db.Add(BranchServiceSegmentDailyUsage.Create(relationship.Id, businessDate, 1, now));
            return true;
        }
        return usage.TryConsume(relationship.Quota, now);
    }

    private async Task ReleaseQuotaAsync(int relationshipId, DateOnly businessDate,
        DateTime now, CancellationToken ct)
    {
        var usage = await _db.Set<BranchServiceSegmentDailyUsage>()
            .SingleOrDefaultAsync(x => x.BranchServiceSegmentId == relationshipId && x.BusinessDate == businessDate, ct);
        usage?.Release(now);
    }

    private async Task<Result<string>> AllocateTicketNumberAsync(int branchId, int serviceId,
        DateOnly businessDate, BranchServiceAvailability availability, DateTime now,
        CancellationToken ct)
    {
        if (!availability.RangeStartNumber.HasValue || !availability.RangeEndNumber.HasValue ||
            availability.RangeStartNumber <= 0 || availability.RangeEndNumber < availability.RangeStartNumber)
            return Result<string>.Fail(new Error("Tickets.Number.RangeUnavailable", TicketRuntimeMessages.NumberRangeExhausted, ErrorType.Conflict));
        var sequence = await _db.Set<TicketNumberSequence>()
            .SingleOrDefaultAsync(x => x.BranchId == branchId && x.ServiceId == serviceId && x.BusinessDate == businessDate, ct);
        int number;
        if (sequence is null)
        {
            number = availability.RangeStartNumber.Value;
            _db.Add(TicketNumberSequence.Create(branchId, serviceId, businessDate, number, now));
        }
        else if (!sequence.TryAllocate(availability.RangeEndNumber.Value, now, out number))
            return Result<string>.Fail(new Error("Tickets.Number.RangeExhausted", TicketRuntimeMessages.NumberRangeExhausted, ErrorType.Conflict));
        return Result<string>.Ok(($"{availability.RangePrefix}{number.ToString(CultureInfo.InvariantCulture)}").Trim());
    }

    private async Task BindDefaultWorkflowAsync(Ticket ticket, int branchId, int serviceId,
        CancellationToken ct)
    {
        var workflow = await _db.Set<ServiceWorkflow>().AsNoTracking().Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.BranchId == branchId && x.LeafServiceId == serviceId &&
                x.IsActive && x.IsDefault, ct);
        if (workflow is not null)
        {
            var serviceIds = workflow.Steps.OrderBy(x => x.StepOrder)
                .Select(x => x.ServiceId).Where(x => x != serviceId).ToList();
            serviceIds.Insert(0, serviceId);
            ticket.BindWorkflow(workflow.Id,
                serviceIds.Distinct().Select((id, index) => (id, index + 1)), _clock.UtcNow);
        }
    }

    private async Task<Result<ValidatedCreateInputs>> ValidateCreateInputsAsync(int serviceId,
        string? lookupValue, IReadOnlyCollection<CustomInputSubmission> submitted,
        string errorCodePrefix, bool requireLookupValueWhenNoCustomInputs,
        CancellationToken ct)
    {
        var definitions = await _db.Set<ServiceCustomInput>().AsNoTracking()
            .Where(x => x.ServiceId == serviceId && x.IsActive).OrderBy(x => x.Order).ToListAsync(ct);

        if (definitions.Count == 0)
        {
            if (submitted.Count != 0)
                return Result<ValidatedCreateInputs>.Fail(new Error(
                    $"{errorCodePrefix}.CustomInputsNotAllowed",
                    TicketRuntimeMessages.CustomInputsNotAllowed, ErrorType.Validation));
            if (requireLookupValueWhenNoCustomInputs && string.IsNullOrWhiteSpace(lookupValue))
                return Result<ValidatedCreateInputs>.Fail(new Error(
                    $"{errorCodePrefix}.FieldRequired", TicketRuntimeMessages.FieldRequired,
                    ErrorType.Validation));
            if (string.IsNullOrWhiteSpace(lookupValue))
                return Result<ValidatedCreateInputs>.Ok(new(null,
                    Array.Empty<InputSnapshot>()));
            var normalizedLookupValue = lookupValue.Trim();
            if (normalizedLookupValue.Length > 3000)
                return Result<ValidatedCreateInputs>.Fail(new Error(
                    $"{errorCodePrefix}.FieldInvalid", TicketRuntimeMessages.FieldInvalid,
                    ErrorType.Validation));
            return Result<ValidatedCreateInputs>.Ok(new(normalizedLookupValue,
                Array.Empty<InputSnapshot>()));
        }

        if (!string.IsNullOrWhiteSpace(lookupValue))
            return Result<ValidatedCreateInputs>.Fail(new Error(
                $"{errorCodePrefix}.FieldNotAllowed", TicketRuntimeMessages.FieldNotAllowed,
                ErrorType.Validation));

        var snapshots = ValidateInputs(definitions, submitted, requireAllRequired: true);
        return snapshots.IsFailure
            ? Result<ValidatedCreateInputs>.Fail(snapshots.Errors)
            : Result<ValidatedCreateInputs>.Ok(new(null, snapshots.Value));
    }

    private static Result<IReadOnlyList<InputSnapshot>> ValidateInputs(
        IReadOnlyCollection<ServiceCustomInput> definitions,
        IReadOnlyCollection<CustomInputSubmission> submitted,
        bool requireAllRequired)
    {
        if (submitted.GroupBy(x => x.ServiceCustomInputId).Any(x => x.Count() > 1))
            return Result<IReadOnlyList<InputSnapshot>>.Fail(new Error(
                "TicketRuntime.CustomInputs.Duplicate",
                TicketRuntimeMessages.DuplicateCustomInput, ErrorType.Validation));
        if (submitted.Any(x => definitions.All(d => d.Id != x.ServiceCustomInputId)))
            return Result<IReadOnlyList<InputSnapshot>>.Fail(new Error(
                "TicketRuntime.CustomInputs.NotApplicable",
                TicketRuntimeMessages.CustomInputNotApplicable, ErrorType.Validation));
        var values = submitted.ToDictionary(x => x.ServiceCustomInputId, x => x.Value ?? string.Empty);
        var snapshots = new List<InputSnapshot>();
        foreach (var definition in definitions)
        {
            values.TryGetValue(definition.Id, out var value);
            value ??= string.Empty;
            if (requireAllRequired && definition.IsRequired && string.IsNullOrWhiteSpace(value))
                return InvalidInputs();
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (definition.Type == ServiceCustomInputType.String)
            {
                if (definition.MinLength.HasValue && value.Length < definition.MinLength ||
                    definition.MaxLength.HasValue && value.Length > definition.MaxLength ||
                    definition.StartWith is not null && !value.StartsWith(definition.StartWith, StringComparison.Ordinal)) return InvalidInputs();
            }
            else if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer) ||
                definition.MinValue.HasValue && integer < definition.MinValue ||
                definition.MaxValue.HasValue && integer > definition.MaxValue) return InvalidInputs();
            snapshots.Add(new(definition.Id, definition.Name, definition.LabelEn,
                definition.LabelAr, definition.Type, value, definition.Order));
        }
        return Result<IReadOnlyList<InputSnapshot>>.Ok(snapshots);
    }

    private static Result<IReadOnlyList<InputSnapshot>> InvalidInputs() =>
        Result<IReadOnlyList<InputSnapshot>>.Fail(new Error("TicketRuntime.CustomInputs.Invalid", TicketRuntimeMessages.CustomInputsInvalid, ErrorType.Validation));

    private sealed record InputSnapshot(int? Id, string Name, string? LabelEn,
        string? LabelAr, ServiceCustomInputType Type, string Value, int? Order)
    {
        public TicketCustomInputValue ToTicket(DateTime now) => TicketCustomInputValue.Create(Id, Name, LabelEn, LabelAr, Type, Value, now, Order);
        public ReservationCustomInputValue ToReservation(DateTime now) => ReservationCustomInputValue.Create(Id, Name, LabelEn, LabelAr, Type, Value, now, Order);
    }

    private sealed record ValidatedCreateInputs(
        string? LookupValue,
        IReadOnlyList<InputSnapshot> Snapshots);

    private static TicketDetailsResponse Map(
        Ticket x,
        TicketPrintModelResponse? print = null) => new(x.Id, x.BranchId,
        x.TicketNumber, x.BusinessDate, x.Status, x.IssuingServiceId, x.CurrentServiceId,
        x.SegmentId, x.ReservationId, x.CurrentWindowId, x.CurrentQueueEnteredOnUtc,
        x.CurrentServiceStartedOnUtc, x.CompletedOnUtc, x.CancelledOnUtc,
        x.CancellationReason, x.BoundWorkflowId, x.CurrentWorkflowStepOrder,
        x.CustomInputValues.OrderBy(v => v.Id).Select(v => new CustomInputValueResponse(v.ServiceCustomInputId,
            v.NameSnapshot, v.LabelEnSnapshot, v.LabelArSnapshot, v.TypeSnapshot, v.Value,
            v.OrderSnapshot)).ToArray(),
        x.ServiceJourneys.OrderBy(v => v.Id).Select(v => new TicketJourneyResponse(v.ServiceId,
            v.EntryType, v.WorkflowStepOrder, v.EnteredWaitingOnUtc, v.ServiceStartedOnUtc,
            v.ServiceEndedOnUtc, v.Outcome)).ToArray(), Convert.ToBase64String(x.RowVersion),
        Field: x.LookupValue,
        Print: print);

    private static ReservationDetailsResponse Map(Reservation x) => new(x.Id, x.BranchId,
        x.ServiceId, x.SegmentId, x.ScheduledOnUtc, x.BusinessDate, x.Status,
        x.CancellationReason, x.CancelledOnUtc, x.ConvertedToTicketOnUtc, x.ExpiredOnUtc,
        x.CustomInputValues.OrderBy(v => v.Id).Select(v => new CustomInputValueResponse(v.ServiceCustomInputId,
            v.NameSnapshot, v.LabelEnSnapshot, v.LabelArSnapshot, v.TypeSnapshot, v.Value,
            v.OrderSnapshot)).ToArray(),
        Convert.ToBase64String(x.RowVersion), Field: x.LookupValue);

    private static Result<IReadOnlyList<KioskReservationSearchItemResponse>> FailKioskSearch(
        string code, string message, ErrorType type) =>
        Result<IReadOnlyList<KioskReservationSearchItemResponse>>.Fail(
            new Error(code, message, type));

    private static Result<TicketDetailsResponse> FailTicket(string code, string message, ErrorType type) => Result<TicketDetailsResponse>.Fail(new Error(code, message, type));
    private static Result<ReservationDetailsResponse> FailReservation(string code, string message, ErrorType type) => Result<ReservationDetailsResponse>.Fail(new Error(code, message, type));
    private static Result<TicketDetailsResponse> ConcurrencyTicket(string code) => FailTicket(code, TicketRuntimeMessages.ConcurrencyConflict, ErrorType.Conflict);
    private static Result<ReservationDetailsResponse> ConcurrencyReservation(string code) => FailReservation(code, TicketRuntimeMessages.ConcurrencyConflict, ErrorType.Conflict);

    private static async Task<Result<TicketDetailsResponse>> RollbackTicketAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction, string code,
        string message, ErrorType type, CancellationToken ct)
    { await transaction.RollbackAsync(ct); return FailTicket(code, message, type); }
    private static async Task<Result<ReservationDetailsResponse>> RollbackReservationAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction, string code,
        string message, ErrorType type, CancellationToken ct)
    { await transaction.RollbackAsync(ct); return FailReservation(code, message, type); }

    private sealed class NullTicketPrintModelBuilder : ITicketPrintModelBuilder
    {
        public static readonly NullTicketPrintModelBuilder Instance = new();

        public Task<TicketPrintModelResponse?> BuildAsync(
            int ticketId,
            CancellationToken cancellationToken) => Task.FromResult<TicketPrintModelResponse?>(null);
    }
}
