using BuildingBlock.Domain.Specification;
using Qcontrol.Application.Features.Branches.Shared;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Branches.Query.GetBranchDetails;

internal sealed class GetBranchDetailsSpec
    : Specification<Branch, GetBranchDetailsResponse>
{
    public GetBranchDetailsSpec(int branchId)
    {
        AddCriteria(x => x.Id == branchId);
        UseNoTracking();

        Select(x => new GetBranchDetailsResponse
        {
            BranchId = x.Id,
            ArabicName = x.ArabicName,
            EnglishName = x.EnglishName,
            IPAddress = x.IPAddress,
            IsUpdatesAvailable = x.IsUpdatesAvailable,
            LastUpdated = x.LastUpdated,
            License = x.License,
            Location = new BranchLocationResponse
            {
                Id = x.Location.Id,
                Governorate = x.Location.Governorate,
                City = x.Location.City,
                Area = x.Location.Area,
                Address = x.Location.Address,
                Longitude = x.Location.Longitude,
                Latitude = x.Location.Latitude
            },
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