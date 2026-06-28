using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.DisplayWindows.Command.UnassignWindowFromDisplay;

internal sealed class UnassignWindowFromDisplayCommandHandler
    : ICommandHandler<UnassignWindowFromDisplayCommand, UnassignWindowFromDisplayResponse>
{
    private readonly IWriteReadRepository<DisplayWindow> _displayWindowReadRepository;
    private readonly IWriteRepository<DisplayWindow> _displayWindowWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UnassignWindowFromDisplayCommandHandler(
        IWriteReadRepository<DisplayWindow> displayWindowReadRepository,
        IWriteRepository<DisplayWindow> displayWindowWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _displayWindowReadRepository = displayWindowReadRepository
            ?? throw new ArgumentNullException(nameof(displayWindowReadRepository));

        _displayWindowWriteRepository = displayWindowWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWindowWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UnassignWindowFromDisplayResponse>> Handle(
        UnassignWindowFromDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Failure(
                "DisplayWindows.Unassign.Unauthenticated",
                ErrorMessage.DisplayWindow_Authentication_Required,
                ErrorType.Security);
        }

        var displayWindow =
            await _displayWindowReadRepository.FirstOrDefaultAsync(
                new GetDisplayWindowLinkSpec(
                    request.DisplayId,
                    request.WindowId),
                cancellationToken);

        if (displayWindow is null)
        {
            return Failure(
                "DisplayWindows.Unassign.LinkNotFound",
                ErrorMessage.DisplayWindow_Unassign_LinkNotFound,
                ErrorType.NotFound);
        }

        _displayWindowWriteRepository.Delete(displayWindow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UnassignWindowFromDisplayResponse>.Ok(
            new UnassignWindowFromDisplayResponse
            {
                DisplayId = request.DisplayId,
                WindowId = request.WindowId,
                Message = ErrorMessage.DisplayWindow_Unassign_Success
            });
    }

    private static Result<UnassignWindowFromDisplayResponse> Failure(
        string code,
        string message,
        ErrorType type)
        => Result<UnassignWindowFromDisplayResponse>.Fail(
            new Error(
                Code: code,
                Message: message,
                Type: type));
}
