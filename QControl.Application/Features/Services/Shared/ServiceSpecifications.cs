using System.Linq.Expressions;
using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceProjection
{
    public static Expression<Func<Service, ServiceHierarchyItem>> ToHierarchyItem =>
        x => new ServiceHierarchyItem
        {
            Id = x.Id,
            ParentServiceId = x.ParentServiceId,
            Scope = x.Scope,
            OwnerBranchId = x.OwnerBranchId,
            OwnerBranchArabicName = x.OwnerBranch == null
                ? null
                : x.OwnerBranch.ArabicName,
            OwnerBranchEnglishName = x.OwnerBranch == null
                ? null
                : x.OwnerBranch.EnglishName,
            ArabicName = x.ArabicName,
            EnglishName = x.EnglishName,
            ServiceCode = x.ServiceCode,
            IsServiceCodeRequired = x.IsServiceCodeRequired,
            ArabicUserMessage = x.ArabicUserMessage,
            EnglishUserMessage = x.EnglishUserMessage,
            IsActive = x.IsActive,
            IsTicketIssuable = x.IsTicketIssuable,
            IsClientInputRequired = x.IsClientInputRequired,
            HasReservation = x.HasReservation,
            OrderNo = x.OrderNo,
            Priority = x.Priority,
            RangePrefix = x.RangePrefix,
            RangeStartNumber = x.RangeStartNumber,
            RangeEndNumber = x.RangeEndNumber,
            WaitingDuration = x.WaitingDuration,
            NoOfTicketCopies = x.NoOfTicketCopies,
            RowVersion = x.RowVersion,
            CreatedByApplicationUserId = x.CreatedByApplicationUserId,
            LastModifiedByApplicationUserId =
                x.LastModifiedByApplicationUserId,
            IsDeleted = x.IsDeleted,
            DeletedOnUtc = x.DeletedOnUtc,
            RestoredOnUtc = x.RestoredOnUtc,
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        };
}

internal sealed class GetAllServiceHierarchyItemsSpec
    : Specification<Service, ServiceHierarchyItem>
{
    public GetAllServiceHierarchyItemsSpec()
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        Select(ServiceProjection.ToHierarchyItem);
    }
}

internal sealed class GetServiceForMutationSpec
    : Specification<Service>
{
    public GetServiceForMutationSpec(int serviceId)
    {
        IgnoreGlobalFilters();
        UseTracking();
        AddCriteria(x => x.Id == serviceId);
    }
}

internal sealed class GetServiceHierarchyItemByIdSpec
    : Specification<Service, ServiceHierarchyItem>
{
    public GetServiceHierarchyItemByIdSpec(
        int serviceId,
        bool includeDeleted = false)
    {
        IgnoreGlobalFilters();

        if (!includeDeleted)
        {
            AddCriteria(x => !x.IsDeleted);
        }

        UseNoTracking();
        AddCriteria(x => x.Id == serviceId);
        Select(ServiceProjection.ToHierarchyItem);
    }
}

internal sealed class ServiceDuplicateArabicNameSpec
    : Specification<Service, int>
{
    public ServiceDuplicateArabicNameSpec(
        string arabicName,
        int? parentServiceId,
        ServiceScope scope,
        int? ownerBranchId,
        int? excludedServiceId = null)
    {
        AddCriteria(x => x.ArabicName == arabicName);
        AddCriteria(x => x.Scope == scope);

        if (parentServiceId.HasValue)
        {
            var parentId = parentServiceId.Value;
            AddCriteria(x => x.ParentServiceId == parentId);
        }
        else
        {
            AddCriteria(x => x.ParentServiceId == null);
        }

        if (ownerBranchId.HasValue)
        {
            var branchId = ownerBranchId.Value;
            AddCriteria(x => x.OwnerBranchId == branchId);
        }
        else
        {
            AddCriteria(x => x.OwnerBranchId == null);
        }

        if (excludedServiceId.HasValue)
        {
            var serviceId = excludedServiceId.Value;
            AddCriteria(x => x.Id != serviceId);
        }

        UseNoTracking();
        Select(x => x.Id);
    }
}

internal sealed class ServiceDuplicateEnglishNameSpec
    : Specification<Service, int>
{
    public ServiceDuplicateEnglishNameSpec(
        string englishName,
        int? parentServiceId,
        ServiceScope scope,
        int? ownerBranchId,
        int? excludedServiceId = null)
    {
        AddCriteria(x => x.EnglishName == englishName);
        AddCriteria(x => x.Scope == scope);

        if (parentServiceId.HasValue)
        {
            var parentId = parentServiceId.Value;
            AddCriteria(x => x.ParentServiceId == parentId);
        }
        else
        {
            AddCriteria(x => x.ParentServiceId == null);
        }

        if (ownerBranchId.HasValue)
        {
            var branchId = ownerBranchId.Value;
            AddCriteria(x => x.OwnerBranchId == branchId);
        }
        else
        {
            AddCriteria(x => x.OwnerBranchId == null);
        }

        if (excludedServiceId.HasValue)
        {
            var serviceId = excludedServiceId.Value;
            AddCriteria(x => x.Id != serviceId);
        }

        UseNoTracking();
        Select(x => x.Id);
    }
}

internal sealed class ServiceDuplicateCodeSpec
    : Specification<Service, int>
{
    public ServiceDuplicateCodeSpec(
        string serviceCode,
        int? excludedServiceId = null)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x => x.ServiceCode == serviceCode);

        if (excludedServiceId.HasValue)
        {
            var serviceId = excludedServiceId.Value;
            AddCriteria(x => x.Id != serviceId);
        }

        Select(x => x.Id);
    }
}

internal sealed class ServiceCodesInUseSpec
    : Specification<Service, string>
{
    public ServiceCodesInUseSpec(IReadOnlyCollection<string> serviceCodes)
    {
        var codes = serviceCodes.ToArray();

        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x =>
            x.ServiceCode != null &&
            codes.Contains(x.ServiceCode));
        Select(x => x.ServiceCode!);
    }
}

internal sealed class ServiceHasChildrenSpec
    : Specification<Service, int>
{
    public ServiceHasChildrenSpec(int serviceId)
    {
        IgnoreGlobalFilters();
        UseNoTracking();
        AddCriteria(x => x.ParentServiceId == serviceId);
        Select(x => x.Id);
    }
}
