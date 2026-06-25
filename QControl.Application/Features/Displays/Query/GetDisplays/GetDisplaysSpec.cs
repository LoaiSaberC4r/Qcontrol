using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Query.GetDisplays;

internal sealed class GetDisplaysSpec
    : Specification<Display, DisplayListItemResponse>
{
    public GetDisplaysSpec(GetDisplaysQuery query)
    {
        if (query.BranchId.HasValue)
        {
            var branchId = query.BranchId.Value;
            AddCriteria(x => x.BranchId == branchId);
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

        AddOrderBy(x => x.BranchId);
        AddOrderBy(x => x.Number);
        AddOrderBy(x => x.Id);

        EnableTotalCount();

        ApplyPaging(
            query.PageNumber,
            query.PageSize);

        UseNoTracking();

        Select(x => new DisplayListItemResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            BranchArabicName = x.Branch.ArabicName,
            BranchEnglishName = x.Branch.EnglishName,
            Number = x.Number,
            IPAddress = x.IPAddress,
            SerialNo = x.SerialNo,
            Type = x.Type
        });
    }
}
