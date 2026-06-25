using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.DeleteWindow;

internal sealed class DeleteWindowCommandHandler
    : ICommandHandler<DeleteWindowCommand, DeleteWindowResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWindowCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _windowWriteRepository = windowWriteRepository
            ?? throw new ArgumentNullException(nameof(windowWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<DeleteWindowResponse>> Handle(
        DeleteWindowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<DeleteWindowResponse>.Fail(
                new Error(
                    Code: "Windows.Delete.Unauthenticated",
                    Message: ErrorMessage.Window_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var window =
            await _windowReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (window is null)
        {
            return Result<DeleteWindowResponse>.Fail(
                new Error(
                    Code: "Windows.Delete.WindowNotFound",
                    Message: ErrorMessage.Window_NotFound,
                    Type: ErrorType.NotFound));
        }

        _windowWriteRepository.Delete(window);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeleteWindowResponse>.Ok(
            new DeleteWindowResponse
            {
                Id = window.Id,
                Message = ErrorMessage.Window_Delete_Success
            });
    }
}
