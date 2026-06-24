using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Command.DeleteWaitingArea;

internal sealed class DeleteWaitingAreaCommandHandler
    : ICommandHandler<DeleteWaitingAreaCommand, DeleteWaitingAreaResponse>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteRepository<WaitingArea> _waitingAreaWriteRepository;
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWaitingAreaCommandHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteRepository<WaitingArea> waitingAreaWriteRepository,
        IWriteReadRepository<Window> windowReadRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));

        _waitingAreaWriteRepository = waitingAreaWriteRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaWriteRepository));

        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<DeleteWaitingAreaResponse>> Handle(
        DeleteWaitingAreaCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<DeleteWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Delete.Unauthenticated",
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
            return Result<DeleteWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Delete.WaitingAreaNotFound",
                    Message: ErrorMessage.WaitingArea_NotFound,
                    Type: ErrorType.NotFound));
        }

        var hasWindows =
            await _windowReadRepository.AnyAsync(
                x => x.WaitingAreaId == request.Id,
                cancellationToken);

        if (hasWindows)
        {
            return Result<DeleteWaitingAreaResponse>.Fail(
                new Error(
                    Code: "WaitingAreas.Delete.HasWindows",
                    Message: ErrorMessage.WaitingArea_Delete_HasWindows,
                    Type: ErrorType.Conflict));
        }

        _waitingAreaWriteRepository.Delete(waitingArea);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeleteWaitingAreaResponse>.Ok(
            new DeleteWaitingAreaResponse
            {
                Id = waitingArea.Id,
                Message = ErrorMessage.WaitingArea_Delete_Success
            });
    }
}
