using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Query.GetDeletedDisplays;

internal sealed class GetDeletedDisplaysSpec
    : Specification<Display, DeletedDisplayListItemResponse>
{
    public GetDeletedDisplaysSpec(GetDeletedDisplaysQuery query)
    {
        IgnoreGlobalFilters();
        AddCriteria(x => x.IsDeleted);

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

        Select(x => new DeletedDisplayListItemResponse
        {
            Id = x.Id,
            BranchId = x.BranchId,
            BranchArabicName = x.Branch.ArabicName,
            BranchEnglishName = x.Branch.EnglishName,
            Number = x.Number,
            IPAddress = x.IPAddress,
            SerialNo = x.SerialNo,
            Type = x.Type,
            DeletedOnUtc = x.DeletedOnUtc,
            RestoredOnUtc = x.RestoredOnUtc
        });
    }
}
