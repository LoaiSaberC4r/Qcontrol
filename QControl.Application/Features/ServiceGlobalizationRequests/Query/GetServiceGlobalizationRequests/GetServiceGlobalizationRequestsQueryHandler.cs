using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Query.GetServiceGlobalizationRequests;

internal sealed class GetServiceGlobalizationRequestsQueryHandler
    : IQueryHandler<
        GetServiceGlobalizationRequestsQuery,
        Pagination<ServiceGlobalizationRequestListItemResponse>>
{
    private readonly IWriteReadRepository<ServiceGlobalizationRequest>
        _requestReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;

    public GetServiceGlobalizationRequestsQueryHandler(
        IWriteReadRepository<ServiceGlobalizationRequest> requestReadRepository,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext)
    {
        _requestReadRepository = requestReadRepository
            ?? throw new ArgumentNullException(nameof(requestReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
    }

    public async Task<Result<Pagination<ServiceGlobalizationRequestListItemResponse>>>
        Handle(
            GetServiceGlobalizationRequestsQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "ServiceGlobalizationRequests.Get.Unauthenticated",
                ServiceGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!_currentBranchContext.IsSystemLevelActor)
        {
            return Failure(
                "ServiceGlobalizationRequests.Get.TechnicalAdminRequired",
                ServiceGlobalizationRequestMessages.TechnicalAdminRequired,
                ErrorType.Security);
        }

        request.SearchText ??= string.Empty;

        var query = BuildFilteredQuery(
            _requestReadRepository.Query().AsNoTracking(),
            request.Status,
            request.RequestType,
            request.BranchId,
            request.SearchText);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.OrderSort == OrderSort.Oldest
            ? query
                .OrderBy(x => x.RequestedOnUtc)
                .ThenBy(x => x.Id)
            : query
                .OrderByDescending(x => x.RequestedOnUtc)
                .ThenByDescending(x => x.Id);

        var items = await Project(query)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<Pagination<ServiceGlobalizationRequestListItemResponse>>
            .Ok(new Pagination<ServiceGlobalizationRequestListItemResponse>(
                request.PageNumber,
                request.PageSize,
                totalCount,
                items
                    .Select(ServiceGlobalizationRequestResponseFactory
                        .ToListItem)
                    .ToList()));
    }

    internal static IQueryable<ServiceGlobalizationRequest> BuildFilteredQuery(
        IQueryable<ServiceGlobalizationRequest> query,
        QControl.Domain.Enums.ServiceGlobalizationRequestStatus? status,
        QControl.Domain.Enums.ServiceGlobalizationRequestType? requestType,
        int? branchId,
        string? searchText)
    {
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (requestType.HasValue)
        {
            query = query.Where(x => x.RequestType == requestType.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(x =>
                x.RootService.ArabicName.Contains(term) ||
                x.RootService.EnglishName.Contains(term) ||
                x.Branch.ArabicName.Contains(term) ||
                x.Branch.EnglishName.Contains(term));
        }

        return query;
    }

    internal static IQueryable<ServiceGlobalizationRequestListProjection>
        Project(IQueryable<ServiceGlobalizationRequest> query)
    {
        return query.Select(x => new ServiceGlobalizationRequestListProjection
        {
            RequestId = x.Id,
            RequestType = x.RequestType,
            Status = x.Status,
            BranchId = x.BranchId,
            BranchArabicName = x.Branch.ArabicName,
            BranchEnglishName = x.Branch.EnglishName,
            RootServiceId = x.RootServiceId,
            RootServiceArabicName = x.RootService.ArabicName,
            RootServiceEnglishName = x.RootService.EnglishName,
            ParentServiceId = x.RootService.ParentServiceId,
            ParentServiceArabicName = x.RootService.ParentService == null
                ? null
                : x.RootService.ParentService.ArabicName,
            ParentServiceEnglishName = x.RootService.ParentService == null
                ? null
                : x.RootService.ParentService.EnglishName,
            ServicesCount = x.Items.Count,
            RequestedByApplicationUserId = x.RequestedByApplicationUserId,
            RequestedOnUtc = x.RequestedOnUtc,
            ReviewedByApplicationUserId = x.ReviewedByApplicationUserId,
            ReviewedOnUtc = x.ReviewedOnUtc,
            RejectionReason = x.RejectionReason,
            RowVersion = x.RowVersion
        });
    }

    private static Result<Pagination<ServiceGlobalizationRequestListItemResponse>>
        Failure(
            string code,
            string message,
            ErrorType type)
        => Result<Pagination<ServiceGlobalizationRequestListItemResponse>>.Fail(
            new Error(code, message, type));
}
