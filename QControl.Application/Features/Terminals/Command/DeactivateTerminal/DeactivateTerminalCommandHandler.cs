using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Terminals.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.DeactivateTerminal;

internal sealed class DeactivateTerminalCommandHandler
    : ICommandHandler<DeactivateTerminalCommand, DeactivateTerminalResponse>
{
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateTerminalCommandHandler(
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteReadRepository<Window> windowReadRepository,
        IWriteRepository<Terminal> terminalWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));
        _terminalWriteRepository = terminalWriteRepository
            ?? throw new ArgumentNullException(nameof(terminalWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<DeactivateTerminalResponse>> Handle(
        DeactivateTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<DeactivateTerminalResponse>.Fail(new Error(
                "Terminals.Deactivate.Unauthenticated",
                ErrorMessage.Terminal_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<DeactivateTerminalResponse>.Fail(new Error(
                "Terminals.Deactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var terminal = await _terminalReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (terminal is null)
        {
            return Result<DeactivateTerminalResponse>.Fail(new Error(
                "Terminals.Deactivate.TerminalNotFound",
                ErrorMessage.Terminal_NotFound,
                ErrorType.NotFound));
        }

        if (!terminal.IsActive)
        {
            return Result<DeactivateTerminalResponse>.Fail(new Error(
                "Terminals.Deactivate.AlreadyInactive",
                ErrorMessage.Terminal_AlreadyInactive,
                ErrorType.Conflict));
        }

        var windowContext = await _windowReadRepository.FirstOrDefaultAsync(
            new GetTerminalWindowContextSpec(terminal.WindowId),
            cancellationToken);

        _concurrencyTokenManager.SetOriginalRowVersion(terminal, rowVersion);
        terminal.Deactivate(_dateTimeProvider.UtcNow, _currentUser.UserId.Value);
        _terminalWriteRepository.Update(terminal);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<DeactivateTerminalResponse>.Fail(new Error(
                "Terminals.Deactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<DeactivateTerminalResponse>.Ok(new DeactivateTerminalResponse
        {
            Id = terminal.Id,
            IsActive = terminal.IsActive,
            EffectiveIsActive =
                windowContext is not null &&
                windowContext.BranchIsActive &&
                windowContext.WaitingAreaIsActive &&
                windowContext.WindowIsActive &&
                terminal.IsActive,
            RowVersion = RowVersionConverter.ToBase64(terminal.RowVersion),
            Message = ErrorMessage.Terminal_Deactivate_Success
        });
    }
}
