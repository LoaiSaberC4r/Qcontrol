using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Query.GetDisplayAvailableWindows;

internal sealed class GetDisplayAvailableWindowsSpec
    : Specification<Window, AvailableWindowResponse>
{
    public GetDisplayAvailableWindowsSpec(
        GetDisplayAvailableWindowsQuery query,
        int branchId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.WaitingArea.BranchId == branchId &&
            !x.DisplayWindows.Any(link => link.DisplayId == query.DisplayId));

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

        Select(x => new AvailableWindowResponse
        {
            Id = x.Id,
            WaitingAreaId = x.WaitingAreaId,
            WaitingAreaNumber = x.WaitingArea.Number,
            WaitingAreaDescriptiveName =
                x.WaitingArea.DescriptiveName,
            Number = x.Number,
            DescriptiveName = x.DescriptiveName,
            IPAddress = x.IPAddress,
            EnableTicketBooking = x.EnableTicketBooking,
            EnableDirectCall = x.EnableDirectCall
        });
    }
}
