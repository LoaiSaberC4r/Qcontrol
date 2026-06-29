using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.ReactivateWindow;

internal sealed class ReactivateWindowCommandHandler
    : ICommandHandler<ReactivateWindowCommand, ReactivateWindowResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateWindowCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _windowWriteRepository = windowWriteRepository
            ?? throw new ArgumentNullException(nameof(windowWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ReactivateWindowResponse>> Handle(
        ReactivateWindowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ReactivateWindowResponse>.Fail(new Error(
                "Windows.Reactivate.Unauthenticated",
                ErrorMessage.Window_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<ReactivateWindowResponse>.Fail(new Error(
                "Windows.Reactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var window = await _windowReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (window is null)
        {
            return Result<ReactivateWindowResponse>.Fail(new Error(
                "Windows.Reactivate.WindowNotFound",
                ErrorMessage.Window_NotFound,
                ErrorType.NotFound));
        }

        if (window.IsActive)
        {
            return Result<ReactivateWindowResponse>.Fail(new Error(
                "Windows.Reactivate.AlreadyActive",
                ErrorMessage.Window_AlreadyActive,
                ErrorType.Conflict));
        }

        var waitingArea = await _waitingAreaReadRepository.GetByIdAsync(window.WaitingAreaId, cancellationToken);
        var branch = await _branchReadRepository.GetByIdAsync(window.BranchId, cancellationToken);

        _concurrencyTokenManager.SetOriginalRowVersion(window, rowVersion);

        window.Reactivate(_dateTimeProvider.UtcNow, _currentUser.UserId.Value);
        _windowWriteRepository.Update(window);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ReactivateWindowResponse>.Fail(new Error(
                "Windows.Reactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<ReactivateWindowResponse>.Ok(new ReactivateWindowResponse
        {
            Id = window.Id,
            IsActive = window.IsActive,
            EffectiveIsActive =
                branch is not null &&
                waitingArea is not null &&
                branch.IsActive &&
                waitingArea.IsActive &&
                window.IsActive,
            RowVersion = RowVersionConverter.ToBase64(window.RowVersion),
            Message = ErrorMessage.Window_Reactivate_Success
        });
    }
}
