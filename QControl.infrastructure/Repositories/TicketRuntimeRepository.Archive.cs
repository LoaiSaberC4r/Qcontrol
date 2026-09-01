using System.Data;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Repositories;

internal sealed partial class TicketRuntimeRepository
{
    public async Task<Result<Pagination<TicketListItemResponse>>> GetArchivedTicketsAsync(
        int branchId, int pageNumber, int pageSize, TicketStatus? status, int? serviceId,
        int? segmentId, string? ticketNumber, DateOnly? businessDate,
        CancellationToken cancellationToken)
    {
        var query = _db.Set<TicketArchive>().AsNoTracking().Where(x => x.BranchId == branchId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (serviceId.HasValue) query = query.Where(x => x.IssuingServiceId == serviceId || x.CurrentServiceId == serviceId);
        if (segmentId.HasValue) query = query.Where(x => x.SegmentId == segmentId);
        if (!string.IsNullOrWhiteSpace(ticketNumber)) query = query.Where(x => x.TicketNumber.Contains(ticketNumber.Trim()));
        if (businessDate.HasValue) query = query.Where(x => x.BusinessDate == businessDate);
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderByDescending(x => x.BusinessDate).ThenByDescending(x => x.OriginalTicketId)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new TicketListItemResponse(x.OriginalTicketId, x.TicketNumber,
                x.BusinessDate, x.Status, x.IssuingServiceId, x.CurrentServiceId,
                x.SegmentId, x.CurrentQueueEnteredOnUtc)).ToListAsync(cancellationToken);
        return Result<Pagination<TicketListItemResponse>>.Ok(new(pageNumber, pageSize, total, data));
    }

    public async Task<Result<TicketDetailsResponse>> GetArchivedTicketAsync(int branchId,
        int ticketId, CancellationToken cancellationToken)
    {
        var archive = await _db.Set<TicketArchive>().AsNoTracking().AsSplitQuery()
            .Include(x => x.Journeys).Include(x => x.History).Include(x => x.CallAttempts)
            .Include(x => x.CustomInputs)
            .SingleOrDefaultAsync(x => x.OriginalTicketId == ticketId && x.BranchId == branchId,
                cancellationToken);
        return archive is null
            ? FailTicket("Tickets.Archive.NotFound", TicketRuntimeMessages.TicketNotFound, ErrorType.NotFound)
            : Result<TicketDetailsResponse>.Ok(Map(archive));
    }

    public async Task<Result<Pagination<ReservationListItemResponse>>> GetArchivedReservationsAsync(
        int branchId, int pageNumber, int pageSize, ReservationStatus? status, int? serviceId,
        int? segmentId, DateOnly? businessDate, CancellationToken cancellationToken)
    {
        var query = _db.Set<ReservationArchive>().AsNoTracking().Where(x => x.BranchId == branchId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (serviceId.HasValue) query = query.Where(x => x.ServiceId == serviceId);
        if (segmentId.HasValue) query = query.Where(x => x.SegmentId == segmentId);
        if (businessDate.HasValue) query = query.Where(x => x.BusinessDate == businessDate);
        var total = await query.CountAsync(cancellationToken);
        var data = await query.OrderByDescending(x => x.BusinessDate).ThenByDescending(x => x.OriginalReservationId)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new ReservationListItemResponse(x.OriginalReservationId,
                x.ScheduledOnUtc, x.Status, x.ServiceId, x.SegmentId)).ToListAsync(cancellationToken);
        return Result<Pagination<ReservationListItemResponse>>.Ok(new(pageNumber, pageSize, total, data));
    }

    public async Task<Result<ReservationDetailsResponse>> GetArchivedReservationAsync(int branchId,
        int reservationId, CancellationToken cancellationToken)
    {
        var archive = await _db.Set<ReservationArchive>().AsNoTracking().AsSplitQuery()
            .Include(x => x.History).Include(x => x.CustomInputs)
            .SingleOrDefaultAsync(x => x.OriginalReservationId == reservationId &&
                x.BranchId == branchId, cancellationToken);
        return archive is null
            ? FailReservation("Reservations.Archive.NotFound", TicketRuntimeMessages.ReservationNotFound, ErrorType.NotFound)
            : Result<ReservationDetailsResponse>.Ok(Map(archive));
    }

