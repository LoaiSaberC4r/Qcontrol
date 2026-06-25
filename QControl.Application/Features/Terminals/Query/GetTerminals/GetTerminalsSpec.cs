using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Query.GetTerminals;

internal sealed class GetTerminalsSpec
    : Specification<Terminal, TerminalListItemResponse>
{
    public GetTerminalsSpec(GetTerminalsQuery query)
    {
        if (query.WindowId.HasValue)
        {
            var windowId = query.WindowId.Value;
            AddCriteria(x => x.WindowId == windowId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchText = query.Search.Trim();

            AddCriteria(x =>
                x.Number.Contains(searchText) ||
                x.IPAddress.Contains(searchText) ||
                x.SerialNo.Contains(searchText) ||
                x.Type.Contains(searchText));
        }

        AddOrderBy(x => x.WindowId);
        AddOrderBy(x => x.Number);
        AddOrderBy(x => x.Id);

        EnableTotalCount();

        ApplyPaging(
            query.PageNumber,
            query.PageSize);

        UseNoTracking();

        Select(x => new TerminalListItemResponse
        {
            Id = x.Id,
            WindowId = x.WindowId,
            WindowNumber = x.Window.Number,
            WaitingAreaId = x.Window.WaitingAreaId,
            WaitingAreaNumber = x.Window.WaitingArea.Number,
            BranchId = x.Window.WaitingArea.BranchId,
            BranchArabicName = x.Window.WaitingArea.Branch.ArabicName,
            BranchEnglishName = x.Window.WaitingArea.Branch.EnglishName,
            Number = x.Number,
            IPAddress = x.IPAddress,
            SerialNo = x.SerialNo,
            Type = x.Type
        });
    }
}
