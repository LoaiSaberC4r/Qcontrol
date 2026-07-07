using System.Linq.Expressions;
using BuildingBlock.Domain.Specification;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Services.Shared;

internal static class ServiceProjection
{
    public static Expression<Func<Service, ServiceHierarchyItem>> ToHierarchyItem =>
        x => new ServiceHierarchyItem
        {
            Id = x.Id,
            ParentServiceId = x.ParentServiceId,
            ArabicName = x.ArabicName,
            EnglishName = x.EnglishName,
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
        if (includeDeleted)
        {
            IgnoreGlobalFilters();
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
        int? excludedServiceId = null)
    {
        AddCriteria(x => x.ArabicName == arabicName);

        if (parentServiceId.HasValue)
        {
            var parentId = parentServiceId.Value;
            AddCriteria(x => x.ParentServiceId == parentId);
        }
        else
        {
            AddCriteria(x => x.ParentServiceId == null);
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
        int? excludedServiceId = null)
    {
        AddCriteria(x => x.EnglishName == englishName);

        if (parentServiceId.HasValue)
        {
            var parentId = parentServiceId.Value;
            AddCriteria(x => x.ParentServiceId == parentId);
        }
        else
        {
            AddCriteria(x => x.ParentServiceId == null);
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
