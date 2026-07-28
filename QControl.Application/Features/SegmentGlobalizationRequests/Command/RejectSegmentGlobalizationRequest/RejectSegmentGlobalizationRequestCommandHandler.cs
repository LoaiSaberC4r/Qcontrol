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

namespace Qcontrol.Application.Features.SegmentGlobalizationRequests.Command.RejectSegmentGlobalizationRequest;

internal sealed class RejectSegmentGlobalizationRequestCommandHandler
    : ICommandHandler<
        RejectSegmentGlobalizationRequestCommand,
        SegmentGlobalizationRequestResponse>
{
    private readonly IWriteReadRepository<SegmentGlobalizationRequest>
        _requests;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _branchContext;
    private readonly IConcurrencyTokenManager _concurrency;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public RejectSegmentGlobalizationRequestCommandHandler(
        IWriteReadRepository<SegmentGlobalizationRequest> requests,
        ICurrentUser currentUser,
        ICurrentBranchContext branchContext,
        IConcurrencyTokenManager concurrency,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _requests = requests;
        _currentUser = currentUser;
        _branchContext = branchContext;
        _concurrency = concurrency;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SegmentGlobalizationRequestResponse>> Handle(
        RejectSegmentGlobalizationRequestCommand request,
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
        if (entity.Status !=
            SegmentGlobalizationRequestStatus.Pending)
        {
            return Failure(
                "SegmentGlobalizationRequests.NotPending",
                SegmentGlobalizationRequestMessages.NotPending,
                ErrorType.Conflict);
        }

        _concurrency.SetOriginalRowVersion(entity, rowVersion);
        entity.Reject(
            _currentUser.UserId.Value,
            _clock.UtcNow,
            request.RejectionReason);

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
                SegmentGlobalizationRequestMessages.Rejected);
        return response is null
            ? Failure(
                "SegmentGlobalizationRequests.NotFoundAfterSave",
                SegmentGlobalizationRequestMessages.NotFound,
                ErrorType.Infrastructure)
            : Result<SegmentGlobalizationRequestResponse>.Ok(response);
    }

    private static Result<SegmentGlobalizationRequestResponse> Failure(
        string code,
        string message,
        ErrorType type) =>
        Result<SegmentGlobalizationRequestResponse>.Fail(
            new Error(code, message, type));
}
