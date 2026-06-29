using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.ReactivateDisplay;

internal sealed class ReactivateDisplayCommandHandler
    : ICommandHandler<ReactivateDisplayCommand, ReactivateDisplayResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateDisplayCommandHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Display> displayWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _displayWriteRepository = displayWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ReactivateDisplayResponse>> Handle(
        ReactivateDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<ReactivateDisplayResponse>.Fail(new Error(
                "Displays.Reactivate.Unauthenticated",
                ErrorMessage.Display_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<ReactivateDisplayResponse>.Fail(new Error(
                "Displays.Reactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var display = await _displayReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (display is null)
        {
            return Result<ReactivateDisplayResponse>.Fail(new Error(
                "Displays.Reactivate.DisplayNotFound",
                ErrorMessage.Display_NotFound,
                ErrorType.NotFound));
        }

        if (display.IsActive)
        {
            return Result<ReactivateDisplayResponse>.Fail(new Error(
                "Displays.Reactivate.AlreadyActive",
                ErrorMessage.Display_AlreadyActive,
                ErrorType.Conflict));
        }

        var branch = await _branchReadRepository.GetByIdAsync(
            display.BranchId,
            cancellationToken);

        _concurrencyTokenManager.SetOriginalRowVersion(display, rowVersion);
        display.Reactivate(_dateTimeProvider.UtcNow, _currentUser.UserId.Value);
        _displayWriteRepository.Update(display);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ReactivateDisplayResponse>.Fail(new Error(
                "Displays.Reactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<ReactivateDisplayResponse>.Ok(new ReactivateDisplayResponse
        {
            Id = display.Id,
            IsActive = display.IsActive,
            EffectiveIsActive =
                branch is not null &&
                branch.IsActive &&
                display.IsActive,
            RowVersion = RowVersionConverter.ToBase64(display.RowVersion),
            Message = ErrorMessage.Display_Reactivate_Success
        });
    }
}
