using System.Data;
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;

internal sealed class UpdateTerminalCommandHandler
    : ICommandHandler<UpdateTerminalCommand, UpdateTerminalResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTerminalCommandHandler(
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

    public async Task<Result<UpdateTerminalResponse>> Handle(
        UpdateTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<UpdateTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.Update.Unauthenticated",
                    Message: ErrorMessage.Terminal_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var normalizedNumber = request.Number.Trim();
        var normalizedIPAddress = request.IPAddress.Trim();
        var normalizedSerialNo = request.SerialNo.Trim();
        var normalizedType = request.Type.Trim();

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        async Task<Result<UpdateTerminalResponse>> RollbackFailure(
            Error error)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<UpdateTerminalResponse>.Fail(error);
        }

        var terminal =
            await _terminalReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (terminal is null)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Update.TerminalNotFound",
                    Message: ErrorMessage.Terminal_NotFound,
                    Type: ErrorType.NotFound));
        }

        var windowContext =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetTerminalWindowContextSpec(terminal.WindowId),
                cancellationToken);

        if (windowContext is null)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Update.TerminalNotFound",
                    Message: ErrorMessage.Terminal_NotFound,
                    Type: ErrorType.NotFound));
        }

        var existingNumberTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalNumberExistsSpec(
                    terminal.WindowId,
                    normalizedNumber,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingNumberTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Update.NumberAlreadyExistsInWindow",
                    Message:
                        ErrorMessage.Terminal_Number_AlreadyExistsInWindow,
                    Type: ErrorType.Conflict));
        }

        var existingIPAddressTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalIPAddressExistsInBranchSpec(
                    windowContext.BranchId,
                    normalizedIPAddress,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingIPAddressTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code:
                        "Terminals.Update.IPAddressAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .Terminal_IPAddress_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        var existingSerialNoTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalSerialNoExistsInBranchSpec(
                    windowContext.BranchId,
                    normalizedSerialNo,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingSerialNoTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code:
                        "Terminals.Update.SerialNoAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .Terminal_SerialNo_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        terminal.Update(
            number: normalizedNumber,
            ipAddress: normalizedIPAddress,
            serialNo: normalizedSerialNo,
            type: normalizedType,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _terminalWriteRepository.Update(terminal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<UpdateTerminalResponse>.Ok(
            new UpdateTerminalResponse
            {
                Id = terminal.Id,
                WindowId = terminal.WindowId,
                Number = terminal.Number,
                IPAddress = terminal.IPAddress,
                SerialNo = terminal.SerialNo,
                Type = terminal.Type,
                LastModifiedByApplicationUserId =
                    terminal.LastModifiedByApplicationUserId,
                ModifiedOnUtc = terminal.ModifiedOnUtc,
                Message = ErrorMessage.Terminal_Update_Success
            });
    }
}
