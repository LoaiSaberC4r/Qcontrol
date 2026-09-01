using QControl.Application.Abstraction.Services;
using Microsoft.EntityFrameworkCore;
using QControl.Domain.Entities;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Services;

internal sealed class ServiceTicketUsageChecker : IServiceTicketUsageChecker
{
    private readonly PlatformWriteDbContext _db;

    public ServiceTicketUsageChecker(PlatformWriteDbContext db) => _db = db;

    public async Task<bool> HasHistoricalTicketsAsync(
        int serviceId,
        CancellationToken cancellationToken)
    {
        if (await _db.Set<Ticket>().IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(x => x.IssuingServiceId == serviceId ||
                           x.CurrentServiceId == serviceId ||
                           x.ServiceJourneys.Any(j => j.ServiceId == serviceId), cancellationToken))
            return true;

        return await _db.Set<TicketArchive>().AsNoTracking()
            .AnyAsync(x => x.IssuingServiceId == serviceId ||
                           x.CurrentServiceId == serviceId ||
                           x.Journeys.Any(j => j.ServiceId == serviceId) ||
                           x.History.Any(h => h.ServiceId == serviceId ||
                                              h.FromServiceId == serviceId ||
                                              h.ToServiceId == serviceId), cancellationToken);
    }
}