    public async Task<int> AutoCancelNoShowTicketsAsync(int batchSize,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var candidates = await _db.Set<Ticket>().AsNoTracking()
            .Where(x => x.Status == TicketStatus.NoShow)
            .Select(x => new
            {
                x.Id,
                Timeout = x.Branch.Configuration == null ? 30 : x.Branch.Configuration.TicketNoShowAutoCancellationMinutes,
                LastNoShow = x.History.Where(h => h.EventType == TicketHistoryEventType.NoShow)
                    .Max(h => (DateTime?)h.OccurredOnUtc)
            })
            .OrderBy(x => x.LastNoShow).Take(batchSize * 4).ToListAsync(cancellationToken);
        var ids = candidates.Where(x => x.LastNoShow.HasValue &&
                x.LastNoShow.Value.AddMinutes(x.Timeout) <= now)
            .Take(batchSize).Select(x => x.Id).ToArray();
        var processed = 0;
        foreach (var id in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _db.ChangeTracker.Clear();
            var ticket = await LoadTicketForMutationAsync(id, cancellationToken);
            if (ticket?.TryCancel(TicketRuntimeMessages.AutomaticNoShowCancellation, null, now) != true) continue;
            try { await _db.SaveChangesAsync(cancellationToken); processed++; }
            catch (DbUpdateConcurrencyException ex) { _logger.LogInformation(ex, "Skipped concurrent NoShow cancellation for Ticket {TicketId}", id); }
        }
        return processed;
    }

