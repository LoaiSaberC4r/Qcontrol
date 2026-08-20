namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

public interface IBranchDisplayConfigurationInput
{
    string DisplayBackgroundColor { get; }
    string MainTitleAr { get; }
    string MainTitleEn { get; }
    string HeaderBackgroundColor { get; }
    string MainTitleTextColor { get; }
    int MainTitleFontSize { get; }
    string TableHeaderBackgroundColor { get; }
    string TableHeaderTextColor { get; }
    string TableRowBackgroundColor { get; }
    string TableRowTextColor { get; }
    string TicketNumberBackgroundColor { get; }
    string TicketNumberTextColor { get; }
    string TicketColumnTitleAr { get; }
    string TicketColumnTitleEn { get; }
    string ServiceColumnTitleAr { get; }
    string ServiceColumnTitleEn { get; }
    string WindowColumnTitleAr { get; }
    string WindowColumnTitleEn { get; }
    string TickerBackgroundColor { get; }
    string TickerTextColor { get; }
    int TickerFontSize { get; }
    bool ShowClock { get; }
    string ClockBackgroundColor { get; }
    string ClockTextColor { get; }
}
