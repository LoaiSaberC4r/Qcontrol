using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Windows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.CreateWindow;

internal sealed class CreateWindowCommandHandler
    : ICommandHandler<CreateWindowCommand, CreateWindowResponse>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWindowCommandHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Window> windowReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));

        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));

        _windowWriteRepository = windowWriteRepository
            ?? throw new ArgumentNullException(nameof(windowWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<CreateWindowResponse>> Handle(
        CreateWindowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<CreateWindowResponse>.Fail(
                new Error(
                    Code: "Windows.Create.Unauthenticated",
                    Message: ErrorMessage.Window_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var waitingAreaExists =
            await _waitingAreaReadRepository.AnyAsync(
                x => x.Id == request.WaitingAreaId,
                cancellationToken);

        if (!waitingAreaExists)
        {
            return Result<CreateWindowResponse>.Fail(
                new Error(
                    Code: "Windows.Create.WaitingAreaNotFound",
                    Message: ErrorMessage.Window_WaitingArea_NotFound,
                    Type: ErrorType.NotFound));
        }

        var normalizedNumber = request.Number.Trim();
        var existingNumberWindowId =
            await _windowReadRepository.FirstOrDefaultAsync(
                new WindowNumberExistsSpec(
                    request.WaitingAreaId,
                    normalizedNumber),
                cancellationToken);

        if (existingNumberWindowId > 0)
        {
            return Result<CreateWindowResponse>.Fail(
                new Error(
                    Code:
                        "Windows.Create.NumberAlreadyExistsInWaitingArea",
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
                        request.WaitingAreaId,
                        normalizedIPAddress),
                    cancellationToken);

            if (existingIPAddressWindowId > 0)
            {
                return Result<CreateWindowResponse>.Fail(
                    new Error(
                        Code:
                            "Windows.Create.IPAddressAlreadyExistsInWaitingArea",
                        Message:
                            ErrorMessage
                                .Window_IPAddress_AlreadyExistsInWaitingArea,
                        Type: ErrorType.Conflict));
            }
        }

        var window = Window.Create(
            waitingAreaId: request.WaitingAreaId,
            number: normalizedNumber,
            descriptiveName: request.DescriptiveName,
            ipAddress: normalizedIPAddress,
            enableTicketBooking: request.EnableTicketBooking,
            enableDirectCall: request.EnableDirectCall,
            createdByApplicationUserId: _currentUser.UserId.Value);

        await _windowWriteRepository.AddAsync(
            window,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateWindowResponse>.Ok(
            new CreateWindowResponse
            {
                Id = window.Id,
                WaitingAreaId = window.WaitingAreaId,
                Number = window.Number,
                DescriptiveName = window.DescriptiveName,
                IPAddress = window.IPAddress,
                EnableTicketBooking = window.EnableTicketBooking,
                EnableDirectCall = window.EnableDirectCall,
                CreatedByApplicationUserId =
                    window.CreatedByApplicationUserId,
                CreatedOnUtc = window.CreatedOnUtc,
                Message = ErrorMessage.Window_Create_Success
            });
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
