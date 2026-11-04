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
        DateTime atUtc, CancellationToken cancellationToken, bool applyAllowedTime = false,
        bool requireKioskEligibility = false)
    {
        var services = _db.Set<Service>().AsNoTracking();
        if (requireKioskEligibility)
        {
            // Discovery counts soft-deleted children when deciding whether a service is a leaf.
            services = services.IgnoreQueryFilters();
        }

        var service = await services
            .Where(x => x.Id == serviceId)
            .Select(x => new
            {
                x.ParentServiceId, x.Scope, x.OwnerBranchId, x.IsActive, x.IsDeleted,
                x.IsTicketIssuable, x.HasReservation, x.RangePrefix, x.RangeStartNumber,
                x.RangeEndNumber,
                HasChildren = requireKioskEligibility && services.Any(child => child.ParentServiceId == x.Id)
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (service is null)
            return new(false, false, false, false, false, false, null, null, null);

        var eligible = service.IsActive;
        var allowedTime = TimeSpan.Zero;
        if (requireKioskEligibility || applyAllowedTime)
        {
            var branch = await _db.Set<Branch>().AsNoTracking()
                .Where(x => x.Id == branchId)
                .Select(x => new { x.IsActive, AllowedTime = x.Configuration == null
                    ? TimeSpan.Zero : x.Configuration.AllowedTime })
                .SingleOrDefaultAsync(cancellationToken);
            if (requireKioskEligibility)
            {
                eligible = eligible && branch is { IsActive: true } &&
                    !service.IsDeleted && !service.HasChildren &&
                    (service.Scope == ServiceScope.Global ||
                     service.Scope == ServiceScope.BranchScoped && service.OwnerBranchId == branchId) &&
                    (service.ParentServiceId is not int parentId ||
                     await AreAncestorsActiveAsync(parentId, cancellationToken));
            }

            if (applyAllowedTime)
                allowedTime = branch?.AllowedTime ?? TimeSpan.Zero;
        }

        var assigned = await _db.Set<BranchService>().AsNoTracking()
            .AnyAsync(x => x.BranchId == branchId && x.ServiceId == serviceId, cancellationToken);
        var day = atUtc.DayOfWeek;
        var time = TimeOnly.FromDateTime(atUtc);
        var allowedUntil = ServiceScheduleAvailability.CalculateAllowedUntil(time, allowedTime);
        var scheduled = await _db.Set<ServiceScheduleTimeSlot>().AsNoTracking()
            .AnyAsync(x => x.ServiceSchedule.BranchId == branchId &&
                           x.ServiceSchedule.ServiceId == serviceId &&
                           x.DayOfWeek == day && allowedUntil >= x.StartTime && time < x.EndTime,
                cancellationToken);
        return new(true, eligible, assigned, scheduled, service.IsTicketIssuable,
            service.HasReservation, service.RangePrefix, service.RangeStartNumber,
            service.RangeEndNumber);
    }

    private async Task<bool> AreAncestorsActiveAsync(int parentId, CancellationToken cancellationToken)
    {
        if (_db.Database.IsSqlServer())
        {
            // A missing parent ends the chain. A repeated ID or an inactive/deleted
            // ancestor invalidates it, matching the discovery hierarchy rules.
            var result = await _db.Database.SqlQuery<bool>($"""
                WITH [Ancestors] AS
                (
                    SELECT [ParentServiceId], [IsActive], [IsDeleted],
                        CAST(CONCAT('/', [Id], '/') AS varchar(max)) AS [VisitedIds],
                        CAST(0 AS bit) AS [IsCycle]
                    FROM [Service]
                    WHERE [Id] = {parentId}

                    UNION ALL

                    SELECT [parent].[ParentServiceId], [parent].[IsActive], [parent].[IsDeleted],
                        CAST(CONCAT([ancestor].[VisitedIds], [parent].[Id], '/') AS varchar(max)),
                        CAST(CASE WHEN CHARINDEX(CONCAT('/', [parent].[Id], '/'),
                            [ancestor].[VisitedIds]) > 0 THEN 1 ELSE 0 END AS bit)
                    FROM [Ancestors] AS [ancestor]
                    INNER JOIN [Service] AS [parent] ON [parent].[Id] = [ancestor].[ParentServiceId]
                    WHERE [ancestor].[IsCycle] = 0 AND [ancestor].[IsActive] = 1 AND [ancestor].[IsDeleted] = 0
                )
                SELECT CAST(CASE WHEN EXISTS
                (
                    SELECT 1 FROM [Ancestors]
                    WHERE [IsCycle] = 1 OR [IsActive] = 0 OR [IsDeleted] = 1
                ) THEN 0 ELSE 1 END AS bit) AS [Value]
                OPTION (MAXRECURSION 0)
                """).ToListAsync(cancellationToken);
            return result.Single();
        }

        // The nonrelational test provider has no SQL commands or database round trips.
        var visited = new HashSet<int>();
        int? nextId = parentId;
        while (nextId is int id)
        {
            if (!visited.Add(id))
                return false;

            var ancestor = await _db.Set<Service>().IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.ParentServiceId, x.IsActive, x.IsDeleted })
                .SingleOrDefaultAsync(cancellationToken);
            if (ancestor is null)
                return true;
            if (!ancestor.IsActive || ancestor.IsDeleted)
                return false;

            nextId = ancestor.ParentServiceId;
        }

        return true;
    }
}
