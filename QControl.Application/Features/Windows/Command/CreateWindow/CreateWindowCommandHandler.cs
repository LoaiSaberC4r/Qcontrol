using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Windows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.CreateWindow;

internal sealed class CreateWindowCommandHandler
    : ICommandHandler<CreateWindowCommand, CreateWindowResponse>
{
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWindowCommandHandler(
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Window> windowReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
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
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<CreateWindowResponse>.Fail(new Error(
                "Windows.Create.Unauthenticated",
                ErrorMessage.Window_Authentication_Required,
                ErrorType.Unauthorized));
        }

        var waitingArea = await _waitingAreaReadRepository.GetByIdAsync(
            request.WaitingAreaId,
            cancellationToken);

        if (waitingArea is null)
        {
            return Result<CreateWindowResponse>.Fail(new Error(
                "Windows.Create.WaitingAreaNotFound",
                ErrorMessage.Window_WaitingArea_NotFound,
                ErrorType.NotFound));
        }

        var branch = await _branchReadRepository.GetByIdAsync(
            waitingArea.BranchId,
            cancellationToken);

        var normalizedNumber = request.Number.Trim();
        var existingNumberWindowId =
            await _windowReadRepository.FirstOrDefaultAsync(
                new WindowNumberExistsSpec(
                    request.WaitingAreaId,
                    normalizedNumber),
                cancellationToken);

        if (existingNumberWindowId > 0)
        {
            return Result<CreateWindowResponse>.Fail(new Error(
                "Windows.Create.NumberAlreadyExistsInWaitingArea",
                ErrorMessage.Window_Number_AlreadyExistsInWaitingArea,
                ErrorType.Conflict));
        }

        var normalizedIPAddress = IPAddressNormalizer.NormalizeOptional(
            request.IPAddress);

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
                return Result<CreateWindowResponse>.Fail(new Error(
                    "Windows.Create.IPAddressAlreadyExistsInWaitingArea",
                    ErrorMessage.Window_IPAddress_AlreadyExistsInWaitingArea,
                    ErrorType.Conflict));
            }
        }

        var window = Window.Create(
            branchId: waitingArea.BranchId,
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
                BranchId = window.BranchId,
                WaitingAreaId = window.WaitingAreaId,
                Number = window.Number,
                DescriptiveName = window.DescriptiveName,
                IPAddress = window.IPAddress,
                EnableTicketBooking = window.EnableTicketBooking,
                EnableDirectCall = window.EnableDirectCall,
                IsActive = window.IsActive,
                EffectiveIsActive =
                    branch is not null &&
                    branch.IsActive &&
                    waitingArea.IsActive &&
                    window.IsActive,
                RowVersion = RowVersionConverter.ToBase64(window.RowVersion),
                CreatedByApplicationUserId =
                    window.CreatedByApplicationUserId,
                CreatedOnUtc = window.CreatedOnUtc,
                Message = ErrorMessage.Window_Create_Success
            });
    }
}
