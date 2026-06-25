using System.Data;
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.RestoreTerminal;

internal sealed class RestoreTerminalCommandHandler
    : ICommandHandler<RestoreTerminalCommand, RestoreTerminalResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreTerminalCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteRepository<Terminal> terminalWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));

        _terminalWriteRepository = terminalWriteRepository
            ?? throw new ArgumentNullException(nameof(terminalWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<RestoreTerminalResponse>> Handle(
        RestoreTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<RestoreTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.Restore.Unauthenticated",
                    Message: ErrorMessage.Terminal_Authentication_Required,
                    Type: ErrorType.Security));
        }

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        async Task<Result<RestoreTerminalResponse>> RollbackFailure(
            Error error)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<RestoreTerminalResponse>.Fail(error);
        }

        var terminal =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new GetTerminalIncludingDeletedSpec(request.Id),
                cancellationToken);

        if (terminal is null)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Restore.TerminalNotFound",
                    Message: ErrorMessage.Terminal_NotFound,
                    Type: ErrorType.NotFound));
        }

        if (!terminal.IsDeleted)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Restore.AlreadyActive",
                    Message: ErrorMessage.Terminal_AlreadyActive,
                    Type: ErrorType.Conflict));
        }

        var windowContext =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetTerminalWindowContextSpec(terminal.WindowId),
                cancellationToken);

        if (windowContext is null)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Restore.WindowNotFound",
                    Message: ErrorMessage.Terminal_Window_NotFound,
                    Type: ErrorType.NotFound));
        }

        if (windowContext.WindowIsDeleted)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Restore.WindowDeleted",
                    Message: ErrorMessage.Terminal_Restore_WindowDeleted,
                    Type: ErrorType.Conflict));
        }

        var existingNumberTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalNumberExistsSpec(
                    terminal.WindowId,
                    terminal.Number,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingNumberTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Restore.NumberConflict",
                    Message:
                        ErrorMessage.Terminal_Number_AlreadyExistsInWindow,
                    Type: ErrorType.Conflict));
        }

        var existingIPAddressTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalIPAddressExistsInBranchSpec(
                    windowContext.BranchId,
                    terminal.IPAddress,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingIPAddressTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Restore.IPAddressConflict",
                    Message:
                        ErrorMessage
                            .Terminal_IPAddress_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        var existingSerialNoTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalSerialNoExistsInBranchSpec(
                    windowContext.BranchId,
                    terminal.SerialNo,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingSerialNoTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Restore.SerialNoConflict",
                    Message:
                        ErrorMessage
                            .Terminal_SerialNo_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        terminal.Restore();
        _terminalWriteRepository.Update(terminal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<RestoreTerminalResponse>.Ok(
            new RestoreTerminalResponse
            {
                Id = terminal.Id,
                WindowId = terminal.WindowId,
                Number = terminal.Number,
                IPAddress = terminal.IPAddress,
                SerialNo = terminal.SerialNo,
                Type = terminal.Type,
                RestoredOnUtc = terminal.RestoredOnUtc,
                Message = ErrorMessage.Terminal_Restore_Success
            });
    }
}
