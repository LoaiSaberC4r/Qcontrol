using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Segments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Segments.Query.GetSegments;

internal sealed class GetSegmentsQueryHandler
    : IQueryHandler<GetSegmentsQuery, Pagination<SegmentResponse>>
{
    private readonly IWriteReadRepository<Segment> _segments;
    private readonly ICurrentUser _currentUser;
    private readonly ISegmentVisibilityPolicy _visibility;

    public GetSegmentsQueryHandler(
        IWriteReadRepository<Segment> segments,
        ICurrentUser currentUser,
        ISegmentVisibilityPolicy visibility)
    {
        _segments = segments;
        _currentUser = currentUser;
        _visibility = visibility;
    }

    public async Task<Result<Pagination<SegmentResponse>>> Handle(
        GetSegmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<SegmentResponse>>.Fail(new Error(
                "Segments.AuthenticationRequired",
                SegmentFeatureMessages.AuthenticationRequired,
                ErrorType.Unauthorized));
        }

        var visibility = _visibility.EnsureCanUseVisibilityContext(
            "Segments.List");
        if (visibility.IsFailure)
        {
            return Result<Pagination<SegmentResponse>>.Fail(
                visibility.Errors);
        }

        var query = _visibility.ApplyVisibleSegments(
            _segments.Query().AsNoTracking());

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var term = request.SearchText.Trim();
            query = query.Where(x =>
                x.ArabicName.Contains(term) ||
                x.EnglishName.Contains(term));
        }

        if (request.Scope.HasValue)
        {
            query = query.Where(x => x.Scope == request.Scope.Value);
        }

        if (request.OwnerBranchId.HasValue)
        {
            query = query.Where(x =>
                x.OwnerBranchId == request.OwnerBranchId.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(x => x.Priority == request.Priority.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var ordered = request.OrderSort == OrderSort.Oldest
            ? query.OrderBy(x => x.Priority).ThenBy(x => x.Id)
            : query.OrderByDescending(x => x.Priority).ThenBy(x => x.Id);

        var rows = await ordered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new SegmentResponse
        {
            Id = x.Id,
            ArabicName = x.ArabicName,
            EnglishName = x.EnglishName,
            Priority = x.Priority,
            Scope = x.Scope,
            OwnerBranchId = x.OwnerBranchId,
            OwnerBranchArabicName = x.OwnerArabic,
            OwnerBranchEnglishName = x.OwnerEnglish,
            IsSystemDefault = x.IsSystemDefault,
            RowVersion = RowVersionConverter.ToBase64(x.RowVersion),
            CreatedOnUtc = x.CreatedOnUtc,
            ModifiedOnUtc = x.ModifiedOnUtc
        }).ToList();

        return Result<Pagination<SegmentResponse>>.Ok(
            new Pagination<SegmentResponse>(
                request.PageNumber,
                request.PageSize,
                total,
                items));
    }
}
