namespace QControl.Application.Abstraction.Services;

public interface IServiceTicketUsageChecker
{
    Task<bool> HasHistoricalTicketsAsync(
        int serviceId,
        CancellationToken cancellationToken);
}
