using BuildingBlock.Domain.EntitiesHelper;
using QControl.Domain.Enums;

namespace QControl.Domain.Entities;

public sealed class TicketPrintConfiguration : AggregateRoot<int>
{
    private readonly List<TicketPrintElement> _elements = new();

    private TicketPrintConfiguration()
    {
    }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;
    public decimal TicketWidthMm { get; private set; }
    public decimal TicketHeightMm { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    public IReadOnlyCollection<TicketPrintElement> Elements => _elements.AsReadOnly();

    public static TicketPrintConfiguration Create(
        int branchId,
        decimal ticketWidthMm,
        decimal ticketHeightMm,
        IEnumerable<TicketPrintElementSettings> elements)
    {
        var configuration = new TicketPrintConfiguration
        {
            BranchId = branchId,
            TicketWidthMm = ticketWidthMm,
            TicketHeightMm = ticketHeightMm
        };

        foreach (var settings in elements)
        {
            configuration._elements.Add(TicketPrintElement.Create(settings));
        }

        return configuration;
    }

    public void Update(
        decimal ticketWidthMm,
        decimal ticketHeightMm,
        IReadOnlyCollection<TicketPrintElementSettings> elements)
    {
        TicketWidthMm = ticketWidthMm;
        TicketHeightMm = ticketHeightMm;

        var submittedByType = elements.ToDictionary(x => x.ElementType);
        foreach (var element in _elements)
        {
            if (submittedByType.TryGetValue(element.ElementType, out var settings))
            {
                element.Update(settings);
            }
        }

        foreach (var settings in elements.Where(settings =>
                     _elements.All(element => element.ElementType != settings.ElementType)))
        {
            _elements.Add(TicketPrintElement.Create(settings));
        }
    }
}

public sealed class TicketPrintElement : Entity<int>
{
    private TicketPrintElement()
    {
    }

    public int TicketPrintConfigurationId { get; private set; }
    public TicketPrintConfiguration TicketPrintConfiguration { get; private set; } = null!;
    public TicketPrintElementType ElementType { get; private set; }
    public bool IsVisible { get; private set; }
    public decimal? XMm { get; private set; }
    public decimal? YMm { get; private set; }
    public decimal? WidthMm { get; private set; }
    public decimal? HeightMm { get; private set; }
    public decimal? FontSizePt { get; private set; }
    public TicketFontWeight? FontWeight { get; private set; }
    public TicketTextAlign? TextAlign { get; private set; }
    public TicketPrintLanguage? Language { get; private set; }

    internal static TicketPrintElement Create(TicketPrintElementSettings settings)
    {
        var element = new TicketPrintElement { ElementType = settings.ElementType };
        element.Update(settings);
        return element;
    }

    internal void Update(TicketPrintElementSettings settings)
    {
        IsVisible = settings.IsVisible;
        XMm = settings.XMm;
        YMm = settings.YMm;
        WidthMm = settings.WidthMm;
        HeightMm = settings.HeightMm;
        FontSizePt = settings.FontSizePt;
        FontWeight = settings.FontWeight;
        TextAlign = settings.TextAlign;
        Language = settings.Language;
    }
}

public sealed record TicketPrintElementSettings(
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
