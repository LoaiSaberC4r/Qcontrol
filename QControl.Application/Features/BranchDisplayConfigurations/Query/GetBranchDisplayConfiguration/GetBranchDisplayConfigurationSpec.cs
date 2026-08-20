using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.BranchDisplayConfigurations.Shared;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchDisplayConfigurations.Query.GetBranchDisplayConfiguration;

internal sealed class GetBranchDisplayConfigurationSpec
    : Specification<BranchDisplayConfiguration, BranchDisplayConfigurationResponse>
{
    public GetBranchDisplayConfigurationSpec(int branchId)
    {
        AddCriteria(x => x.BranchId == branchId);
        UseNoTracking();
        Select(x => new BranchDisplayConfigurationResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            DisplayBackgroundColor = x.DisplayBackgroundColor,
            MainTitleAr = x.MainTitleAr,
            MainTitleEn = x.MainTitleEn,
            HeaderBackgroundColor = x.HeaderBackgroundColor,
            MainTitleTextColor = x.MainTitleTextColor,
            MainTitleFontSize = x.MainTitleFontSize,
            TableHeaderBackgroundColor = x.TableHeaderBackgroundColor,
            TableHeaderTextColor = x.TableHeaderTextColor,
            TableRowBackgroundColor = x.TableRowBackgroundColor,
            TableRowTextColor = x.TableRowTextColor,
            TicketNumberBackgroundColor = x.TicketNumberBackgroundColor,
            TicketNumberTextColor = x.TicketNumberTextColor,
            TicketColumnTitleAr = x.TicketColumnTitleAr,
            TicketColumnTitleEn = x.TicketColumnTitleEn,
            ServiceColumnTitleAr = x.ServiceColumnTitleAr,
            ServiceColumnTitleEn = x.ServiceColumnTitleEn,
            WindowColumnTitleAr = x.WindowColumnTitleAr,
            WindowColumnTitleEn = x.WindowColumnTitleEn,
            TickerBackgroundColor = x.TickerBackgroundColor,
            TickerTextColor = x.TickerTextColor,
            TickerFontSize = x.TickerFontSize,
            ShowClock = x.ShowClock,
            ClockBackgroundColor = x.ClockBackgroundColor,
            ClockTextColor = x.ClockTextColor,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        });
    }
}
