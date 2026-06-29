using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Time;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.DeactivateDisplay;

internal sealed class DeactivateDisplayCommandHandler
    : ICommandHandler<DeactivateDisplayCommand, DeactivateDisplayResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateDisplayCommandHandler(
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

    public async Task<Result<DeactivateDisplayResponse>> Handle(
        DeactivateDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<DeactivateDisplayResponse>.Fail(new Error(
                "Displays.Deactivate.Unauthenticated",
                ErrorMessage.Display_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<DeactivateDisplayResponse>.Fail(new Error(
                "Displays.Deactivate.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var display = await _displayReadRepository.GetByIdTrackedAsync(
            request.Id,
            cancellationToken);

        if (display is null)
        {
            return Result<DeactivateDisplayResponse>.Fail(new Error(
                "Displays.Deactivate.DisplayNotFound",
                ErrorMessage.Display_NotFound,
                ErrorType.NotFound));
        }

        if (!display.IsActive)
        {
            return Result<DeactivateDisplayResponse>.Fail(new Error(
                "Displays.Deactivate.AlreadyInactive",
                ErrorMessage.Display_AlreadyInactive,
                ErrorType.Conflict));
        }

        var branch = await _branchReadRepository.GetByIdAsync(
            display.BranchId,
            cancellationToken);

        _concurrencyTokenManager.SetOriginalRowVersion(display, rowVersion);
        display.Deactivate(_dateTimeProvider.UtcNow, _currentUser.UserId.Value);
        _displayWriteRepository.Update(display);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<DeactivateDisplayResponse>.Fail(new Error(
                "Displays.Deactivate.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<DeactivateDisplayResponse>.Ok(new DeactivateDisplayResponse
        {
            Id = display.Id,
            IsActive = display.IsActive,
            EffectiveIsActive =
                branch is not null &&
                branch.IsActive &&
                display.IsActive,
            RowVersion = RowVersionConverter.ToBase64(display.RowVersion),
            Message = ErrorMessage.Display_Deactivate_Success
        });
    }
}
