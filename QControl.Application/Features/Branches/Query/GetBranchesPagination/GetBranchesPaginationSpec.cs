using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.Branches.Shared;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchesPagination;

internal sealed class GetBranchesPaginationSpec
    : Specification<Branch, BranchPaginationItemResponse>
{
    public GetBranchesPaginationSpec(
        GetBranchesPaginationQuery searchParameters)
    {
        if (!string.IsNullOrWhiteSpace(searchParameters.SearchText))
        {
            var searchText = searchParameters.SearchText.Trim();

            AddCriteria(x =>
                (x.ArabicName != null && x.ArabicName.Contains(searchText)) ||
                (x.EnglishName != null && x.EnglishName.Contains(searchText)) ||
                x.IPAddress.Contains(searchText) ||
                (x.Location.Governorate != null &&
                    x.Location.Governorate.Contains(searchText)) ||
                (x.Location.City != null &&
                    x.Location.City.Contains(searchText)) ||
                (x.Location.Area != null &&
                    x.Location.Area.Contains(searchText)) ||
                (x.Location.Address != null &&
                    x.Location.Address.Contains(searchText)));
        }

        if (searchParameters.OrderSort == OrderSort.Oldest)
        {
            AddOrderBy(x => x.CreatedOnUtc);
            AddOrderBy(x => x.Id);
        }
        else
        {
            AddOrderByDescending(x => x.CreatedOnUtc);
            AddOrderByDescending(x => x.Id);
        }

        EnableTotalCount();

        ApplyPaging(
            searchParameters.PageNumber,
            searchParameters.PageSize);

        UseNoTracking();

        Select(x => new BranchPaginationItemResponse
        {
            BranchId = x.Id,
            ArabicName = x.ArabicName,
            EnglishName = x.EnglishName,
            IPAddress = x.IPAddress,
            IsUpdatesAvailable = x.IsUpdatesAvailable,
            LastUpdated = x.LastUpdated,
            Governorate = x.Location.Governorate,
            City = x.Location.City,
            CreatedBy = new BranchAuditUserResponse
            {
                ApplicationUserId = x.CreatedByApplicationUser.Id,
                UserName = x.CreatedByApplicationUser.UserName,
                NameEn = x.CreatedByApplicationUser.NameEn,
                NameAr = x.CreatedByApplicationUser.NameAr
            },
            CreatedOnUtc = x.CreatedOnUtc,
            LastModifiedBy = x.LastModifiedByApplicationUser == null
                ? null
                : new BranchAuditUserResponse
                {
                    ApplicationUserId = x.LastModifiedByApplicationUser.Id,
                    UserName = x.LastModifiedByApplicationUser.UserName,
                    NameEn = x.LastModifiedByApplicationUser.NameEn,
                    NameAr = x.LastModifiedByApplicationUser.NameAr
                },
            ModifiedOnUtc = x.ModifiedOnUtc
        });
    }
}