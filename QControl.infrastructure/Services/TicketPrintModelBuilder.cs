using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using QControl.Application.Abstraction.Services;
using QControl.Application.Features.TicketRuntime.Shared;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using QControl.infrastructure.Persistence;

namespace QControl.infrastructure.Services;

internal sealed class TicketPrintModelBuilder : ITicketPrintModelBuilder
{
    private readonly PlatformWriteDbContext _db;
    private readonly ITicketsAheadCalculator _ticketsAhead;
    private readonly ILogger<TicketPrintModelBuilder> _logger;

    public TicketPrintModelBuilder(
        PlatformWriteDbContext db,
        ITicketsAheadCalculator ticketsAhead,
        ILogger<TicketPrintModelBuilder> logger)
    {
        _db = db;
        _ticketsAhead = ticketsAhead;
        _logger = logger;
    }

    public async Task<TicketPrintModelResponse?> BuildAsync(
        int ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await _db.Set<TicketPrintConfiguration>()
                .AsNoTracking()
                .Include(x => x.Elements)
                .SingleOrDefaultAsync(x => x.BranchId == _db.Set<Ticket>()
                    .Where(ticket => ticket.Id == ticketId)
                    .Select(ticket => ticket.BranchId)
                    .Single(), cancellationToken);
            if (configuration is null)
            {
                return null;
            }

            var data = await _db.Set<Ticket>()
                .AsNoTracking()
                .Where(x => x.Id == ticketId)
                .Select(x => new TicketPrintData
                {
                    Id = x.Id,
                    BranchId = x.BranchId,
                    TicketNumber = x.TicketNumber,
                    LookupValue = x.LookupValue,
                    IssuingServiceId = x.IssuingServiceId,
                    BranchNameAr = x.Branch.ArabicName,
                    BranchNameEn = x.Branch.EnglishName,
                    LogoPath = x.Branch.Branding == null ? null : x.Branch.Branding.LogoPath,
                    ServiceNameAr = x.IssuingService.ArabicName,
                    ServiceNameEn = x.IssuingService.EnglishName,
                    WaitingDuration = x.IssuingService.WaitingDuration ?? 0,
                    SegmentNameAr = x.Segment.ArabicName,
                    SegmentNameEn = x.Segment.EnglishName
                })
                .SingleOrDefaultAsync(cancellationToken);
            if (data is null)
            {
                return null;
            }

            var customInputs = await _db.Set<TicketCustomInputValue>()
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderBy(x => x.OrderSnapshot == null)
                .ThenBy(x => x.OrderSnapshot)
                .ThenBy(x => x.Id)
                .Select(x => new CustomInputPrintData(
                    x.Id, x.NameSnapshot, x.LabelArSnapshot, x.LabelEnSnapshot,
                    x.Value, x.OrderSnapshot))
                .ToListAsync(cancellationToken);
            var hierarchy = await LoadServiceHierarchyAsync(
                data.IssuingServiceId, cancellationToken);
            var ticketsAhead = await _ticketsAhead.CalculateAsync(data.Id, data.BranchId,
                data.IssuingServiceId, cancellationToken);
            var elements = configuration.Elements
                .OrderBy(x => x.ElementType)
                .Select(element => BuildElement(element, data, hierarchy, customInputs,
                    ticketsAhead))
                .ToArray();
            return new TicketPrintModelResponse(configuration.TicketWidthMm,
                configuration.TicketHeightMm, elements);
        }
        catch (Exception exception)
        {
            // Print presentation must never invalidate an otherwise valid ticket issuance.
            _logger.LogWarning(exception,
                "Unable to build the print model for ticket {TicketId}; issuance will continue without Print.",
                ticketId);
            return null;
        }
    }

    private static TicketPrintRuntimeElementResponse BuildElement(
        TicketPrintElement element,
        TicketPrintData ticket,
        IReadOnlyList<ServicePathRow> hierarchy,
        IReadOnlyList<CustomInputPrintData> customInputs,
        int? ticketsAhead)
    {
        var isLogo = element.ElementType == TicketPrintElementType.BranchLogo;
        var text = element.IsVisible && !isLogo
            ? ResolveText(element.ElementType, element.Language!.Value, ticket, hierarchy,
                customInputs, ticketsAhead)
            : null;
        var imageUrl = element.IsVisible && isLogo ? ToMediaUrl(ticket.LogoPath) : null;

        return new TicketPrintRuntimeElementResponse(element.ElementType, element.IsVisible,
            element.XMm, element.YMm, element.WidthMm, element.HeightMm, element.FontSizePt,
            element.FontWeight, element.TextAlign, element.Language, text, imageUrl,
            isLogo ? null : TicketPrintOverflowBehavior.TrimWithEllipsis);
    }

    private static string ResolveText(
        TicketPrintElementType type,
        TicketPrintLanguage language,
        TicketPrintData ticket,
        IReadOnlyList<ServicePathRow> hierarchy,
        IReadOnlyList<CustomInputPrintData> customInputs,
        int? ticketsAhead)
    {
        return type switch
        {
            TicketPrintElementType.BranchName => TicketPrintContentFormatter.SelectLanguage(
                ticket.BranchNameAr, ticket.BranchNameEn, language),
            TicketPrintElementType.LeafServiceName => TicketPrintContentFormatter.SelectLanguage(
                ticket.ServiceNameAr, ticket.ServiceNameEn, language),
            TicketPrintElementType.ServiceFullTree =>
                TicketPrintContentFormatter.FormatServiceFullTree(
                    hierarchy.Select(x => new TicketPrintLocalizedValue(
                        x.ArabicName, x.EnglishName)), language),
            TicketPrintElementType.TicketNumber => TicketPrintContentFormatter.FormatTicketNumber(
                ticket.TicketNumber, language),
            TicketPrintElementType.SegmentName => TicketPrintContentFormatter.SelectLanguage(
                ticket.SegmentNameAr, ticket.SegmentNameEn, language),
            TicketPrintElementType.EstimatedWaitingDuration =>
                ticketsAhead.HasValue
                    ? TicketPrintContentFormatter.FormatEstimatedWaitingDuration(
                        ticketsAhead.Value, ticket.WaitingDuration, language)
                    : string.Empty,
            TicketPrintElementType.CustomInputsOrField =>
                TicketPrintContentFormatter.FormatCustomInputsOrField(
                    customInputs.Select(x => new TicketPrintCustomInputSnapshot(
                        x.Id, x.NameSnapshot, x.LabelEnSnapshot, x.LabelArSnapshot,
                        x.Value, x.OrderSnapshot)),
                    ticket.LookupValue, language),
            _ => string.Empty
        };
    }

    private async Task<IReadOnlyList<ServicePathRow>> LoadServiceHierarchyAsync(
        int serviceId,
        CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = _db.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = """
                WITH ServicePath AS
                (
                    SELECT [Id], [ParentServiceId], [ArabicName], [EnglishName], 0 AS [Depth]
                    FROM [Service]
                    WHERE [Id] = @serviceId
                    UNION ALL
                    SELECT parent.[Id], parent.[ParentServiceId], parent.[ArabicName],
                           parent.[EnglishName], child.[Depth] + 1
                    FROM [Service] parent
                    INNER JOIN ServicePath child ON child.[ParentServiceId] = parent.[Id]
                )
                SELECT [Id], [ArabicName], [EnglishName], [Depth]
                FROM ServicePath
                ORDER BY [Depth] DESC
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@serviceId";
            parameter.Value = serviceId;
            command.Parameters.Add(parameter);

            var result = new List<ServicePathRow>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new ServicePathRow(reader.GetInt32(0), reader.GetString(1),
                    reader.GetString(2), reader.GetInt32(3)));
            }

            return result;
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static string? ToMediaUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var normalized = path.Trim().Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("Media/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["Media/".Length..];
        }

        return $"/Media/{normalized}";
    }

    private sealed class TicketPrintData
    {
        public int Id { get; init; }
        public int BranchId { get; init; }
        public int IssuingServiceId { get; init; }
        public string TicketNumber { get; init; } = string.Empty;
        public string? LookupValue { get; init; }
        public string BranchNameAr { get; init; } = string.Empty;
        public string BranchNameEn { get; init; } = string.Empty;
        public string? LogoPath { get; init; }
        public string ServiceNameAr { get; init; } = string.Empty;
        public string ServiceNameEn { get; init; } = string.Empty;
        public int WaitingDuration { get; init; }
        public string SegmentNameAr { get; init; } = string.Empty;
        public string SegmentNameEn { get; init; } = string.Empty;
    }

    private sealed record ServicePathRow(
        int Id, string ArabicName, string EnglishName, int Depth);

    private sealed record CustomInputPrintData(
        int Id,
        string NameSnapshot,
        string? LabelArSnapshot,
        string? LabelEnSnapshot,
        string Value,
        int? OrderSnapshot);
}

/// <summary>
/// QControl's current call API records an attempt for an explicitly supplied ticket id and has
/// no queue-selection/ranking operation. Consequently TicketsAhead is unavailable. Returning
/// null leaves the configured slot empty without inventing priority, segment, reservation, or
/// timestamp rules.
/// </summary>
internal sealed class ExplicitTicketSelectionAheadCalculator : ITicketsAheadCalculator
{
    public Task<int?> CalculateAsync(
        int ticketId,
        int branchId,
        int issuingServiceId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<int?>(null);
    }
}
