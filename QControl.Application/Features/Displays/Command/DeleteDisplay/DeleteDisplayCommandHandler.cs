using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.DeleteDisplay;

internal sealed class DeleteDisplayCommandHandler
    : ICommandHandler<DeleteDisplayCommand, DeleteDisplayResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDisplayCommandHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteRepository<Display> displayWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _displayWriteRepository = displayWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<DeleteDisplayResponse>> Handle(
        DeleteDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<DeleteDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Delete.Unauthenticated",
                    Message: ErrorMessage.Display_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var display =
            await _displayReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (display is null)
        {
            return Result<DeleteDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Delete.DisplayNotFound",
                    Message: ErrorMessage.Display_NotFound,
                    Type: ErrorType.NotFound));
        }

        _displayWriteRepository.Delete(display);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeleteDisplayResponse>.Ok(
            new DeleteDisplayResponse
            {
                Id = display.Id,
                Message = ErrorMessage.Display_Delete_Success
            });
    }
}
