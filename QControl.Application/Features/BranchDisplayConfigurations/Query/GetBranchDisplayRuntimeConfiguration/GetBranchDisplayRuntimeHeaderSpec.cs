using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayRuntimeConfiguration;

internal sealed class GetBranchDisplayRuntimeHeaderSpec
    : Specification<Branch, BranchDisplayRuntimeConfigurationResponse>
{
    public GetBranchDisplayRuntimeHeaderSpec(int branchId)
    {
        AddCriteria(x => x.Id == branchId);
        UseNoTracking();
        Select(x => new BranchDisplayRuntimeConfigurationResponse
        {
            BranchId = x.Id,
            Branding = new BranchDisplayRuntimeBrandingResponse
            {
                LogoUrl = x.Branding == null || x.Branding.LogoPath == null
                    ? null
                    : "/Media/" + x.Branding.LogoPath
            },
            DisplayConfiguration = x.DisplayConfiguration == null
                ? null
                : new BranchDisplayRuntimeVisualConfigurationResponse
                {
                    DisplayBackgroundColor = x.DisplayConfiguration.DisplayBackgroundColor,
                    MainTitleAr = x.DisplayConfiguration.MainTitleAr,
                    MainTitleEn = x.DisplayConfiguration.MainTitleEn,
                    HeaderBackgroundColor = x.DisplayConfiguration.HeaderBackgroundColor,
                    MainTitleTextColor = x.DisplayConfiguration.MainTitleTextColor,
                    MainTitleFontSize = x.DisplayConfiguration.MainTitleFontSize,
                    TableHeaderBackgroundColor = x.DisplayConfiguration.TableHeaderBackgroundColor,
                    TableHeaderTextColor = x.DisplayConfiguration.TableHeaderTextColor,
                    TableRowBackgroundColor = x.DisplayConfiguration.TableRowBackgroundColor,
                    TableRowTextColor = x.DisplayConfiguration.TableRowTextColor,
                    TicketNumberBackgroundColor = x.DisplayConfiguration.TicketNumberBackgroundColor,
                    TicketNumberTextColor = x.DisplayConfiguration.TicketNumberTextColor,
                    TicketColumnTitleAr = x.DisplayConfiguration.TicketColumnTitleAr,
                    TicketColumnTitleEn = x.DisplayConfiguration.TicketColumnTitleEn,
                    ServiceColumnTitleAr = x.DisplayConfiguration.ServiceColumnTitleAr,
                    ServiceColumnTitleEn = x.DisplayConfiguration.ServiceColumnTitleEn,
                    WindowColumnTitleAr = x.DisplayConfiguration.WindowColumnTitleAr,
                    WindowColumnTitleEn = x.DisplayConfiguration.WindowColumnTitleEn,
                    TickerBackgroundColor = x.DisplayConfiguration.TickerBackgroundColor,
                    TickerTextColor = x.DisplayConfiguration.TickerTextColor,
                    TickerFontSize = x.DisplayConfiguration.TickerFontSize,
                    ShowClock = x.DisplayConfiguration.ShowClock,
                    ClockBackgroundColor = x.DisplayConfiguration.ClockBackgroundColor,
                    ClockTextColor = x.DisplayConfiguration.ClockTextColor
                }
        });
    }
}
