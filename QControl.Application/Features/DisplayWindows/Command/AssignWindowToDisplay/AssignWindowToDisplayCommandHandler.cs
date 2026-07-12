using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Command.AssignWindowToDisplay;

internal sealed class AssignWindowToDisplayCommandHandler
    : ICommandHandler<AssignWindowToDisplayCommand, AssignWindowToDisplayResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<DisplayWindow> _displayWindowReadRepository;
    private readonly IWriteRepository<DisplayWindow> _displayWindowWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AssignWindowToDisplayCommandHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<DisplayWindow> displayWindowReadRepository,
        IWriteRepository<DisplayWindow> displayWindowWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _displayWindowReadRepository = displayWindowReadRepository
            ?? throw new ArgumentNullException(nameof(displayWindowReadRepository));

        _displayWindowWriteRepository = displayWindowWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWindowWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<AssignWindowToDisplayResponse>> Handle(
        AssignWindowToDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Failure(
                "DisplayWindows.Assign.Unauthenticated",
                ErrorMessage.DisplayWindow_Authentication_Required,
                ErrorType.Unauthorized);
        }

        var display =
            await _displayReadRepository.FirstOrDefaultAsync(
                new GetDisplayForAssignmentSpec(request.DisplayId),
                cancellationToken);

        if (display is null)
        {
            return Failure(
                "DisplayWindows.Assign.DisplayNotFound",
                ErrorMessage.DisplayWindow_Display_NotFound,
                ErrorType.NotFound);
        }

        var window =
            await _windowReadRepository.FirstOrDefaultAsync(
                new GetWindowForAssignmentSpec(request.WindowId),
                cancellationToken);

        if (window is null)
        {
            return Failure(
                "DisplayWindows.Assign.WindowNotFound",
                ErrorMessage.DisplayWindow_Window_NotFound,
                ErrorType.NotFound);
        }

        var alreadyLinked =
            await _displayWindowReadRepository.AnyAsync(
                x =>
                    x.DisplayId == request.DisplayId &&
                    x.WindowId == request.WindowId,
                cancellationToken);

        if (alreadyLinked)
        {
            return Failure(
                "DisplayWindows.Assign.AlreadyLinked",
                ErrorMessage.DisplayWindow_Assign_AlreadyLinked,
                ErrorType.Conflict);
        }

        var displayWindow = DisplayWindow.Create(
            display.BranchId,
            request.DisplayId,
            request.WindowId,
            _currentUser.UserId.Value);

        await _displayWindowWriteRepository.AddAsync(
            displayWindow,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AssignWindowToDisplayResponse>.Ok(
            new AssignWindowToDisplayResponse
            {
                DisplayId = request.DisplayId,
                WindowId = request.WindowId,
                Message = ErrorMessage.DisplayWindow_Assign_Success
            });
    }

    private static Result<AssignWindowToDisplayResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<AssignWindowToDisplayResponse>.Fail(
            new Error(
                Code: code,
                Message: message,
                Type: type));
}
