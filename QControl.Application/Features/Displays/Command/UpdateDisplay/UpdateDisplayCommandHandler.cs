using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Displays.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Application.Shared.Operational;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.UpdateDisplay;

internal sealed class UpdateDisplayCommandHandler
    : ICommandHandler<UpdateDisplayCommand, UpdateDisplayResponse>
{
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly IConcurrencyTokenManager _concurrencyTokenManager;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDisplayCommandHandler(
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Display> displayReadRepository,
        IWriteRepository<Display> displayWriteRepository,
        IConcurrencyTokenManager concurrencyTokenManager,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));
        _displayWriteRepository = displayWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWriteRepository));
        _concurrencyTokenManager = concurrencyTokenManager
            ?? throw new ArgumentNullException(nameof(concurrencyTokenManager));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateDisplayResponse>> Handle(
        UpdateDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UpdateDisplayResponse>.Fail(new Error(
                "Displays.Update.Unauthenticated",
                ErrorMessage.Display_Authentication_Required,
                ErrorType.Unauthorized));
        }

        if (!RowVersionConverter.TryDecode(request.RowVersion, out var rowVersion))
        {
            return Result<UpdateDisplayResponse>.Fail(new Error(
                "Displays.Update.InvalidRowVersion",
                ErrorMessage.RowVersion_Invalid,
                ErrorType.Validation));
        }

        var normalizedNumber = request.Number.Trim();
        var normalizedIPAddress = IPAddressNormalizer.TryNormalize(
            request.IPAddress,
            out var canonicalIPAddress)
            ? canonicalIPAddress
            : request.IPAddress.Trim();
        var normalizedSerialNo = request.SerialNo.Trim();
        var normalizedType = request.Type.Trim();

        var display =
            await _displayReadRepository.GetByIdTrackedAsync(
                request.Id,
                cancellationToken);

        if (display is null)
        {
            return DisplayNotFound();
        }

        var branch = await _branchReadRepository.GetByIdAsync(
            display.BranchId,
            cancellationToken);

        var duplicate =
            await CheckUpdateDuplicatesAsync(
                display.BranchId,
                display.Id,
                normalizedNumber,
                normalizedIPAddress,
                normalizedSerialNo,
                cancellationToken);

        if (duplicate.IsFailure)
        {
            return duplicate;
        }

        _concurrencyTokenManager.SetOriginalRowVersion(display, rowVersion);

        display.Update(
            number: normalizedNumber,
            ipAddress: normalizedIPAddress,
            serialNo: normalizedSerialNo,
            type: normalizedType,
            lastModifiedByApplicationUserId: _currentUser.UserId.Value);

        _displayWriteRepository.Update(display);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<UpdateDisplayResponse>.Fail(new Error(
                "Displays.Update.ConcurrencyConflict",
                ErrorMessage.Concurrency_Conflict,
                ErrorType.Conflict));
        }
        catch (DbUpdateException ex)
            when (DisplayUniqueConstraintErrorMapper.TryMapUpdate(
                ex,
                out var error))
        {
            return Result<UpdateDisplayResponse>.Fail(error);
        }

        return Result<UpdateDisplayResponse>.Ok(
            new UpdateDisplayResponse
            {
                Id = display.Id,
                BranchId = display.BranchId,
                Number = display.Number,
                IPAddress = display.IPAddress,
                SerialNo = display.SerialNo,
                Type = display.Type,
                IsActive = display.IsActive,
                EffectiveIsActive =
                    branch is not null &&
                    branch.IsActive &&
                    display.IsActive,
                RowVersion = RowVersionConverter.ToBase64(display.RowVersion),
                LastModifiedByApplicationUserId =
                    display.LastModifiedByApplicationUserId,
                ModifiedOnUtc = display.ModifiedOnUtc,
                Message = ErrorMessage.Display_Update_Success
            });
    }

    private async Task<Result<UpdateDisplayResponse>> CheckUpdateDuplicatesAsync(
        int branchId,
        int displayId,
        string number,
        string ipAddress,
        string serialNo,
        CancellationToken cancellationToken)
    {
        var existingNumberDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplayNumberExistsInBranchSpec(
                    branchId,
                    number,
                    excludedDisplayId: displayId),
                cancellationToken);

        if (existingNumberDisplayId > 0)
        {
            return Result<UpdateDisplayResponse>.Fail(new Error(
                "Displays.Update.NumberAlreadyExistsInBranch",
                ErrorMessage.Display_Number_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var existingIPAddressDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplayIPAddressExistsInBranchSpec(
                    branchId,
                    ipAddress,
                    excludedDisplayId: displayId),
                cancellationToken);

        if (existingIPAddressDisplayId > 0)
        {
            return Result<UpdateDisplayResponse>.Fail(new Error(
                "Displays.Update.IPAddressAlreadyExistsInBranch",
                ErrorMessage.Display_IPAddress_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        var existingSerialNoDisplayId =
            await _displayReadRepository.FirstOrDefaultAsync(
                new DisplaySerialNoExistsInBranchSpec(
                    branchId,
                    serialNo,
                    excludedDisplayId: displayId),
                cancellationToken);

        if (existingSerialNoDisplayId > 0)
        {
            return Result<UpdateDisplayResponse>.Fail(new Error(
                "Displays.Update.SerialNoAlreadyExistsInBranch",
                ErrorMessage.Display_SerialNo_AlreadyExistsInBranch,
                ErrorType.Conflict));
        }

        return Result<UpdateDisplayResponse>.Ok(null!);
    }

    private static Result<UpdateDisplayResponse> DisplayNotFound()
        => Result<UpdateDisplayResponse>.Fail(new Error(
            "Displays.Update.DisplayNotFound",
            ErrorMessage.Display_NotFound,
            ErrorType.NotFound));
}
