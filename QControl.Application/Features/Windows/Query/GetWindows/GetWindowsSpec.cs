using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Query.GetWindows;

internal sealed class GetWindowsSpec
    : Specification<Window, WindowListItemResponse>
{
    public GetWindowsSpec(GetWindowsQuery query)
    {
        if (query.WaitingAreaId.HasValue)
        {
            var waitingAreaId = query.WaitingAreaId.Value;
            AddCriteria(x => x.WaitingAreaId == waitingAreaId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchText = query.Search.Trim();

            AddCriteria(x =>
                x.Number.Contains(searchText) ||
                (x.DescriptiveName != null &&
                    x.DescriptiveName.Contains(searchText)) ||
                (x.IPAddress != null &&
                    x.IPAddress.Contains(searchText)));
        }

        AddOrderBy(x => x.WaitingAreaId);
        AddOrderBy(x => x.Number);
        AddOrderBy(x => x.Id);

        EnableTotalCount();

        ApplyPaging(
            query.PageNumber,
            query.PageSize);

        UseNoTracking();

        Select(x => new WindowListItemResponse
        {
            Id = x.Id,
            WaitingAreaId = x.WaitingAreaId,
            WaitingAreaNumber = x.WaitingArea.Number,
            WaitingAreaDescriptiveName = x.WaitingArea.DescriptiveName,
            Number = x.Number,
            DescriptiveName = x.DescriptiveName,
            IPAddress = x.IPAddress,
            EnableTicketBooking = x.EnableTicketBooking,
            EnableDirectCall = x.EnableDirectCall
        });
    }
}
