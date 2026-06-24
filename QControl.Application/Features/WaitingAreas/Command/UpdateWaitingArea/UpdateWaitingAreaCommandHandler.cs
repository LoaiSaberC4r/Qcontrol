using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Command.UpdateWaitingArea;

internal sealed class UpdateWaitingAreaCommandHandler
    : ICommandHandler<UpdateWaitingAreaCommand, UpdateWaitingAreaResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteRepository<WaitingArea> _waitingAreaWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWaitingAreaCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteRepository<WaitingArea> waitingAreaWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));

        _waitingAreaWriteRepository = waitingAreaWriteRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateWaitingAreaResponse>> Handle(
        UpdateWaitingAreaCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Update.Unauthenticated",
                    Message:
                        ErrorMessage.WaitingArea_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var waitingArea =
            await _waitingAreaReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (waitingArea is null)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Update.WaitingAreaNotFound",
                    Message: ErrorMessage.WaitingArea_NotFound,
                    Type: ErrorType.NotFound));
        }

        var branchExists =
            await _branchReadRepository.AnyAsync(
                x => x.Id == request.BranchId,
                cancellationToken);

        if (!branchExists)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Update.BranchNotFound",
                    Message: ErrorMessage.WaitingArea_Branch_NotFound,
                    Type: ErrorType.NotFound));
        }

        var numberAlreadyExists =
            await _waitingAreaReadRepository.AnyAsync(
                x =>
                    x.Id != request.Id &&
                    x.BranchId == request.BranchId &&
                    x.Number == request.Number,
                cancellationToken);

        if (numberAlreadyExists)
        {
            return Result<UpdateWaitingAreaResponse>.Fail(
                new Error(
                    Code:
                        "WaitingAreas.Update.NumberAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .WaitingArea_Number_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        waitingArea.Update(
            branchId: request.BranchId,
            number: request.Number,
            audioDevice: request.AudioDevice,
            controlDevice: request.ControlDevice,
            descriptiveName: request.DescriptiveName,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _waitingAreaWriteRepository.Update(waitingArea);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateWaitingAreaResponse>.Ok(
            new UpdateWaitingAreaResponse
            {
                Id = waitingArea.Id,
                BranchId = waitingArea.BranchId,
                Number = waitingArea.Number,
                AudioDevice = waitingArea.AudioDevice,
                ControlDevice = waitingArea.ControlDevice,
                DescriptiveName = waitingArea.DescriptiveName,
                LastModifiedByApplicationUserId =
                    waitingArea.LastModifiedByApplicationUserId,
                ModifiedOnUtc = waitingArea.ModifiedOnUtc,
                Message = ErrorMessage.WaitingArea_Update_Success
            });
    }
}
