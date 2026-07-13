using QControl.Domain.Enums;
using QControl.Application.Shared.Operational;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;

public sealed class ServiceGlobalizationRequestSummaryResponse
{
    public int RequestId { get; init; }

    public ServiceGlobalizationRequestType RequestType { get; init; }

    public ServiceGlobalizationRequestStatus Status { get; init; }

    public DateTime RequestedOnUtc { get; init; }
}

public sealed class ServiceGlobalizationRequestListItemResponse
{
    public int RequestId { get; init; }

    public ServiceGlobalizationRequestType RequestType { get; init; }

    public ServiceGlobalizationRequestStatus Status { get; init; }

    public int BranchId { get; init; }

    public string BranchArabicName { get; init; } = string.Empty;

    public string BranchEnglishName { get; init; } = string.Empty;

    public int RootServiceId { get; init; }

    public string RootServiceArabicName { get; init; } = string.Empty;

    public string RootServiceEnglishName { get; init; } = string.Empty;

    public int? ParentServiceId { get; init; }

    public string? ParentServiceArabicName { get; init; }

    public string? ParentServiceEnglishName { get; init; }

    public int ServicesCount { get; init; }

    public Guid RequestedByApplicationUserId { get; init; }

    public DateTime RequestedOnUtc { get; init; }

    public Guid? ReviewedByApplicationUserId { get; init; }

    public DateTime? ReviewedOnUtc { get; init; }

    public string? RejectionReason { get; init; }

    public string RowVersion { get; init; } = string.Empty;
}

public sealed class ServiceGlobalizationRequestDetailsResponse
{
    public int RequestId { get; init; }

    public ServiceGlobalizationRequestType RequestType { get; init; }

    public ServiceGlobalizationRequestStatus Status { get; init; }

    public int BranchId { get; init; }

    public string BranchArabicName { get; init; } = string.Empty;

    public string BranchEnglishName { get; init; } = string.Empty;

    public int RootServiceId { get; init; }

    public Guid RequestedByApplicationUserId { get; init; }

    public DateTime RequestedOnUtc { get; init; }

    public Guid? ReviewedByApplicationUserId { get; init; }

    public DateTime? ReviewedOnUtc { get; init; }

    public string? RejectionReason { get; init; }

    public ServiceGlobalizationParentServiceResponse? ParentService { get; init; }

    public IReadOnlyList<ServiceGlobalizationRequestServiceNodeResponse>
        Services { get; init; } =
            Array.Empty<ServiceGlobalizationRequestServiceNodeResponse>();

    public string RowVersion { get; init; } = string.Empty;
}

public sealed class ServiceGlobalizationParentServiceResponse
{
    public int ServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public ServiceScope Scope { get; init; }

    public int? OwnerBranchId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }
}

public sealed class ServiceGlobalizationRequestServiceNodeResponse
{
    public int ServiceId { get; init; }

    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public ServiceScope Scope { get; init; }

    public int? OwnerBranchId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public IReadOnlyList<ServiceGlobalizationRequestServiceNodeResponse>
        Children { get; init; } =
            Array.Empty<ServiceGlobalizationRequestServiceNodeResponse>();
}

public sealed class ApproveServiceGlobalizationRequestResponse
{
    public int RequestId { get; init; }

    public ServiceGlobalizationRequestStatus Status { get; init; }

    public int BranchId { get; init; }

    public IReadOnlyList<int> PromotedServiceIds { get; init; } =
        Array.Empty<int>();

    public Guid ReviewedByApplicationUserId { get; init; }

    public DateTime ReviewedOnUtc { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}

public sealed class RejectServiceGlobalizationRequestResponse
{
    public int RequestId { get; init; }

    public ServiceGlobalizationRequestStatus Status { get; init; }

    public int BranchId { get; init; }

    public string? RejectionReason { get; init; }

    public Guid ReviewedByApplicationUserId { get; init; }

    public DateTime ReviewedOnUtc { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}

internal sealed class ServiceGlobalizationRequestServiceProjection
{
    public int ServiceId { get; init; }

    public int? ParentServiceId { get; init; }

    public string ArabicName { get; init; } = string.Empty;

