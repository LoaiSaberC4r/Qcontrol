using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;

internal sealed class UpdateWaitingAreaCommandHandler
    : ICommandHandler<UpdateWaitingAreaCommand, UpdateWaitingAreaResponse>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<WaitingArea> _waitingAreaWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWaitingAreaCommandHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<WaitingArea> waitingAreaWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
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
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateWaitingAreaResponse>> Handle(
        UpdateWaitingAreaCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Update.Unauthenticated",
                ErrorMessage.WaitingArea_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<UpdateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Update.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var waitingArea =
            await _waitingAreaReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (waitingArea is null)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Update.WaitingAreaNotFound",
                ErrorMessage.WaitingArea_NotFound,
                ErrorType.NotFound));
        }

        var numberAlreadyExists =
            await _waitingAreaReadRepository.AnyAsync(
                x =>
                    x.Id != request.Id &&
                    x.BranchId == waitingArea.BranchId &&
                    x.Number == request.Number,
                cancellationToken);

        if (numberAlreadyExists)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Update.NumberAlreadyExistsInBranch",
                ErrorMessage.WaitingArea_Number_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var branch = await _branchReadRepository.GetByIdAsync(
            waitingArea.BranchId,
            cancellationToken);

        _concurrencyTokenManager.SetOriginalRowVersion(
            waitingArea,
            rowVersion);

        waitingArea.Update(
            number: request.Number,
            audioDevice: request.AudioDevice,
            controlDevice: request.ControlDevice,
            descriptiveName: request.DescriptiveName,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _waitingAreaWriteRepository.Update(waitingArea);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.Update.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<UpdateWaitingAreaResponse>.Ok(
            new UpdateWaitingAreaResponse
            {
                Id = waitingArea.Id,
                BranchId = waitingArea.BranchId,
                Number = waitingArea.Number,
                AudioDevice = waitingArea.AudioDevice,
                ControlDevice = waitingArea.ControlDevice,
                DescriptiveName = waitingArea.DescriptiveName,
                IsActive = waitingArea.IsActive,
                EffectiveIsActive =
                    branch is not null &&
                    branch.IsActive &&
                    waitingArea.IsActive,
                RowVersion = RowVersionConverter.ToBase64(waitingArea.RowVersion),
                LastModifiedByApplicationUserId =
                    waitingArea.LastModifiedByApplicationUserId,
                ModifiedOnUtc = waitingArea.ModifiedOnUtc,
                Message = ErrorMessage.WaitingArea_Update_Success
            });
    }
}
