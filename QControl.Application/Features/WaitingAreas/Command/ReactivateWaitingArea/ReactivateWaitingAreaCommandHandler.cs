using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Command.ReactivateWaitingArea;

internal sealed class ReactivateWaitingAreaCommandHandler
    : ICommandHandler<ReactivateWaitingAreaCommand, ReactivateWaitingAreaResponse>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<WaitingArea> _waitingAreaWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateWaitingAreaCommandHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<WaitingArea> waitingAreaWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _waitingAreaWriteRepository = waitingAreaWriteRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ReactivateWaitingAreaResponse>> Handle(
        ReactivateWaitingAreaCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ReactivateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Reactivate.Unauthenticated",
                ErrorMessage.WaitingArea_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<ReactivateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Reactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var waitingArea = await _waitingAreaReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (waitingArea is null)
        {
            return Result<ReactivateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Reactivate.WaitingAreaNotFound",
                ErrorMessage.WaitingArea_NotFound,
                ErrorType.NotFound));
        }

        if (waitingArea.IsActive)
        {
            return Result<ReactivateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Reactivate.AlreadyActive",
                ErrorMessage.WaitingArea_AlreadyActive,
                ErrorType.Conflict));
        }

        var branch = await _branchReadRepository.GetByIdAsync(
            waitingArea.BranchId,
            cancellationToken);

        _concurrencyTokenManager.SetOriginalRowVersion(waitingArea, rowVersion);

        waitingArea.Reactivate(
            _dateTimeProvider.UtcNow,
            _currentUser.UserId.Value);

        _waitingAreaWriteRepository.Update(waitingArea);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ReactivateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Reactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<ReactivateWaitingAreaResponse>.Ok(
            new ReactivateWaitingAreaResponse
            {
                Id = waitingArea.Id,
                IsActive = waitingArea.IsActive,
                EffectiveIsActive =
                    branch is not null &&
                    branch.IsActive &&
                    waitingArea.IsActive,
                RowVersion = RowVersionConverter.ToBase64(waitingArea.RowVersion),
                Message = ErrorMessage.WaitingArea_Reactivate_Success
            });
    }
}
