using System.Data;
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.CreateTerminal;

internal sealed class CreateTerminalCommandHandler
    : ICommandHandler<CreateTerminalCommand, CreateTerminalResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTerminalCommandHandler(
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

    public async Task<Result<CreateTerminalResponse>> Handle(
        CreateTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<CreateTerminalResponse>.Fail(
                new Error(
                    Code: "Terminals.Create.Unauthenticated",
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

        async Task<Result<CreateTerminalResponse>> RollbackFailure(
            Error error)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<CreateTerminalResponse>.Fail(error);
        }

        var windowContext =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetTerminalWindowContextSpec(request.WindowId),
                cancellationToken);

        if (windowContext is null)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Create.WindowNotFound",
                    Message: ErrorMessage.Terminal_Window_NotFound,
                    Type: ErrorType.NotFound));
        }

        if (windowContext.WindowIsDeleted)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Create.WindowDeleted",
                    Message: ErrorMessage.Terminal_Window_Deleted,
                    Type: ErrorType.Conflict));
        }

        var existingNumberTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalNumberExistsSpec(
                    request.WindowId,
                    normalizedNumber),
                cancellationToken);

        if (existingNumberTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code: "Terminals.Create.NumberAlreadyExistsInWindow",
                    Message:
                        ErrorMessage.Terminal_Number_AlreadyExistsInWindow,
                    Type: ErrorType.Conflict));
        }

        var existingIPAddressTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalIPAddressExistsInBranchSpec(
                    windowContext.BranchId,
                    normalizedIPAddress),
                cancellationToken);

        if (existingIPAddressTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code:
                        "Terminals.Create.IPAddressAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .Terminal_IPAddress_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        var existingSerialNoTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalSerialNoExistsInBranchSpec(
                    windowContext.BranchId,
                    normalizedSerialNo),
                cancellationToken);

        if (existingSerialNoTerminalId > 0)
        {
            return await RollbackFailure(
                new Error(
                    Code:
                        "Terminals.Create.SerialNoAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .Terminal_SerialNo_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        var terminal = Terminal.Create(
            windowId: request.WindowId,
            number: normalizedNumber,
            ipAddress: normalizedIPAddress,
            serialNo: normalizedSerialNo,
            type: normalizedType,
            createdByApplicationUserId: _currentUser.UserId.Value);

        await _terminalWriteRepository.AddAsync(
            terminal,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<CreateTerminalResponse>.Ok(
            new CreateTerminalResponse
            {
                Id = terminal.Id,
                WindowId = terminal.WindowId,
                Number = terminal.Number,
                IPAddress = terminal.IPAddress,
                SerialNo = terminal.SerialNo,
                Type = terminal.Type,
                CreatedByApplicationUserId =
                    terminal.CreatedByApplicationUserId,
                CreatedOnUtc = terminal.CreatedOnUtc,
                Message = ErrorMessage.Terminal_Create_Success
            });
    }
}
