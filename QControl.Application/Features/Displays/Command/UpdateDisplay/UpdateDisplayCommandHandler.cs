using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Qcontrol.Application.Features.Displays.Shared;
using Qcontrol.Domain.Resources;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.Displays.Command.UpdateDisplay;

internal sealed class UpdateDisplayCommandHandler
    : ICommandHandler<UpdateDisplayCommand, UpdateDisplayResponse>
{
    private readonly IWriteReadRepository<Display> _displayReadRepository;
    private readonly IWriteRepository<Display> _displayWriteRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDisplayCommandHandler(
        IWriteReadRepository<Display> displayReadRepository,
        IWriteRepository<Display> displayWriteRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _displayReadRepository = displayReadRepository
            ?? throw new ArgumentNullException(nameof(displayReadRepository));

        _displayWriteRepository = displayWriteRepository
            ?? throw new ArgumentNullException(nameof(displayWriteRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<UpdateDisplayResponse>> Handle(
        UpdateDisplayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            return Result<UpdateDisplayResponse>.Fail(
                new Error(
                    Code: "Displays.Update.Unauthenticated",
                    Message: ErrorMessage.Display_Authentication_Required,
                    Type: ErrorType.Security));
        }

        var normalizedNumber = request.Number.Trim();
        var normalizedIPAddress = request.IPAddress.Trim();
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
            return Result<UpdateDisplayResponse>.Fail(
                new Error(
                    Code:
                        "Displays.Update.NumberAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .Display_Number_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
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
            return Result<UpdateDisplayResponse>.Fail(
                new Error(
                    Code:
                        "Displays.Update.IPAddressAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .Display_IPAddress_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
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
            return Result<UpdateDisplayResponse>.Fail(
                new Error(
                    Code:
                        "Displays.Update.SerialNoAlreadyExistsInBranch",
                    Message:
                        ErrorMessage
                            .Display_SerialNo_AlreadyExistsInBranch,
                    Type: ErrorType.Conflict));
        }

        return Result<UpdateDisplayResponse>.Ok(null!);
    }

    private static Result<UpdateDisplayResponse> DisplayNotFound()
        => Result<UpdateDisplayResponse>.Fail(
            new Error(
                Code: "Displays.Update.DisplayNotFound",
                Message: ErrorMessage.Display_NotFound,
                Type: ErrorType.NotFound));
}
