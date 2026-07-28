using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.BranchServiceSegments.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServiceSegments.Command.UnassignBranchServiceSegment;

internal sealed class UnassignBranchServiceSegmentCommandHandler
    : ICommandHandler<
        UnassignBranchServiceSegmentCommand,
        UnassignBranchServiceSegmentResponse>
{
    private readonly IWriteReadRepository<Branch> _branches;
    private readonly IWriteReadRepository<Service> _services;
    private readonly IWriteReadRepository<BranchService> _branchServices;
    private readonly IWriteReadRepository<BranchServiceSegment> _assignments;
    private readonly IWriteRepository<BranchServiceSegment> _assignmentWrite;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public UnassignBranchServiceSegmentCommandHandler(
        IWriteReadRepository<Branch> branches,
        IWriteReadRepository<Service> services,
        IWriteReadRepository<BranchService> branchServices,
        IWriteReadRepository<BranchServiceSegment> assignments,
        IWriteRepository<BranchServiceSegment> assignmentWrite,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext,
        IConcurrencyTokenManager concurrency,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _branches = branches;
        _services = services;
        _branchServices = branchServices;
        _assignments = assignments;
        _assignmentWrite = assignmentWrite;
        _currentUser = currentUser;
        _branchContext = branchContext;
        _concurrency = concurrency;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UnassignBranchServiceSegmentResponse>> Handle(
        UnassignBranchServiceSegmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "BranchServiceSegments.AuthenticationRequired",
                BranchServiceSegmentMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!RowVersionConverter.TryDecode(
            request.RowVersion,
            out var rowVersion))
        {
            return Failure(
                "BranchServiceSegments.RowVersionInvalid",
                BranchServiceSegmentMessages.InvalidRowVersion,
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
            return Result<UnassignBranchServiceSegmentResponse>.Fail(
                context.Errors);
        }

        request.BranchServiceIdForInvalidation =
            context.Value.BranchServiceId;
        var relationship = await _assignments.Query()
            .AsTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.BranchServiceId == context.Value.BranchServiceId &&
                    x.SegmentId == request.SegmentId,
                cancellationToken);
        if (relationship is null)
        {
            return Failure(
                "BranchServiceSegments.RelationshipNotFound",
                BranchServiceSegmentMessages.RelationshipNotFound,
                ErrorType.NotFound);
        }

        var isDefault = await _assignments.Query()
            .Where(x => x.Id == relationship.Id)
            .Select(x => x.Segment.IsSystemDefault)
            .SingleAsync(cancellationToken);
        if (isDefault)
        {
            return Failure(
                "BranchServiceSegments.DefaultCannotBeUnassigned",
                BranchServiceSegmentMessages.DefaultCannotBeUnassigned,
                ErrorType.Validation);
        }

        var defaultRelationship = await _assignments.Query()
            .AsTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.BranchServiceId == context.Value.BranchServiceId &&
                    x.Segment.IsSystemDefault,
                cancellationToken);
        if (defaultRelationship is null)
        {
            return Failure(
                "BranchServiceSegments.DefaultAssignmentNotFound",
                BranchServiceSegmentMessages.RelationshipNotFound,
                ErrorType.Conflict);
        }

        _concurrency.SetOriginalRowVersion(relationship, rowVersion);
        var currentNonDefaultTotal = await _assignments.Query()
            .Where(x =>
                x.BranchServiceId == context.Value.BranchServiceId &&
                !x.Segment.IsSystemDefault)
            .SumAsync(x => (int?)x.Quota, cancellationToken) ?? 0;
        var removedQuota = relationship.Quota;
        var defaultQuota =
            BranchServiceSegmentQuotaCalculator.CalculateDefaultQuota(
                context.Value.Capacity,
                currentNonDefaultTotal - removedQuota);
        if (defaultQuota.IsFailure)
        {
            return Result<UnassignBranchServiceSegmentResponse>.Fail(
                defaultQuota.Errors);
        }

        var updatedDefaultQuota = defaultQuota.Value;
        defaultRelationship.UpdateQuota(
            updatedDefaultQuota,
            _currentUser.UserId.Value,
            _clock.UtcNow);
        _assignmentWrite.Delete(relationship);

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
        catch (DbUpdateException)
        {
            return Failure(
                "BranchServiceSegments.PersistenceConflict",
                BranchServiceSegmentMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }

        return Result<UnassignBranchServiceSegmentResponse>.Ok(new()
        {
            BranchId = request.BranchId,
            LeafServiceId = request.LeafServiceId,
            SegmentId = request.SegmentId,
            RemovedQuota = removedQuota,
            UpdatedDefaultQuota = updatedDefaultQuota,
            Message = BranchServiceSegmentMessages.UnassignSuccess
        });
    }

    private static Result<UnassignBranchServiceSegmentResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<UnassignBranchServiceSegmentResponse>.Fail(
            new Error(code, message, type));
}
