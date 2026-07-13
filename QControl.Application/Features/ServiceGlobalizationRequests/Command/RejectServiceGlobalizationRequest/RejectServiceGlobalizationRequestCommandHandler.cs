using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.ServiceGlobalizationRequests.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Abstraction.Security;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;
using QControl.Domain.Enums;

namespace Qcontrol.Application.Features.ServiceGlobalizationRequests.Command.RejectServiceGlobalizationRequest;

internal sealed class RejectServiceGlobalizationRequestCommandHandler
    : ICommandHandler<
        RejectServiceGlobalizationRequestCommand,
        RejectServiceGlobalizationRequestResponse>
{
    private readonly IWriteReadRepository<ServiceGlobalizationRequest>
        _requestReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchContext _currentBranchContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public RejectServiceGlobalizationRequestCommandHandler(
        IWriteReadRepository<ServiceGlobalizationRequest> requestReadRepository,
        ICurrentUser currentUser,
        ICurrentBranchContext currentBranchContext,
        IDateTimeProvider dateTimeProvider,
        IConcurrencyTokenManager concurrencyTokenManager,
        IUnitOfWork unitOfWork)
    {
        _requestReadRepository = requestReadRepository
            ?? throw new ArgumentNullException(nameof(requestReadRepository));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchContext = currentBranchContext
            ?? throw new ArgumentNullException(nameof(currentBranchContext));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<RejectServiceGlobalizationRequestResponse>> Handle(
        RejectServiceGlobalizationRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Failure(
                "ServiceGlobalizationRequests.Reject.Unauthenticated",
                ServiceGlobalizationRequestMessages.AuthenticationRequired,
                ErrorType.Unauthorized);
        }

        if (!_currentBranchContext.IsSystemLevelActor)
        {
            return Failure(
                "ServiceGlobalizationRequests.Reject.TechnicalAdminRequired",
                ServiceGlobalizationRequestMessages.TechnicalAdminRequired,
                ErrorType.Security);
        }

        if (!RowVersionConverter.TryDecode(
                request.RowVersion,
                out var rowVersion))
        {
            return Failure(
                "ServiceGlobalizationRequests.Reject.InvalidRowVersion",
                ServiceGlobalizationRequestMessages.InvalidRowVersion,
                ErrorType.Validation);
        }

        var entity = await _requestReadRepository.Query()
            .FirstOrDefaultAsync(
                x => x.Id == request.RequestId,
                cancellationToken);

        if (entity is null)
        {
            return Failure(
                "ServiceGlobalizationRequests.Reject.NotFound",
                ServiceGlobalizationRequestMessages.RequestNotFound,
                ErrorType.NotFound);
        }

        request.BranchIdForInvalidation = entity.BranchId;

        var statusError = ValidatePendingStatus(entity.Status);
        if (statusError is not null)
        {
            return Result<RejectServiceGlobalizationRequestResponse>.Fail(
                statusError);
        }

        _concurrencyTokenManager.SetOriginalRowVersion(entity, rowVersion);

        var reviewedOnUtc = _dateTimeProvider.UtcNow;
        var reviewerId = _currentUser.UserId.Value;

        entity.Reject(
            reviewerId,
            reviewedOnUtc,
            request.RejectionReason);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Failure(
                "ServiceGlobalizationRequests.Reject.ConcurrencyConflict",
                ServiceGlobalizationRequestMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }
        catch (DbUpdateException)
        {
            return Failure(
                "ServiceGlobalizationRequests.Reject.PersistenceConflict",
                ServiceGlobalizationRequestMessages.ConcurrencyConflict,
                ErrorType.Conflict);
        }

        return Result<RejectServiceGlobalizationRequestResponse>.Ok(
            new RejectServiceGlobalizationRequestResponse
            {
                RequestId = entity.Id,
                Status = entity.Status,
                BranchId = entity.BranchId,
                RejectionReason = entity.RejectionReason,
                ReviewedByApplicationUserId = reviewerId,
                ReviewedOnUtc = reviewedOnUtc,
                RowVersion = RowVersionConverter.ToBase64(entity.RowVersion),
                Message =
                    ServiceGlobalizationRequestMessages.RejectedSuccessfully
            });
    }

    private static Error? ValidatePendingStatus(
        ServiceGlobalizationRequestStatus status)
    {
        return status switch
        {
            ServiceGlobalizationRequestStatus.Pending => null,
            ServiceGlobalizationRequestStatus.Approved => new Error(
                "ServiceGlobalizationRequests.Reject.AlreadyApproved",
                ServiceGlobalizationRequestMessages.AlreadyApproved,
                ErrorType.Conflict),
            ServiceGlobalizationRequestStatus.Rejected => new Error(
                "ServiceGlobalizationRequests.Reject.AlreadyRejected",
                ServiceGlobalizationRequestMessages.AlreadyRejected,
                ErrorType.Conflict),
            _ => new Error(
                "ServiceGlobalizationRequests.Reject.NotPending",
                ServiceGlobalizationRequestMessages.NotPending,
                ErrorType.Conflict)
        };
    }

    private static Result<RejectServiceGlobalizationRequestResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<RejectServiceGlobalizationRequestResponse>.Fail(
            new Error(code, message, type));
}
