using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

internal static class BranchDisplayConfigurationResponseFactory
{
    public static BranchDisplayConfigurationResponse FromEntity(
        BranchDisplayConfiguration configuration) =>
        new()
        {
            Id = configuration.Id,
            BranchId = configuration.BranchId,
            DisplayBackgroundColor = configuration.DisplayBackgroundColor,
            MainTitleAr = configuration.MainTitleAr,
            MainTitleEn = configuration.MainTitleEn,
            HeaderBackgroundColor = configuration.HeaderBackgroundColor,
            MainTitleTextColor = configuration.MainTitleTextColor,
            MainTitleFontSize = configuration.MainTitleFontSize,
            TableHeaderBackgroundColor = configuration.TableHeaderBackgroundColor,
            TableHeaderTextColor = configuration.TableHeaderTextColor,
            TableRowBackgroundColor = configuration.TableRowBackgroundColor,
            TableRowTextColor = configuration.TableRowTextColor,
            TicketNumberBackgroundColor = configuration.TicketNumberBackgroundColor,
            TicketNumberTextColor = configuration.TicketNumberTextColor,
            TicketColumnTitleAr = configuration.TicketColumnTitleAr,
            TicketColumnTitleEn = configuration.TicketColumnTitleEn,
            ServiceColumnTitleAr = configuration.ServiceColumnTitleAr,
            ServiceColumnTitleEn = configuration.ServiceColumnTitleEn,
            WindowColumnTitleAr = configuration.WindowColumnTitleAr,
            WindowColumnTitleEn = configuration.WindowColumnTitleEn,
            TickerBackgroundColor = configuration.TickerBackgroundColor,
            TickerTextColor = configuration.TickerTextColor,
            TickerFontSize = configuration.TickerFontSize,
            ShowClock = configuration.ShowClock,
            ClockBackgroundColor = configuration.ClockBackgroundColor,
            ClockTextColor = configuration.ClockTextColor,
            RowVersion = RowVersionConverter.ToBase64(configuration.RowVersion)
        };
}
