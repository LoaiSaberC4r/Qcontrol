using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.DeleteWindow;

internal sealed class DeleteWindowCommandHandler
    : ICommandHandler<DeleteWindowCommand, DeleteWindowResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWindowCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        IWriteRepository<Terminal> terminalWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));

        _windowWriteRepository = windowWriteRepository
            ?? throw new ArgumentNullException(nameof(windowWriteRepository));

        _terminalWriteRepository = terminalWriteRepository
            ?? throw new ArgumentNullException(nameof(terminalWriteRepository));

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

        var activeTerminals =
            await _terminalReadRepository.ListAsync(
                new GetActiveTerminalsForWindowSpec(window.Id),
                cancellationToken);

        _terminalWriteRepository.DeleteRange(activeTerminals);
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
