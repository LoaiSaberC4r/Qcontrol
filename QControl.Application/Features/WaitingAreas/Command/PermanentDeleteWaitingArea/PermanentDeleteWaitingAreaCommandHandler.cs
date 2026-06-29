using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.WaitingAreas.Command.PermanentDeleteWaitingArea;

internal sealed class PermanentDeleteWaitingAreaCommandHandler
    : ICommandHandler<PermanentDeleteWaitingAreaCommand, PermanentDeleteWaitingAreaResponse>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteRepository<WaitingArea> _waitingAreaWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public PermanentDeleteWaitingAreaCommandHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Window> windowReadRepository,
        IWriteRepository<WaitingArea> waitingAreaWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));
        _waitingAreaWriteRepository = waitingAreaWriteRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PermanentDeleteWaitingAreaResponse>> Handle(
        PermanentDeleteWaitingAreaCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.PermanentDelete.Unauthenticated",
                ErrorMessage.WaitingArea_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<PermanentDeleteWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.PermanentDelete.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var waitingArea = await _waitingAreaReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (waitingArea is null)
        {
            return Result<PermanentDeleteWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.PermanentDelete.WaitingAreaNotFound",
                ErrorMessage.WaitingArea_NotFound,
                ErrorType.NotFound));
        }

        if (waitingArea.IsActive)
        {
            return Result<PermanentDeleteWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.PermanentDelete.MustBeInactive",
                ErrorMessage.PermanentDelete_RequiresInactive,
                ErrorType.Conflict));
        }

        var hasWindows = await _windowReadRepository.AnyAsync(
            x => x.WaitingAreaId == waitingArea.Id,
            cancellationToken);

        if (hasWindows)
        {
            return Result<PermanentDeleteWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.PermanentDelete.HasWindows",
                ErrorMessage.WaitingArea_PermanentDelete_HasWindows,
                ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(waitingArea, rowVersion);
        _waitingAreaWriteRepository.Delete(waitingArea);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PermanentDeleteWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.PermanentDelete.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException)
        {
            return Result<PermanentDeleteWaitingAreaResponse>.Fail(new Error(
                "WaitingAreas.PermanentDelete.HasWindows",
                ErrorMessage.WaitingArea_PermanentDelete_HasWindows,
                ErrorType.Conflict));
        }

        return Result<PermanentDeleteWaitingAreaResponse>.Ok(
            new PermanentDeleteWaitingAreaResponse
            {
                Id = request.Id,
                Message = ErrorMessage.WaitingArea_PermanentDelete_Success
            });
    }
}
