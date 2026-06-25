using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.PermanentDeleteWindow;

internal sealed class PermanentDeleteWindowCommandHandler
    : ICommandHandler<PermanentDeleteWindowCommand, PermanentDeleteWindowResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<Terminal> _terminalReadRepository;
    private readonly IWriteReadRepository<DisplayWindow> _displayWindowReadRepository;
    private readonly IWindowPermanentDeleteRepository _permanentDeleteRepository;
    private readonly ICurrentUser _currentUser;

    public PermanentDeleteWindowCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<Terminal> terminalReadRepository,
        IWriteReadRepository<DisplayWindow> displayWindowReadRepository,
        IWindowPermanentDeleteRepository permanentDeleteRepository,
        ICurrentUser currentUser)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _terminalReadRepository = terminalReadRepository
            ?? throw new ArgumentNullException(nameof(terminalReadRepository));

        _displayWindowReadRepository = displayWindowReadRepository
            ?? throw new ArgumentNullException(nameof(displayWindowReadRepository));

        _permanentDeleteRepository = permanentDeleteRepository
            ?? throw new ArgumentNullException(nameof(permanentDeleteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<PermanentDeleteWindowResponse>> Handle(
        PermanentDeleteWindowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(
                new Error(
                    Code: "Windows.PermanentDelete.Unauthenticated",
                    Message: ErrorMessage.Window_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var window =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetWindowForPermanentDeleteSpec(request.Id),
                cancellationToken);

        if (window is null)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(
                new Error(
                    Code: "Windows.PermanentDelete.WindowNotFound",
                    Message: ErrorMessage.Window_NotFound,
                    Type: ErrorType.NotFound));
        }

        if (!window.IsDeleted)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(
                new Error(
                    Code: "Windows.PermanentDelete.MustBeSoftDeleted",
                    Message:
                        ErrorMessage.Window_PermanentDelete_MustBeSoftDeleted,
                    Type: ErrorType.Conflict));
        }

        var hasTerminal =
            await _terminalReadRepository.AnyAsync(
                x => x.WindowId == request.Id,
                cancellationToken);

        if (hasTerminal)
        {
            return HasRelatedRecords();
        }

        var hasDisplayWindow =
            await _displayWindowReadRepository.AnyAsync(
                x => x.WindowId == request.Id,
                cancellationToken);

        if (hasDisplayWindow)
        {
            return HasRelatedRecords();
        }

        int deletedRows;
        try
        {
            deletedRows =
                await _permanentDeleteRepository.DeletePermanentlyAsync(
                    request.Id,
                    cancellationToken);
        }
        catch (WindowPermanentDeleteConflictException)
        {
            return HasRelatedRecords();
        }

        if (deletedRows == 0)
        {
            return Result<PermanentDeleteWindowResponse>.Fail(
                new Error(
                    Code: "Windows.PermanentDelete.WindowNotFound",
                    Message: ErrorMessage.Window_NotFound,
                    Type: ErrorType.NotFound));
        }

        return Result<PermanentDeleteWindowResponse>.Ok(
            new PermanentDeleteWindowResponse
            {
                Id = request.Id,
                Message = ErrorMessage.Window_PermanentDelete_Success
            });
    }

    private static Result<PermanentDeleteWindowResponse> HasRelatedRecords()
        => Result<PermanentDeleteWindowResponse>.Fail(
            new Error(
                Code: "Windows.PermanentDelete.HasRelatedRecords",
                Message:
                    ErrorMessage.Window_PermanentDelete_HasRelatedRecords,
                Type: ErrorType.Conflict));
}
