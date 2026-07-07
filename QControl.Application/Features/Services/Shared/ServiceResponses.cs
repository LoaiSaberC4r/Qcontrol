using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Services.Shared;

public sealed class ServiceResponse
{
    public int Id { get; init; }

    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public string? IconUrl { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool IsTicketIssuable { get; init; }

    public bool HasChildren { get; init; }

    public bool CanIssueTicket { get; init; }

    public bool IsDeleted { get; init; }

    public int OrderNo { get; init; }

    public int Priority { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string? Message { get; init; }
}

public sealed class ServiceDetailsResponse
{
    public int Id { get; init; }

    public int? ParentServiceId { get; init; }

    public string? ParentServiceNameAr { get; init; }

    public string? ParentServiceNameEn { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public string? ArabicUserMessage { get; init; }

    public string? EnglishUserMessage { get; init; }

    public string? LogoUrl { get; init; }

    public string? IconUrl { get; init; }

    public IReadOnlyList<ServiceImageResponse> AdsImages { get; init; } =
        Array.Empty<ServiceImageResponse>();

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool IsTicketIssuable { get; init; }

    public bool IsClientInputRequired { get; init; }

    public bool HasReservation { get; init; }

    public bool HasChildren { get; init; }

    public bool CanIssueTicket { get; init; }

    public bool IsDeleted { get; init; }

    public DateTime? DeletedOnUtc { get; init; }

    public DateTime? RestoredOnUtc { get; init; }

    public int OrderNo { get; init; }

    public int Priority { get; init; }

    public string? RangePrefix { get; init; }

    public int? RangeStartNumber { get; init; }

    public int? RangeEndNumber { get; init; }

    public int? WaitingDuration { get; init; }

    public int? NoOfTicketCopies { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public Guid CreatedByApplicationUserId { get; init; }

    public Guid? LastModifiedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime? ModifiedOnUtc { get; init; }
}

public sealed class ServiceTreeNodeResponse
{
    public int Id { get; init; }

    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool IsTicketIssuable { get; init; }

    public bool HasChildren { get; init; }

    public bool CanIssueTicket { get; init; }

    public bool IsDeleted { get; init; }

    public int OrderNo { get; init; }

    public IReadOnlyList<ServiceTreeNodeResponse> Children { get; init; } =
        Array.Empty<ServiceTreeNodeResponse>();
}

public sealed class AvailableParentServiceResponse
{
    public int Id { get; init; }

    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public bool EffectiveIsActive { get; init; }
}

public sealed class ServiceImageResponse
{
    public int Id { get; init; }

    public int ServiceId { get; init; }

    public string ImageUrl { get; init; } = string.Empty;

    public ServiceImageType ImageType { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string? Message { get; init; }
}

public sealed class ServiceImageDeleteResponse
{
    public int Id { get; init; }

    public int ServiceId { get; init; }

    public ServiceImageType ImageType { get; init; }

    public string? Message { get; init; }
}

public sealed class ServiceDeleteResponse
{
    public int Id { get; init; }

    public bool IsDeleted { get; init; }

    public bool IsActive { get; init; }

    public string? RowVersion { get; init; }

    public string? Message { get; init; }
}

public sealed class ServiceRestoreResponse
{
    public int Id { get; init; }

    public bool IsDeleted { get; init; }

    public bool IsActive { get; init; }

    public bool EffectiveIsActive { get; init; }

    public bool CanIssueTicket { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string? Message { get; init; }
}

internal static class ServiceResponseFactory
{
    public static ServiceResponse FromItem(
        ServiceHierarchyItem item,
        ServiceHierarchyState state,
        ServiceImagesForResponse? images = null,
        string? message = null)
    {
        return new ServiceResponse
        {
            Id = item.Id,
            ParentServiceId = item.ParentServiceId,
            ArabicName = item.ArabicName,
            EnglishName = item.EnglishName,
            LogoUrl = images?.LogoUrl,
            IconUrl = images?.IconUrl,
            IsActive = item.IsActive,
            EffectiveIsActive = state.EffectiveIsActive,
            IsTicketIssuable = item.IsTicketIssuable,
            HasChildren = state.HasChildren,
            CanIssueTicket = state.CanIssueTicket,
            IsDeleted = item.IsDeleted,
            OrderNo = item.OrderNo,
            Priority = item.Priority,
            RowVersion = RowVersionConverter.ToBase64(item.RowVersion),
            Message = message
        };
    }

    public static ServiceDetailsResponse ToDetails(
        ServiceHierarchyItem item,
        ServiceHierarchyState state,
        ServiceHierarchyItem? parent,
        ServiceImagesForResponse? images = null)
    {
        return new ServiceDetailsResponse
        {
            Id = item.Id,
            ParentServiceId = item.ParentServiceId,
            ParentServiceNameAr = parent?.ArabicName,
            ParentServiceNameEn = parent?.EnglishName,
            ArabicName = item.ArabicName,
            EnglishName = item.EnglishName,
            ArabicUserMessage = item.ArabicUserMessage,
            EnglishUserMessage = item.EnglishUserMessage,
            LogoUrl = images?.LogoUrl,
            IconUrl = images?.IconUrl,
            AdsImages = images?.AdsImages ?? Array.Empty<ServiceImageResponse>(),
            IsActive = item.IsActive,
            EffectiveIsActive = state.EffectiveIsActive,
            IsTicketIssuable = item.IsTicketIssuable,
            IsClientInputRequired = item.IsClientInputRequired,
            HasReservation = item.HasReservation,
            HasChildren = state.HasChildren,
            CanIssueTicket = state.CanIssueTicket,
            IsDeleted = item.IsDeleted,
            DeletedOnUtc = item.DeletedOnUtc,
            RestoredOnUtc = item.RestoredOnUtc,
            OrderNo = item.OrderNo,
            Priority = item.Priority,
            RangePrefix = item.RangePrefix,
            RangeStartNumber = item.RangeStartNumber,
            RangeEndNumber = item.RangeEndNumber,
            WaitingDuration = item.WaitingDuration,
            NoOfTicketCopies = item.NoOfTicketCopies,
            RowVersion = RowVersionConverter.ToBase64(item.RowVersion),
            CreatedByApplicationUserId = item.CreatedByApplicationUserId,
            LastModifiedByApplicationUserId =
                item.LastModifiedByApplicationUserId,
            CreatedOnUtc = item.CreatedOnUtc,
            ModifiedOnUtc = item.ModifiedOnUtc
        };
    }

    public static ServiceRestoreResponse Restored(
        ServiceHierarchyItem item,
        ServiceHierarchyState state,
        string? message)
    {
        return new ServiceRestoreResponse
        {
            Id = item.Id,
            IsDeleted = item.IsDeleted,
            IsActive = item.IsActive,
            EffectiveIsActive = state.EffectiveIsActive,
            CanIssueTicket = state.CanIssueTicket,
            RowVersion = RowVersionConverter.ToBase64(item.RowVersion),
            Message = message
        };
    }
}

internal sealed class ServiceImagesForResponse
{
    public string? LogoUrl { get; init; }

    public string? IconUrl { get; init; }

    public IReadOnlyList<ServiceImageResponse> AdsImages { get; init; } =
        Array.Empty<ServiceImageResponse>();

    public static ServiceImagesForResponse Empty { get; } = new();
}

internal static class ServiceImageResponseFactory
{
    public static ServiceImageResponse FromEntity(
        ServiceImage image,
        string? message = null)
    {
        return new ServiceImageResponse
        {
            Id = image.Id,
            ServiceId = image.ServiceId,
            ImageUrl = ServiceMediaUrlMapper.ToMediaUrl(image.ImagePath) ??
                string.Empty,
            ImageType = image.ImageType,
            DisplayOrder = image.DisplayOrder,
            IsActive = image.IsActive,
            RowVersion = RowVersionConverter.ToBase64(image.RowVersion),
            Message = message
        };
    }

    public static ServiceImagesForResponse ToImagesForResponse(
        IEnumerable<ServiceImage> images,
        bool includeAds)
    {
        var orderedImages = images
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToList();

        var logo = orderedImages.FirstOrDefault(
            x => x.ImageType == ServiceImageType.Logo);
        var icon = orderedImages.FirstOrDefault(
            x => x.ImageType == ServiceImageType.Icon);

        return new ServiceImagesForResponse
        {
            LogoUrl = ServiceMediaUrlMapper.ToMediaUrl(logo?.ImagePath),
            IconUrl = ServiceMediaUrlMapper.ToMediaUrl(icon?.ImagePath),
            AdsImages = includeAds
                ? orderedImages
                    .Where(x => x.ImageType == ServiceImageType.Ads)
                    .Select(x => FromEntity(x))
                    .ToList()
                : Array.Empty<ServiceImageResponse>()
        };
    }

    public static IReadOnlyDictionary<int, ServiceImagesForResponse>
        GroupForServices(
            IEnumerable<ServiceImage> images,
            bool includeAds)
    {
        return images
            .GroupBy(x => x.ServiceId)
            .ToDictionary(
                x => x.Key,
                x => ToImagesForResponse(x, includeAds));
    }
}
