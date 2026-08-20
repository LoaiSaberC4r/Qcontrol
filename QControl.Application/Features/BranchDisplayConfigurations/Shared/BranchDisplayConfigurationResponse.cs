namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

public sealed class BranchDisplayConfigurationResponse
{
    public int Id { get; init; }
    public int BranchId { get; init; }
    public string DisplayBackgroundColor { get; init; } = string.Empty;
    public string MainTitleAr { get; init; } = string.Empty;
    public string MainTitleEn { get; init; } = string.Empty;
    public string HeaderBackgroundColor { get; init; } = string.Empty;
    public string MainTitleTextColor { get; init; } = string.Empty;
    public int MainTitleFontSize { get; init; }
    public string TableHeaderBackgroundColor { get; init; } = string.Empty;
    public string TableHeaderTextColor { get; init; } = string.Empty;
    public string TableRowBackgroundColor { get; init; } = string.Empty;
    public string TableRowTextColor { get; init; } = string.Empty;
    public string TicketNumberBackgroundColor { get; init; } = string.Empty;
    public string TicketNumberTextColor { get; init; } = string.Empty;
    public string TicketColumnTitleAr { get; init; } = string.Empty;
    public string TicketColumnTitleEn { get; init; } = string.Empty;
    public string ServiceColumnTitleAr { get; init; } = string.Empty;
    public string ServiceColumnTitleEn { get; init; } = string.Empty;
    public string WindowColumnTitleAr { get; init; } = string.Empty;
    public string WindowColumnTitleEn { get; init; } = string.Empty;
    public string TickerBackgroundColor { get; init; } = string.Empty;
    public string TickerTextColor { get; init; } = string.Empty;
    public int TickerFontSize { get; init; }
    public bool ShowClock { get; init; }
    public string ClockBackgroundColor { get; init; } = string.Empty;
    public string ClockTextColor { get; init; } = string.Empty;
    public string RowVersion { get; init; } = string.Empty;
}
