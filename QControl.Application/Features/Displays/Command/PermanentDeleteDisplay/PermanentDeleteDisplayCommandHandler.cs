using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;

internal sealed class PermanentDeleteDisplayCommandHandler
    : ICommandHandler<PermanentDeleteDisplayCommand, PermanentDeleteDisplayResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<DisplayWindow> _displayWindowReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public PermanentDeleteDisplayCommandHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteReadRepository<DisplayWindow> displayWindowReadRepository,
        IWriteRepository<Display> displayWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _displayWindowReadRepository = displayWindowReadRepository
            ?? throw new ArgumentNullException(
                nameof(displayWindowReadRepository));

        _displayWriteRepository = displayWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWriteRepository));

        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PermanentDeleteDisplayResponse>> Handle(
        PermanentDeleteDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.PermanentDelete.Unauthenticated",
                    Message: ErrorMessage.Display_Authentication_Required,
                    Type: ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<PermanentDeleteDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.PermanentDelete.InvalidRowVersion",
                    Message: ErrorMessage.RowVersion_Invalid,
                    Type: ErrorType.Validation));
        }

        var display =
            await _displayReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (display is null)
        {
            return DisplayNotFound();
        }

        if (display.IsActive)
        {
            return Result<PermanentDeleteDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.PermanentDelete.MustBeInactive",
                    Message: ErrorMessage.PermanentDelete_RequiresInactive,
                    Type: ErrorType.Conflict));
        }

        var hasDisplayWindowLinks =
            await _displayWindowReadRepository.AnyAsync(
                x => x.DisplayId == request.Id,
                cancellationToken);

        if (hasDisplayWindowLinks)
        {
            return Result<PermanentDeleteDisplayResponse>.Fail(
                new Error(
                    Code:
                        "Displays.PermanentDelete.HasDisplayWindowLinks",
                    Message:
                        ErrorMessage
                            .Display_PermanentDelete_HasDisplayWindowLinks,
                    Type: ErrorType.Conflict));
        }

        _concurrencyTokenManager.SetOriginalRowVersion(display, rowVersion);
        _displayWriteRepository.Delete(display);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PermanentDeleteDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.PermanentDelete.ConcurrencyConflict",
                    Message: ErrorMessage.Concurrency_Conflict,
                    Type: ErrorType.Conflict));
        }
        catch (DbUpdateException)
        {
            return HasRelatedRecords();
        }

        return Result<PermanentDeleteDisplayResponse>.Ok(
            new PermanentDeleteDisplayResponse
            {
                Id = request.Id,
                Message = ErrorMessage.Display_PermanentDelete_Success
            });
    }

    private static Result<PermanentDeleteDisplayResponse> DisplayNotFound()
        => Result<PermanentDeleteDisplayResponse>.Fail(
            new Error(
                Code: "Displays.PermanentDelete.DisplayNotFound",
                Message: ErrorMessage.Display_NotFound,
                Type: ErrorType.NotFound));

    private static Result<PermanentDeleteDisplayResponse> HasRelatedRecords()
        => Result<PermanentDeleteDisplayResponse>.Fail(
            new Error(
                Code: "Displays.PermanentDelete.HasRelatedRecords",
                Message:
                    ErrorMessage
                        .Display_PermanentDelete_HasRelatedRecords,
                Type: ErrorType.Conflict));
}
