using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Services;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Services;

internal sealed class BranchServiceAvailabilityChecker : IBranchServiceAvailabilityChecker
{
    private readonly PlatformWriteDbContext _db;
    public BranchServiceAvailabilityChecker(PlatformWriteDbContext db) => _db = db;

    public async Task<BranchServiceAvailability> CheckAsync(int branchId, int serviceId,
        DateTime atUtc, CancellationToken cancellationToken, bool applyAllowedTime = false)
    {
        // Include deleted nodes and children, just as discovery does, so ancestor
        // activity and leaf status use the same hierarchy semantics at any depth.
        var items = await _db.Set<Service>().IgnoreQueryFilters().AsNoTracking()
            .Select(x => new ServiceHierarchyItem
            {
                Id = x.Id, ParentServiceId = x.ParentServiceId,
                Scope = x.Scope, OwnerBranchId = x.OwnerBranchId,
                IsActive = x.IsActive, IsDeleted = x.IsDeleted,
                IsTicketIssuable = x.IsTicketIssuable, HasReservation = x.HasReservation,
                RangePrefix = x.RangePrefix, RangeStartNumber = x.RangeStartNumber,
                RangeEndNumber = x.RangeEndNumber
            })
            .ToListAsync(cancellationToken);
        var service = items.SingleOrDefault(x => x.Id == serviceId);
        if (service is null)
            return new(false, false, false, false, false, false, null, null, null);

        var branch = await _db.Set<Branch>().AsNoTracking()
            .Where(x => x.Id == branchId)
            .Select(x => new { x.IsActive, AllowedTime = x.Configuration == null
                ? TimeSpan.Zero : x.Configuration.AllowedTime })
            .SingleOrDefaultAsync(cancellationToken);
        var state = ServiceHierarchyCalculator.ComputeStates(items)[serviceId];
        var eligible = branch is { IsActive: true } &&
            state.EffectiveIsActive && !state.HasChildren &&
            (service.Scope == ServiceScope.Global ||
             service.Scope == ServiceScope.BranchScoped && service.OwnerBranchId == branchId);
        var assigned = await _db.Set<BranchService>().AsNoTracking()
            .AnyAsync(x => x.BranchId == branchId && x.ServiceId == serviceId, cancellationToken);
        var day = atUtc.DayOfWeek;
        var time = TimeOnly.FromDateTime(atUtc);
        var allowedUntil = ServiceScheduleAvailability.CalculateAllowedUntil(time,
            applyAllowedTime ? branch?.AllowedTime ?? TimeSpan.Zero : TimeSpan.Zero);
        var scheduled = await _db.Set<ServiceScheduleTimeSlot>().AsNoTracking()
            .AnyAsync(x => x.ServiceSchedule.BranchId == branchId &&
                           x.ServiceSchedule.ServiceId == serviceId &&
                           x.DayOfWeek == day && allowedUntil >= x.StartTime && time < x.EndTime,
                cancellationToken);
        return new(true, eligible, assigned, scheduled, service.IsTicketIssuable,
            service.HasReservation, service.RangePrefix, service.RangeStartNumber,
            service.RangeEndNumber);
    }
}
