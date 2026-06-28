using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayLinkedWindows;

internal sealed class GetDisplayLinkedWindowsSpec
    : Specification<DisplayWindow, DisplayLinkedWindowResponse>
{
    public GetDisplayLinkedWindowsSpec(GetDisplayLinkedWindowsQuery query)
    {
        IgnoreGlobalFilters();
        UseNoTracking();

        AddCriteria(x => x.DisplayId == query.DisplayId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchText = query.Search.Trim();

            AddCriteria(x =>
                x.Window.Number.Contains(searchText) ||
                (x.Window.DescriptiveName != null &&
                    x.Window.DescriptiveName.Contains(searchText)) ||
                (x.Window.IPAddress != null &&
                    x.Window.IPAddress.Contains(searchText)));
        }

        AddOrderBy(x => x.Window.WaitingAreaId);
        AddOrderBy(x => x.Window.Number);
        AddOrderBy(x => x.Window.Id);

        EnableTotalCount();

        ApplyPaging(
            query.PageNumber,
            query.PageSize);

        Select(x => new DisplayLinkedWindowResponse
        {
            Id = x.Window.Id,
            WaitingAreaId = x.Window.WaitingAreaId,
            WaitingAreaNumber = x.Window.WaitingArea.Number,
            WaitingAreaDescriptiveName =
                x.Window.WaitingArea.DescriptiveName,
            Number = x.Window.Number,
            DescriptiveName = x.Window.DescriptiveName,
            IPAddress = x.Window.IPAddress,
            EnableTicketBooking = x.Window.EnableTicketBooking,
            EnableDirectCall = x.Window.EnableDirectCall,
            IsDeleted = x.Window.IsDeleted
        });
    }
}
