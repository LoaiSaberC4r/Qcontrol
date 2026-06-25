using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Windows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.UpdateWindow;

internal sealed class UpdateWindowCommandHandler
    : ICommandHandler<UpdateWindowCommand, UpdateWindowResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWindowCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _windowWriteRepository = windowWriteRepository
            ?? throw new ArgumentNullException(nameof(windowWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateWindowResponse>> Handle(
        UpdateWindowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<UpdateWindowResponse>.Fail(
                new Error(
                    Code: "Windows.Update.Unauthenticated",
                    Message: ErrorMessage.Window_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var window =
            await _windowReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (window is null)
        {
            return Result<UpdateWindowResponse>.Fail(
                new Error(
                    Code: "Windows.Update.WindowNotFound",
                    Message: ErrorMessage.Window_NotFound,
                    Type: ErrorType.NotFound));
        }

        var normalizedNumber = request.Number.Trim();
        var existingNumberWindowId =
            await _windowReadRepository.FirstOrDefaultAsync(
                new WindowNumberExistsSpec(
                    window.WaitingAreaId,
                    normalizedNumber,
                    excludedWindowId: window.Id),
                cancellationToken);

        if (existingNumberWindowId > 0)
        {
            return Result<UpdateWindowResponse>.Fail(
                new Error(
                    Code:
                        "Windows.Update.NumberAlreadyExistsInWaitingArea",
                    Message:
                        ErrorMessage
                            .Window_Number_AlreadyExistsInWaitingArea,
                    Type: ErrorType.Conflict));
        }

        var normalizedIPAddress = NormalizeOptional(request.IPAddress);

        if (normalizedIPAddress is not null)
        {
            var existingIPAddressWindowId =
                await _windowReadRepository.FirstOrDefaultAsync(
                    new WindowIPAddressExistsSpec(
                        window.WaitingAreaId,
                        normalizedIPAddress,
                        excludedWindowId: window.Id),
                    cancellationToken);

            if (existingIPAddressWindowId > 0)
            {
                return Result<UpdateWindowResponse>.Fail(
                    new Error(
                        Code:
                            "Windows.Update.IPAddressAlreadyExistsInWaitingArea",
                        Message:
                            ErrorMessage
                                .Window_IPAddress_AlreadyExistsInWaitingArea,
                        Type: ErrorType.Conflict));
            }
        }

        window.Update(
            number: normalizedNumber,
            descriptiveName: request.DescriptiveName,
            ipAddress: normalizedIPAddress,
            enableTicketBooking: request.EnableTicketBooking,
            enableDirectCall: request.EnableDirectCall,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _windowWriteRepository.Update(window);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateWindowResponse>.Ok(
            new UpdateWindowResponse
            {
                Id = window.Id,
                WaitingAreaId = window.WaitingAreaId,
                Number = window.Number,
                DescriptiveName = window.DescriptiveName,
                IPAddress = window.IPAddress,
                EnableTicketBooking = window.EnableTicketBooking,
                EnableDirectCall = window.EnableDirectCall,
                LastModifiedByApplicationUserId =
                    window.LastModifiedByApplicationUserId,
                ModifiedOnUtc = window.ModifiedOnUtc,
                Message = ErrorMessage.Window_Update_Success
            });
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
