using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;

internal sealed class PermanentDeleteWindowCommandHandler
    : ICommandHandler<PermanentDeleteWindowCommand, PermanentDeleteWindowResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteReadRepository<DisplayWindow> _displayWindowReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public PermanentDeleteWindowCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteReadRepository<DisplayWindow> displayWindowReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));
        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));
        _displayWindowReadRepository = displayWindowReadRepository
            ?? throw new ArgumentNullException(nameof(displayWindowReadRepository));
        _windowWriteRepository = windowWriteRepository
            ?? throw new ArgumentNullException(nameof(windowWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PermanentDeleteWindowResponse>> Handle(
        PermanentDeleteWindowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(new Error(
                "Windows.PermanentDelete.Unauthenticated",
                ErrorMessage.Window_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<PermanentDeleteWindowResponse>.Fail(new Error(
                "Windows.PermanentDelete.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var window = await _windowReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (window is null)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(new Error(
                "Windows.PermanentDelete.WindowNotFound",
                ErrorMessage.Window_NotFound,
                ErrorType.NotFound));
        }

        if (window.IsActive)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(new Error(
                "Windows.PermanentDelete.MustBeInactive",
                ErrorMessage.PermanentDelete_RequiresInactive,
                ErrorType.Conflict));
        }

        var hasTerminal = await _terminalReadRepository.AnyAsync(
            x => x.WindowId == request.Id,
            cancellationToken);

        if (hasTerminal)
        {
            return HasRelatedRecords();
        }

        var hasDisplayWindow = await _displayWindowReadRepository.AnyAsync(
            x => x.WindowId == request.Id,
            cancellationToken);

        if (hasDisplayWindow)
        {
            return HasRelatedRecords();
        }

        _concurrencyTokenManager.SetOriginalRowVersion(window, rowVersion);
        _windowWriteRepository.Delete(window);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(new Error(
                "Windows.PermanentDelete.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException)
        {
            return HasRelatedRecords();
        }

        return Result<PermanentDeleteWindowResponse>.Ok(
            new PermanentDeleteWindowResponse
            {
                Id = request.Id,
                Message = ErrorMessage.Window_PermanentDelete_Success
            });
    }

    private static Result<PermanentDeleteWindowResponse> HasRelatedRecords()
        => Result<PermanentDeleteWindowResponse>.Fail(new Error(
            "Windows.PermanentDelete.HasRelatedRecords",
            ErrorMessage.Window_PermanentDelete_HasRelatedRecords,
            ErrorType.Conflict));
}
