using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;

internal static class BranchDisplayConfigurationSettingsFactory
{
    public static BranchDisplayConfigurationSettings FromInput(
        IBranchDisplayConfigurationInput input) =>
        new(
            input.DisplayBackgroundColor,
            input.MainTitleAr,
            input.MainTitleEn,
            input.HeaderBackgroundColor,
            input.MainTitleTextColor,
            input.MainTitleFontSize,
            input.TableHeaderBackgroundColor,
            input.TableHeaderTextColor,
            input.TableRowBackgroundColor,
            input.TableRowTextColor,
            input.TicketNumberBackgroundColor,
            input.TicketNumberTextColor,
            input.TicketColumnTitleAr,
            input.TicketColumnTitleEn,
            input.ServiceColumnTitleAr,
            input.ServiceColumnTitleEn,
            input.WindowColumnTitleAr,
            input.WindowColumnTitleEn,
            input.TickerBackgroundColor,
            input.TickerTextColor,
            input.TickerFontSize,
            input.ShowClock,
            input.ClockBackgroundColor,
            input.ClockTextColor);
}
