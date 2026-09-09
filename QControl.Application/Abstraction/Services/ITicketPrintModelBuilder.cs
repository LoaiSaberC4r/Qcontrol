using QControl.Application.Features.TicketRuntime.Shared;

namespace QControl.Application.Abstraction.Services;

public interface ITicketPrintModelBuilder
{
    Task<TicketPrintModelResponse?> BuildAsync(
        int ticketId,
        CancellationToken cancellationToken);
}

public interface ITicketsAheadCalculator
{
    Task<int?> CalculateAsync(
        int ticketId,
        int branchId,
        int issuingServiceId,
        CancellationToken cancellationToken);
}
