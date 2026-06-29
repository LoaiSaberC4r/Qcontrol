using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
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
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<CreateTerminalResponse>.Fail(new Error(
                "Terminals.Create.Unauthenticated",
                ErrorMessage.Terminal_Authentication_Required,
                ErrorType.Unauthorized));
        }

        var normalizedNumber = request.Number.Trim();
        var normalizedIPAddress = IPAddressNormalizer.TryNormalize(
            request.IPAddress,
            out var canonicalIPAddress)
            ? canonicalIPAddress
            : request.IPAddress.Trim();
        var normalizedSerialNo = request.SerialNo.Trim();
        var normalizedType = request.Type.Trim();

        var windowContext =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetTerminalWindowContextSpec(request.WindowId),
                cancellationToken);

        if (windowContext is null)
        {
            return Result<CreateTerminalResponse>.Fail(new Error(
                "Terminals.Create.WindowNotFound",
                ErrorMessage.Terminal_Window_NotFound,
                ErrorType.NotFound));
        }

        var existingNumberTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalNumberExistsSpec(
                    request.WindowId,
                    normalizedNumber),
                cancellationToken);

        if (existingNumberTerminalId > 0)
        {
            return Result<CreateTerminalResponse>.Fail(new Error(
                "Terminals.Create.NumberAlreadyExistsInWindow",
                ErrorMessage.Terminal_Number_AlreadyExistsInWindow,
                ErrorType.Conflict));
        }

        var existingIPAddressTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalIPAddressExistsInBranchSpec(
                    windowContext.BranchId,
                    normalizedIPAddress),
                cancellationToken);

        if (existingIPAddressTerminalId > 0)
        {
            return Result<CreateTerminalResponse>.Fail(new Error(
                "Terminals.Create.IPAddressAlreadyExistsInBranch",
                ErrorMessage.Terminal_IPAddress_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var existingSerialNoTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalSerialNoExistsInBranchSpec(
                    windowContext.BranchId,
                    normalizedSerialNo),
                cancellationToken);

        if (existingSerialNoTerminalId > 0)
        {
            return Result<CreateTerminalResponse>.Fail(new Error(
                "Terminals.Create.SerialNoAlreadyExistsInBranch",
                ErrorMessage.Terminal_SerialNo_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var terminal = Terminal.Create(
            branchId: windowContext.BranchId,
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

        return Result<CreateTerminalResponse>.Ok(
            new CreateTerminalResponse
            {
                Id = terminal.Id,
                BranchId = terminal.BranchId,
                WindowId = terminal.WindowId,
                Number = terminal.Number,
                IPAddress = terminal.IPAddress,
                SerialNo = terminal.SerialNo,
                Type = terminal.Type,
                IsActive = terminal.IsActive,
                EffectiveIsActive =
                    windowContext.BranchIsActive &&
                    windowContext.WaitingAreaIsActive &&
                    windowContext.WindowIsActive &&
                    terminal.IsActive,
                RowVersion = RowVersionConverter.ToBase64(terminal.RowVersion),
                CreatedByApplicationUserId =
                    terminal.CreatedByApplicationUserId,
                CreatedOnUtc = terminal.CreatedOnUtc,
                Message = ErrorMessage.Terminal_Create_Success
            });
    }
}
