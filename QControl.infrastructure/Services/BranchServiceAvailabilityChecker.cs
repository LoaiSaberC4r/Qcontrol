using Microsoft.EntityFrameworkCore;
using QControl.Application.Abstraction.Services;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Services;

internal sealed class BranchServiceAvailabilityChecker : IBranchServiceAvailabilityChecker
{
    private readonly PlatformWriteDbContext _db;
    public BranchServiceAvailabilityChecker(PlatformWriteDbContext db) => _db = db;

    public async Task<BranchServiceAvailability> CheckAsync(int branchId, int serviceId,
        DateTime atUtc, CancellationToken cancellationToken)
    {
        var service = await _db.Set<Service>().AsNoTracking()
            .Where(x => x.Id == serviceId)
            .Select(x => new { x.IsActive, x.IsTicketIssuable, x.HasReservation,
                x.RangePrefix, x.RangeStartNumber, x.RangeEndNumber })
            .SingleOrDefaultAsync(cancellationToken);
        if (service is null)
            return new(false, false, false, false, false, false, null, null, null);

        var assigned = await _db.Set<BranchService>().AsNoTracking()
            .AnyAsync(x => x.BranchId == branchId && x.ServiceId == serviceId, cancellationToken);
        var day = atUtc.DayOfWeek;
        var time = TimeOnly.FromDateTime(atUtc);
        var scheduled = await _db.Set<ServiceScheduleTimeSlot>().AsNoTracking()
            .AnyAsync(x => x.ServiceSchedule.BranchId == branchId &&
                           x.ServiceSchedule.ServiceId == serviceId &&
                           x.DayOfWeek == day && time >= x.StartTime && time < x.EndTime,
                cancellationToken);
        return new(true, service.IsActive, assigned, scheduled, service.IsTicketIssuable,
            service.HasReservation, service.RangePrefix, service.RangeStartNumber,
            service.RangeEndNumber);
    }
}
