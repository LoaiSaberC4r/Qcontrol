using QControl.Application.Abstraction.Services;

namespace QControl.infrastructure.Services;

internal sealed class ServiceTicketUsageChecker : IServiceTicketUsageChecker
{
    public Task<bool> HasHistoricalTicketsAsync(
        int serviceId,
        CancellationToken cancellationToken)
    {
        // Ticket management has not been implemented in this solution yet.
        // Replace this with a Ticket repository-backed check when the Ticket
        // module is added.
        return Task.FromResult(false);
    }
}
