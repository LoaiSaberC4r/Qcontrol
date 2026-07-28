using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.BranchServiceSegments.Command.AssignBranchServiceSegments;

internal sealed class AssignBranchServiceSegmentsCommandHandler
    : ICommandHandler<
        AssignBranchServiceSegmentsCommand,
        AssignedBranchServiceSegmentsResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<Service> _services;
    private readonly IWriteReadRepository<BranchService> _branchServices;
    private readonly IWriteReadRepository<Segment> _segments;
    private readonly IWriteReadRepository<BranchServiceSegment> _assignments;
    private readonly IWriteRepository<BranchServiceSegment> _assignmentWrite;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public AssignBranchServiceSegmentsCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<Service> services,
        IWriteReadRepository<BranchService> branchServices,
        IWriteReadRepository<Segment> segments,
        IWriteReadRepository<BranchServiceSegment> assignments,
        IWriteRepository<BranchServiceSegment> assignmentWrite,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _services = services;
        _branchServices = branchServices;
        _segments = segments;
        _assignments = assignments;
        _assignmentWrite = assignmentWrite;
        _currentUser = currentUser;
        _branchContext = branchContext;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AssignedBranchServiceSegmentsResponse>> Handle(
        AssignBranchServiceSegmentsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "BranchServiceSegments.AuthenticationRequired",
                BranchServiceSegmentMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (request.Items.Count == 0)
        {
            return Failure(
                "BranchServiceSegments.ItemsRequired",
                BranchServiceSegmentMessages.ItemsRequired,
                ErrorType.Validation);
        }

        if (request.Items.Select(x => x.SegmentId).Distinct().Count() !=
            request.Items.Count)
        {
            return Failure(
                "BranchServiceSegments.DuplicateSegmentIds",
                BranchServiceSegmentMessages.DuplicateSegmentIds,
                ErrorType.Validation);
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
            return Result<AssignedBranchServiceSegmentsResponse>.Fail(
                context.Errors);
        }

        request.BranchServiceIdForInvalidation =
            context.Value.BranchServiceId;
        var ids = request.Items.Select(x => x.SegmentId).ToArray();
        var segmentRows = await _segments.Query()
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Scope,
                x.OwnerBranchId,
                x.IsSystemDefault
            })
            .ToListAsync(cancellationToken);
        if (segmentRows.Count != ids.Length)
        {
            return Failure(
                "BranchServiceSegments.SegmentNotFound",
                BranchServiceSegmentMessages.SegmentNotFound,
                ErrorType.NotFound);
        }

        if (segmentRows.Any(x => x.IsSystemDefault))
        {
            return Failure(
                "BranchServiceSegments.DefaultCannotBeAssigned",
                BranchServiceSegmentMessages.DefaultCannotBeAssigned,
                ErrorType.Validation);
        }

        if (segmentRows.Any(x =>
            x.Scope != SegmentScope.Global &&
            (x.Scope != SegmentScope.BranchScoped ||
             x.OwnerBranchId != request.BranchId)))
        {
            return Failure(
                "BranchServiceSegments.SegmentNotVisible",
                BranchServiceSegmentMessages.SegmentNotVisible,
                ErrorType.Security);
        }

        var existingIds = await _assignments.Query()
            .Where(x =>
                x.BranchServiceId == context.Value.BranchServiceId &&
                ids.Contains(x.SegmentId))
            .Select(x => x.SegmentId)
            .ToArrayAsync(cancellationToken);
        if (existingIds.Length > 0)
        {
            return Failure(
                "BranchServiceSegments.SegmentAlreadyAssigned",
                BranchServiceSegmentMessages.SegmentAlreadyAssigned,
                ErrorType.Conflict);
        }

        var existingNonDefault = await _assignments.Query()
            .Where(x =>
                x.BranchServiceId == context.Value.BranchServiceId &&
                !x.Segment.IsSystemDefault)
            .SumAsync(x => (int?)x.Quota, cancellationToken) ?? 0;
        var requestedQuota = request.Items.Sum(x => (long)x.Quota);
        var total = existingNonDefault + requestedQuota;
        var defaultQuota =
            BranchServiceSegmentQuotaCalculator.CalculateDefaultQuota(
                context.Value.Capacity,
                total);
        if (defaultQuota.IsFailure)
        {
            return Result<AssignedBranchServiceSegmentsResponse>.Fail(
                defaultQuota.Errors);
        }

        var userId = _currentUser.UserId.Value;
        var additions = request.Items.Select(x =>
            BranchServiceSegment.Create(
                context.Value.BranchServiceId,
                x.SegmentId,
                x.Quota,
                userId)).ToList();
        await _assignmentWrite.AddRangeAsync(additions, cancellationToken);

        var defaultResult = await GetOrCreateDefaultAsync(
            context.Value,
            userId,
            cancellationToken);
        if (defaultResult.IsFailure)
        {
            return Result<AssignedBranchServiceSegmentsResponse>.Fail(
                defaultResult.Errors);
        }

        defaultResult.Value.UpdateQuota(
            defaultQuota.Value,
            userId,
            _clock.UtcNow);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure(
                "BranchServiceSegments.ConcurrencyConflict",
                BranchServiceSegmentMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }
        catch (DbUpdateException ex)
            when (ex.ToString().Contains(
                "UX_BranchServiceSegment_BranchServiceId_SegmentId",
                StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                "BranchServiceSegments.SegmentAlreadyAssigned",
                BranchServiceSegmentMessages.SegmentAlreadyAssigned,
                ErrorType.Conflict);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "BranchServiceSegments.PersistenceConflict",
                BranchServiceSegmentMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }

        return Result<AssignedBranchServiceSegmentsResponse>.Ok(
            await BranchServiceSegmentResponseLoader.LoadAsync(
                context.Value,
                _assignments,
                cancellationToken,
                BranchServiceSegmentMessages.AssignSuccess));
    }

    private async Task<Result<BranchServiceSegment>> GetOrCreateDefaultAsync(
        BranchServiceSegmentContext context,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var assignment = await _assignments.Query()
            .AsTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.BranchServiceId == context.BranchServiceId &&
                    x.Segment.IsSystemDefault,
                cancellationToken);
        if (assignment is not null)
        {
            return Result<BranchServiceSegment>.Ok(assignment);
        }

        var defaultId = await _segments.Query()
            .AsNoTracking()
            .Where(x => x.IsSystemDefault)
            .Select(x => x.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (defaultId <= 0)
        {
            return Result<BranchServiceSegment>.Fail(new Error(
                "BranchServiceSegments.DefaultSegmentNotFound",
                BranchServiceSegmentMessages.SegmentNotFound,
                ErrorType.Infrastructure));
        }

        assignment = BranchServiceSegment.Create(
            context.BranchServiceId,
            defaultId,
            context.Capacity,
            userId);
        await _assignmentWrite.AddAsync(assignment, cancellationToken);
        return Result<BranchServiceSegment>.Ok(assignment);
    }

    private static Result<AssignedBranchServiceSegmentsResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<AssignedBranchServiceSegmentsResponse>.Fail(
            new Error(code, message, type));
}