    public string EnglishName { get; init; } = string.Empty;

    public ServiceScope Scope { get; init; }

    public int? OwnerBranchId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }
}

internal sealed class ServiceGlobalizationRequestListProjection
{
    public int RequestId { get; init; }

    public ServiceGlobalizationRequestType RequestType { get; init; }

    public ServiceGlobalizationRequestStatus Status { get; init; }

    public int BranchId { get; init; }

    public string BranchArabicName { get; init; } = string.Empty;

    public string BranchEnglishName { get; init; } = string.Empty;

    public int RootServiceId { get; init; }

    public string RootServiceArabicName { get; init; } = string.Empty;

    public string RootServiceEnglishName { get; init; } = string.Empty;

    public int? ParentServiceId { get; init; }

    public string? ParentServiceArabicName { get; init; }

    public string? ParentServiceEnglishName { get; init; }

    public int ServicesCount { get; init; }

    public Guid RequestedByApplicationUserId { get; init; }

    public DateTime RequestedOnUtc { get; init; }

    public Guid? ReviewedByApplicationUserId { get; init; }

    public DateTime? ReviewedOnUtc { get; init; }

    public string? RejectionReason { get; init; }

    public byte[] RowVersion { get; init; } = Array.Empty<byte>();
}

internal static class ServiceGlobalizationRequestResponseFactory
{
    public static ServiceGlobalizationRequestListItemResponse ToListItem(
        ServiceGlobalizationRequestListProjection item)
    {
        return new ServiceGlobalizationRequestListItemResponse
        {
            RequestId = item.RequestId,
            RequestType = item.RequestType,
            Status = item.Status,
            BranchId = item.BranchId,
            BranchArabicName = item.BranchArabicName,
            BranchEnglishName = item.BranchEnglishName,
            RootServiceId = item.RootServiceId,
            RootServiceArabicName = item.RootServiceArabicName,
            RootServiceEnglishName = item.RootServiceEnglishName,
            ParentServiceId = item.ParentServiceId,
            ParentServiceArabicName = item.ParentServiceArabicName,
            ParentServiceEnglishName = item.ParentServiceEnglishName,
            ServicesCount = item.ServicesCount,
            RequestedByApplicationUserId =
                item.RequestedByApplicationUserId,
            RequestedOnUtc = item.RequestedOnUtc,
            ReviewedByApplicationUserId =
                item.ReviewedByApplicationUserId,
            ReviewedOnUtc = item.ReviewedOnUtc,
            RejectionReason = item.RejectionReason,
            RowVersion = RowVersionConverter.ToBase64(item.RowVersion)
        };
    }

    public static IReadOnlyList<ServiceGlobalizationRequestServiceNodeResponse>
        BuildRequestServiceTree(
            IReadOnlyCollection<ServiceGlobalizationRequestServiceProjection>
                services)
    {
        var serviceIds = services
            .Select(x => x.ServiceId)
            .ToHashSet();

        return services
            .Where(x =>
                !x.ParentServiceId.HasValue ||
                !serviceIds.Contains(x.ParentServiceId.Value))
            .OrderBy(x => x.ArabicName)
            .ThenBy(x => x.ServiceId)
            .Select(x => BuildNode(x, services))
            .ToList();
    }

    private static ServiceGlobalizationRequestServiceNodeResponse BuildNode(
        ServiceGlobalizationRequestServiceProjection service,
        IReadOnlyCollection<ServiceGlobalizationRequestServiceProjection>
            services)
    {
        return new ServiceGlobalizationRequestServiceNodeResponse
        {
            ServiceId = service.ServiceId,
            ParentServiceId = service.ParentServiceId,
            ArabicName = service.ArabicName,
            EnglishName = service.EnglishName,
            Scope = service.Scope,
            OwnerBranchId = service.OwnerBranchId,
            IsActive = service.IsActive,
            IsDeleted = service.IsDeleted,
            Children = services
                .Where(x => x.ParentServiceId == service.ServiceId)
                .OrderBy(x => x.ArabicName)
                .ThenBy(x => x.ServiceId)
                .Select(x => BuildNode(x, services))
                .ToList()
        };
    }
}
