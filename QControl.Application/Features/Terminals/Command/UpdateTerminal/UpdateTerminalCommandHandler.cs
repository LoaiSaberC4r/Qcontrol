using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.UpdateTerminal;

internal sealed class UpdateTerminalCommandHandler
    : ICommandHandler<UpdateTerminalCommand, UpdateTerminalResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTerminalCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteRepository<Terminal> terminalWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));
        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));
        _terminalWriteRepository = terminalWriteRepository
            ?? throw new ArgumentNullException(nameof(terminalWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateTerminalResponse>> Handle(
        UpdateTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.Unauthenticated",
                ErrorMessage.Terminal_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var normalizedNumber = request.Number.Trim();
        var normalizedIPAddress = IPAddressNormalizer.TryNormalize(
            request.IPAddress,
            out var canonicalIPAddress)
            ? canonicalIPAddress
            : request.IPAddress.Trim();
        var normalizedSerialNo = request.SerialNo.Trim();
        var normalizedType = request.Type.Trim();

        var terminal =
            await _terminalReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (terminal is null)
        {
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.TerminalNotFound",
                ErrorMessage.Terminal_NotFound,
                ErrorType.NotFound));
        }

        var windowContext =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetTerminalWindowContextSpec(terminal.WindowId),
                cancellationToken);

        if (windowContext is null)
        {
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.TerminalNotFound",
                ErrorMessage.Terminal_NotFound,
                ErrorType.NotFound));
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
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.NumberAlreadyExistsInWindow",
                ErrorMessage.Terminal_Number_AlreadyExistsInWindow,
                ErrorType.Conflict));
        }

        var existingIPAddressTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalIPAddressExistsInBranchSpec(
                    terminal.BranchId,
                    normalizedIPAddress,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingIPAddressTerminalId > 0)
        {
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.IPAddressAlreadyExistsInBranch",
                ErrorMessage.Terminal_IPAddress_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var existingSerialNoTerminalId =
            await _terminalReadRepository.FirstOrDefaultAsync(
                new TerminalSerialNoExistsInBranchSpec(
                    terminal.BranchId,
                    normalizedSerialNo,
                    excludedTerminalId: terminal.Id),
                cancellationToken);

        if (existingSerialNoTerminalId > 0)
        {
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.SerialNoAlreadyExistsInBranch",
                ErrorMessage.Terminal_SerialNo_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(terminal, rowVersion);

        terminal.Update(
            number: normalizedNumber,
            ipAddress: normalizedIPAddress,
            serialNo: normalizedSerialNo,
            type: normalizedType,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _terminalWriteRepository.Update(terminal);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<UpdateTerminalResponse>.Fail(new Error(
                "Terminals.Update.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<UpdateTerminalResponse>.Ok(
            new UpdateTerminalResponse
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
                LastModifiedByApplicationUserId =
                    terminal.LastModifiedByApplicationUserId,
                ModifiedOnUtc = terminal.ModifiedOnUtc,
                Message = ErrorMessage.Terminal_Update_Success
            });
    }
}
