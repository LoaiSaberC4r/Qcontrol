using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.Segments.Query.GetSegmentById;

internal sealed class GetSegmentByIdQueryHandler
    : IQueryHandler<GetSegmentByIdQuery, SegmentDetailsResponse>
{
    private readonly IWriteReadRepository<Segment> _segments;
    private readonly IWriteReadRepository<BranchServiceSegment> _assignments;
    private readonly IWriteReadRepository<SegmentGlobalizationRequest>
        _requests;
    private readonly ICurrentUser _currentUser;
    private readonly ISegmentVisibilityPolicy _visibility;

    public GetSegmentByIdQueryHandler(
        IWriteReadRepository<Segment> segments,
        IWriteReadRepository<BranchServiceSegment> assignments,
        IWriteReadRepository<SegmentGlobalizationRequest> requests,
        ICurrentUser currentUser,
        ISegmentVisibilityPolicy visibility)
    {
        _segments = segments;
        _assignments = assignments;
        _requests = requests;
        _currentUser = currentUser;
        _visibility = visibility;
    }

    public async Task<Result<SegmentDetailsResponse>> Handle(
        GetSegmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "Segments.AuthenticationRequired",
                SegmentFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var row = await _segments.Query()
            .AsNoTracking()
            .Where(x => x.Id == request.SegmentId)
            .Select(x => new
            {
                x.Id,
                x.ArabicName,
                x.EnglishName,
                x.Priority,
                x.Scope,
                x.OwnerBranchId,
                OwnerArabic = x.OwnerBranch == null
                    ? null
                    : x.OwnerBranch.ArabicName,
                OwnerEnglish = x.OwnerBranch == null
                    ? null
                    : x.OwnerBranch.EnglishName,
                x.IsSystemDefault,
                x.RowVersion,
                x.CreatedOnUtc,
                x.ModifiedOnUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return Failure(
                "Segments.NotFound",
                SegmentFeatureMessages.NotFound,
                ErrorType.NotFound);
        }

        if (!_visibility.CanView(row.Scope, row.OwnerBranchId))
        {
            return Failure(
                "Segments.ForeignBranchSegmentForbidden",
                SegmentFeatureMessages.ForeignBranchForbidden,
                ErrorType.Security);
        }

        var assignedCount = await _assignments.Query()
            .AsNoTracking()
            .CountAsync(x => x.SegmentId == row.Id, cancellationToken);
        var pendingRequestId = await _requests.Query()
            .AsNoTracking()
            .Where(x =>
                x.SegmentId == row.Id &&
                x.Status == SegmentGlobalizationRequestStatus.Pending)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<SegmentDetailsResponse>.Ok(
            new SegmentDetailsResponse
            {
                Id = row.Id,
                ArabicName = row.ArabicName,
                EnglishName = row.EnglishName,
                Priority = row.Priority,
                Scope = row.Scope,
                OwnerBranchId = row.OwnerBranchId,
                OwnerBranchArabicName = row.OwnerArabic,
                OwnerBranchEnglishName = row.OwnerEnglish,
                IsSystemDefault = row.IsSystemDefault,
                RowVersion = RowVersionConverter.ToBase64(row.RowVersion),
                CreatedOnUtc = row.CreatedOnUtc,
                ModifiedOnUtc = row.ModifiedOnUtc,
                AssignedLeafServicesCount = assignedCount,
                PendingGlobalizationRequestId = pendingRequestId
            });
    }

    private static Result<SegmentDetailsResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<SegmentDetailsResponse>.Fail(new Error(code, message, type));
}
