using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.SegmentGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Command.ApproveSegmentGlobalizationRequest;

internal sealed class ApproveSegmentGlobalizationRequestCommandHandler
    : ICommandHandler<
        ApproveSegmentGlobalizationRequestCommand,
        SegmentGlobalizationRequestResponse>
{
    private readonly IWriteReadRepository<SegmentGlobalizationRequest>
        _requests;
    private readonly IWriteReadRepository<Segment> _segments;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveSegmentGlobalizationRequestCommandHandler(
        IWriteReadRepository<SegmentGlobalizationRequest> requests,
        IWriteReadRepository<Segment> segments,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext,
        IConcurrencyTokenManager concurrency,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _requests = requests;
        _segments = segments;
        _currentUser = currentUser;
        _branchContext = branchContext;
        _concurrency = concurrency;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SegmentGlobalizationRequestResponse>> Handle(
        ApproveSegmentGlobalizationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var auth = ValidateActor();
        if (auth is not null)
        {
            return Result<SegmentGlobalizationRequestResponse>.Fail(auth);
        }

        if (!RowVersionConverter.TryDecode(
            request.RowVersion,
            out var rowVersion))
        {
            return Failure(
                "SegmentGlobalizationRequests.RowVersionInvalid",
                SegmentGlobalizationRequestMessages.InvalidRowVersion,
                ErrorType.Validation);
        }

        var entity = await _requests.Query()
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.RequestId,
                cancellationToken);
        if (entity is null)
        {
            return Failure(
                "SegmentGlobalizationRequests.NotFound",
                SegmentGlobalizationRequestMessages.NotFound,
                ErrorType.NotFound);
        }

        request.BranchIdForInvalidation = entity.BranchId;
        request.SegmentIdForInvalidation = entity.SegmentId;
        if (entity.Status !=
            SegmentGlobalizationRequestStatus.Pending)
        {
            return Failure(
                "SegmentGlobalizationRequests.NotPending",
                SegmentGlobalizationRequestMessages.NotPending,
                ErrorType.Conflict);
        }

        var segment = await _segments.Query()
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == entity.SegmentId,
                cancellationToken);
        if (segment is null)
        {
            return Failure(
                "SegmentGlobalizationRequests.SegmentNotFound",
                SegmentGlobalizationRequestMessages.SegmentNotFound,
                ErrorType.NotFound);
        }

        if (segment.IsSystemDefault)
        {
            return Failure(
                "SegmentGlobalizationRequests.DefaultCannotPromote",
                SegmentGlobalizationRequestMessages.DefaultCannotPromote,
                ErrorType.Conflict);
        }

        if (segment.Scope != SegmentScope.BranchScoped)
        {
            return Failure(
                "SegmentGlobalizationRequests.ScopeChanged",
                SegmentGlobalizationRequestMessages.ScopeChanged,
                ErrorType.Conflict);
        }

        if (segment.OwnerBranchId != entity.BranchId)
        {
            return Failure(
                "SegmentGlobalizationRequests.OwnerChanged",
                SegmentGlobalizationRequestMessages.OwnerChanged,
                ErrorType.Conflict);
        }

        _concurrency.SetOriginalRowVersion(entity, rowVersion);
        var now = _clock.UtcNow;
        var reviewer = _currentUser.UserId!.Value;
        segment.PromoteToGlobal(reviewer, now);
        entity.Approve(reviewer, now);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure(
                "SegmentGlobalizationRequests.ConcurrencyConflict",
                SegmentGlobalizationRequestMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "SegmentGlobalizationRequests.PersistenceConflict",
                SegmentGlobalizationRequestMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }

        var response =
            await SegmentGlobalizationRequestResponseLoader.LoadAsync(
                entity.Id,
                _requests,
                cancellationToken,
                SegmentGlobalizationRequestMessages.Approved);
        return response is null
            ? Failure(
                "SegmentGlobalizationRequests.NotFoundAfterSave",
                SegmentGlobalizationRequestMessages.NotFound,
                ErrorType.Infrastructure)
            : Result<SegmentGlobalizationRequestResponse>.Ok(response);
    }

    private Error? ValidateActor()
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return new Error(
                "SegmentGlobalizationRequests.AuthenticationRequired",
                SegmentGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        return _branchContext.IsSystemLevelActor
            ? null
            : new Error(
                "SegmentGlobalizationRequests.TechnicalAdminRequired",
                SegmentGlobalizationRequestMessages.TechnicalAdminRequired,
                ErrorType.Security);
    }

    private static Result<SegmentGlobalizationRequestResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<SegmentGlobalizationRequestResponse>.Fail(
            new Error(code, message, type));
}
