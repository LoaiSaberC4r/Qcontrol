using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Terminals.Command.PermanentDeleteTerminal;

internal sealed class PermanentDeleteTerminalCommandHandler
    : ICommandHandler<PermanentDeleteTerminalCommand, PermanentDeleteTerminalResponse>
{
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteRepository<Terminal> _terminalWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public PermanentDeleteTerminalCommandHandler(
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteRepository<Terminal> terminalWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
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

    public async Task<Result<PermanentDeleteTerminalResponse>> Handle(
        PermanentDeleteTerminalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(new Error(
                "Terminals.PermanentDelete.Unauthenticated",
                ErrorMessage.Terminal_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(new Error(
                "Terminals.PermanentDelete.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var terminal = await _terminalReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (terminal is null)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(new Error(
                "Terminals.PermanentDelete.TerminalNotFound",
                ErrorMessage.Terminal_NotFound,
                ErrorType.NotFound));
        }

        if (terminal.IsActive)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(new Error(
                "Terminals.PermanentDelete.MustBeInactive",
                ErrorMessage.PermanentDelete_RequiresInactive,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(terminal, rowVersion);
        _terminalWriteRepository.Delete(terminal);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(new Error(
                "Terminals.PermanentDelete.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException)
        {
            return Result<PermanentDeleteTerminalResponse>.Fail(new Error(
                "Terminals.PermanentDelete.HasRelatedRecords",
                ErrorMessage.Terminal_PermanentDelete_HasRelatedRecords,
                ErrorType.Conflict));
        }

        return Result<PermanentDeleteTerminalResponse>.Ok(
            new PermanentDeleteTerminalResponse
            {
                Id = request.Id,
                Message = ErrorMessage.Terminal_PermanentDelete_Success
            });
    }
}
