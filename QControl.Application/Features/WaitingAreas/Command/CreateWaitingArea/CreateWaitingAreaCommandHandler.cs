using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Command.CreateWaitingArea;

internal sealed class CreateWaitingAreaCommandHandler
    : ICommandHandler<CreateWaitingAreaCommand, CreateWaitingAreaResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteRepository<WaitingArea> _waitingAreaWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWaitingAreaCommandHandler(
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

    public async Task<Result<CreateWaitingAreaResponse>> Handle(
        CreateWaitingAreaCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<CreateWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Create.Unauthenticated",
                    Message:
                        ErrorMessage.WaitingArea_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var branchExists =
            await _branchReadRepository.AnyAsync(
                x => x.Id == request.BranchId,
                cancellationToken);

        if (!branchExists)
        {
            return Result<CreateWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Create.BranchNotFound",
                    Message: ErrorMessage.WaitingArea_Branch_NotFound,
                    Type: ErrorType.NotFound));
        }

        var numberAlreadyExists =
            await _waitingAreaReadRepository.AnyAsync(
                x =>
                    x.BranchId == request.BranchId &&
                    x.Number == request.Number,
                cancellationToken);

        if (numberAlreadyExists)
        {
            return Result<CreateWaitingAreaResponse>.Fail(
                new Error(
                    Code:
                        "WaitingAreas.Create.NumberAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .WaitingArea_Number_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        var waitingArea = WaitingArea.Create(
            branchId: request.BranchId,
            number: request.Number,
            audioDevice: request.AudioDevice,
            controlDevice: request.ControlDevice,
            descriptiveName: request.DescriptiveName,
            createdByApplicationUserId: _currentUser.UserId.Value);

        await _waitingAreaWriteRepository.AddAsync(
            waitingArea,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateWaitingAreaResponse>.Ok(
            new CreateWaitingAreaResponse
            {
                Id = waitingArea.Id,
                BranchId = waitingArea.BranchId,
                Number = waitingArea.Number,
                AudioDevice = waitingArea.AudioDevice,
                ControlDevice = waitingArea.ControlDevice,
                DescriptiveName = waitingArea.DescriptiveName,
                CreatedByApplicationUserId =
                    waitingArea.CreatedByApplicationUserId,
                CreatedOnUtc = waitingArea.CreatedOnUtc,
                Message = ErrorMessage.WaitingArea_Create_Success
            });
    }
}
