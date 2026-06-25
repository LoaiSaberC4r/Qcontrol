using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.PermanentDeleteDisplay;

internal sealed class PermanentDeleteDisplayCommandHandler
    : ICommandHandler<PermanentDeleteDisplayCommand, PermanentDeleteDisplayResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<DisplayWindow> _displayWindowReadRepository;
    private readonly IDisplayPermanentDeleteRepository _permanentDeleteRepository;
    private readonly ICurrentUser _currentUser;

    public PermanentDeleteDisplayCommandHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteReadRepository<DisplayWindow> displayWindowReadRepository,
        IDisplayPermanentDeleteRepository permanentDeleteRepository,
        ICurrentUser currentUser)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _displayWindowReadRepository = displayWindowReadRepository
            ?? throw new ArgumentNullException(
                nameof(displayWindowReadRepository));

        _permanentDeleteRepository = permanentDeleteRepository
            ?? throw new ArgumentNullException(nameof(permanentDeleteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
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
                    Type: ErrorType.Security));
        }

        var display =
            await _displayReadRepository.FirstOrDefaultAsync(
                new GetDisplayForPermanentDeleteSpec(request.Id),
                cancellationToken);

        if (display is null)
        {
            return DisplayNotFound();
        }

        if (!display.IsDeleted)
        {
            return Result<PermanentDeleteDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.PermanentDelete.MustBeSoftDeleted",
                    Message:
                        ErrorMessage
                            .Display_PermanentDelete_MustBeSoftDeleted,
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

        int deletedRows;
        try
        {
            deletedRows =
                await _permanentDeleteRepository.DeletePermanentlyAsync(
                    request.Id,
                    cancellationToken);
        }
        catch (DisplayPermanentDeleteConflictException)
        {
            return HasRelatedRecords();
        }

        if (deletedRows == 0)
        {
            return DisplayNotFound();
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
