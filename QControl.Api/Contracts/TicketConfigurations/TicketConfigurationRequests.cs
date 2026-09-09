using QControl.Domain.Enums;

namespace QControl.Api.Contracts.TicketConfigurations;

public class TicketConfigurationRequest
{
    public decimal TicketWidthMm { get; init; }
    public decimal TicketHeightMm { get; init; }
    public IReadOnlyCollection<TicketPrintElementRequest> Elements { get; init; }
        = Array.Empty<TicketPrintElementRequest>();
}

public sealed class UpdateTicketConfigurationRequest : TicketConfigurationRequest
{
    public string RowVersion { get; init; } = string.Empty;
}

public sealed record TicketPrintElementRequest
{
    public TicketPrintElementType ElementType { get; init; }
    public bool IsVisible { get; init; }
    public decimal? XMm { get; init; }
    public decimal? YMm { get; init; }
    public decimal? WidthMm { get; init; }
    public decimal? HeightMm { get; init; }
    public decimal? FontSizePt { get; init; }
    public TicketFontWeight? FontWeight { get; init; }
    public TicketTextAlign? TextAlign { get; init; }
    public TicketPrintLanguage? Language { get; init; }
}
