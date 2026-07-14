using BuildingBlock.Domain.Specification;
using QControl.Application.Shared.Operational;
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

        AddCriteria(x => x.BranchId == branchId);

        AddCriteria(x =>
            !x.DisplayWindows.Any(link => link.DisplayId == query.DisplayId));

        if (query.IsActive.HasValue)
        {
            AddCriteria(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchText = query.Search.Trim();

            if (StatusSearchTerm.TryParse(searchText, out var status))
            {
                AddCriteria(x => x.IsActive == status);
            }
            else
            {
                AddCriteria(x =>
                    x.Number.Contains(searchText) ||
                    (x.DescriptiveName != null &&
                        x.DescriptiveName.Contains(searchText)) ||
                    (x.IPAddress != null &&
                        x.IPAddress.Contains(searchText)));
            }
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
            EnableDirectCall = x.EnableDirectCall,
            IsActive = x.IsActive,
            EffectiveIsActive =
                x.WaitingArea.Branch.IsActive &&
                x.WaitingArea.IsActive &&
                x.IsActive
        });
    }
}
