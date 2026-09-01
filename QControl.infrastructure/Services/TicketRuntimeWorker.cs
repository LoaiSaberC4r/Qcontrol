using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QControl.Application.Abstraction.Services;

namespace QControl.infrastructure.Services;

internal sealed class TicketRuntimeWorker : BackgroundService
{
    private const int BatchSize = 100;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketRuntimeWorker> _logger;
    public TicketRuntimeWorker(IServiceScopeFactory scopeFactory,
        ILogger<TicketRuntimeWorker> logger)
    { _scopeFactory = scopeFactory; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var runtime = scope.ServiceProvider.GetRequiredService<ITicketRuntimeRepository>();
                await runtime.AutoCancelNoShowTicketsAsync(BatchSize, stoppingToken);
                await runtime.ProcessReservationEndOfDayAsync(BatchSize, stoppingToken);
                await runtime.ArchiveEligibleTicketsAsync(BatchSize, stoppingToken);
                await runtime.ArchiveEligibleReservationsAsync(BatchSize, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QControl ticket/reservation lifecycle worker failed; it will retry.");
            }

            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        }
    }
}
