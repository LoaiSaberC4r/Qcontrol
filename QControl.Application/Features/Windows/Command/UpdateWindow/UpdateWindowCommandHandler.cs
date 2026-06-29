using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Windows.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Windows.Command.UpdateWindow;

internal sealed class UpdateWindowCommandHandler
    : ICommandHandler<UpdateWindowCommand, UpdateWindowResponse>
{
    private readonly IWriteReadRepository<Window> _windowReadRepository;
    private readonly IWriteReadRepository<WaitingArea> _waitingAreaReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteRepository<Window> _windowWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWindowCommandHandler(
        IWriteReadRepository<Window> windowReadRepository,
        IWriteReadRepository<WaitingArea> waitingAreaReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteRepository<Window> windowWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _windowReadRepository = windowReadRepository
            ?? throw new ArgumentNullException(nameof(windowReadRepository));
        _waitingAreaReadRepository = waitingAreaReadRepository
            ?? throw new ArgumentNullException(nameof(waitingAreaReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _windowWriteRepository = windowWriteRepository
            ?? throw new ArgumentNullException(nameof(windowWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateWindowResponse>> Handle(
        UpdateWindowCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UpdateWindowResponse>.Fail(new Error(
                "Windows.Update.Unauthenticated",
                ErrorMessage.Window_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<UpdateWindowResponse>.Fail(new Error(
                "Windows.Update.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var window =
            await _windowReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (window is null)
        {
            return Result<UpdateWindowResponse>.Fail(new Error(
                "Windows.Update.WindowNotFound",
                ErrorMessage.Window_NotFound,
                ErrorType.NotFound));
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
            return Result<UpdateWindowResponse>.Fail(new Error(
                "Windows.Update.NumberAlreadyExistsInWaitingArea",
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
                        window.WaitingAreaId,
                        normalizedIPAddress,
                        excludedWindowId: window.Id),
                    cancellationToken);

            if (existingIPAddressWindowId > 0)
            {
                return Result<UpdateWindowResponse>.Fail(new Error(
                    "Windows.Update.IPAddressAlreadyExistsInWaitingArea",
                    ErrorMessage.Window_IPAddress_AlreadyExistsInWaitingArea,
                    ErrorType.Conflict));
            }
        }

        var waitingArea = await _waitingAreaReadRepository.GetByIdAsync(
            window.WaitingAreaId,
            cancellationToken);

        var branch = await _branchReadRepository.GetByIdAsync(
            window.BranchId,
            cancellationToken);

        _concurrencyTokenManager.SetOriginalRowVersion(window, rowVersion);

        window.Update(
            number: normalizedNumber,
            descriptiveName: request.DescriptiveName,
            ipAddress: normalizedIPAddress,
            enableTicketBooking: request.EnableTicketBooking,
            enableDirectCall: request.EnableDirectCall,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _windowWriteRepository.Update(window);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<UpdateWindowResponse>.Fail(new Error(
                "Windows.Update.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }

        return Result<UpdateWindowResponse>.Ok(
            new UpdateWindowResponse
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
                    waitingArea is not null &&
                    branch.IsActive &&
                    waitingArea.IsActive &&
                    window.IsActive,
                RowVersion = RowVersionConverter.ToBase64(window.RowVersion),
                LastModifiedByApplicationUserId =
                    window.LastModifiedByApplicationUserId,
                ModifiedOnUtc = window.ModifiedOnUtc,
                Message = ErrorMessage.Window_Update_Success
            });
    }
}
