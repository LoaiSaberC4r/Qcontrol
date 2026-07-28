using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Query.GetSegmentGlobalizationRequests;

internal sealed class GetSegmentGlobalizationRequestsQueryHandler
    : IQueryHandler<
        GetSegmentGlobalizationRequestsQuery,
        Pagination<SegmentGlobalizationRequestResponse>>
{
    private readonly IWriteReadRepository<SegmentGlobalizationRequest>
        _requests;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;

    public GetSegmentGlobalizationRequestsQueryHandler(
        IWriteReadRepository<SegmentGlobalizationRequest> requests,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext)
    {
        _requests = requests;
        _currentUser = currentUser;
        _branchContext = branchContext;
    }

    public async Task<Result<Pagination<SegmentGlobalizationRequestResponse>>>
        Handle(
            GetSegmentGlobalizationRequestsQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "SegmentGlobalizationRequests.AuthenticationRequired",
                SegmentGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!_branchContext.IsSystemLevelActor)
        {
            return Failure(
                "SegmentGlobalizationRequests.TechnicalAdminRequired",
                SegmentGlobalizationRequestMessages.TechnicalAdminRequired,
                ErrorType.Security);
        }

        return await LoadAsync(
            _requests,
            request.Status,
            request.BranchId,
            request.SearchText,
            request.PageNumber,
            request.PageSize,
            request.OrderSort,
            cancellationToken);
    }

    internal static async Task<
        Result<Pagination<SegmentGlobalizationRequestResponse>>> LoadAsync(
            IWriteReadRepository<SegmentGlobalizationRequest> requests,
            QControl.Domain.Enums.SegmentGlobalizationRequestStatus? status,
            int? branchId,
            string? searchText,
            int pageNumber,
            int pageSize,
            OrderSort orderSort,
            CancellationToken cancellationToken)
    {
        var query = requests.Query().AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(x =>
                x.Segment.ArabicName.Contains(term) ||
                x.Segment.EnglishName.Contains(term) ||
                x.Branch.ArabicName.Contains(term) ||
                x.Branch.EnglishName.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        query = orderSort == OrderSort.Oldest
            ? query.OrderBy(x => x.RequestedOnUtc).ThenBy(x => x.Id)
            : query.OrderByDescending(x => x.RequestedOnUtc)
                .ThenByDescending(x => x.Id);
        var rows = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.BranchId,
                BranchArabic = x.Branch.ArabicName,
                BranchEnglish = x.Branch.EnglishName,
                x.SegmentId,
                SegmentArabic = x.Segment.ArabicName,
                SegmentEnglish = x.Segment.EnglishName,
                SegmentScope = x.Segment.Scope,
                SegmentOwner = x.Segment.OwnerBranchId,
                SegmentPriority = x.Segment.Priority,
                x.RequestedByApplicationUserId,
                RequestedByName = x.RequestedByApplicationUser.NameEn,
                x.RequestedOnUtc,
                x.ReviewedByApplicationUserId,
                ReviewedByName = x.ReviewedByApplicationUser == null
                    ? null
                    : x.ReviewedByApplicationUser.NameEn,
                x.ReviewedOnUtc,
                x.RejectionReason,
                x.RowVersion
            })
            .ToListAsync(cancellationToken);
        var data = rows.Select(x => new SegmentGlobalizationRequestResponse
        {
            RequestId = x.Id,
            Status = x.Status,
            BranchId = x.BranchId,
            BranchArabicName = x.BranchArabic,
            BranchEnglishName = x.BranchEnglish,
            SegmentId = x.SegmentId,
            SegmentArabicName = x.SegmentArabic,
            SegmentEnglishName = x.SegmentEnglish,
            SegmentScope = x.SegmentScope,
            SegmentOwnerBranchId = x.SegmentOwner,
            SegmentPriority = x.SegmentPriority,
            RequestedByApplicationUserId = x.RequestedByApplicationUserId,
            RequestedByName = x.RequestedByName,
            RequestedOnUtc = x.RequestedOnUtc,
            ReviewedByApplicationUserId = x.ReviewedByApplicationUserId,
            ReviewedByName = x.ReviewedByName,
            ReviewedOnUtc = x.ReviewedOnUtc,
            RejectionReason = x.RejectionReason,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion)
        }).ToList();

        return Result<Pagination<SegmentGlobalizationRequestResponse>>.Ok(
            new Pagination<SegmentGlobalizationRequestResponse>(
                pageNumber,
                pageSize,
                total,
                data));
    }

    private static Result<Pagination<SegmentGlobalizationRequestResponse>>
        Failure(
            string code,
            string message,
            ErrorType type) =>
            Result<Pagination<SegmentGlobalizationRequestResponse>>.Fail(
                new Error(code, message, type));
}
