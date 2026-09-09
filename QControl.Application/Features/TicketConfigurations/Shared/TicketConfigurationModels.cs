using QControl.Domain.Enums;

namespace QControl.Application.Features.TicketConfigurations.Shared;

public sealed record TicketPrintElementInput
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

public abstract record TicketConfigurationCommandBase
{
    public int BranchId { get; init; }
    public decimal TicketWidthMm { get; init; }
    public decimal TicketHeightMm { get; init; }
    public IReadOnlyCollection<TicketPrintElementInput> Elements { get; init; }
        = Array.Empty<TicketPrintElementInput>();
}

public sealed record TicketConfigurationResponse
{
    public int Id { get; init; }
    public int BranchId { get; init; }
    public decimal TicketWidthMm { get; init; }
    public decimal TicketHeightMm { get; init; }
    public IReadOnlyList<TicketPrintElementResponse> Elements { get; init; }
        = Array.Empty<TicketPrintElementResponse>();
    public string RowVersion { get; init; } = string.Empty;
}

public sealed record TicketPrintElementResponse(
    TicketPrintElementType ElementType,
    bool IsVisible,
    decimal? XMm,
    decimal? YMm,
    decimal? WidthMm,
    decimal? HeightMm,
    decimal? FontSizePt,
    TicketFontWeight? FontWeight,
    TicketTextAlign? TextAlign,
    TicketPrintLanguage? Language);
