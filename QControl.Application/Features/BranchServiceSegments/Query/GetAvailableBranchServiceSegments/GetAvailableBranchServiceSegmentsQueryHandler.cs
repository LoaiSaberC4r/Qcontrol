using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceSegments.Query.GetAvailableBranchServiceSegments;

internal sealed class GetAvailableBranchServiceSegmentsQueryHandler
    : IQueryHandler<
        GetAvailableBranchServiceSegmentsQuery,
        AvailableBranchServiceSegmentsResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<Service> _services;
    private readonly IWriteReadRepository<BranchService> _branchServices;
    private readonly IWriteReadRepository<BranchServiceSegment> _assignments;
    private readonly IWriteReadRepository<Segment> _segments;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;

    public GetAvailableBranchServiceSegmentsQueryHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<Service> services,
        IWriteReadRepository<BranchService> branchServices,
        IWriteReadRepository<BranchServiceSegment> assignments,
        IWriteReadRepository<Segment> segments,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext)
    {
        _branches = branches;
        _services = services;
        _branchServices = branchServices;
        _assignments = assignments;
        _segments = segments;
        _currentUser = currentUser;
        _branchContext = branchContext;
    }

    public async Task<Result<AvailableBranchServiceSegmentsResponse>> Handle(
        GetAvailableBranchServiceSegmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "BranchServiceSegments.AuthenticationRequired",
                BranchServiceSegmentMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        var context = await BranchServiceSegmentContextResolver.ResolveAsync(
            request.BranchId,
            request.LeafServiceId,
            _branches,
            _services,
            _branchServices,
            _branchContext,
            cancellationToken);
        if (context.IsFailure)
        {
            return Result<AvailableBranchServiceSegmentsResponse>.Fail(
                context.Errors);
        }

        var assignedIds = _assignments.Query()
            .Where(x =>
                x.BranchServiceId == context.Value.BranchServiceId)
            .Select(x => x.SegmentId);
        var query = _segments.Query()
            .AsNoTracking()
            .Where(x =>
                !x.IsSystemDefault &&
                !assignedIds.Contains(x.Id) &&
                (x.Scope == SegmentScope.Global ||
                 (x.Scope == SegmentScope.BranchScoped &&
                  x.OwnerBranchId == request.BranchId)));

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

        if (request.Priority.HasValue)
        {
            query = query.Where(x => x.Priority == request.Priority.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AvailableBranchServiceSegmentResponse
            {
                SegmentId = x.Id,
                ArabicName = x.ArabicName,
                EnglishName = x.EnglishName,
                Scope = x.Scope,
                OwnerBranchId = x.OwnerBranchId,
                Priority = x.Priority
            })
            .ToListAsync(cancellationToken);
        var allocated = await _assignments.Query()
            .Where(x =>
                x.BranchServiceId == context.Value.BranchServiceId &&
                !x.Segment.IsSystemDefault)
            .SumAsync(x => (int?)x.Quota, cancellationToken) ?? 0;

        return Result<AvailableBranchServiceSegmentsResponse>.Ok(new()
        {
            BranchId = request.BranchId,
            LeafServiceId = request.LeafServiceId,
            RemainingQuota = context.Value.Capacity - allocated,
            Segments = new Pagination<AvailableBranchServiceSegmentResponse>(
                request.PageNumber,
                request.PageSize,
                total,
                items)
        });
    }

    private static Result<AvailableBranchServiceSegmentsResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<AvailableBranchServiceSegmentsResponse>.Fail(
            new Error(code, message, type));
}
