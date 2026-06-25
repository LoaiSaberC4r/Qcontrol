using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.DeleteTerminal;

internal sealed class DeleteTerminalCommandHandler
    : ICommandHandler<DeleteTerminalCommand, DeleteTerminalResponse>
{
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTerminalCommandHandler(
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteRepository<Terminal> terminalWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));

        _terminalWriteRepository = terminalWriteRepository
            ?? throw new ArgumentNullException(nameof(terminalWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<DeleteTerminalResponse>> Handle(
        DeleteTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<DeleteTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.Delete.Unauthenticated",
                    Message: ErrorMessage.Terminal_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var terminal =
            await _terminalReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (terminal is null)
        {
            return Result<DeleteTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.Delete.TerminalNotFound",
                    Message: ErrorMessage.Terminal_NotFound,
                    Type: ErrorType.NotFound));
        }

        _terminalWriteRepository.Delete(terminal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeleteTerminalResponse>.Ok(
            new DeleteTerminalResponse
            {
                Id = terminal.Id,
                Message = ErrorMessage.Terminal_Delete_Success
            });
    }
}