    public async Task<int> ProcessReservationEndOfDayAsync(int batchSize,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var today = DateOnly.FromDateTime(now);
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var reservations = await _db.Set<Reservation>()
                .Where(x => x.BusinessDate < today &&
                    (x.Status == ReservationStatus.Active || x.Status == ReservationStatus.NoShow))
                .OrderBy(x => x.BusinessDate).ThenBy(x => x.Id).Take(batchSize)
                .ToListAsync(cancellationToken);
            foreach (var reservation in reservations)
            {
                if (reservation.Status == ReservationStatus.Active)
                    reservation.TryMarkNoShow(now);
                if (reservation.TryExpire(now))
                    await ReleaseQuotaAsync(reservation.BranchServiceSegmentId,
                        reservation.BusinessDate, now, cancellationToken);
            }
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return reservations.Count;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Reservation end-of-day lifecycle concurrency conflict");
            return 0;
        }
    }

    public async Task<int> ArchiveEligibleTicketsAsync(int batchSize,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var candidates = await _db.Set<Ticket>().AsNoTracking()
            .Where(x => x.Status == TicketStatus.Completed || x.Status == TicketStatus.Cancelled)
            .Select(x => new
            {
                x.Id,
                TerminalOn = x.Status == TicketStatus.Completed ? x.CompletedOnUtc : x.CancelledOnUtc,
                Retention = x.Branch.Configuration == null ? 30 : x.Branch.Configuration.TicketArchiveRetentionDays
            })
            .OrderBy(x => x.TerminalOn).Take(batchSize * 4).ToListAsync(cancellationToken);
        var ids = candidates.Where(x => x.TerminalOn.HasValue &&
                x.TerminalOn.Value.AddDays(x.Retention) <= now)
            .Take(batchSize).Select(x => x.Id).ToArray();
        var count = 0;
        foreach (var id in ids)
            if (await ArchiveTicketAsync(id, now, cancellationToken)) count++;
        return count;
    }

    private async Task<bool> ArchiveTicketAsync(int ticketId, DateTime archivedOnUtc,
        CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var ticket = await _db.Set<Ticket>().AsSplitQuery()
                .Include(x => x.ServiceJourneys).Include(x => x.History)
                .Include(x => x.CallAttempts).Include(x => x.CustomInputValues)
                .Include(x => x.WorkflowSteps)
                .SingleOrDefaultAsync(x => x.Id == ticketId, cancellationToken);
            if (ticket is null || ticket.Status is not (TicketStatus.Completed or TicketStatus.Cancelled))
            { await transaction.RollbackAsync(cancellationToken); return false; }
            if (await _db.Set<TicketArchive>().AnyAsync(x => x.OriginalTicketId == ticketId, cancellationToken))
            { await transaction.RollbackAsync(cancellationToken); return false; }
            var archive = CreateArchive(ticket, archivedOnUtc);
            _db.Add(archive);
            await _db.SaveChangesAsync(cancellationToken);
            _db.Remove(ticket);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to archive Ticket {TicketId}; operational data was preserved", ticketId);
            return false;
        }
    }

    public async Task<int> ArchiveEligibleReservationsAsync(int batchSize,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var ids = await _db.Set<Reservation>().AsNoTracking()
            .Where(x => x.BusinessDate < today &&
                (x.Status == ReservationStatus.ConvertedToTicket || x.Status == ReservationStatus.Cancelled || x.Status == ReservationStatus.Expired) &&
                !_db.Set<Ticket>().Any(t => t.ReservationId == x.Id))
            .OrderBy(x => x.BusinessDate).ThenBy(x => x.Id).Select(x => x.Id)
            .Take(batchSize).ToArrayAsync(cancellationToken);
        var count = 0;
        foreach (var id in ids)
            if (await ArchiveReservationAsync(id, _clock.UtcNow, cancellationToken)) count++;
        return count;
    }

    private async Task<bool> ArchiveReservationAsync(int reservationId, DateTime archivedOnUtc,
        CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var reservation = await _db.Set<Reservation>().AsSplitQuery()
                .Include(x => x.History).Include(x => x.CustomInputValues)
                .SingleOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
            if (reservation is null || reservation.Status is not
                (ReservationStatus.ConvertedToTicket or ReservationStatus.Cancelled or ReservationStatus.Expired))
            { await transaction.RollbackAsync(cancellationToken); return false; }
            if (await _db.Set<ReservationArchive>().AnyAsync(x => x.OriginalReservationId == reservationId, cancellationToken))
            { await transaction.RollbackAsync(cancellationToken); return false; }
            _db.Add(CreateArchive(reservation, archivedOnUtc));
            await _db.SaveChangesAsync(cancellationToken);
            _db.Remove(reservation);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to archive Reservation {ReservationId}; operational data was preserved", reservationId);
            return false;
        }
    }

    private static TicketArchive CreateArchive(Ticket x, DateTime archivedOnUtc)
    {
        var archive = new TicketArchive
        {
            OriginalTicketId = x.Id, BranchId = x.BranchId, IssuingServiceId = x.IssuingServiceId,
            CurrentServiceId = x.CurrentServiceId, SegmentId = x.SegmentId, ReservationId = x.ReservationId,
            TicketNumber = x.TicketNumber, BusinessDate = x.BusinessDate, Status = x.Status,
            CurrentQueueEnteredOnUtc = x.CurrentQueueEnteredOnUtc,
            CurrentServiceStartedOnUtc = x.CurrentServiceStartedOnUtc, CompletedOnUtc = x.CompletedOnUtc,
            CancelledOnUtc = x.CancelledOnUtc, CancellationReason = x.CancellationReason,
            BoundWorkflowId = x.BoundWorkflowId, CurrentWorkflowStepOrder = x.CurrentWorkflowStepOrder,
            CreatedOnUtc = x.CreatedOnUtc, ArchivedOnUtc = archivedOnUtc
        };
        archive.Journeys.AddRange(x.ServiceJourneys.Select(j => new TicketServiceJourneyArchive
        { OriginalJourneyId = j.Id, ServiceId = j.ServiceId, EntryType = j.EntryType,
            WorkflowStepOrder = j.WorkflowStepOrder, EnteredWaitingOnUtc = j.EnteredWaitingOnUtc,
            ServiceStartedOnUtc = j.ServiceStartedOnUtc, ServiceEndedOnUtc = j.ServiceEndedOnUtc, Outcome = j.Outcome }));
        archive.History.AddRange(x.History.Select(h => new TicketHistoryArchive
        { OriginalHistoryId = h.Id, EventType = h.EventType, FromStatus = h.FromStatus,
            ToStatus = h.ToStatus, ServiceId = h.ServiceId, FromServiceId = h.FromServiceId,
            ToServiceId = h.ToServiceId, WindowId = h.WindowId,
            PerformerApplicationUserId = h.PerformerApplicationUserId, Reason = h.Reason,
            OccurredOnUtc = h.OccurredOnUtc }));
        archive.History.Add(new TicketHistoryArchive { EventType = TicketHistoryEventType.Archived,
            FromStatus = x.Status, ToStatus = x.Status, OccurredOnUtc = archivedOnUtc });
        archive.CallAttempts.AddRange(x.CallAttempts.Select(a => new TicketCallAttemptArchive
        { OriginalCallAttemptId = a.Id, CallCycleNumber = a.CallCycleNumber,
            AttemptNumber = a.AttemptNumber, WindowId = a.WindowId,
            PerformerApplicationUserId = a.PerformerApplicationUserId, CalledOnUtc = a.CalledOnUtc }));
        archive.CustomInputs.AddRange(x.CustomInputValues.Select(v => new TicketCustomInputValueArchive
        { ServiceCustomInputId = v.ServiceCustomInputId, NameSnapshot = v.NameSnapshot,
            LabelEnSnapshot = v.LabelEnSnapshot, LabelArSnapshot = v.LabelArSnapshot,
            TypeSnapshot = v.TypeSnapshot, Value = v.Value }));
        archive.WorkflowSteps.AddRange(x.WorkflowSteps.Select(s => new TicketWorkflowStepSnapshotArchive
        { SourceWorkflowId = s.SourceWorkflowId, ServiceId = s.ServiceId, StepOrder = s.StepOrder }));
        return archive;
    }

    private static ReservationArchive CreateArchive(Reservation x, DateTime archivedOnUtc)
    {
        var archive = new ReservationArchive
        {
            OriginalReservationId = x.Id, BranchId = x.BranchId, ServiceId = x.ServiceId,
            SegmentId = x.SegmentId, BranchServiceSegmentId = x.BranchServiceSegmentId,
            ScheduledOnUtc = x.ScheduledOnUtc, BusinessDate = x.BusinessDate, Status = x.Status,
            CancellationReason = x.CancellationReason, CancelledOnUtc = x.CancelledOnUtc,
            ConvertedToTicketOnUtc = x.ConvertedToTicketOnUtc, ExpiredOnUtc = x.ExpiredOnUtc,
            CreatedOnUtc = x.CreatedOnUtc, ArchivedOnUtc = archivedOnUtc
        };
        archive.History.AddRange(x.History.Select(h => new ReservationHistoryArchive
        { OriginalHistoryId = h.Id, EventType = h.EventType, FromStatus = h.FromStatus,
            ToStatus = h.ToStatus, PerformerApplicationUserId = h.PerformerApplicationUserId,
            Reason = h.Reason, OccurredOnUtc = h.OccurredOnUtc }));
        archive.History.Add(new ReservationHistoryArchive { EventType = ReservationHistoryEventType.Archived,
            FromStatus = x.Status, ToStatus = x.Status, OccurredOnUtc = archivedOnUtc });
        archive.CustomInputs.AddRange(x.CustomInputValues.Select(v => new ReservationCustomInputValueArchive
        { ServiceCustomInputId = v.ServiceCustomInputId, NameSnapshot = v.NameSnapshot,
            LabelEnSnapshot = v.LabelEnSnapshot, LabelArSnapshot = v.LabelArSnapshot,
            TypeSnapshot = v.TypeSnapshot, Value = v.Value }));
        return archive;
    }

    private static TicketDetailsResponse Map(TicketArchive x) => new(x.OriginalTicketId,
        x.BranchId, x.TicketNumber, x.BusinessDate, x.Status, x.IssuingServiceId,
        x.CurrentServiceId, x.SegmentId, x.ReservationId, null, x.CurrentQueueEnteredOnUtc,
        x.CurrentServiceStartedOnUtc, x.CompletedOnUtc, x.CancelledOnUtc,
        x.CancellationReason, x.BoundWorkflowId, x.CurrentWorkflowStepOrder,
        x.CustomInputs.OrderBy(v => v.Id).Select(v => new CustomInputValueResponse(v.ServiceCustomInputId,
            v.NameSnapshot, v.LabelEnSnapshot, v.LabelArSnapshot, v.TypeSnapshot, v.Value)).ToArray(),
        x.Journeys.OrderBy(v => v.OriginalJourneyId).Select(v => new TicketJourneyResponse(v.ServiceId,
            v.EntryType, v.WorkflowStepOrder, v.EnteredWaitingOnUtc, v.ServiceStartedOnUtc,
            v.ServiceEndedOnUtc, v.Outcome)).ToArray(), null, true,
        x.History.OrderBy(v => v.OccurredOnUtc).ThenBy(v => v.Id).Select(v => new TicketHistoryResponse(
            v.OriginalHistoryId ?? v.Id, v.EventType, v.FromStatus, v.ToStatus, v.ServiceId,
            v.FromServiceId, v.ToServiceId, v.WindowId, v.PerformerApplicationUserId,
            v.Reason, v.OccurredOnUtc)).ToArray(),
        x.CallAttempts.OrderBy(v => v.CallCycleNumber).ThenBy(v => v.AttemptNumber)
            .Select(v => new TicketCallAttemptResponse(v.CallCycleNumber, v.AttemptNumber,
                v.WindowId, v.PerformerApplicationUserId, v.CalledOnUtc)).ToArray());

    private static ReservationDetailsResponse Map(ReservationArchive x) => new(
        x.OriginalReservationId, x.BranchId, x.ServiceId, x.SegmentId, x.ScheduledOnUtc,
        x.BusinessDate, x.Status, x.CancellationReason, x.CancelledOnUtc,
        x.ConvertedToTicketOnUtc, x.ExpiredOnUtc,
        x.CustomInputs.OrderBy(v => v.Id).Select(v => new CustomInputValueResponse(v.ServiceCustomInputId,
            v.NameSnapshot, v.LabelEnSnapshot, v.LabelArSnapshot, v.TypeSnapshot, v.Value)).ToArray(),
        null, true, x.History.OrderBy(v => v.OccurredOnUtc).ThenBy(v => v.Id)
            .Select(v => new ReservationHistoryResponse(v.OriginalHistoryId ?? v.Id,
                v.EventType, v.FromStatus, v.ToStatus, v.PerformerApplicationUserId,
                v.Reason, v.OccurredOnUtc)).ToArray());
}
